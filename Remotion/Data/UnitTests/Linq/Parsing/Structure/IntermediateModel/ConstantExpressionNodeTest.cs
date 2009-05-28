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
using System.Linq.Expressions;
using NUnit.Framework;
using Remotion.Data.Linq.Parsing.Structure.IntermediateModel;
using Rhino.Mocks;

namespace Remotion.Data.UnitTests.Linq.Parsing.Structure.IntermediateModel
{
  [TestFixture]
  public class ConstantExpressionNodeTest
  {
    [Test]
    public void Resolve_ReplacesParameter_WithIdentifierReference ()
    {
      var node = new ConstantExpressionNode (typeof (int), new [] { 1, 2, 3, 4, 5 });
      var expression = ExpressionHelper.CreateLambdaExpression<int, bool> (i => i > 5);

      var result = node.Resolve (expression.Parameters[0], expression.Body);

      var expectedResult = Expression.MakeBinary (ExpressionType.GreaterThan, new IdentifierReferenceExpression (node), Expression.Constant (5));
      ExpressionTreeComparer.CheckAreEqualTrees (expectedResult, result);
    }

    [Test]
    [ExpectedException (typeof (InvalidOperationException), ExpectedMessage = "ConstantExpressionNode does not support to get a resolved expression.")]
    public void GetResolvedExpression_ThrowsInvalidOperationException ()
    {
      var node = new ConstantExpressionNode (typeof (int), new[] { 1, 2, 3, 4, 5 });
      node.GetResolvedExpression ();
    }
  }
}