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
using System.Linq;
using System.Linq.Expressions;
using NUnit.Framework;
using Remotion.Linq.Clauses.ExpressionTreeVisitors;
using Remotion.Linq.Development.UnitTesting;
using Remotion.Linq.UnitTests.TestDomain;

namespace Remotion.Linq.UnitTests.Clauses.ExpressionTreeVisitors.IntegrationTests
{
  [TestFixture]
  public class AccessorFindingExpressionTreeVisitorIntegrationTest
  {
    [Test]
    public void NewExpressionForAnonymousType()
    {
      Expression<Func<Cook, dynamic>> lambaExpression = c => new { OuterValue = new { InnerValue = c.FirstName } };

      // new OuterType (...)
      var resultInitExpression = ((NewExpression) lambaExpression.Body);
      // new InnerType (...)
      var derivedValueInitExpression = (NewExpression) resultInitExpression.Arguments[0];
      // c.FirstName
      var derivedValueStringValuePropertyExpression = (MemberExpression) derivedValueInitExpression.Arguments[0];

      var inputParameter = Expression.Parameter (resultInitExpression.Type, "input");

      var result = AccessorFindingExpressionVisitor.FindAccessorLambda (
          searchedExpression: derivedValueStringValuePropertyExpression,
          fullExpression: resultInitExpression,
          inputParameter: inputParameter);

      // Expression<Func<dynamic, string>> input => input.OuterValue.InnerValue;
      var expectedParameter = Expression.Parameter (resultInitExpression.Type, "input");
#if !NET_3_5
      var expectedOuterProperty = Expression.Property (expectedParameter, "OuterValue");
      var expectedInnerProperty = Expression.Property (expectedOuterProperty, "InnerValue");
      var expectedResult = Expression.Lambda (expectedInnerProperty, expectedParameter);
#else
      var expectedOuterGetter = Expression.Call (expectedParameter, "get_OuterValue", new Type[0]);
      var expectedInnerGetter = Expression.Call (expectedOuterGetter, "get_InnerValue", new Type[0]);
      var expectedResult = Expression.Lambda (expectedInnerGetter, expectedParameter);
#endif

      ExpressionTreeComparer.CheckAreEqualTrees (expectedResult, result);
    }

    [Test]
    public void MemberInitExpression ()
    {
      Expression<Func<Cook, Result<DerivedValue>>> lambaExpression =
          c => new Result<DerivedValue> { Value = new DerivedValue { StringValue = c.FirstName } };

      // new Result { ... }
      var resultInitExpression = ((MemberInitExpression) lambaExpression.Body);
      // BaseValue = new DerivedValue { ... }
      var derivedValueMemberAssignment = ((MemberAssignment) resultInitExpression.Bindings[0]);
      // new DerivedValue { ... }
      var derivedValueInitExpression = (MemberInitExpression) (derivedValueMemberAssignment.Expression);
      // StringValue = c.FirstName
      var derivedValueStringValueMemberAssignment = ((MemberAssignment) derivedValueInitExpression.Bindings[0]);
      // c.FirstName
      var derivedValueStringValuePropertyExpression = (MemberExpression) derivedValueStringValueMemberAssignment.Expression;

      var inputParameter = Expression.Parameter (typeof (Result<DerivedValue>), "input");

      var result = AccessorFindingExpressionVisitor.FindAccessorLambda (
          searchedExpression: derivedValueStringValuePropertyExpression,
          fullExpression: resultInitExpression,
          inputParameter: inputParameter);

      Expression<Func<Result<DerivedValue>, string>> expectedResult = input => input.Value.StringValue;
      ExpressionTreeComparer.CheckAreEqualTrees (expectedResult, result);
    }

    [Test]
    public void MemberInitExpressionForDerivedTypeCanBeUsedWithPropertyTypedAsBaseType ()
    {
      Expression<Func<Cook, Result<BaseValue>>> lambaExpression =
          c => new Result<BaseValue> { Value = new DerivedValue { StringValue = c.FirstName } };

      // new Result { ... }
      var resultInitExpression = ((MemberInitExpression) lambaExpression.Body);
      // BaseValue = new DerivedValue { ... }
      var derivedValueMemberAssignment = ((MemberAssignment) resultInitExpression.Bindings[0]);
      // new DerivedValue { ... }
      var derivedValueInitExpression = (MemberInitExpression) (derivedValueMemberAssignment.Expression);
      // StringValue = c.FirstName
      var derivedValueStringValueMemberAssignment = ((MemberAssignment) derivedValueInitExpression.Bindings[0]);
      // c.FirstName
      var derivedValueStringValuePropertyExpression = (MemberExpression) derivedValueStringValueMemberAssignment.Expression;

      var inputParameter = Expression.Parameter (typeof (Result<BaseValue>), "input");

      var result = AccessorFindingExpressionVisitor.FindAccessorLambda (
          searchedExpression: derivedValueStringValuePropertyExpression,
          fullExpression: resultInitExpression,
          inputParameter: inputParameter);

      Expression<Func<Result<BaseValue>, string>> expectedResult = input => ((DerivedValue) input.Value).StringValue;
      ExpressionTreeComparer.CheckAreEqualTrees (expectedResult, result);
    }
  }
}