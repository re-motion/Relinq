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
using System.Linq;
using System.Linq.Expressions;
using NUnit.Framework;
using NUnit.Framework.SyntaxHelpers;
using Remotion.Data.Linq.Parsing.ExpressionTreeVisitors.Transformation.PredefinedTransformations;

namespace Remotion.Data.Linq.UnitTests.Linq.Core.Parsing.ExpressionTreeVisitors.Transformation.PredefinedTransformations
{
  [TestFixture]
  public class QueryableConstantExpressionTransformerTest
  {
    private QueryableConstantExpressionTransformer _transformer;

    [SetUp]
    public void SetUp ()
    {
      _transformer = new QueryableConstantExpressionTransformer ();
    }

    [Test]
    public void SupportedExpressionTypes ()
    {
      Assert.That (_transformer.SupportedExpressionTypes, Is.EqualTo (new[] { ExpressionType.Constant }));
    }

    [Test]
    public void Transform_OrdinaryConstant ()
    {
      var expression = Expression.Constant (0);

      var result = _transformer.Transform (expression);

      Assert.That (result, Is.SameAs (expression));
    }

    [Test]
    public void Transform_QueryableConstant_IsInlined ()
    {
      var query = ExpressionHelper.CreateCookQueryable ();
      var expression = Expression.Constant (query);

      var result = _transformer.Transform (expression);

      Assert.That (result, Is.SameAs (query.Expression));
    }

    [Test]
    public void Transform_QueryableConstant_IsInlined_AndPartiallyEvaluated ()
    {
      var querySource = ExpressionHelper.CreateCookQueryable ();
      var query = querySource.Where (c => "1" == 1.ToString());
      var expression = Expression.Constant (query);

      var result = _transformer.Transform (expression);

      var expectedExpression = querySource.Where (c => true).Expression;
      ExpressionTreeComparer.CheckAreEqualTrees (expectedExpression, result);
    }
  }
}