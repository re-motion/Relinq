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
using NUnit.Framework;
using NUnit.Framework.SyntaxHelpers;
using Remotion.Data.Linq.Clauses.Expressions;
using Remotion.Data.Linq.Parsing.ExpressionTreeVisitors.TreeEvaluation;
using Remotion.Data.UnitTests.Linq.Parsing.Structure.TestDomain;

namespace Remotion.Data.UnitTests.Linq.Parsing.ExpressionTreeVisitors.TreeEvaluation
{
  [TestFixture]
  public class EvaluatableTreeFindingVisitorTest
  {
    [Test]
    public void SimpleExpression_IsEvaluatable ()
    {
      var expression = Expression.Constant (0);
      var evaluationInfo = EvaluatableTreeFindingVisitor.Analyze (expression);

      Assert.That (evaluationInfo.IsEvaluatableExpression (expression), Is.True);
    }

    [Test]
    public void NestedExpression_InnerAndOuterAreEvaluatable ()
    {
      var innerExpressionLeft = Expression.Constant (0);
      var innerExpressionRight = Expression.Constant (0);
      var outerExpression = Expression.MakeBinary (ExpressionType.Add, innerExpressionLeft, innerExpressionRight);
      var evaluationInfo = EvaluatableTreeFindingVisitor.Analyze (outerExpression);

      Assert.That (evaluationInfo.IsEvaluatableExpression (outerExpression), Is.True);
      Assert.That (evaluationInfo.IsEvaluatableExpression (innerExpressionLeft), Is.True);
      Assert.That (evaluationInfo.IsEvaluatableExpression (innerExpressionRight), Is.True);
    }

    [Test]
    public void ParameterExpression_IsNotEvaluatable ()
    {
      var expression = ExpressionHelper.CreateParameterExpression ();
      var evaluationInfo = EvaluatableTreeFindingVisitor.Analyze (expression);

      Assert.That (evaluationInfo.IsEvaluatableExpression (expression), Is.False);
    }

    [Test]
    public void ExpressionContainingParameterExpression_IsNotEvaluatable ()
    {
      var expression = Expression.MakeBinary (
          ExpressionType.Equal, 
          ExpressionHelper.CreateParameterExpression (), 
          ExpressionHelper.CreateParameterExpression ());
      
      var evaluationInfo = EvaluatableTreeFindingVisitor.Analyze (expression);
      Assert.That (evaluationInfo.IsEvaluatableExpression (expression), Is.False);
    }

    [Test]
    public void ParameterExpression_SiblingCanBeEvaluatable ()
    {
      var expression = Expression.MakeBinary (
          ExpressionType.Equal,
          ExpressionHelper.CreateParameterExpression (),
          Expression.Constant (0));

      var evaluationInfo = EvaluatableTreeFindingVisitor.Analyze (expression);
      Assert.That (evaluationInfo.IsEvaluatableExpression (expression.Right), Is.True);
    }

    [Test]
    public void NonStandardExpressions_AreNotEvaluatable ()
    {
      var expression = new SubQueryExpression (ExpressionHelper.CreateQueryModel_Student());
      var evaluationInfo = EvaluatableTreeFindingVisitor.Analyze (expression);

      Assert.That (evaluationInfo.IsEvaluatableExpression (expression), Is.False);
    }

    [Test]
    public void NullExpression_InOtherExpression_IsIgnored ()
    {
      var expression = Expression.MakeBinary (
          ExpressionType.Equal,
          ExpressionHelper.CreateParameterExpression (),
          ExpressionHelper.CreateParameterExpression ());

      Assert.That (expression.Conversion, Is.Null);

      var evaluationInfo = EvaluatableTreeFindingVisitor.Analyze (expression);
      Assert.That (evaluationInfo.Count, Is.EqualTo (0));
    }

    [Test]
    public void MethodCall_WithIQueryableObject_IsNotEvaluatable ()
    {
      var source = ExpressionHelper.CreateStudentQueryable ();
      var expression = ExpressionHelper.MakeExpression (() => source.ToString());

      var evaluationInfo = EvaluatableTreeFindingVisitor.Analyze (expression);
      Assert.That (evaluationInfo.IsEvaluatableExpression (expression), Is.False);
    }

    [Test]
    public void MethodCall_WithIQueryableParameter_IsNotEvaluatable ()
    {
      var source = ExpressionHelper.CreateStudentQueryable ();
      var expression = ExpressionHelper.MakeExpression (() => source.Count ());

      var evaluationInfo = EvaluatableTreeFindingVisitor.Analyze (expression);
      Assert.That (evaluationInfo.IsEvaluatableExpression (expression), Is.False);
    }

    [Test]
    public void MemberExpression_WithIQueryableObject_IsNotEvaluatable ()
    {
      var source = new QueryableFakeWithCount<int>();
      var expression = ExpressionHelper.MakeExpression (() => source.Count);

      var evaluationInfo = EvaluatableTreeFindingVisitor.Analyze (expression);
      Assert.That (evaluationInfo.IsEvaluatableExpression (expression), Is.False);
    }

    [Test]
    public void MemberInitialization_WithParametersInMemberAssignments_IsNotEvaluatable ()
    {
      var expression = (MemberInitExpression) ExpressionHelper.MakeExpression<int, AnonymousType> (i => new AnonymousType { a = i, b = 1 });

      var evaluationInfo = EvaluatableTreeFindingVisitor.Analyze (expression);
      Assert.That (evaluationInfo.IsEvaluatableExpression (expression), Is.False);
      Assert.That (evaluationInfo.IsEvaluatableExpression (expression.NewExpression), Is.False);
    }

    [Test]
    public void ListInitialization_WithParametersInMemberAssignments_IsNotEvaluatable ()
    {
      var expression = (ListInitExpression) ExpressionHelper.MakeExpression<int, List<int>> (i => new List<int> { i, 1 });

      var evaluationInfo = EvaluatableTreeFindingVisitor.Analyze (expression);
      Assert.That (evaluationInfo.IsEvaluatableExpression (expression), Is.False);
      Assert.That (evaluationInfo.IsEvaluatableExpression (expression.NewExpression), Is.False);
    }

    [Test]
    public void MemberInitialization_WithoutParametersInMemberAssignments_IsEvaluatable ()
    {
      var expression = (MemberInitExpression) ExpressionHelper.MakeExpression<int, AnonymousType> (i => new AnonymousType { a = 1, b = 1 });

      var evaluationInfo = EvaluatableTreeFindingVisitor.Analyze (expression);
      Assert.That (evaluationInfo.IsEvaluatableExpression (expression), Is.True);
      Assert.That (evaluationInfo.IsEvaluatableExpression (expression.NewExpression), Is.True);
    }

    [Test]
    public void ListInitialization_WithoutParametersInMemberAssignments_IsEvaluatable ()
    {
      var expression = (ListInitExpression) ExpressionHelper.MakeExpression<int, List<int>> (i => new List<int> { 2, 1 });

      var evaluationInfo = EvaluatableTreeFindingVisitor.Analyze (expression);
      Assert.That (evaluationInfo.IsEvaluatableExpression (expression), Is.True);
      Assert.That (evaluationInfo.IsEvaluatableExpression (expression.NewExpression), Is.True);
    }

    [Test]
    public void VisitUnknownExpression_Ignored ()
    {
      var expression = new UnknownExpression (typeof (object));
      var result = EvaluatableTreeFindingVisitor.Analyze (expression);

      Assert.That (result.IsEvaluatableExpression (expression), Is.False);
    }
  }
}