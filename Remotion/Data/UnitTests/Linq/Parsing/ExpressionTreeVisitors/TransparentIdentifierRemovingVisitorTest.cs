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
using System.Linq;
using NUnit.Framework;
using NUnit.Framework.SyntaxHelpers;
using Remotion.Data.Linq.Parsing.ExpressionTreeVisitors;
using Remotion.Data.Linq.Parsing.Structure;
using Remotion.Data.Linq.Parsing.Structure.IntermediateModel;

namespace Remotion.Data.UnitTests.Linq.Parsing.ExpressionTreeVisitors
{
  [TestFixture]
  [Ignore ("TODO 1172: Implement")]
  public class TransparentIdentifierRemovingVisitorTest
  {
    [Test]
    public void IntegrationTest_WithSimpleExpressions ()
    {
      var expression1 = ExpressionHelper.MakeExpression<int, int> (i => new AnonymousType { a = i, b = 1 }.a);
      var expression2 = ExpressionHelper.MakeExpression<int, int> (i => new AnonymousType { a = i, b = 1 }.b);
      var expectedResult1 = ExpressionHelper.MakeExpression<int, int> (i => i);
      var expectedResult2 = ExpressionHelper.MakeExpression<int, int> (i => 1);

      var result1 = TransparentIdentifierRemovingVisitor.ReplaceTransparentIdentifiers (expression1);
      var result2 = TransparentIdentifierRemovingVisitor.ReplaceTransparentIdentifiers (expression1);

      ExpressionTreeComparer.CheckAreEqualTrees (expectedResult1, result1);
      ExpressionTreeComparer.CheckAreEqualTrees (expectedResult2, result2);
    }

    [Test]
    public void IntegrationTest_WithExpressionNodes ()
    {
      var query = from a in ExpressionHelper.CreateQuerySource ()
                  from b in ExpressionHelper.CreateQuerySource()
                  select a;

      var nodeTypeRegistry = new ExpressionNodeTypeRegistry ();
      nodeTypeRegistry.Register (SelectExpressionNode.SupportedMethods, typeof (SelectExpressionNode));
      nodeTypeRegistry.Register (SelectManyExpressionNode.SupportedMethods, typeof (SelectManyExpressionNode));
      
      var selectNode = (SelectExpressionNode) new ExpressionTreeParser (nodeTypeRegistry).Parse (query.Expression);

      var selectProjection = selectNode.GetResolvedSelector (); // new { a = IR (a), b = IR (b) }.a
      Console.WriteLine (selectProjection);

      var result = TransparentIdentifierRemovingVisitor.ReplaceTransparentIdentifiers (selectProjection);
      Console.WriteLine (result);

      // IR(a)
      Assert.That (result, Is.InstanceOfType (typeof (IdentifierReferenceExpression)));
      Assert.That (((IdentifierReferenceExpression) result).ReferencedQuerySource, Is.InstanceOfType (typeof (ConstantExpressionNode)));
    }
  }
}