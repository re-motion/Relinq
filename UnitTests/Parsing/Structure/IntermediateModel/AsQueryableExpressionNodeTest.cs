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
using Remotion.Linq.Development.UnitTesting;
using Remotion.Linq.Parsing.Structure.IntermediateModel;
using Remotion.Linq.Utilities;
using Rhino.Mocks;

namespace Remotion.Linq.UnitTests.Parsing.Structure.IntermediateModel
{
  [TestFixture]
  public class AsQueryableExpressionNodeTest : ExpressionNodeTestBase
  {
    private AsQueryableExpressionNode _node;

    public override void SetUp ()
    {
      base.SetUp ();
      _node = new AsQueryableExpressionNode (
          CreateParseInfo (SourceNode, "u", ReflectionUtility.GetMethod (() => Queryable.AsQueryable<int> (null))));
    }

    [Test]
    public void GetSupportedMethods ()
    {
      Assert.That (
          AsQueryableExpressionNode.GetSupportedMethods(),
          Is.EquivalentTo (
              new[]
              {
                GetGenericMethodDefinition (() => Queryable.AsQueryable<object> (null)),
                GetGenericMethodDefinition (() => Queryable.AsQueryable (null))
              }));
    }

    [Test]
    public void Initialization ()
    {
      Assert.That (_node.NodeResultType, Is.SameAs (typeof (IQueryable<int>)));
      Assert.That (_node.Source, Is.SameAs (SourceNode));
      Assert.That (_node.AssociatedIdentifier, Is.EqualTo ("u"));
    }

    [Test]
    public void Resolve_PassesExpressionToSource ()
    {
      var sourceMock = MockRepository.GenerateMock<IExpressionNode>();
      var node = new AsQueryableExpressionNode (CreateParseInfo (sourceMock));
      var expression = ExpressionHelper.CreateLambdaExpression();
      var parameter = ExpressionHelper.CreateParameterExpression();
      var expectedResult = ExpressionHelper.CreateExpression();
      sourceMock.Expect (mock => mock.Resolve (parameter, expression, ClauseGenerationContext)).Return (expectedResult);

      var result = node.Resolve (parameter, expression, ClauseGenerationContext);

      sourceMock.VerifyAllExpectations();
      Assert.That (result, Is.SameAs (expectedResult));
    }

    [Test]
    public void Apply ()
    {
      var result = _node.Apply (QueryModel, ClauseGenerationContext);
      Assert.That (result, Is.SameAs (QueryModel));

      Assert.That (QueryModel.ResultOperators, Is.Empty);
      Assert.That (QueryModel.BodyClauses, Is.Empty);
    }
  }
}
