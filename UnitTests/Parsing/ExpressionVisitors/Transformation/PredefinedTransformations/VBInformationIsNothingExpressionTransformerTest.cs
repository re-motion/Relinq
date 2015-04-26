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
using Microsoft.VisualBasic;
using NUnit.Framework;
using Remotion.Linq.Development.UnitTesting;
using Remotion.Linq.Parsing.ExpressionVisitors.Transformation.PredefinedTransformations;

namespace Remotion.Linq.UnitTests.Parsing.ExpressionVisitors.Transformation.PredefinedTransformations
{
  [TestFixture]
  public class VBInformationIsNothingExpressionTransformerTest
  {
    private VBInformationIsNothingExpressionTransformer _transformer;

    [SetUp]
    public void SetUp ()
    {
      _transformer = new VBInformationIsNothingExpressionTransformer();
    }

    [Test]
    public void SupportedExpressionTypes ()
    {
      Assert.That (_transformer.SupportedExpressionTypes, Is.EqualTo (new[] { ExpressionType.Call }));
    }

    [Test]
    public void Transform_OtherMethod ()
    {
      var expression = Expression.Call (typeof (DateTime), "get_Now", Type.EmptyTypes);

      var result = _transformer.Transform (expression);

      Assert.That (result, Is.SameAs (expression));
    }

    [Test]
    public void Transform_IsNothingMethod ()
    {
      var argument = Expression.Convert (Expression.Constant (0), typeof (object));
      var expression = Expression.Call (typeof (Information).GetMethod ("IsNothing"), argument);

      var result = _transformer.Transform (expression);

      var expectedExpression = Expression.Equal (argument, Expression.Constant (null));
      ExpressionTreeComparer.CheckAreEqualTrees (expectedExpression, result);
    }
  }
}