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
using System.Linq.Expressions;
using System.Reflection;
using NUnit.Framework;
using Remotion.Linq.Clauses.Expressions;
using Remotion.Linq.Clauses.ExpressionTreeVisitors;
using Remotion.Linq.Development.UnitTesting;
using Remotion.Linq.UnitTests.Parsing;
using Remotion.Linq.UnitTests.TestDomain;

namespace Remotion.Linq.UnitTests.Clauses.ExpressionTreeVisitors
{
  [TestFixture]
  public class AccessorFindingExpressionTreeVisitorTest
  {
    private ConstantExpression _searchedExpression;

    private ConstructorInfo _anonymousTypeCtorWithoutArgs;
    private ConstructorInfo _anonymousTypeCtorWithArgs;
    
    private MethodInfo _anonymousTypeAGetter;
    private MethodInfo _anonymousTypeBGetter;
    private PropertyInfo _anonymousTypeAProperty;
    private PropertyInfo _anonymousTypeBProperty;

    private ParameterExpression _simpleInputParameter;
    private ParameterExpression _nestedInputParameter;
    private ParameterExpression _intInputParameter;

    [SetUp]
    public void SetUp ()
    {
      _searchedExpression = Expression.Constant (0);
      _anonymousTypeCtorWithoutArgs = typeof (AnonymousType).GetConstructor (Type.EmptyTypes);
      _anonymousTypeCtorWithArgs = typeof (AnonymousType).GetConstructor (new[] { typeof (int), typeof (int) });

      _anonymousTypeAGetter = typeof (AnonymousType).GetMethod ("get_a");
      _anonymousTypeBGetter = typeof (AnonymousType).GetMethod ("get_b");

      _anonymousTypeAProperty = typeof (AnonymousType).GetProperty ("a");
      _anonymousTypeBProperty = typeof (AnonymousType).GetProperty ("b");

      _simpleInputParameter = Expression.Parameter (typeof (AnonymousType), "input");
      _nestedInputParameter = Expression.Parameter (typeof (AnonymousType<int, AnonymousType>), "input");
      _intInputParameter = Expression.Parameter (typeof (int), "input");
    }

    [Test]
    public void TrivialExpression ()
    {
      var result = AccessorFindingExpressionVisitor.FindAccessorLambda (_searchedExpression, _searchedExpression, _intInputParameter);

      Expression<Func<int, int>> expectedResult = input => input;
      ExpressionTreeComparer.CheckAreEqualTrees (expectedResult, result);
    }

    [Test]
    public void TrivialExpression_WithEqualsTrue_ButNotReferenceEquals ()
    {
      var searchedExpression1 = new QuerySourceReferenceExpression (ExpressionHelper.CreateMainFromClause<Cook>());
      var searchedExpression2 = new QuerySourceReferenceExpression (searchedExpression1.ReferencedQuerySource);

      var inputParameter = Expression.Parameter (typeof (Cook), "input");
      var result = AccessorFindingExpressionVisitor.FindAccessorLambda (searchedExpression1, searchedExpression2, inputParameter);

      Expression<Func<Cook, Cook>> expectedResult = input => input;
      ExpressionTreeComparer.CheckAreEqualTrees (expectedResult, result);
    }

    [Test]
    public void ConvertExpression ()
    {
      var parameter = Expression.Parameter (typeof (long), "input");
      var fullExpression = Expression.Convert (_searchedExpression, typeof (long));
      var result = AccessorFindingExpressionVisitor.FindAccessorLambda (_searchedExpression, fullExpression, parameter);

      Expression<Func<long, int>> expectedResult = input => (int) input;
      ExpressionTreeComparer.CheckAreEqualTrees (expectedResult, result);
    }

    [Test]
    public void ConvertCheckedExpression ()
    {
      var parameter = Expression.Parameter (typeof (long), "input");
      var fullExpression = Expression.ConvertChecked (_searchedExpression, typeof (long));
      var result = AccessorFindingExpressionVisitor.FindAccessorLambda (_searchedExpression, fullExpression, parameter);

      Expression<Func<long, int>> expectedResult = input => (int) input;
      ExpressionTreeComparer.CheckAreEqualTrees (expectedResult, result);
    }

    [Test]
    public void SimpleNewExpression ()
    {
      // new AnonymousType (get_a = _searchedExpression, get_b = 1)
      var fullExpression = Expression.New (
          _anonymousTypeCtorWithArgs,
          new[] { _searchedExpression, Expression.Constant (1) },
          _anonymousTypeAGetter,
          _anonymousTypeBGetter);
      var result = AccessorFindingExpressionVisitor.FindAccessorLambda (_searchedExpression, fullExpression, _simpleInputParameter);

      var inputParameter = Expression.Parameter (typeof (AnonymousType), "input");
      var expectedResult = Expression.Lambda (
#if !NET_3_5
          Expression.MakeMemberAccess (inputParameter, _anonymousTypeAProperty),
#else
          Expression.Call (inputParameter, _anonymousTypeAGetter),
#endif
          inputParameter);

      ExpressionTreeComparer.CheckAreEqualTrees (expectedResult, result);
    }

    [Test]
    public void SimpleMemberBindingExpression ()
    {
      // new AnonymousType() { a = _searchedExpression, b = 1 }
      var fullExpression = Expression.MemberInit (
          Expression.New (_anonymousTypeCtorWithoutArgs),
          Expression.Bind (_anonymousTypeAProperty, _searchedExpression),
          Expression.Bind (_anonymousTypeBProperty, Expression.Constant (1)));
      var result = AccessorFindingExpressionVisitor.FindAccessorLambda (_searchedExpression, fullExpression, _simpleInputParameter);

      Expression<Func<AnonymousType, int>> expectedResult = input => input.a;
      ExpressionTreeComparer.CheckAreEqualTrees (expectedResult, result);
    }

    [Test]
    public void NestedNewExpression ()
    {
      var outerAnonymousTypeCtor = typeof (AnonymousType<int, AnonymousType>).GetConstructor (new[] { typeof (int), typeof (AnonymousType) });
      var outerAnonymousTypeAGetter = typeof (AnonymousType<int, AnonymousType>).GetMethod ("get_a");
      var outerAnonymousTypeBGetter = typeof (AnonymousType<int, AnonymousType>).GetMethod ("get_b");
#if !NET_3_5
      var outerAnonymousTypeBProperty = typeof (AnonymousType<int, AnonymousType>).GetProperty ("b");
#endif

      // new AnonymousType (get_a = 2, get_b = new AnonymousType (get_a = _searchedExpression, get_b = 1))
      var innerExpression = Expression.New (
          _anonymousTypeCtorWithArgs,
          new[] { _searchedExpression, Expression.Constant (1) },
          _anonymousTypeAGetter,
          _anonymousTypeBGetter);
      var fullExpression = Expression.New (
          outerAnonymousTypeCtor,
          new Expression[] { Expression.Constant (2), innerExpression },
          outerAnonymousTypeAGetter,
          outerAnonymousTypeBGetter);
      var result = AccessorFindingExpressionVisitor.FindAccessorLambda (_searchedExpression, fullExpression, _nestedInputParameter);

      var inputParameter = Expression.Parameter (typeof (AnonymousType<int, AnonymousType>), "input");
       // input => input.get_b().get_a()
      var expectedResult = Expression.Lambda (
#if !NET_3_5
          Expression.MakeMemberAccess (Expression.MakeMemberAccess (inputParameter, outerAnonymousTypeBProperty), _anonymousTypeAProperty),
#else
          Expression.Call (Expression.Call (inputParameter, outerAnonymousTypeBGetter), _anonymousTypeAGetter),
#endif
          inputParameter);

      ExpressionTreeComparer.CheckAreEqualTrees (expectedResult, result);
    }

    [Test]
    public void NestedMemberBindingExpression ()
    {
      var outerAnonymousTypeCtor =  typeof (AnonymousType<int, AnonymousType>).GetConstructor (Type.EmptyTypes);
      var outerAnonymousTypeAProperty =  typeof (AnonymousType<int, AnonymousType>).GetProperty ("a");
      var outerAnonymousTypeBProperty =  typeof (AnonymousType<int, AnonymousType>).GetProperty ("b");

      // new AnonymousType() { a = 2, b = new AnonymousType() { a = _searchedExpression, b = 1 } }
      var innerExpression = Expression.MemberInit (
          Expression.New (_anonymousTypeCtorWithoutArgs),
          Expression.Bind (_anonymousTypeAProperty, _searchedExpression),
          Expression.Bind (_anonymousTypeBProperty, Expression.Constant (1)));
      var fullExpression = Expression.MemberInit (
          Expression.New (outerAnonymousTypeCtor),
          Expression.Bind (outerAnonymousTypeAProperty, Expression.Constant (2)),
          Expression.Bind (outerAnonymousTypeBProperty, innerExpression));

      var result = AccessorFindingExpressionVisitor.FindAccessorLambda (_searchedExpression, fullExpression, _nestedInputParameter);

      Expression<Func<AnonymousType<int, AnonymousType>, int>> expectedResult = input => input.b.a;
      ExpressionTreeComparer.CheckAreEqualTrees (expectedResult, result);
    }

    [Test]
    public void NestedMemberBindingExpressionWithDerivedType ()
    {
      var outerTypeCtor =  typeof (Result<BaseValue>).GetConstructor (Type.EmptyTypes);
      var outerTypeValueProperty =  typeof (Result<BaseValue>).GetProperty ("Value");
      var innerTypeCtor = typeof (DerivedValue).GetConstructor (Type.EmptyTypes);
      var innerTypeStringValueProperty = typeof (DerivedValue).GetProperty ("StringValue");
      var searchedExpression = Expression.Constant ("a");
      var nestedInputParameter = Expression.Parameter (typeof (Result<BaseValue>), "input");

      // new Result<BaseValue>() { Value = new DerivedValue() { StringValue = _searchedExpression } }
      var innerExpression = Expression.MemberInit (
          Expression.New (innerTypeCtor),
          Expression.Bind (innerTypeStringValueProperty, searchedExpression));
      var fullExpression = Expression.MemberInit (
          Expression.New (outerTypeCtor),
          Expression.Bind (outerTypeValueProperty, innerExpression));

      var result = AccessorFindingExpressionVisitor.FindAccessorLambda (searchedExpression, fullExpression, nestedInputParameter);

      Expression<Func<Result<BaseValue>, string>> expectedResult = input => ((DerivedValue) input.Value).StringValue;
      ExpressionTreeComparer.CheckAreEqualTrees (expectedResult, result);
    }

    [Test]
    [ExpectedException (typeof (ArgumentException), ExpectedMessage = "The given expression 'new AnonymousType() {a = 3, b = 1}' does not contain the "
        + "searched expression '0' in a nested NewExpression with member assignments or a MemberBindingExpression.\r\nParameter name: fullExpression")]
    public void SearchedExpressionNotFound ()
    {
      var fullExpression = Expression.MemberInit (
          Expression.New (_anonymousTypeCtorWithoutArgs),
          Expression.Bind (_anonymousTypeAProperty, Expression.Constant (3)),
          Expression.Bind (_anonymousTypeBProperty, Expression.Constant (1)));

      AccessorFindingExpressionVisitor.FindAccessorLambda (_searchedExpression, fullExpression, _simpleInputParameter);
    }

    [Test]
    [ExpectedException (typeof (ArgumentException), ExpectedMessage = "The given expression 'new AnonymousType() {a = (0 + 0), b = 1}' does not contain the "
        + "searched expression '0' in a nested NewExpression with member assignments or a MemberBindingExpression.\r\nParameter name: fullExpression")]
    public void SearchedExpressionNotFound_AlthoughInOtherExpression ()
    {
      var fullExpression = Expression.MemberInit (
          Expression.New (_anonymousTypeCtorWithoutArgs),
          Expression.Bind (_anonymousTypeAProperty, Expression.MakeBinary (ExpressionType.Add, _searchedExpression, _searchedExpression)),
          Expression.Bind (_anonymousTypeBProperty, Expression.Constant (1)));

      AccessorFindingExpressionVisitor.FindAccessorLambda (_searchedExpression, fullExpression, _simpleInputParameter);
    }

    [Test]
    [ExpectedException (typeof (ArgumentException), ExpectedMessage = "The given expression 'new AnonymousType(0, 0)' does not contain the "
        + "searched expression '0' in a nested NewExpression with member assignments or a MemberBindingExpression.\r\nParameter name: fullExpression")]
    public void SearchedExpressionNotFound_AlthoughInNewExpressionWithoutMember ()
    {
      var fullExpression = Expression.New (_anonymousTypeCtorWithArgs, _searchedExpression, _searchedExpression);

      AccessorFindingExpressionVisitor.FindAccessorLambda (_searchedExpression, fullExpression, _simpleInputParameter);
    }

    [Test]
    [ExpectedException (typeof (ArgumentException), ExpectedMessage = "The given expression 'new AnonymousType() {List = {Void Add(Int32)(0)}, b = 1}' does not contain the "
        + "searched expression '0' in a nested NewExpression with member assignments or a MemberBindingExpression.\r\nParameter name: fullExpression")]
    public void SearchedExpressionNotFound_AlthoughInOtherMemberBinding ()
    {
      var anonymousTypeListProperty = typeof (AnonymousType).GetProperty ("List");
      var listAddMethod = typeof (List<int>).GetMethod ("Add");
      
      var fullExpression = Expression.MemberInit (
          Expression.New (_anonymousTypeCtorWithoutArgs),
          Expression.ListBind (anonymousTypeListProperty, Expression.ElementInit (listAddMethod, _searchedExpression)),
          Expression.Bind (_anonymousTypeBProperty, Expression.Constant (1)));

      AccessorFindingExpressionVisitor.FindAccessorLambda (_searchedExpression, fullExpression, _simpleInputParameter);
    }

    [Test]
    [ExpectedException (typeof (ArgumentException), ExpectedMessage = "The given expression '+0' does not contain the searched expression '0' in a "
        + "nested NewExpression with member assignments or a MemberBindingExpression.\r\nParameter name: fullExpression")]
    public void SearchedExpressionNotFound_AlthoughInUnaryPlusExpression ()
    {
      var fullExpression = Expression.UnaryPlus (_searchedExpression);
      AccessorFindingExpressionVisitor.FindAccessorLambda (_searchedExpression, fullExpression, _intInputParameter);
    }
  }
}
