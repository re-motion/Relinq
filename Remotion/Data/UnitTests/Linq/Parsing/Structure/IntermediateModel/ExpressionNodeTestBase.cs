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
using NUnit.Framework;
using NUnit.Framework.SyntaxHelpers;
using Remotion.Data.Linq;
using Remotion.Data.Linq.Clauses;
using Remotion.Data.Linq.Clauses.Expressions;
using Remotion.Data.Linq.Parsing.Structure;
using Remotion.Data.Linq.Parsing.Structure.IntermediateModel;

namespace Remotion.Data.UnitTests.Linq.Parsing.Structure.IntermediateModel
{
  public abstract class ExpressionNodeTestBase
  {
    [SetUp]
    public virtual void SetUp ()
    {
      SourceNode = ExpressionNodeObjectMother.CreateConstant();
      QuerySourceClauseMapping = new QuerySourceClauseMapping();
      ClauseGenerationContext = new ClauseGenerationContext(
          QuerySourceClauseMapping, 
          MethodCallExpressionNodeTypeRegistry.CreateDefault(),
          new ResultModificationExpressionNodeRegistry ());

      SourceClause = (FromClauseBase) SourceNode.CreateClause (null, ClauseGenerationContext);
      SourceReference = new QuerySourceReferenceExpression (SourceClause);
    }

    public IQuerySourceExpressionNode SourceNode { get; private set; }
    public FromClauseBase SourceClause { get; private set; }
    public QuerySourceReferenceExpression SourceReference { get; private set; }
    public ClauseGenerationContext ClauseGenerationContext { get; private set; }
    public QuerySourceClauseMapping QuerySourceClauseMapping { get; private set; }

    public Expression<Func<int, string>> OptionalSelector
    {
      get { return (i => i.ToString ()); }
    }

    protected MethodInfo GetGenericMethodDefinition<TReturn> (Expression<Func<IQueryable<int>, TReturn>> methodCallLambda)
    {
      return GetMethod (methodCallLambda).GetGenericMethodDefinition ();
    }

    protected MethodInfo GetMethod<TReturn> (Expression<Func<IQueryable<int>, TReturn>> methodCallLambda)
    {
      var methodCallExpression = (MethodCallExpression) ExpressionHelper.MakeExpression (methodCallLambda);
      return methodCallExpression.Method;
    }


    protected void TestApplyToSelectClause (ResultModificationExpressionNodeBase node, Type expectedResultModificationType)
    {
      var selectClause = ExpressionHelper.CreateSelectClause();

      node.ApplyToSelectClause (selectClause, ClauseGenerationContext);

      Assert.That (selectClause.ResultModifications.Count, Is.EqualTo (1));
      Assert.That (selectClause.ResultModifications[0], Is.InstanceOfType (expectedResultModificationType));
      Assert.That (selectClause.ResultModifications[0].SelectClause, Is.SameAs (selectClause));
    }

    protected void TestApplyToSelectClause_WithOptionalPredicate (ResultModificationExpressionNodeBase node)
    {
      var selectClause = ExpressionHelper.CreateSelectClause ();
      var previousPreviousClause = selectClause.PreviousClause;

      // chain: previousPreviousClause <- previousClause

      node.ApplyToSelectClause (selectClause, ClauseGenerationContext);

      // chain: previousPreviousClause <- whereClause <- previousClause

      Assert.That (selectClause.PreviousClause, Is.Not.SameAs (previousPreviousClause));
      var newWhereClause = (WhereClause) selectClause.PreviousClause;
      Assert.That (newWhereClause.PreviousClause, Is.SameAs (previousPreviousClause));
      Assert.That (newWhereClause.Predicate, Is.SameAs (node.GetResolvedOptionalPredicate (ClauseGenerationContext)));
    }

    protected void TestApplyToSelectClause_WithOptionalSelector (ResultModificationExpressionNodeBase node)
    {
      var selectorOfPreviousClause = (MemberExpression) ExpressionHelper.MakeExpression<Student, int> (s => s.ID);
      var expectedNewSelector = (MethodCallExpression) ExpressionHelper.MakeExpression<Student, string> (s => s.ID.ToString());

      var previousPreviousClause = ExpressionHelper.CreateClause ();
      var selectClause = new SelectClause (previousPreviousClause, selectorOfPreviousClause);

      node.ApplyToSelectClause(selectClause, ClauseGenerationContext);

      ExpressionTreeComparer.CheckAreEqualTrees (expectedNewSelector, selectClause.Selector);
    }

    protected MethodCallExpressionParseInfo CreateParseInfo ()
    {
      return CreateParseInfo (SourceNode);
    }

    protected MethodCallExpressionParseInfo CreateParseInfo (IExpressionNode source)
    {
      return CreateParseInfo (source, "x");
    }

    protected MethodCallExpressionParseInfo CreateParseInfo (IExpressionNode source, string associatedIdentifier)
    {
      return new MethodCallExpressionParseInfo (associatedIdentifier, source, ExpressionHelper.CreateMethodCallExpression ());
    }

    protected MethodCallExpressionParseInfo CreateParseInfo (MethodInfo method)
    {
      var arguments = from p in method.GetParameters ()
                      let t = p.ParameterType
                      let defaultValue = t.IsValueType ? Activator.CreateInstance (t) : null
                      select Expression.Constant (defaultValue, t);
      var methodCallExpression = Expression.Call (method, arguments.ToArray());

      return new MethodCallExpressionParseInfo ("x", SourceNode, methodCallExpression);
    }
  }
}