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
using NUnit.Framework.SyntaxHelpers;
using Remotion.Data.Linq.Clauses.ResultOperators;
using Remotion.Data.Linq.Parsing.Structure.IntermediateModel;
using System.Linq;
using Rhino.Mocks;

namespace Remotion.Data.UnitTests.Linq.Parsing.Structure.IntermediateModel
{
  [TestFixture]
  public class MaxExpressionNodeTest : ExpressionNodeTestBase
  {
    private MaxExpressionNode _node;

    public override void SetUp ()
    {
      base.SetUp ();
      _node = new MaxExpressionNode (CreateParseInfo (), null);
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
    public void ApplySelector ()
    {
      TestApply (_node, typeof (MaxResultOperator));
    }
  }
}