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
using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using Remotion.Data.Linq.Clauses.Expressions;
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
    /// <returns>The result of the operator. This can be an enumerable, a single item, or a scalar value, depending on the operator.</returns>
    public abstract object ExecuteInMemory (object input);

    /// <summary>
    /// Clones this item, adjusting all <see cref="QuerySourceReferenceExpression"/> instances held by it as defined by
    /// <paramref name="cloneContext"/>.
    /// </summary>
    /// <param name="cloneContext">The clone context to use for replacing <see cref="QuerySourceReferenceExpression"/> objects.</param>
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

    protected object InvokeGenericOnEnumerable<TResult> (object input, Expression<Func<IEnumerable<object>, TResult>> methodInvocation, params object[] args)
    {
      ArgumentUtility.CheckNotNull ("input", input);
      ArgumentUtility.CheckNotNull ("methodInvocation", methodInvocation);
      ArgumentUtility.CheckNotNull ("args", args);

      var methodCallExpression = methodInvocation.Body as MethodCallExpression;
      // TODO 1319: Throw exception if null

      var allArguments = new object[1 + args.Length];
      allArguments[0] = input;
      args.CopyTo (allArguments, 1);

      try
      {
        var itemType = ReflectionUtility.GetAscribedGenericArguments (input.GetType (), typeof (IEnumerable<>))[0];
        return methodCallExpression.Method.GetGenericMethodDefinition ().MakeGenericMethod (itemType).Invoke (this, allArguments);
      }
      catch (TargetInvocationException ex)
      {
        throw ex.InnerException.PreserveStackTrace ();
      }
    }
  }
}