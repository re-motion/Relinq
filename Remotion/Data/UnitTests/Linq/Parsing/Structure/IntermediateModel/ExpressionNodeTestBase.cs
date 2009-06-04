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
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using NUnit.Framework;
using NUnit.Framework.SyntaxHelpers;
using Remotion.Data.Linq.Clauses;
using Remotion.Data.Linq.Parsing.Structure.IntermediateModel;

namespace Remotion.Data.UnitTests.Linq.Parsing.Structure.IntermediateModel
{
  public abstract class ExpressionNodeTestBase
  {
    [SetUp]
    public virtual void SetUp ()
    {
      SourceStub = ExpressionNodeObjectMother.CreateConstant();
      SourceReference = new QuerySourceReferenceExpression (SourceStub);
    }

    public IQuerySourceExpressionNode SourceStub { get; private set; }
    public QuerySourceReferenceExpression SourceReference { get; private set; }

    protected MethodInfo GetGenericMethodDefinition<TReturn> (Expression<Func<IQueryable<int>, TReturn>> methodCallLambda)
    {
      return GetMethod (methodCallLambda).GetGenericMethodDefinition ();
    }

    protected MethodInfo GetMethod<TReturn> (Expression<Func<IQueryable<int>, TReturn>> methodCallLambda)
    {
      var methodCallExpression = (MethodCallExpression) ExpressionHelper.MakeExpression (methodCallLambda);
      return methodCallExpression.Method;
    }


    protected void TestCreateClause_PreviousClauseIsSelect (IExpressionNode node, Type expectedResultModificationType)
    {
      var previousClause = ExpressionHelper.CreateSelectClause();

      var clause = (SelectClause)node.CreateClause(previousClause);

      Assert.That (clause, Is.SameAs (previousClause));
      Assert.That (clause.ResultModifications.Count, Is.EqualTo (1));
      Assert.That (clause.ResultModifications[0], Is.InstanceOfType (expectedResultModificationType));
      Assert.That (clause.ResultModifications[0].SelectClause, Is.SameAs (clause));
    }

    protected void TestCreateClause_PreviousClauseIsNoSelect (IExpressionNode node, Type expectedResultModificationType)
    {
      var previousClause = ExpressionHelper.CreateMainFromClause ();

      var clause = (SelectClause) node.CreateClause (previousClause);

      Assert.That (clause.PreviousClause, Is.SameAs (previousClause));
      Assert.That (clause.ResultModifications.Count, Is.EqualTo (1));
      Assert.That (clause.ResultModifications[0], Is.InstanceOfType (expectedResultModificationType));
      Assert.That (clause.ResultModifications[0].SelectClause, Is.SameAs (clause));

      var expectedSelectorParameter = node.Source.CreateParameterForOutput ();
      var expectedSelector = Expression.Lambda (expectedSelectorParameter, expectedSelectorParameter);
      ExpressionTreeComparer.CheckAreEqualTrees (expectedSelector, clause.Selector);
    }

    protected void TestCreateClause_WithOptionalPredicate (IExpressionNode node, LambdaExpression optionalPredicate)
    {
      var previousClause = ExpressionHelper.CreateSelectClause ();
      var previousPreviousClause = previousClause.PreviousClause;

      // chain: previousPreviousClause <- previousClause

      var clause = (SelectClause) node.CreateClause (previousClause);

      // chain: previousPreviousClause <- whereClause <- previousClause

      Assert.That (clause, Is.SameAs (previousClause));
      Assert.That (clause.PreviousClause, Is.Not.SameAs (previousPreviousClause));
      var newWhereClause = (WhereClause) clause.PreviousClause;
      Assert.That (newWhereClause.PreviousClause, Is.SameAs (previousPreviousClause));
      Assert.That (newWhereClause.Predicate, Is.SameAs (optionalPredicate));
    }

    protected void TestCreateClause_WithOptionalSelector (IExpressionNode node, Expression<Func<Student, int>> selectorOfPreviousClause, Expression<Func<Student, string>> expectedNewSelector)
    {
      var previousPreviousClause = ExpressionHelper.CreateClause ();
      var previousClause = new SelectClause (previousPreviousClause, selectorOfPreviousClause);

      var clause = (SelectClause) node.CreateClause (previousClause);

      Assert.That (clause, Is.SameAs (previousClause));
      Assert.That (clause.Selector, Is.Not.SameAs (selectorOfPreviousClause));

      ExpressionTreeComparer.CheckAreEqualTrees (expectedNewSelector, clause.Selector);
    }
  }
}