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
using Remotion.Linq.Clauses.Expressions;
using Remotion.Linq.Development.UnitTesting;
using Remotion.Linq.Parsing.Structure.IntermediateModel;

namespace Remotion.Linq.UnitTests.Parsing.Structure.IntermediateModel
{
  [TestFixture]
  public class MainSourceExpressionNodeTest : ExpressionNodeTestBase
  {
    private MainSourceExpressionNode _node;

    public override void SetUp ()
    {
      base.SetUp ();
      _node = ExpressionNodeObjectMother.CreateMainSource ();
    }

    [Test]
    public void Initialization_QuerySourceElementType ()
    {
      Assert.That (_node.QuerySourceElementType, Is.EqualTo (typeof (int)));
    }

    [Test]
    public void Initialization_TypeNotEnumerable ()
    { 
      Assert.That (
          () => new MainSourceExpressionNode ("x", Expression.Constant(5)),
          Throws.ArgumentException
              .With.Message.EqualTo (
                  "Parameter 'expression.Type' is a 'System.Int32', which cannot be assigned to type 'System.Collections.IEnumerable'."
                  + "\r\nParameter name: expression.Type"));
    }
    
    [Test]
    public void Resolve_ReplacesParameter_WithQuerySourceReference ()
    {
      var expression = ExpressionHelper.CreateLambdaExpression<int, bool> (i => i > 5);
      var clause = ExpressionHelper.CreateMainFromClause_Int ();
      ClauseGenerationContext.AddContextInfo (_node, clause);

      var result = _node.Resolve (expression.Parameters[0], expression.Body, ClauseGenerationContext);

      var expectedResult = Expression.MakeBinary (ExpressionType.GreaterThan, new QuerySourceReferenceExpression (clause), Expression.Constant (5));
      ExpressionTreeComparer.CheckAreEqualTrees (expectedResult, result);
    }

    [Test]
    public void Resolve_WithoutClause ()
    {
      var expression = ExpressionHelper.CreateLambdaExpression<int, bool> (i => i > 5);
      Assert.That (
          () => _node.Resolve (expression.Parameters[0], expression.Body, ClauseGenerationContext),
          Throws.InvalidOperationException
              .With.Message.EqualTo (
                  "Cannot retrieve an IQuerySource for the given MainSourceExpressionNode. "
                  + "Be sure to call Apply before calling methods that require IQuerySources, and pass in the same QuerySourceClauseMapping to both."));
    }

    [Test]
    public void Apply ()
    {
      var queryModel = _node.Apply (null, ClauseGenerationContext);
      
      Assert.That (queryModel.MainFromClause, Is.Not.Null);
      Assert.That (queryModel.MainFromClause.ItemType, Is.EqualTo (typeof (int)));
      Assert.That (queryModel.MainFromClause.ItemName, Is.EqualTo ("x"));
      Assert.That (queryModel.MainFromClause.FromExpression, Is.SameAs (_node.ParsedExpression));
      Assert.That (queryModel.SelectClause, Is.Not.Null);
      Assert.That (((QuerySourceReferenceExpression) queryModel.SelectClause.Selector).ReferencedQuerySource, Is.SameAs (queryModel.MainFromClause));
      Assert.That (queryModel.ResultTypeOverride, Is.SameAs (typeof (int[])));
    }

    [Test]
    public void Apply_Throws ()
    {
      Assert.That (
          () => _node.Apply (QueryModel, ClauseGenerationContext),
          Throws.ArgumentException);
    }
  }
}
