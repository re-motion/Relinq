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
using System.Collections;
using System.Linq;
using NUnit.Framework;
using NUnit.Framework.SyntaxHelpers;
using Remotion.Data.Linq.Clauses.ResultOperators;
using Remotion.Data.Linq.Parsing.Structure.IntermediateModel;
using System.Collections.Generic;

namespace Remotion.Data.UnitTests.Linq.Parsing.Structure.IntermediateModel
{
  [TestFixture]
  public class CountExpressionNodeTest : ExpressionNodeTestBase
  {
    private CountExpressionNode _node;

    public override void SetUp ()
    {
      base.SetUp ();
      _node = new CountExpressionNode (CreateParseInfo (), null);
    }

    [Test]
    public void SupportedMethods_WithoutPredicate ()
    {
      AssertSupportedMethod_Generic (CountExpressionNode.SupportedMethods, q => q.Count (), e => e.Count ());
    }

    [Test]
    public void SupportedMethods_WithoutPredicate_FromCollections ()
    {
      Assert.That (CountExpressionNode.SupportedMethods, List.Contains (typeof (List<>).GetProperty ("Count").GetGetMethod()));
      Assert.That (CountExpressionNode.SupportedMethods, List.Contains (typeof (ArrayList).GetProperty ("Count").GetGetMethod ()));
      Assert.That (CountExpressionNode.SupportedMethods, List.Contains (typeof (ICollection<>).GetProperty ("Count").GetGetMethod ()));
      Assert.That (CountExpressionNode.SupportedMethods, List.Contains (typeof (ICollection).GetProperty ("Count").GetGetMethod ()));
      Assert.That (CountExpressionNode.SupportedMethods, List.Contains (typeof (Array).GetProperty ("Length").GetGetMethod ()));
    }

    [Test]
    public void SupportedMethod_WithPredicate ()
    {
      AssertSupportedMethod_Generic (CountExpressionNode.SupportedMethods, q => q.Count (o => o == null), e => e.Count (o => o == null));
    }

    [Test]
    [ExpectedException (typeof (InvalidOperationException))]
    public void Resolve_ThrowsInvalidOperationException ()
    {
      _node.Resolve (ExpressionHelper.CreateParameterExpression (), ExpressionHelper.CreateExpression (), ClauseGenerationContext);
    }

    [Test]
    public void Apply ()
    {
      TestApply (_node, typeof (CountResultOperator));
    }
  }
}
