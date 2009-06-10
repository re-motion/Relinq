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
using NUnit.Framework;
using System.Linq.Expressions;
using NUnit.Framework.SyntaxHelpers;
using Remotion.Data.Linq.Expressions;
using Remotion.Data.Linq.Parsing.ExpressionTreeVisitors;
using Remotion.Collections;

namespace Remotion.Data.UnitTests.Linq.Parsing.ExpressionTreeVisitors
{
  [TestFixture]
  public class PartialTreeEvaluatingVisitorTest
  {
    [Test]
    public void EvaluateTopBinary ()
    {
      Expression treeRoot = Expression.Add (Expression.Constant (1), Expression.Constant (2));
      Expression result = PartialTreeEvaluatingVisitor.EvaluateIndependentSubtrees (treeRoot);
      Expression expected = Expression.Constant (3);
      ExpressionTreeComparer.CheckAreEqualTrees (expected, result);
    }

    [Test]
    public void EvaluateTopMemberAccess ()
    {
      Tuple<int, int> tuple = Tuple.NewTuple (1, 2);

      Expression treeRoot = Expression.MakeMemberAccess (Expression.Constant (tuple), typeof (Tuple<int, int>).GetProperty ("A"));
      Expression result = PartialTreeEvaluatingVisitor.EvaluateIndependentSubtrees (treeRoot);
      Expression expected = Expression.Constant (1);
      ExpressionTreeComparer.CheckAreEqualTrees (expected, result);
    }

    [Test]
    public void EvaluateTopLambda()
    {
      Expression treeRoot = Expression.Lambda (Expression.Constant (0), Expression.Parameter (typeof (string), "s"));
      Expression result = PartialTreeEvaluatingVisitor.EvaluateIndependentSubtrees (treeRoot);
      Assert.AreSame (result, result);
    }

    [Test]
    public void EvaluateBinaryInLambdaWithoutParameter ()
    {
      Expression treeRoot = Expression.Lambda (Expression.Add (Expression.Constant (5), Expression.Constant (1)),
                                               Expression.Parameter (typeof (string), "s"));
      Expression result = PartialTreeEvaluatingVisitor.EvaluateIndependentSubtrees (treeRoot);
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
      Expression treeRoot = Expression.Lambda (typeof (System.Func<int, int>), add, parameter);

      Expression result = PartialTreeEvaluatingVisitor.EvaluateIndependentSubtrees (treeRoot);
      Expression expected = Expression.Lambda (Expression.Add (Expression.Multiply (parameter, constant1), Expression.Constant (12)), parameter);
      ExpressionTreeComparer.CheckAreEqualTrees (expected, result);
    }

    [Test]
    public void EvaluateLambdaWithParameterFromOutside ()
    {
      ParameterExpression outsideParameter = Expression.Parameter (typeof (int), "p");
      LambdaExpression lambdaExpression = Expression.Lambda (outsideParameter);

      Expression result = PartialTreeEvaluatingVisitor.EvaluateIndependentSubtrees (lambdaExpression);
      Assert.AreSame (lambdaExpression, result);
    }

    [Test]
    public void EvaluateLambdaWithSubQuery  ()
    {
      SubQueryExpression subQuery = new SubQueryExpression(ExpressionHelper.CreateQueryModel());
      LambdaExpression lambdaExpression = Expression.Lambda (subQuery);

      Expression result = PartialTreeEvaluatingVisitor.EvaluateIndependentSubtrees (lambdaExpression);
      Assert.AreSame (lambdaExpression, result);
    }

    [Test]
    public void EvaluateWholeQueryTree ()
    {
// ReSharper disable ConvertToConstant.Local
      var i = 1;
// ReSharper restore ConvertToConstant.Local

      var source1 = ExpressionHelper.CreateQuerySource ();
      var source2 = ExpressionHelper.CreateQuerySource ();
      var query = from s1 in source1
                  from s2 in source2
                  where 2 > i + 5
                  select s1.ID + (1 + i);

      var partiallyEvaluatedExpression = PartialTreeEvaluatingVisitor.EvaluateIndependentSubtrees (query.Expression);

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
      var source = ExpressionHelper.CreateQuerySource ();
      var query = from s1 in source
                  where false
                  select 0 + int.Parse ("0");

      var partiallyEvaluatedExpression = PartialTreeEvaluatingVisitor.EvaluateIndependentSubtrees (query.Expression);

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
      var source = ExpressionHelper.CreateQuerySource ();

      Expression<Func<Student, bool>> predicate = s1 => false;
      var queryExpression = ExpressionHelper.MakeExpression (() => source.Where (predicate));

      Assert.That (((MethodCallExpression) queryExpression).Arguments[1].NodeType, Is.EqualTo (ExpressionType.MemberAccess),
          "Usually, this would be a UnaryExpression (Quote containing the Lambda); but we pass a MemberExpression containing the lambda.");

      var partiallyEvaluatedExpression = PartialTreeEvaluatingVisitor.EvaluateIndependentSubtrees (queryExpression);
      var whereMethodCallExpression = (MethodCallExpression) partiallyEvaluatedExpression;
      var wherePredicateNavigator = new ExpressionTreeNavigator (whereMethodCallExpression.Arguments[1]);
      var wherePredicateLambdaNavigator = new ExpressionTreeNavigator ((Expression) wherePredicateNavigator.Value);

      Assert.That (wherePredicateLambdaNavigator.Body.Value, Is.EqualTo (false));
    }

    [Test]
    public void EvaluateWholeQueryTree_WithoutLambdas ()
    {
      var source = ExpressionHelper.CreateQuerySource ();

      var queryExpression = ExpressionHelper.MakeExpression (() => source.Count ());

      var partiallyEvaluatedExpression = PartialTreeEvaluatingVisitor.EvaluateIndependentSubtrees (queryExpression);
      var countMethodCallExpression = (MethodCallExpression) partiallyEvaluatedExpression;

      Assert.That (countMethodCallExpression.Method.Name, Is.EqualTo ("Count"));
    }
  }
}