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
using Microsoft.VisualBasic;
using NUnit.Framework;
using Remotion.Linq.Parsing.ExpressionTreeVisitors.Transformation.PredefinedTransformations;

namespace Remotion.Linq.UnitTests.Linq.Core.Parsing.ExpressionTreeVisitors.Transformation.PredefinedTransformations
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