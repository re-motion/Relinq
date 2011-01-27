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
using System.Linq.Expressions;
using NUnit.Framework;
using NUnit.Framework.SyntaxHelpers;
using Remotion.Data.Linq.Parsing.ExpressionTreeVisitors.Transformation.PredefinedTransformations;

namespace Remotion.Data.Linq.UnitTests.Linq.Core.Parsing.ExpressionTreeVisitors.Transformation.PredefinedTransformations
{
  [TestFixture]
  public class InvocationOfLambdaExpressionTransformerTest
  {
    private InvocationOfLambdaExpressionTransformer _transformer;

    [SetUp]
    public void SetUp ()
    {
      _transformer = new InvocationOfLambdaExpressionTransformer();
    }

    [Test]
    public void SupportedExpressionTypes ()
    {
      Assert.That (_transformer.SupportedExpressionTypes, Is.EqualTo (new[] { ExpressionType.Invoke }));
    }

    [Test]
    public void Transform_InnerLambdaExpression ()
    {
      Expression<Func<int, double, string>> innerExpression = (p1, p2) => (p1 + p2).ToString();
      var invokeExpression = Expression.Invoke (innerExpression, Expression.Constant (1), Expression.Constant (2.0));

      var result = _transformer.Transform (invokeExpression);

      var expectedExpression = Expression.Call (
          Expression.Add (
              Expression.Convert (Expression.Constant (1), typeof (double)), 
              Expression.Constant (2.0)),
          typeof (double).GetMethod ("ToString", Type.EmptyTypes));
      ExpressionTreeComparer.CheckAreEqualTrees (expectedExpression, result);
    }

    [Test]
    public void Transform_OtherInnerExpression ()
    {
      Func<int, double, string> innerDelegate = (p1, p2) => (p1 + p2).ToString ();
      var invokeExpression = Expression.Invoke (Expression.Constant(innerDelegate), Expression.Constant (1), Expression.Constant (2.0));

      var result = _transformer.Transform (invokeExpression);

      Assert.That (result, Is.SameAs (invokeExpression));
    }
  }
}