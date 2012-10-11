// This file is part of the re-linq project (relinq.codeplex.com)
// Copyright (c) rubicon IT GmbH, www.rubicon.eu
// 
// re-linq is free software; you can redistribute it and/or modify it under 
// the terms of the GNU Lesser General Public License as published by the 
// Free Software Foundation; either version 2.1 of the License, 
// or (at your option) any later version.
// 
// re-linq is distributed in the hope that it will be useful, 
// but WITHOUT ANY WARRANTY; without even the implied warranty of 
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the 
// GNU Lesser General Public License for more details.
// 
// You should have received a copy of the GNU Lesser General Public License
// along with re-linq; if not, see http://www.gnu.org/licenses.
// 
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using NUnit.Framework;
using Remotion.Linq.Clauses.ResultOperators;
using Remotion.Linq.Parsing.Structure.IntermediateModel;

namespace Remotion.Linq.UnitTests.Linq.Core.Parsing.Structure.IntermediateModel
{
  [TestFixture]
  public class ContainsExpressionNodeTest : ExpressionNodeTestBase
  {
    private ContainsExpressionNode _node;

    public override void SetUp ()
    {
      base.SetUp ();
      _node = new ContainsExpressionNode (CreateParseInfo (), Expression.Constant("test"));
    }

    [Test]
    public void SupportedMethods ()
    {
      AssertSupportedMethod_Generic (ContainsExpressionNode.SupportedMethods, q => q.Contains (null), e => e.Contains (null));
    }

    [Test]
    public void SupportedMethodNames ()
    {
      AssertSupportedMethods_ByName (
          ContainsExpressionNode.SupportedMethodNames,
          () => ((IList<int>) null).Contains (0),
          () => ((ICollection<int>) null).Contains (1),
          () => ((IList) null).Contains (2),
          () => ((List<int>) null).Contains (3));

      AssertNotSupportedMethods_ByName (ContainsExpressionNode.SupportedMethodNames, () => ((string) null).Contains ("x"));
      AssertNotSupportedMethods_ByName (ContainsExpressionNode.SupportedMethodNames, () => ((IDictionary) null).Contains ("x"));
      AssertNotSupportedMethods_ByName (ContainsExpressionNode.SupportedMethodNames, () => ((Hashtable) null).Contains ("x"));
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
      TestApply (_node, typeof (ContainsResultOperator));
    }
  }
}
