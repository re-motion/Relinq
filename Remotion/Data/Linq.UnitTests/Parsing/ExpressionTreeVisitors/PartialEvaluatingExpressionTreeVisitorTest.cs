// This file is part of the re-motion Core Framework (www.re-motion.org)
// Copyright (C) 2005-2009 rubicon informationstechnologie gmbh, www.rubicon.eu
// 
// The re-motion Core Framework is free software; you can redistribute it 
// and/or modify it under the terms of the GNU Lesser General Public License 
// as published by the Free Software Foundation; either version 2.1 of the 
// License, or (at your option) any later version.
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
using Remotion.Data.Linq.Parsing.ExpressionTreeVisitors;
using Remotion.Data.Linq.UnitTests.TestDomain;
using Remotion.Data.Linq.UnitTests.TestUtilities;

namespace Remotion.Data.Linq.UnitTests.Parsing.ExpressionTreeVisitors
{
  [TestFixture]
  public class PartialEvaluatingExpressionTreeVisitorTest
  {
    [Test]
    public void EvaluateTopBinary ()
    {
      Expression treeRoot = Expression.Add (Expression.Constant (1), Expression.Constant (2));
      Expression result = PartialEvaluatingExpressionTreeVisitor.EvaluateIndependentSubtrees (treeRoot);
      Expression expected = Expression.Constant (3);
      ExpressionTreeComparer.CheckAreEqualTrees (expected, result);
    }

    [Test]
    public void EvaluateTopMemberAccess ()
    {
      Tuple<int, int> tuple = Tuple.Create (1, 2);

      Expression treeRoot = Expression.MakeMemberAccess (Expression.Constant (tuple), typeof (Tuple<int, int>).GetProperty ("Item1"));
      Expression result = PartialEvaluatingExpressionTreeVisitor.EvaluateIndependentSubtrees (treeRoot);
      Expression expected = Expression.Constant (1);
      ExpressionTreeComparer.CheckAreEqualTrees (expected, result);
    }

    [Test]
    public void EvaluateTopLambda()
    {
      Expression treeRoot = Expression.Lambda (Expression.Constant (0), Expression.Parameter (typeof (string), "s"));
      Expression result = PartialEvaluatingExpressionTreeVisitor.EvaluateIndependentSubtrees (treeRoot);
      Assert.AreSame (result, result);
    }

    [Test]
    public void EvaluateBinaryInLambdaWithoutParameter ()
    {
      Expression treeRoot = Expression.Lambda (Expression.Add (Expression.Constant (5), Expression.Constant (1)),
                                               Expression.Parameter (typeof (string), "s"));
      Expression result = PartialEvaluatingExpressionTreeVisitor.EvaluateIndependentSubtrees (treeRoot);
      Expression expected = Expression.Lambda (Expression.Constant (6), Expression.Parameter (typeof (string), "s"));
      ExpressionTreeComparer.CheckAreEqualTrees (expected, result);
    }

    [Test]
    public void EvaluateBinaryInLambdaWithParameter ()
    {
      ParameterExpression parameter = Expression.Parameter (typeof (int), "p");
      Expression constant1 = Expression.Constant (3);
      Expression constant2 = Expression.Constant (4);
      Expression constant3 = Expression.Constant (3);
      Expression multiply1 = Expression.Multiply (parameter, constant1);
      Expression multiply2 = Expression.Multiply (constant2, constant3);
      Expression add = Expression.Add (multiply1, multiply2);
      Expression treeRoot = Expression.Lambda (typeof (Func<int, int>), add, parameter);

      Expression result = PartialEvaluatingExpressionTreeVisitor.EvaluateIndependentSubtrees (treeRoot);
      Expression expected = Expression.Lambda (Expression.Add (Expression.Multiply (parameter, constant1), Expression.Constant (12)), parameter);
      ExpressionTreeComparer.CheckAreEqualTrees (expected, result);
    }

    [Test]
    public void EvaluateLambdaWithParameterFromOutside ()
    {
      ParameterExpression outsideParameter = Expression.Parameter (typeof (int), "p");
      LambdaExpression lambdaExpression = Expression.Lambda (outsideParameter);

      Expression result = PartialEvaluatingExpressionTreeVisitor.EvaluateIndependentSubtrees (lambdaExpression);
      Assert.AreSame (lambdaExpression, result);
    }

    [Test]
    public void EvaluateLambdaWithSubQuery  ()
    {
      var subQuery = new SubQueryExpression(ExpressionHelper.CreateQueryModel_Student());
      LambdaExpression lambdaExpression = Expression.Lambda (subQuery);

      Expression result = PartialEvaluatingExpressionTreeVisitor.EvaluateIndependentSubtrees (lambdaExpression);
      Assert.AreSame (lambdaExpression, result);
    }

    [Test]
    public void EvaluateWholeQueryTree ()
    {
// ReSharper disable ConvertToConstant.Local
      var i = 1;
// ReSharper restore ConvertToConstant.Local

      var source1 = ExpressionHelper.CreateStudentQueryable ();
      var source2 = ExpressionHelper.CreateStudentQueryable ();
      var query = from s1 in source1
                  from s2 in source2
                  where 2 > i + 5
                  select s1.ID + (1 + i);

      var partiallyEvaluatedExpression = PartialEvaluatingExpressionTreeVisitor.EvaluateIndependentSubtrees (query.Expression);

      var selectMethodCallExpression = (MethodCallExpression) partiallyEvaluatedExpression;
      var whereMethodCallExpression = (MethodCallExpression) selectMethodCallExpression.Arguments[0];
      var selectManyMethodCallExpression = (MethodCallExpression) whereMethodCallExpression.Arguments[0];

      var selectSelectorNavigator = new ExpressionTreeNavigator (selectMethodCallExpression.Arguments[1]);
      var wherePredicateNavigator = new ExpressionTreeNavigator (whereMethodCallExpression.Arguments[1]);
      var selectManyCollectionSelectorNavigator = new ExpressionTreeNavigator (selectManyMethodCallExpression.Arguments[1]);

      Assert.That (selectSelectorNavigator.Operand.Body.Right.Value, Is.EqualTo (2));
      Assert.That (wherePredicateNavigator.Operand.Body.Value, Is.EqualTo (false));
      Assert.That (selectManyCollectionSelectorNavigator.Operand.Body.Value, Is.SameAs (source2));
    }

    [Test]
    public void EvaluateWholeQueryTree_ThatDoesNotUseItsParameters ()
    {
      var source = ExpressionHelper.CreateStudentQueryable ();
      var query = from s1 in source
                  where false
                  select 0 + int.Parse ("0");

      var partiallyEvaluatedExpression = PartialEvaluatingExpressionTreeVisitor.EvaluateIndependentSubtrees (query.Expression);

      var selectMethodCallExpression = (MethodCallExpression) partiallyEvaluatedExpression;
      var whereMethodCallExpression = (MethodCallExpression) selectMethodCallExpression.Arguments[0];

      var selectSelectorNavigator = new ExpressionTreeNavigator (selectMethodCallExpression.Arguments[1]);
      var wherePredicateNavigator = new ExpressionTreeNavigator (whereMethodCallExpression.Arguments[1]);

      Assert.That (selectSelectorNavigator.Operand.Body.Value, Is.EqualTo (0));
      Assert.That (wherePredicateNavigator.Operand.Body.Value, Is.EqualTo (false));
    }

    [Test]
    public void EvaluateWholeQueryTree_WhoseLambdasAreInMemberExpressions_InsteadOfUnaryExpressions ()
    {
      var source = ExpressionHelper.CreateStudentQueryable ();

      Expression<Func<Cook, bool>> predicate = s1 => false;
      var queryExpression = ExpressionHelper.MakeExpression (() => source.Where (predicate));

      Assert.That (((MethodCallExpression) queryExpression).Arguments[1].NodeType, Is.EqualTo (ExpressionType.MemberAccess),
          "Usually, this would be a UnaryExpression (Quote containing the Lambda); but we pass a MemberExpression containing the lambda.");

      var partiallyEvaluatedExpression = PartialEvaluatingExpressionTreeVisitor.EvaluateIndependentSubtrees (queryExpression);
      var whereMethodCallExpression = (MethodCallExpression) partiallyEvaluatedExpression;
      var wherePredicateNavigator = new ExpressionTreeNavigator (whereMethodCallExpression.Arguments[1]);
      var wherePredicateLambdaNavigator = new ExpressionTreeNavigator ((Expression) wherePredicateNavigator.Value);

      Assert.That (wherePredicateLambdaNavigator.Body.Value, Is.EqualTo (false));
    }

    [Test]
    public void EvaluateWholeQueryTree_WithoutLambdas ()
    {
      var source = ExpressionHelper.CreateStudentQueryable ();

      var queryExpression = ExpressionHelper.MakeExpression (() => source.Count ());

      var partiallyEvaluatedExpression = PartialEvaluatingExpressionTreeVisitor.EvaluateIndependentSubtrees (queryExpression);
      var countMethodCallExpression = (MethodCallExpression) partiallyEvaluatedExpression;

      Assert.That (countMethodCallExpression.Method.Name, Is.EqualTo ("Count"));
    }

    [Test]
    public void EvaluateMemberInitialization_WithParametersInMemberAssignments_IsNotEvaluated ()
    {
      var queryExpression = ExpressionHelper.MakeExpression<int, AnonymousType> (i => new AnonymousType { a = i, b = 1 });

      var partiallyEvaluatedExpression = PartialEvaluatingExpressionTreeVisitor.EvaluateIndependentSubtrees (queryExpression);
      Assert.That (partiallyEvaluatedExpression, Is.SameAs (queryExpression));
    }

    [Test]
    public void EvaluateListInitialization_WithParametersInMemberAssignments_IsNotEvaluated ()
    {
      var queryExpression = ExpressionHelper.MakeExpression<int, List<int>> (i => new List<int> { i, 1 });

      var partiallyEvaluatedExpression = PartialEvaluatingExpressionTreeVisitor.EvaluateIndependentSubtrees (queryExpression);
      Assert.That (partiallyEvaluatedExpression, Is.SameAs (queryExpression));
    }

    [Test]
    public void EvaluateMemberInitialization_WithoutParametersInMemberAssignments_IsEvaluated ()
    {
      var queryExpression = ExpressionHelper.MakeExpression<int, AnonymousType> (i => new AnonymousType { a = 2, b = 1 });

      var partiallyEvaluatedExpression = PartialEvaluatingExpressionTreeVisitor.EvaluateIndependentSubtrees (queryExpression);
      Assert.That (((ConstantExpression) partiallyEvaluatedExpression).Value, Is.InstanceOfType (typeof (AnonymousType)));
      Assert.That (((AnonymousType) ((ConstantExpression) partiallyEvaluatedExpression).Value).a, Is.EqualTo (2));
      Assert.That (((AnonymousType) ((ConstantExpression) partiallyEvaluatedExpression).Value).b, Is.EqualTo (1));
    }

    [Test]
    public void EvaluateListInitialization_WithoutParametersInMemberAssignments_IsEvaluated ()
    {
      var queryExpression = ExpressionHelper.MakeExpression<int, List<int>> (i => new List<int> { 2, 1 });

      var partiallyEvaluatedExpression = PartialEvaluatingExpressionTreeVisitor.EvaluateIndependentSubtrees (queryExpression);
      Assert.That (((ConstantExpression) partiallyEvaluatedExpression).Value, Is.InstanceOfType (typeof (List<int>)));
      Assert.That (((ConstantExpression) partiallyEvaluatedExpression).Value, Is.EqualTo (new[] {2, 1}));
    }

    [Test]
    public void VisitUnknownExpression_Ignored ()
    {
      var expression = new UnknownExpression (typeof (object));
      var result = PartialEvaluatingExpressionTreeVisitor.EvaluateIndependentSubtrees (expression);

      Assert.That (result, Is.SameAs (expression));
    }
  }
}
