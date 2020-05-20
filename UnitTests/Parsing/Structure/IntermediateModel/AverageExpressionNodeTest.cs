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
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Remotion.Linq.Clauses.ResultOperators;
using Remotion.Linq.Development.UnitTesting;
using Remotion.Linq.Parsing.Structure.IntermediateModel;

namespace Remotion.Linq.UnitTests.Parsing.Structure.IntermediateModel
{
  [TestFixture]
  public class AverageExpressionNodeTest : ExpressionNodeTestBase
  {
    private AverageExpressionNode _node;

    [SetUp]
    public override void SetUp ()
    {
      base.SetUp();
      _node = new AverageExpressionNode (CreateParseInfo(), null);
    }

    [Test]
    public void GetSupportedMethods ()
    {
      Assert.That (
          AverageExpressionNode.GetSupportedMethods(),
          Is.EquivalentTo (
              new[]
              {
                  GetGenericMethodDefinition (() => Queryable.Average ((IQueryable<decimal>) null)),
                  GetGenericMethodDefinition (() => Queryable.Average ((IQueryable<decimal?>) null)),
                  GetGenericMethodDefinition (() => Queryable.Average ((IQueryable<double>) null)),
                  GetGenericMethodDefinition (() => Queryable.Average ((IQueryable<double?>) null)),
                  GetGenericMethodDefinition (() => Queryable.Average ((IQueryable<int>) null)),
                  GetGenericMethodDefinition (() => Queryable.Average ((IQueryable<int?>) null)),
                  GetGenericMethodDefinition (() => Queryable.Average ((IQueryable<long>) null)),
                  GetGenericMethodDefinition (() => Queryable.Average ((IQueryable<long?>) null)),
                  GetGenericMethodDefinition (() => Queryable.Average ((IQueryable<float>) null)),
                  GetGenericMethodDefinition (() => Queryable.Average ((IQueryable<float?>) null)),
                  GetGenericMethodDefinition (() => Queryable.Average<object> (null, o => (decimal) 0)),
                  GetGenericMethodDefinition (() => Queryable.Average<object> (null, o => (decimal?) 0)),
                  GetGenericMethodDefinition (() => Queryable.Average<object> (null, o => (double) 0)),
                  GetGenericMethodDefinition (() => Queryable.Average<object> (null, o => (double?) 0)),
                  GetGenericMethodDefinition (() => Queryable.Average<object> (null, o => (int) 0)),
                  GetGenericMethodDefinition (() => Queryable.Average<object> (null, o => (int?) 0)),
                  GetGenericMethodDefinition (() => Queryable.Average<object> (null, o => (long) 0)),
                  GetGenericMethodDefinition (() => Queryable.Average<object> (null, o => (long?) 0)),
                  GetGenericMethodDefinition (() => Queryable.Average<object> (null, o => (float) 0)),
                  GetGenericMethodDefinition (() => Queryable.Average<object> (null, o => (float?) 0)),
                  GetGenericMethodDefinition (() => Enumerable.Average ((IEnumerable<decimal>) null)),
                  GetGenericMethodDefinition (() => Enumerable.Average ((IEnumerable<decimal?>) null)),
                  GetGenericMethodDefinition (() => Enumerable.Average ((IEnumerable<double>) null)),
                  GetGenericMethodDefinition (() => Enumerable.Average ((IEnumerable<double?>) null)),
                  GetGenericMethodDefinition (() => Enumerable.Average ((IEnumerable<int>) null)),
                  GetGenericMethodDefinition (() => Enumerable.Average ((IEnumerable<int?>) null)),
                  GetGenericMethodDefinition (() => Enumerable.Average ((IEnumerable<long>) null)),
                  GetGenericMethodDefinition (() => Enumerable.Average ((IEnumerable<long?>) null)),
                  GetGenericMethodDefinition (() => Enumerable.Average ((IEnumerable<float>) null)),
                  GetGenericMethodDefinition (() => Enumerable.Average ((IEnumerable<float?>) null)),
                  GetGenericMethodDefinition (() => Enumerable.Average<object> (null, o => (decimal) 0)),
                  GetGenericMethodDefinition (() => Enumerable.Average<object> (null, o => (decimal?) 0)),
                  GetGenericMethodDefinition (() => Enumerable.Average<object> (null, o => (double) 0)),
                  GetGenericMethodDefinition (() => Enumerable.Average<object> (null, o => (double?) 0)),
                  GetGenericMethodDefinition (() => Enumerable.Average<object> (null, o => (int) 0)),
                  GetGenericMethodDefinition (() => Enumerable.Average<object> (null, o => (int?) 0)),
                  GetGenericMethodDefinition (() => Enumerable.Average<object> (null, o => (long) 0)),
                  GetGenericMethodDefinition (() => Enumerable.Average<object> (null, o => (long?) 0)),
                  GetGenericMethodDefinition (() => Enumerable.Average<object> (null, o => (float) 0)),
                  GetGenericMethodDefinition (() => Enumerable.Average<object> (null, o => (float?) 0))
              }));
    }


    [Test]
    public void Resolve_ThrowsInvalidOperationException ()
    {
      Assert.That (
          () => _node.Resolve (ExpressionHelper.CreateParameterExpression (), ExpressionHelper.CreateExpression (), ClauseGenerationContext),
          Throws.InstanceOf<NotSupportedException>()
              .With.Message.EqualTo (
                  "AverageExpressionNode does not support resolving of expressions, because it does not stream any data to the following node."));
    }

    [Test]
    public void Apply ()
    {
      TestApply (_node, typeof (AverageResultOperator));
    }
  }
}
