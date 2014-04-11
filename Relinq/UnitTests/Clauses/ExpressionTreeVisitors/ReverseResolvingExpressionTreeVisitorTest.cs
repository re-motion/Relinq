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
using NUnit.Framework;
using Remotion.Linq.Clauses;
using Remotion.Linq.Clauses.Expressions;
using Remotion.Linq.Clauses.ExpressionTreeVisitors;
using Remotion.Linq.Development.UnitTesting;
using Remotion.Linq.UnitTests.Parsing;
using Remotion.Linq.UnitTests.TestDomain;

namespace Remotion.Linq.UnitTests.Clauses.ExpressionTreeVisitors
{
  [TestFixture]
  public class ReverseResolvingExpressionTreeVisitorTest
  {
    private ConstructorInfo _anonymousTypeCtor;
    
    private MainFromClause _fromClause1;
    private QuerySourceReferenceExpression _querySource1;
    private MainFromClause _fromClause2;
    private QuerySourceReferenceExpression _querySource2;

    private NewExpression _itemExpression;

    [SetUp]
    public void SetUp ()
    {
      _anonymousTypeCtor = typeof (AnonymousType<Cook, Cook>).GetConstructor (new[] { typeof (Cook), typeof (Cook) });

      _fromClause1 = ExpressionHelper.CreateMainFromClause_Int ("s1", typeof (Cook), ExpressionHelper.CreateQueryable<Cook> ());
      _querySource1 = new QuerySourceReferenceExpression (_fromClause1);

      _fromClause2 = ExpressionHelper.CreateMainFromClause_Int ("s2", typeof (Cook), ExpressionHelper.CreateQueryable<Cook> ());
      _querySource2 = new QuerySourceReferenceExpression (_fromClause2);

      _itemExpression = Expression.New (
          _anonymousTypeCtor,
          new Expression[] { _querySource1, _querySource2 },
          new MemberInfo[] { typeof (AnonymousType<Cook, Cook>).GetProperty ("a"), typeof (AnonymousType<Cook, Cook>).GetProperty ("b") });
    }

    [Test]
    public void ReverseResolve_NoReferenceExpression ()
    {
      // itemExpression: new AnonymousType<Cook, Cook> ( a = [s1], b = [s2] )
      // resolvedExpression: 0
      // expected result: input => 0

      var resolvedExpression = Expression.Constant (0);

      LambdaExpression lambdaExpression = ReverseResolvingExpressionTreeVisitor.ReverseResolve (_itemExpression, resolvedExpression);

      var expectedExpression = ExpressionHelper.CreateLambdaExpression<AnonymousType<Cook, Cook>, int> (input => 0);
      ExpressionTreeComparer.CheckAreEqualTrees (expectedExpression, lambdaExpression);
    }

    [Test]
    public void ReverseResolve_TopLevelReferenceExpression ()
    {
      // itemExpression: new AnonymousType<Cook, Cook> ( a = [s1], b = [s2] )
      // resolvedExpression: [s1]
      // expected result: input => input.a

      var resolvedExpression = _querySource1;

      LambdaExpression lambdaExpression = ReverseResolvingExpressionTreeVisitor.ReverseResolve (_itemExpression, resolvedExpression);

      var expectedExpression = ExpressionHelper.CreateLambdaExpression<AnonymousType<Cook, Cook>, Cook> (input => input.a);
      ExpressionTreeComparer.CheckAreEqualTrees (expectedExpression, lambdaExpression);
    }

    [Test]
    [ExpectedException (typeof (InvalidOperationException), ExpectedMessage = "Cannot create a LambdaExpression that retrieves the value of '[s3]' "
        + "from items with a structure of 'new AnonymousType`2(a = [s1], b = [s2])'. The item expression does not contain the value or it is too "
        + "complex.")]
    public void ReverseResolve_NonAccessibleReferenceExpression_Throws ()
    {
      // itemExpression: new AnonymousType<Cook, Cook> ( a = [s1], b = [s2] )
      // resolvedExpression: [s3]
      // expected result: exception

      var fromClause3 = ExpressionHelper.CreateMainFromClause_Int ("s3", typeof (Cook), ExpressionHelper.CreateQueryable<Cook>());
      var resolvedExpression = new QuerySourceReferenceExpression (fromClause3);

      ReverseResolvingExpressionTreeVisitor.ReverseResolve (_itemExpression, resolvedExpression);
    }

    [Test]
    public void ReverseResolve_MultipleNestedReferenceExpressions ()
    {
      // itemExpression: new AnonymousType<Cook, Cook> ( a = [s1], b = [s2] )
      // resolvedExpression: [s1].ID + [s2].ID
      // expected result: input => input.a.ID + input.b.ID

      var resolvedExpression = ExpressionHelper.Resolve<Cook, Cook, int> (_fromClause1, _fromClause2, (s1, s2) => s1.ID + s2.ID);

      LambdaExpression lambdaExpression = ReverseResolvingExpressionTreeVisitor.ReverseResolve (_itemExpression, resolvedExpression);

      var expectedExpression = ExpressionHelper.CreateLambdaExpression<AnonymousType<Cook, Cook>, int> (input => input.a.ID + input.b.ID);
      ExpressionTreeComparer.CheckAreEqualTrees (expectedExpression, lambdaExpression);
    }

    [Test]
    public void ReverseResolveLambda ()
    {
      // itemExpression: new AnonymousType<Cook, Cook> ( a = [s1], b = [s2] )
      // resolvedExpression: (x, y) => 0
      // expected result: (x, input, y) => 0

      var parameter1 = Expression.Parameter (typeof (int), "x");
      var parameter2 = Expression.Parameter (typeof (string), "y");
      var resolvedExpression = Expression.Lambda (Expression.Constant (0), parameter1, parameter2);

      var lambdaExpression = ReverseResolvingExpressionTreeVisitor.ReverseResolveLambda (_itemExpression, resolvedExpression, 1);

      var expectedExpression = ExpressionHelper.CreateLambdaExpression<int, AnonymousType<Cook, Cook>, string, int> ((x, input, y) => 0);
      ExpressionTreeComparer.CheckAreEqualTrees (expectedExpression, lambdaExpression);
    }

    [Test]
    [ExpectedException (typeof (ArgumentOutOfRangeException))]
    public void ReverseResolveLambda_InvalidPosition_TooBig ()
    {
      // itemExpression: new AnonymousType<Cook, Cook> ( a = [s1], b = [s2] )
      // resolvedExpression: (x, y) => 0
      // expected result: (x, input, y) => 0

      var parameter1 = Expression.Parameter (typeof (int), "x");
      var parameter2 = Expression.Parameter (typeof (string), "y");
      var resolvedExpression = Expression.Lambda (Expression.Constant (0), parameter1, parameter2);

      ReverseResolvingExpressionTreeVisitor.ReverseResolveLambda (_itemExpression, resolvedExpression, 3);
    }

    [Test]
    [ExpectedException (typeof (ArgumentOutOfRangeException))]
    public void ReverseResolveLambda_InvalidPosition_TooSmall ()
    {
      // itemExpression: new AnonymousType<Cook, Cook> ( a = [s1], b = [s2] )
      // resolvedExpression: (x, y) => 0
      // expected result: (x, input, y) => 0

      var parameter1 = Expression.Parameter (typeof (int), "x");
      var parameter2 = Expression.Parameter (typeof (string), "y");
      var resolvedExpression = Expression.Lambda (Expression.Constant (0), parameter1, parameter2);

      ReverseResolvingExpressionTreeVisitor.ReverseResolveLambda (_itemExpression, resolvedExpression, -1);
    }
  }
}
