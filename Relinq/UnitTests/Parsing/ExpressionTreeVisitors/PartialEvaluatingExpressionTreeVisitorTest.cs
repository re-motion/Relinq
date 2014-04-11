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
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using NUnit.Framework;
using Remotion.Linq.Clauses.Expressions;
using Remotion.Linq.Development.UnitTesting;
using Remotion.Linq.Development.UnitTesting.Clauses.Expressions;
using Remotion.Linq.Parsing.ExpressionTreeVisitors;
using Remotion.Linq.UnitTests.TestDomain;

namespace Remotion.Linq.UnitTests.Parsing.ExpressionTreeVisitors
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
      Assert.That (result, Is.SameAs (result));
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
      Assert.That (result, Is.SameAs (lambdaExpression));
    }

    [Test]
    public void EvaluateLambdaWithSubQuery  ()
    {
      var subQuery = new SubQueryExpression(ExpressionHelper.CreateQueryModel<Cook>());
      LambdaExpression lambdaExpression = Expression.Lambda (subQuery);

      Expression result = PartialEvaluatingExpressionTreeVisitor.EvaluateIndependentSubtrees (lambdaExpression);
      Assert.That (result, Is.SameAs (lambdaExpression));
    }

    [Test]
    public void EvaluateWholeQueryTree ()
    {
// ReSharper disable ConvertToConstant.Local
      var i = 1;
// ReSharper restore ConvertToConstant.Local

      var source1 = ExpressionHelper.CreateQueryable<Cook> ();
      var source2 = ExpressionHelper.CreateQueryable<Cook> ();
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
      var source = ExpressionHelper.CreateQueryable<Cook> ();
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
      var source = ExpressionHelper.CreateQueryable<Cook> ();

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
      var source = ExpressionHelper.CreateQueryable<Cook> ();

      var queryExpression = ExpressionHelper.MakeExpression (() => source.Count ());

      var partiallyEvaluatedExpression = PartialEvaluatingExpressionTreeVisitor.EvaluateIndependentSubtrees (queryExpression);
      var countMethodCallExpression = (MethodCallExpression) partiallyEvaluatedExpression;

      Assert.That (countMethodCallExpression.Method.Name, Is.EqualTo ("Count"));
    }

    [Test]
    public void EvaluateExpression_WithException_BacksOut_AndEvaluatesAsMuchAsPossible ()
    {
      // "p && <nullVariableHolder.Item1>.Length > (0 + 3)" becomes "p && Exception (NullReferenceException, Exception (NullReferenceException, null.Length) > 3)"
      var nullVariableHolder = Tuple.Create ((string) null, (string) null);
      var nullExpression = Expression.Property (Expression.Constant (nullVariableHolder), "Item1");
      var throwingExpression = Expression.Property (nullExpression, "Length");
      var evaluatableOuterExpression = Expression.GreaterThan (throwingExpression, Expression.Add (Expression.Constant (0), Expression.Constant (3)));
      var nonEvaluatableOutermostExpression = Expression.AndAlso (Expression.Parameter (typeof (bool), "p"), evaluatableOuterExpression);

      var result = PartialEvaluatingExpressionTreeVisitor.EvaluateIndependentSubtrees (nonEvaluatableOutermostExpression);

      // p && Exception (...)
      Assert.That (result, Is.InstanceOf<BinaryExpression> ());
      Assert.That (((BinaryExpression) result).Left, Is.SameAs (nonEvaluatableOutermostExpression.Left));
      Assert.That (((BinaryExpression) result).Right, Is.TypeOf<PartialEvaluationExceptionExpression> ());

      // Exception (NullReferenceException, Exception (...) > 3)
      var exceptionExpression1 = (PartialEvaluationExceptionExpression) ((BinaryExpression) result).Right;
      Assert.That (exceptionExpression1.Exception, Is.InstanceOf<NullReferenceException>());
      var evaluatedExpression1 = exceptionExpression1.EvaluatedExpression;

      Assert.That (evaluatedExpression1, Is.AssignableTo<BinaryExpression> ());
      Assert.That (((BinaryExpression) evaluatedExpression1).Left, Is.TypeOf<PartialEvaluationExceptionExpression>());
      Assert.That (((BinaryExpression) evaluatedExpression1).Right, Is.InstanceOf<ConstantExpression> ().With.Property ("Value").EqualTo (3));

      // Exception (NullReferenceException, null.Length)
      var exceptionExpression2 = (PartialEvaluationExceptionExpression) ((BinaryExpression) evaluatedExpression1).Left;
      Assert.That (exceptionExpression2.Exception, Is.InstanceOf<NullReferenceException> ());
      Assert.That (exceptionExpression2.EvaluatedExpression, Is.InstanceOf<MemberExpression> ());
      var memberExpression = ((MemberExpression) exceptionExpression2.EvaluatedExpression);
      Assert.That (memberExpression.Expression, Is.InstanceOf<ConstantExpression>().With.Property ("Value").EqualTo (null));
      Assert.That (memberExpression.Member, Is.EqualTo (typeof (string).GetProperty ("Length")));
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
      Assert.That (((ConstantExpression) partiallyEvaluatedExpression).Value, Is.InstanceOf (typeof (AnonymousType)));
      Assert.That (((AnonymousType) ((ConstantExpression) partiallyEvaluatedExpression).Value).a, Is.EqualTo (2));
      Assert.That (((AnonymousType) ((ConstantExpression) partiallyEvaluatedExpression).Value).b, Is.EqualTo (1));
    }

    [Test]
    public void EvaluateListInitialization_WithoutParametersInMemberAssignments_IsEvaluated ()
    {
      var queryExpression = ExpressionHelper.MakeExpression<int, List<int>> (i => new List<int> { 2, 1 });

      var partiallyEvaluatedExpression = PartialEvaluatingExpressionTreeVisitor.EvaluateIndependentSubtrees (queryExpression);
      Assert.That (((ConstantExpression) partiallyEvaluatedExpression).Value, Is.InstanceOf (typeof (List<int>)));
      Assert.That (((ConstantExpression) partiallyEvaluatedExpression).Value, Is.EqualTo (new[] {2, 1}));
    }

    [Test]
    public void EvaluateOrdinaryConstant_Ignored ()
    {
      var expression = Expression.Constant (0);

      var result = PartialEvaluatingExpressionTreeVisitor.EvaluateIndependentSubtrees (expression);

      Assert.That (result, Is.SameAs (expression));
    }

    [Test]
    public void EvalueQueryableConstant_Inlined ()
    {
      var query = ExpressionHelper.CreateQueryable<Cook> ();
      var expression = Expression.Constant (query);

      var result = PartialEvaluatingExpressionTreeVisitor.EvaluateIndependentSubtrees (expression);

      Assert.That (result, Is.SameAs (query.Expression));
    }

    [Test]
    public void EvaluateQueryableConstant_InlinedPart_IsPartiallyEvaluated ()
    {
      var querySource = ExpressionHelper.CreateQueryable<Cook> ();
      var query = querySource.Where (c => "1" == 1.ToString ());
      var expression = Expression.Constant (query);

      var result = PartialEvaluatingExpressionTreeVisitor.EvaluateIndependentSubtrees (expression);

      var expectedExpression = querySource.Where (c => true).Expression;
      ExpressionTreeComparer.CheckAreEqualTrees (expectedExpression, result);
    }

    [Test]
    public void EvaluateQueryableConstant_InClosureMember ()
    {
      var innerQuery = from c in ExpressionHelper.CreateQueryable<Cook>() where c != null select c;
      var outerExpression = ExpressionHelper.MakeExpression (() => innerQuery);
      Assert.That (outerExpression.NodeType, Is.EqualTo (ExpressionType.MemberAccess));

      // outerExpression: <DisplayClass>.innerQuery
      // innerQuery.Expression: constantCookQueryable.Where (c => c != null)

      // transformation 1: constantInnerQuery
      // transformation 2: constantCookQueryable.Where (c => c != null)

      var result = PartialEvaluatingExpressionTreeVisitor.EvaluateIndependentSubtrees (outerExpression);
      Assert.That (result, Is.SameAs (innerQuery.Expression));
    }

    [Test]
    public void VisitUnknownNonExtensionExpression_Ignored ()
    {
      var expression = new UnknownExpression (typeof (object));
      var result = PartialEvaluatingExpressionTreeVisitor.EvaluateIndependentSubtrees (expression);

      Assert.That (result, Is.SameAs (expression));
    }

    [Test]
    public void VisitExtensionExpression ()
    {
      var innerExpression = Expression.MakeBinary (ExpressionType.Equal, Expression.Constant (0), Expression.Constant (0));
      var extensionExpression = new TestExtensionExpression (innerExpression);
      
      var result = PartialEvaluatingExpressionTreeVisitor.EvaluateIndependentSubtrees (extensionExpression);

      var expected = new TestExtensionExpression (Expression.Constant (true));
      ExpressionTreeComparer.CheckAreEqualTrees (expected, result);
    }
  }
}
