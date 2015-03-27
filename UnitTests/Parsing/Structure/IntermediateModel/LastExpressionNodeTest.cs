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
using System.Linq;
using NUnit.Framework;
using Remotion.Linq.Clauses.ResultOperators;
using Remotion.Linq.Development.UnitTesting;
using Remotion.Linq.Parsing.Structure.IntermediateModel;
using Remotion.Linq.UnitTests.TestDomain;
using Remotion.Linq.Utilities;

namespace Remotion.Linq.UnitTests.Parsing.Structure.IntermediateModel
{
  [TestFixture]
  public class LastExpressionNodeTest : ExpressionNodeTestBase
  {
    private LastExpressionNode _node;

    [SetUp]
    public override void SetUp ()
    {
      base.SetUp ();

      _node = new LastExpressionNode (CreateParseInfo (), null);
    }

    [Test]
    public void GetSupportedMethods ()
    {
      Assert.That (
          LastExpressionNode.GetSupportedMethods(),
          Is.EquivalentTo (
              new[]
              {
                  GetGenericMethodDefinition (() => Queryable.Last<object> (null)),
                  GetGenericMethodDefinition (() => Queryable.Last<object> (null, null)),
                  GetGenericMethodDefinition (() => Queryable.LastOrDefault<object> (null)),
                  GetGenericMethodDefinition (() => Queryable.LastOrDefault<object> (null, null)),
                  GetGenericMethodDefinition (() => Enumerable.Last<object> (null)),
                  GetGenericMethodDefinition (() => Enumerable.Last<object> (null, null)),
                  GetGenericMethodDefinition (() => Enumerable.LastOrDefault<object> (null)),
                  GetGenericMethodDefinition (() => Enumerable.LastOrDefault<object> (null, null)),
              }));
    }

    [Test]
    [ExpectedException (typeof (NotSupportedException), ExpectedMessage = 
        "LastExpressionNode does not support resolving of expressions, because it does not stream any data to the following node.")]
    public void Resolve_ThrowsInvalidOperationException ()
    {
      _node.Resolve (ExpressionHelper.CreateParameterExpression (), ExpressionHelper.CreateExpression (), ClauseGenerationContext);
    }

    [Test]
    public void Apply()
    {
      TestApply (_node, typeof (LastResultOperator));
    }

    [Test]
    public void Apply_NoDefaultAllowed ()
    {
      var node = new LastExpressionNode (CreateParseInfo (ReflectionUtility.GetMethod (() => Queryable.Last<Cook> (null))), null);
      
      node.Apply (QueryModel, ClauseGenerationContext);
      
      Assert.That (((LastResultOperator) QueryModel.ResultOperators[0]).ReturnDefaultWhenEmpty, Is.False);
    }

    [Test]
    public void Apply_DefaultAllowed ()
    {
      var node = new LastExpressionNode (CreateParseInfo (ReflectionUtility.GetMethod (() => Queryable.LastOrDefault<Cook> (null, null))), null);
      
      node.Apply (QueryModel, ClauseGenerationContext);

      Assert.That (((LastResultOperator) QueryModel.ResultOperators[0]).ReturnDefaultWhenEmpty, Is.True);
    }
  }
}
