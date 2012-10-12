// This file is part of the re-linq project (relinq.codeplex.com)
// Copyright (c) rubicon IT GmbH, www.rubicon.eu
// 
// re-linq is free software; you can redistribute it and/or modify it under 
// the terms of the GNU Lesser General Public License as published by the 
// Free Software Foundation; either version 2.1 of the License, 
// or (at your option) any later version.
// 
// re-linq is distributed in the hope that it will be useful, 
// but WITHOUT ANY WARRANTY; without even the implied warranty of 
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the 
// GNU Lesser General Public License for more details.
// 
// You should have received a copy of the GNU Lesser General Public License
// along with re-linq; if not, see http://www.gnu.org/licenses.
// 
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using Remotion.Linq.Clauses.ExpressionTreeVisitors;
using Remotion.Linq.Clauses.ResultOperators;
using Remotion.Linq.Clauses.StreamedData;
using Remotion.Linq.Utilities;

namespace Remotion.Linq.Clauses
{
  /// <summary>
  /// Represents an operation that is executed on the result set of the query, aggregating, filtering, or restricting the number of result items
  /// before the query result is returned.
  /// </summary>
  public abstract class ResultOperatorBase
  {
    /// <summary>
    /// Executes this result operator in memory, on a given input. Executing result operators in memory should only be 
    /// performed if the target query system does not support the operator.
    /// </summary>
    /// <param name="input">The input for the result operator. This must match the type of <see cref="IStreamedData"/> expected by the operator.</param>
    /// <returns>The result of the operator.</returns>
    /// <seealso cref="InvokeGenericExecuteMethod{TInput,TResult}"/>
    public abstract IStreamedData ExecuteInMemory (IStreamedData input);

    /// <summary>
    /// Gets information about the data streamed out of this <see cref="ResultOperatorBase"/>. This contains the result type a query would have if 
    /// it ended with this <see cref="ResultOperatorBase"/>, and it optionally includes an <see cref="StreamedSequenceInfo.ItemExpression"/> describing
    /// the streamed sequence's items.
    /// </summary>
    /// <param name="inputInfo">Information about the data produced by the preceding <see cref="ResultOperatorBase"/>, or the <see cref="SelectClause"/>
    /// of the query if no previous <see cref="ResultOperatorBase"/> exists.</param>
    /// <returns>Gets information about the data streamed out of this <see cref="ResultOperatorBase"/>.</returns>
    public abstract IStreamedDataInfo GetOutputDataInfo (IStreamedDataInfo inputInfo);

    /// <summary>
    /// Clones this item, registering its clone with the <paramref name="cloneContext"/> if it is a query source clause.
    /// </summary>
    /// <param name="cloneContext">The clones of all query source clauses are registered with this <see cref="CloneContext"/>.</param>
    /// <returns>A clone of this item.</returns>
    public abstract ResultOperatorBase Clone (CloneContext cloneContext);

    /// <summary>
    /// Accepts the specified visitor by calling its <see cref="IQueryModelVisitor.VisitResultOperator"/> method.
    /// </summary>
    /// <param name="visitor">The visitor to accept.</param>
    /// <param name="queryModel">The query model in whose context this clause is visited.</param>
    /// <param name="index">The index of this item in the <paramref name="queryModel"/>'s <see cref="QueryModel.ResultOperators"/> collection.</param>
    public virtual void Accept (IQueryModelVisitor visitor, QueryModel queryModel, int index)
    {
      ArgumentUtility.CheckNotNull ("visitor", visitor);
      ArgumentUtility.CheckNotNull ("queryModel", queryModel);

      visitor.VisitResultOperator (this, queryModel, index);
    }

    /// <summary>
    /// Transforms all the expressions in this item via the given <paramref name="transformation"/> delegate. Subclasses must apply the 
    /// <paramref name="transformation"/> to any expressions they hold. If a subclass does not hold any expressions, it shouldn't do anything
    /// in the implementation of this method.
    /// </summary>
    /// <param name="transformation">The transformation object. This delegate is called for each <see cref="Expression"/> within this
    /// item, and those expressions will be replaced with what the delegate returns.</param>
    public abstract void TransformExpressions (Func<Expression, Expression> transformation);

    /// <summary>
    /// Invokes a given generic method on an <see cref="IStreamedData"/> input via Reflection. Use this to implement 
    /// <see cref="ExecuteInMemory(IStreamedData)"/> by defining a strongly typed, generic variant 
    /// of <see cref="ExecuteInMemory(IStreamedData)"/>; then invoke that strongly typed 
    /// variant via  <see cref="InvokeGenericExecuteMethod{TInput,TResult}"/>.
    /// </summary>
    /// <typeparam name="TInput">The type of <see cref="IStreamedData"/> expected as an input to <paramref name="genericExecuteCaller"/>.</typeparam>
    /// <typeparam name="TResult">The type of <see cref="IStreamedData"/> expected as the output of <paramref name="genericExecuteCaller"/>.</typeparam>
    /// <param name="input">The input <see cref="IStreamedData"/> object to invoke the method on..</param>
    /// <param name="genericExecuteCaller">A delegate holding exactly one public generic method with exactly one generic argument. This method is
    /// called via Reflection on the given <paramref name="input"/> argument.</param>
    /// <returns>The result of invoking the method in <paramref name="genericExecuteCaller"/> on <paramref name="input"/>.</returns>
    /// <example>
    /// The <see cref="CountResultOperator"/> uses this method as follows:
    /// <code>
    /// public IStreamedData ExecuteInMemory (IStreamedData input)
    /// {
    ///   ArgumentUtility.CheckNotNull ("input", input);
    ///   return InvokeGenericExecuteMethod&lt;StreamedSequence, StreamedValue&gt; (input, ExecuteInMemory&lt;object&gt;);
    /// }
    ///
    /// public StreamedValue ExecuteInMemory&lt;T&gt; (StreamedSequence input)
    /// {
    ///   var sequence = input.GetTypedSequence&lt;T&gt; ();
    ///   var result = sequence.Sequence.Count ();
    ///   return new StreamedValue (result);
    /// }
    /// </code>
    /// </example>
    protected TResult InvokeGenericExecuteMethod<TInput, TResult> (IStreamedData input, Func<TInput, TResult> genericExecuteCaller)
      where TInput : IStreamedData
      where TResult : IStreamedData
    {
      ArgumentUtility.CheckNotNull ("input", input);
      ArgumentUtility.CheckNotNull ("genericExecuteCaller", genericExecuteCaller);

      var method = genericExecuteCaller.Method;
      if (!method.IsGenericMethod || method.GetGenericArguments ().Length != 1)
      {
        throw new ArgumentException (
            "Method to invoke ('" + method.Name + "') must be a generic method with exactly one generic argument.",
            "genericExecuteCaller");
      }

      var closedGenericMethod = input.DataInfo.MakeClosedGenericExecuteMethod (method.GetGenericMethodDefinition ());
      return (TResult) InvokeExecuteMethod (closedGenericMethod, input);
    }

    /// <summary>
    /// Invokes the given <paramref name="method"/> via reflection on the given <paramref name="input"/>.
    /// </summary>
    /// <param name="input">The input to invoke the method with.</param>
    /// <param name="method">The method to be invoked.</param>
    /// <returns>The result of the invocation</returns>
    protected object InvokeExecuteMethod (MethodInfo method, object input)
    {
      if (!method.IsPublic)
        throw new ArgumentException ("Method to invoke ('" + method.Name + "') must be a public method.", "method");

      var targetObject = method.IsStatic ? null : this;
      try
      {
        return method.Invoke (targetObject, new[] { input });
      }
      catch (TargetInvocationException ex)
      {
        throw ex.InnerException;
      }
      catch (ArgumentException ex)
      {
        var message = string.Format ("Cannot call method '{0}' on input of type '{1}': {2}", method.Name, input.GetType (), ex.Message);
        throw new ArgumentException (message, "method");
      }
    }

    /// <summary>
    /// Gets the constant value of the given expression, assuming it is a <see cref="ConstantExpression"/>. If it is
    /// not, an <see cref="InvalidOperationException"/> is thrown.
    /// </summary>
    /// <typeparam name="T">The expected value type. If the value is not of this type, an <see cref="InvalidOperationException"/> is thrown.</typeparam>
    /// <param name="expressionName">A string describing the value; this will be included in the exception message if an exception is thrown.</param>
    /// <param name="expression">The expression whose value to get.</param>
    /// <returns>
    /// The constant value of the given <paramref name="expression"/>.
    /// </returns>
    protected T GetConstantValueFromExpression<T> (string expressionName, Expression expression)
    {
      ArgumentUtility.CheckNotNull ("expression", expression);
      if (!typeof (T).IsAssignableFrom (expression.Type))
      {
        var message = string.Format (
            "The value stored by the {0} expression ('{1}') is not of type '{2}', it is of type '{3}'.",
            expressionName,
            FormattingExpressionTreeVisitor.Format (expression),
            typeof (T),
            expression.Type);
        throw new InvalidOperationException (message);
      }

      var itemAsConstantExpression = expression as ConstantExpression;
      if (itemAsConstantExpression != null)
      {
        return (T) itemAsConstantExpression.Value;
      }
      else
      {
        var message = string.Format (
            "The {0} expression ('{1}') is no ConstantExpression, it is a {2}.",
            expressionName,
            FormattingExpressionTreeVisitor.Format (expression),
            expression.GetType ().Name);
        throw new InvalidOperationException (message);
      }
    }

    protected void CheckSequenceItemType (StreamedSequenceInfo sequenceInfo, Type expectedItemType)
    {
      if (!expectedItemType.IsAssignableFrom (sequenceInfo.ResultItemType))
      {
        var message = string.Format (
            "The input sequence must have items of type '{0}', but it has items of type '{1}'.",
            expectedItemType,
            sequenceInfo.ResultItemType);

        throw new ArgumentTypeException (message, "inputInfo", typeof (IEnumerable<>).MakeGenericType (expectedItemType), sequenceInfo.ResultItemType);
      }
    }
  }
}
