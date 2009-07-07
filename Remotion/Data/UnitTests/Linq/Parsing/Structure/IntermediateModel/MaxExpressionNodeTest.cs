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
using NUnit.Framework;
using NUnit.Framework.SyntaxHelpers;
using Remotion.Data.Linq.Clauses.ResultModifications;
using Remotion.Data.Linq.Parsing.Structure.IntermediateModel;
using System.Linq;
using Rhino.Mocks;

namespace Remotion.Data.UnitTests.Linq.Parsing.Structure.IntermediateModel
{
  [TestFixture]
  public class MaxExpressionNodeTest : ExpressionNodeTestBase
  {
    private MaxExpressionNode _node;
    private MaxExpressionNode _nodeWithSelector;

    public override void SetUp ()
    {
      base.SetUp ();
      _node = new MaxExpressionNode (CreateParseInfo (), null);
      _nodeWithSelector = new MaxExpressionNode (CreateParseInfo (), OptionalSelector);
    }

    [Test]
    public void SupportedMethod_WithoutSelector ()
    {
      AssertSupportedMethod_Generic (MaxExpressionNode.SupportedMethods, q => q.Max(), e => e.Max());
    }

    [Test]
    public void SupportedMethod_WithSelector ()
    {
      AssertSupportedMethod_Generic (MaxExpressionNode.SupportedMethods, q => q.Max (i => i.ToString()), e => e.Max (i => i.ToString()));
    }

    [Test]
    [ExpectedException (typeof (InvalidOperationException))]
    public void Resolve_ThrowsInvalidOperationException ()
    {
      _node.Resolve (ExpressionHelper.CreateParameterExpression (), ExpressionHelper.CreateExpression (), ClauseGenerationContext);
    }

    [Test]
    public void GetResolvedSelector ()
    {
      var expectedResult = ExpressionHelper.Resolve<int, string> (SourceClause, i => i.ToString());

      var result = _nodeWithSelector.GetResolvedOptionalSelector (ClauseGenerationContext);

      ExpressionTreeComparer.CheckAreEqualTrees (expectedResult, result);
    }

    [Test]
    public void GetResolvedSelector_Null ()
    {
      var sourceMock = MockRepository.GenerateMock<IExpressionNode> ();
      var node = new MaxExpressionNode (CreateParseInfo (sourceMock), null);
      var result = node.GetResolvedOptionalSelector (ClauseGenerationContext);
      Assert.That (result, Is.Null);
    }

    [Test]
    public void Apply_WithoutSelector ()
    {
      TestApply (_node, typeof (MaxResultModification));
    }

    [Test]
    public void Apply_WithSelector_AdjustsSelectClause ()
    {
      TestApply_WithOptionalSelector (_nodeWithSelector);
    }
  }
}