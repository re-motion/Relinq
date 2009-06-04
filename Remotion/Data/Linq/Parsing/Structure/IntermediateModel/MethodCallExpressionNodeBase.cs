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
using System.Linq.Expressions;
using System.Reflection;
using Remotion.Data.Linq.Clauses;
using Remotion.Data.Linq.Parsing.ExpressionTreeVisitors;
using Remotion.Utilities;

namespace Remotion.Data.Linq.Parsing.Structure.IntermediateModel
{
  /// <summary>
  /// Base class for <see cref="IExpressionNode"/> implementations that represent instantiations of <see cref="MethodCallExpression"/>.
  /// </summary>
  public abstract class MethodCallExpressionNodeBase : IExpressionNode
  {
    /// <summary>
    /// Gets the <see cref="MethodInfo"/> from a given <see cref="LambdaExpression"/> that has to wrap a <see cref="MethodCallExpression"/>.
    /// If the method is a generic method, its open generic method definition is returned.
    /// This method can be used for registration of the node type with an <see cref="MethodCallExpressionNodeTypeRegistry"/>.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="methodCall">The method call.</param>
    /// <returns></returns>
    protected static MethodInfo GetSupportedMethod<T> (Expression<Func<T>> methodCall)
    {
      var method = ParserUtility.GetMethod (methodCall);
      return method.IsGenericMethod ? method.GetGenericMethodDefinition() : method;
    }

    protected MethodCallExpressionNodeBase (IExpressionNode source)
    {
      Source = source;
    }

    public IExpressionNode Source { get; private set; }

    public abstract Expression Resolve (ParameterExpression inputParameter, Expression expressionToBeResolved);
    public abstract ParameterExpression CreateParameterForOutput ();
    public abstract IClause CreateClause (IClause previousClause);

    protected InvalidOperationException CreateResolveNotSupportedException ()
    {
      return
          new InvalidOperationException (
              GetType().Name + " does not support resolving of expressions, because it does not stream any data to the following node.");
    }

    protected InvalidOperationException CreateOutputParameterNotSupportedException ()
    {
      return
          new InvalidOperationException (
              GetType().Name + " does not support creating a parameter for its output because it does not stream any data to the following node.");
    }

    /// <summary>
    /// Gets the <see cref="SelectClause"/> needed when implementing the <see cref="CreateClause"/> method for a result modification node.
    /// </summary>
    /// <returns>The previous clause if it is a <see cref="SelectClause"/>, or a new clause with the given <paramref name="previousClause"/>
    /// and an identity projection if it is not.</returns>
    /// <remarks>
    /// Result modification nodes such as <see cref="CountExpressionNode"/> or <see cref="DistinctExpressionNode"/> do not identify real 
    /// clauses, they represent result modifications in the preceding <see cref="SelectClause"/>.
    /// Therefore, implementations of <see cref="CreateClause"/> will usually not add new clauses, but instead call 
    /// <see cref="SelectClause.AddResultModification"/> on the <paramref name="previousClause"/>. If, however, the <paramref name="previousClause"/>
    /// is not a <see cref="SelectClause"/> because it was optimized away, a new trivial <see cref="SelectClause"/> must be added by the node. This
    /// is implemented by this method.
    /// </remarks>
    protected SelectClause GetSelectClauseForResultModification (IClause previousClause)
    {
      ArgumentUtility.CheckNotNull ("previousClause", previousClause);

      var selectClause = previousClause as SelectClause;

      if (selectClause == null)
      {
        var selectorParameter = Source.CreateParameterForOutput();
        selectClause = new SelectClause (previousClause, Expression.Lambda (selectorParameter, selectorParameter));
      }

      return selectClause;
    }

    /// <summary>
    /// Gets and injects the <see cref="WhereClause"/> when implementing the <see cref="CreateClause"/> method for a result modification node with an 
    /// optional predicate if that predicate is not <see langword="null"/>.
    /// </summary>
    /// <remarks>
    /// Result modification nodes such as <see cref="CountExpressionNode"/> or <see cref="DistinctExpressionNode"/>
    /// do not identify real clauses, they represent result modifications in the preceding <see cref="SelectClause"/>.
    /// Some of them contain optional predicates, which need to be transformed into <see cref="WhereClause"/> in the <see cref="CreateClause"/> method.
    /// That <see cref="WhereClause"/> will be inserted before the <paramref name="selectClause"/> modified by the result modification node.
    /// Creation and insertion of this <see cref="WhereClause"/> is implemented by this method.
    /// </remarks>
    protected void CreateWhereClauseForResultModification (SelectClause selectClause, LambdaExpression optionalPredicate)
    {
      ArgumentUtility.CheckNotNull ("selectClause", selectClause);

      if (optionalPredicate != null)
      {
        var whereClause = new WhereClause (selectClause.PreviousClause, optionalPredicate);
        selectClause.PreviousClause = whereClause;
      }
    }

    /// <summary>
    /// Adjusts the <see cref="SelectClause.Selector"/> of the <paramref name="selectClause"/> modified by a result modification node for a nodes with an 
    /// optional selector if that selector is not <see langword="null"/>.
    /// </summary>
    /// <remarks>
    /// Result modification nodes such as <see cref="MinExpressionNode"/> or <see cref="SumExpressionNode"/>
    /// do not identify real clauses, they represent result modifications in the preceding <see cref="SelectClause"/>.
    /// Some of them contain optional selectors, which need to be combined with the <see cref="SelectClause.Selector"/> of the 
    /// <paramref name="selectClause"/> modified by the node.
    /// This process of adjusting the selector is implemented by this method.
    /// </remarks>
    protected void AdjustSelectorForResultModification (SelectClause selectClause, LambdaExpression optionalSelector)
    {
      ArgumentUtility.CheckNotNull ("selectClause", selectClause);

      if (optionalSelector != null)
      {
        // for a selectClause.Selector of x => x.Property1
        // and an OptionalSelector of a => a.Property2
        // make x => x.Property1.Property2 by replacing a (OptionalSelector.Parameters[0]) with the body of selectClause.Selector
        var newSelectorBody = ReplacingVisitor.Replace (optionalSelector.Parameters[0], selectClause.Selector.Body, optionalSelector.Body);
        var newSelector = Expression.Lambda (newSelectorBody, selectClause.Selector.Parameters[0]);
        selectClause.Selector = newSelector;
      }
    }
  }
}