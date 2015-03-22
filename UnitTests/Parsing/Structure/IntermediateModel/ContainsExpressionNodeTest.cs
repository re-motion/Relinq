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
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using NUnit.Framework;
using Remotion.Linq.Clauses.ResultOperators;
using Remotion.Linq.Development.UnitTesting;
using Remotion.Linq.Parsing.Structure.IntermediateModel;

namespace Remotion.Linq.UnitTests.Parsing.Structure.IntermediateModel
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
    public void GetSupportedMethods ()
    {
      Assert.That (
          ContainsExpressionNode.GetSupportedMethods(),
          Is.EquivalentTo (
              new[]
              {
                  GetGenericMethodDefinition (() => Queryable.Contains<object> (null, null)),
                  GetGenericMethodDefinition (() => Enumerable.Contains<object> (null, null))
              }));
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
    [ExpectedException (typeof (NotSupportedException), ExpectedMessage = 
        "ContainsExpressionNode does not support resolving of expressions, because it does not stream any data to the following node.")]
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
