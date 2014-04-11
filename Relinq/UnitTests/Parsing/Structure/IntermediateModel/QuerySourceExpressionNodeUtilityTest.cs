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
using System.Linq.Expressions;
using NUnit.Framework;
using Remotion.Linq.Clauses;
using Remotion.Linq.Development.UnitTesting;
using Remotion.Linq.Parsing.Structure.IntermediateModel;
using Remotion.Linq.Parsing.Structure.NodeTypeProviders;

namespace Remotion.Linq.UnitTests.Parsing.Structure.IntermediateModel
{
  [TestFixture]
  public class QuerySourceExpressionNodeUtilityTest
  {
    private IQuerySourceExpressionNode _node;
    private IQuerySource _querySource;
    private ClauseGenerationContext _context;

    [SetUp]
    public void SetUp ()
    {
      _context = new ClauseGenerationContext (new MethodInfoBasedNodeTypeRegistry());
      _node = new MainSourceExpressionNode ("x", Expression.Constant (new int[0]));
      _querySource = new MainFromClause ("x", typeof (int), Expression.Constant (new int[0]));
    }

    [Test]
    public void GetQuerySourceForNode ()
    {
      _context.AddContextInfo (_node, _querySource);

      Assert.That (QuerySourceExpressionNodeUtility.GetQuerySourceForNode (_node, _context), Is.SameAs (_querySource));
    }

    [Test]
    [ExpectedException (typeof (InvalidOperationException), ExpectedMessage = "Cannot retrieve an IQuerySource for the given MainSourceExpressionNode. " 
        + "Be sure to call Apply before calling methods that require IQuerySources, and pass in the same QuerySourceClauseMapping to both.")]
    public void GetQuerySourceForNode_NoClauseRegistered ()
    {
      QuerySourceExpressionNodeUtility.GetQuerySourceForNode (_node, _context);
    }

    [Test]
    public void ReplaceParameterWithReference ()
    {
      _context.AddContextInfo (_node, _querySource);

      var parameter = Expression.Parameter (typeof (int), "x");
      var expression = Expression.MakeBinary (ExpressionType.Add, Expression.Constant (1), parameter);
      var result = QuerySourceExpressionNodeUtility.ReplaceParameterWithReference (_node, parameter, expression, _context);

      var expected = ExpressionHelper.Resolve<int, int> (_querySource, (i => 1 + i));

      ExpressionTreeComparer.CheckAreEqualTrees (expected, result);
    }

    [Test]
    [ExpectedException (typeof (InvalidOperationException), ExpectedMessage = "Cannot retrieve an IQuerySource for the given MainSourceExpressionNode. "
        + "Be sure to call Apply before calling methods that require IQuerySources, and pass in the same QuerySourceClauseMapping to both.")]
    public void ReplaceParameterWithReference_NoClauseRegistered ()
    {
      var parameter = Expression.Parameter (typeof (int), "x");
      var expression = Expression.MakeBinary (ExpressionType.Add, Expression.Constant (1), parameter);
      QuerySourceExpressionNodeUtility.ReplaceParameterWithReference (_node, parameter, expression, _context);
    }
  }
}
