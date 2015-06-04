// Copyright (c) rubicon IT GmbH, www.rubicon.eu
//
// See the NOTICE file distributed with this work for additional information
// regarding copyright ownership.  rubicon licenses this file to you under 
// the Apache License, Version 2.0 (the "License"); you may not use this 
// file except in compliance with the License.  You may obtain a copy of the 
// License at
//
//   http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software 
// distributed under the License is distributed on an "AS IS" BASIS, WITHOUT 
// WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.  See the 
// License for the specific language governing permissions and limitations
// under the License.
// 
using System;
using System.Linq.Expressions;
using System.Reflection;
using Remotion.Linq.Clauses.StreamedData;
using Remotion.Linq.Utilities;
using Remotion.Utilities;

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
    public void Accept (IQueryModelVisitor visitor, QueryModel queryModel, int index)
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
      if (!typeof (T).GetTypeInfo().IsAssignableFrom (expression.Type.GetTypeInfo()))
      {
        var message = string.Format (
            "The value stored by the {0} expression ('{1}') is not of type '{2}', it is of type '{3}'.",
            expressionName,
            expression.BuildString(),
            typeof (T),
            expression.Type);
        throw new ArgumentException (message, "expression");
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
            expression.BuildString(),
            expression.GetType ().Name);
        throw new ArgumentException (message, "expression");
      }
    }

    protected void CheckSequenceItemType (StreamedSequenceInfo inputInfo, Type expectedItemType)
    {
      if (!expectedItemType.GetTypeInfo().IsAssignableFrom (inputInfo.ResultItemType.GetTypeInfo()))
      {
        var message = string.Format (
            "The input sequence must have items of type '{0}', but it has items of type '{1}'.",
            expectedItemType,
            inputInfo.ResultItemType);

        throw new ArgumentException (message, "inputInfo");
      }
    }
  }
}
