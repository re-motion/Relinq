// This file is part of the re-motion Core Framework (www.re-motion.org)
// Copyright (C) 2005-2009 rubicon informationstechnologie gmbh, www.rubicon.eu
// 
// The re-motion Core Framework is free software; you can redistribute it 
// and/or modify it under the terms of the GNU Lesser General Public License 
// version 3.0 as published by the Free Software Foundation.
// 
// re-motion is distributed in the hope that it will be useful, 
// but WITHOUT ANY WARRANTY; without even the implied warranty of 
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the 
// GNU Lesser General Public License for more details.
// 
// You should have received a copy of the GNU Lesser General Public License
// along with re-motion; if not, see http://www.gnu.org/licenses.
// 
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Remotion.Data.Linq.Clauses.ResultOperators;
using Remotion.Data.Linq.Parsing;
using Remotion.Utilities;

namespace Remotion.Data.Linq.Clauses
{
  /// <summary>
  /// Represents an operation that is executed on the result set of the query, aggregating, filtering, or restricting the number of result items
  /// before the query result is returned.
  /// </summary>
  public abstract class ResultOperatorBase
  {
    protected ResultOperatorBase (IExecutionStrategy executionStrategy)
    {
      ArgumentUtility.CheckNotNull ("executionStrategy", executionStrategy);
      ExecutionStrategy = executionStrategy;
    }

    /// <summary>
    /// Gets the execution strategy to use for this <see cref="ResultOperatorBase"/>. The execution strategy defines how to dispatch a query
    /// to an implementation of <see cref="IQueryExecutor"/> when the <see cref="QueryProviderBase"/> needs to execute a query.
    /// </summary>
    public IExecutionStrategy ExecutionStrategy { get; private set; }

    /// <summary>
    /// Executes this result operator in memory, on a given input. Executing result operators in memory should only be 
    /// performed if the target query system does not support the operator.
    /// </summary>
    /// <param name="input">The input for the result operator. This must match the type expected by the operator.</param>
    /// <returns>The result of the operator. This can be an enumerable, a single item, or a scalar value, depending on the operator.</returns>
    /// <seealso cref="InvokeGenericOnEnumerable{TResult}"/>
    public abstract object ExecuteInMemory (object input);

    /// <summary>
    /// Gets the result type a query would have if it ended with this <see cref="ResultOperatorBase"/>. This can be an instantiation of 
    /// <see cref="IQueryable{T}"/>, the type of a single item, or a scalar type, depending on the kind of this <see cref="ResultOperatorBase"/>.
    /// Use <see cref="QueryModel.ResultType"/> to obtain the real result type of a query model, including all other 
    /// <see cref="QueryModel.ResultOperators"/>.
    /// </summary>
    /// <param name="inputResultType">The result type produced by the preceding <see cref="ResultOperatorBase"/>, or the <see cref="SelectClause"/>
    /// or the query if no previous <see cref="ResultOperatorBase"/> exists.</param>
    /// <returns>Gets the result type a query would have if it ended with this <see cref="ResultOperatorBase"/></returns>
    public abstract Type GetResultType (Type inputResultType);

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
    /// Transforms all the expressions in this item via the given <paramref name="transformation"/> delegate. Subclasses must override this method
    /// if they hold any expressions.
    /// </summary>
    /// <param name="transformation">The transformation object. This delegate is called for each <see cref="Expression"/> within this
    /// item, and those expressions will be replaced with what the delegate returns.</param>
    public virtual void TransformExpressions (Func<Expression, Expression> transformation)
    {
      //nothing to do here
    }

    /// <summary>
    /// Invokes a given generic method on an enumerable input via Reflection. Use this to implement <see cref="ExecuteInMemory"/> by defining
    /// a strongly typed, generic variant of <see cref="ExecuteInMemory"/>; then invoke that strongly typed variant via 
    /// <see cref="InvokeGenericOnEnumerable{TResult}"/>.
    /// </summary>
    /// <typeparam name="TResult">The result type of the method passed via <paramref name="genericMethodCaller"/>.</typeparam>
    /// <param name="input">The input object to invoke the method on. If this object does not implement <see cref="IEnumerable{T}"/>, this
    /// method will throw an <see cref="ArgumentTypeException"/>.</param>
    /// <param name="genericMethodCaller">A delegate holding exactly one public generic method with exactly one generic argument. This method is
    /// called via Reflection on the given <paramref name="input"/> argument.</param>
    /// <returns>The result of invoking the method in <paramref name="genericMethodCaller"/> on <paramref name="input"/>.</returns>
    /// <example>
    /// The <see cref="TakeResultOperator"/> uses this method as follows:
    /// <code>
    /// public override object ExecuteInMemory (object input)
    /// {
    ///   ArgumentUtility.CheckNotNull ("input", input);
    ///   return InvokeGenericOnEnumerable&lt;IEnumerable&lt;object&gt;&gt; (input, ExecuteInMemory);
    /// }
    /// 
    /// public IEnumerable&lt;T&gt; ExecuteInMemory&lt;T&gt; (IEnumerable&lt;T&gt; input)
    /// {
    ///   ArgumentUtility.CheckNotNull ("input", input);
    ///   return input.Take (Count);
    /// }
    /// </code>
    /// </example>
    protected object InvokeGenericOnEnumerable<TResult> (object input, Func<IEnumerable<object>, TResult> genericMethodCaller)
    {
      ArgumentUtility.CheckNotNull ("input", input);
      ArgumentUtility.CheckNotNull ("genericMethodCaller", genericMethodCaller);

      Type itemType = GetInputItemType (input);

      var method = genericMethodCaller.Method;
      if (!method.IsGenericMethod || !method.IsPublic)
      {
        throw new ArgumentException (
            "Method to invoke ('" + method.Name + "') must be a public generic method with exactly one generic argument.", 
            "genericMethodCaller");
      }

      var closedGenericMethod = method.GetGenericMethodDefinition ().MakeGenericMethod (itemType);
      return InvokeExecuteMethod (input, closedGenericMethod);
    }

    /// <summary>
    /// Gets the type of the items enumerated by <paramref name="input"/>.
    /// </summary>
    /// <param name="input">The input whose item type to retrieve. Must implement <see cref="IEnumerable{T}"/>, otherwise an exception
    /// is thrown.</param>
    /// <returns>The item type enumerated by <paramref name="input"/>.</returns>
    protected Type GetInputItemType (object input)
    {
      Type itemType = ReflectionUtility.GetItemTypeOfIEnumerable (input.GetType (), "input");
      return itemType;
    }

    /// <summary>
    /// Invokes the given <paramref name="method"/> via reflection on the given <paramref name="input"/>.
    /// </summary>
    /// <param name="input">The input to invoke the method with.</param>
    /// <param name="method">The method to be invoked.</param>
    /// <returns>The result of the invocation</returns>
    protected object InvokeExecuteMethod (object input, MethodInfo method)
    {
      try
      {
        return method.Invoke (this, new[] { input });
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
  }
}