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
using System.Linq.Expressions;
using System.Reflection;
using NUnit.Framework;
using Remotion.Data.Linq.Clauses.Expressions;
using Remotion.Data.Linq.Clauses.ExpressionTreeVisitors;
using Remotion.Data.Linq.UnitTests.Parsing;
using Remotion.Data.Linq.UnitTests.TestDomain;

namespace Remotion.Data.Linq.UnitTests.Clauses.ExpressionTreeVisitors
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
      var result = AccessorFindingExpressionTreeVisitor.FindAccessorLambda (_searchedExpression, _searchedExpression, _intInputParameter);

      Expression<Func<int, int>> expectedResult = input => input;
      ExpressionTreeComparer.CheckAreEqualTrees (expectedResult, result);
    }

    [Test]
    public void TrivialExpression_WithEqualsTrue_ButNotReferenceEquals ()
    {
      var searchedExpression1 = new QuerySourceReferenceExpression (ExpressionHelper.CreateMainFromClause_Student());
      var searchedExpression2 = new QuerySourceReferenceExpression (searchedExpression1.ReferencedQuerySource);

      var inputParameter = Expression.Parameter (typeof (Chef), "input");
      var result = AccessorFindingExpressionTreeVisitor.FindAccessorLambda (searchedExpression1, searchedExpression2, inputParameter);

      Expression<Func<Chef, Chef>> expectedResult = input => input;
      ExpressionTreeComparer.CheckAreEqualTrees (expectedResult, result);
    }

    [Test]
    public void ConvertExpression ()
    {
      var parameter = Expression.Parameter (typeof (long), "input");
      var fullExpression = Expression.Convert (_searchedExpression, typeof (long));
      var result = AccessorFindingExpressionTreeVisitor.FindAccessorLambda (_searchedExpression, fullExpression, parameter);

      Expression<Func<long, int>> expectedResult = input => (int) input;
      ExpressionTreeComparer.CheckAreEqualTrees (expectedResult, result);
    }

    [Test]
    public void ConvertCheckedExpression ()
    {
      var parameter = Expression.Parameter (typeof (long), "input");
      var fullExpression = Expression.ConvertChecked (_searchedExpression, typeof (long));
      var result = AccessorFindingExpressionTreeVisitor.FindAccessorLambda (_searchedExpression, fullExpression, parameter);

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
      var result = AccessorFindingExpressionTreeVisitor.FindAccessorLambda (_searchedExpression, fullExpression, _simpleInputParameter);

      var inputParameter = Expression.Parameter (typeof (AnonymousType), "input");
      var expectedResult = Expression.Lambda (
#if NET_3_5
          Expression.Call (inputParameter, _anonymousTypeAGetter), 
#else
          Expression.MakeMemberAccess (inputParameter, _anonymousTypeAProperty), // .NET 4.0's Expression.New substitutes "get_a()" with "a"
#endif
          inputParameter);  // input => input.get_a()
      
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
      var result = AccessorFindingExpressionTreeVisitor.FindAccessorLambda (_searchedExpression, fullExpression, _simpleInputParameter);

      Expression<Func<AnonymousType, int>> expectedResult = input => input.a;
      ExpressionTreeComparer.CheckAreEqualTrees (expectedResult, result);
    }

    [Test]
    public void NestedNewExpression ()
    {
      var outerAnonymousTypeCtor = typeof (AnonymousType<int, AnonymousType>).GetConstructor (new[] { typeof (int), typeof (AnonymousType) });
      var outerAnonymousTypeAGetter = typeof (AnonymousType<int, AnonymousType>).GetMethod ("get_a");
      var outerAnonymousTypeBGetter = typeof (AnonymousType<int, AnonymousType>).GetMethod ("get_b");
      var outerAnonymousTypeBProperty = typeof (AnonymousType<int, AnonymousType>).GetProperty ("b");

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
      var result = AccessorFindingExpressionTreeVisitor.FindAccessorLambda (_searchedExpression, fullExpression, _nestedInputParameter);

      var inputParameter = Expression.Parameter (typeof (AnonymousType<int, AnonymousType>), "input");
      var expectedResult = Expression.Lambda (
#if NET_3_5
          Expression.Call (Expression.Call (inputParameter, outerAnonymousTypeBGetter), _anonymousTypeAGetter), 
#else
          Expression.MakeMemberAccess (Expression.MakeMemberAccess (inputParameter, outerAnonymousTypeBProperty), _anonymousTypeAProperty),
#endif
          inputParameter);  // input => input.get_b().get_a()

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

      var result = AccessorFindingExpressionTreeVisitor.FindAccessorLambda (_searchedExpression, fullExpression, _nestedInputParameter);

      Expression<Func<AnonymousType<int, AnonymousType>, int>> expectedResult = input => input.b.a;
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

      AccessorFindingExpressionTreeVisitor.FindAccessorLambda (_searchedExpression, fullExpression, _simpleInputParameter);
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

      AccessorFindingExpressionTreeVisitor.FindAccessorLambda (_searchedExpression, fullExpression, _simpleInputParameter);
    }

    [Test]
    [ExpectedException (typeof (ArgumentException), ExpectedMessage = "The given expression 'new AnonymousType(0, 0)' does not contain the "
        + "searched expression '0' in a nested NewExpression with member assignments or a MemberBindingExpression.\r\nParameter name: fullExpression")]
    public void SearchedExpressionNotFound_AlthoughInNewExpressionWithoutMember ()
    {
      var fullExpression = Expression.New (_anonymousTypeCtorWithArgs, _searchedExpression, _searchedExpression);

      AccessorFindingExpressionTreeVisitor.FindAccessorLambda (_searchedExpression, fullExpression, _simpleInputParameter);
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

      AccessorFindingExpressionTreeVisitor.FindAccessorLambda (_searchedExpression, fullExpression, _simpleInputParameter);
    }

    [Test]
    [ExpectedException (typeof (ArgumentException), ExpectedMessage = "The given expression '+0' does not contain the searched expression '0' in a "
        + "nested NewExpression with member assignments or a MemberBindingExpression.\r\nParameter name: fullExpression")]
    public void SearchedExpressionNotFound_AlthoughInUnaryPlusExpression ()
    {
      var fullExpression = Expression.UnaryPlus (_searchedExpression);
      AccessorFindingExpressionTreeVisitor.FindAccessorLambda (_searchedExpression, fullExpression, _intInputParameter);
    }
  }
}
