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
using Remotion.Linq.Clauses.Expressions;
using Remotion.Linq.Clauses.ExpressionVisitors;
using Remotion.Linq.Development.UnitTesting;
using Remotion.Linq.Development.UnitTesting.Clauses.Expressions;
using Remotion.Linq.UnitTests.Parsing.ExpressionVisitors;
using Remotion.Linq.UnitTests.TestDomain;

namespace Remotion.Linq.UnitTests.Clauses.ExpressionVisitors
{
  [TestFixture]
  public class CloningExpressionVisitorTest
  {
    private QuerySourceMapping _querySourceMapping;
    private MainFromClause _oldFromClause;
    private MainFromClause _newFromClause;

    [SetUp]
    public void SetUp ()
    {
      _oldFromClause = ExpressionHelper.CreateMainFromClause_Int ();
      _newFromClause = ExpressionHelper.CreateMainFromClause_Int ();

      _querySourceMapping = new QuerySourceMapping ();
      _querySourceMapping.AddMapping (_oldFromClause, new QuerySourceReferenceExpression (_newFromClause));
    }
    
    [Test]
    public void Replaces_SubQueryExpressions ()
    {
      var expression = new SubQueryExpression (ExpressionHelper.CreateQueryModel<Cook> ());
      var result = CloningExpressionVisitor.AdjustExpressionAfterCloning (expression, _querySourceMapping);

      Assert.That (((SubQueryExpression) result).QueryModel, Is.Not.SameAs (expression.QueryModel));
    }

    [Test]
    public void Replaces_SubQueryExpressions_WithCorrectCloneContext ()
    {
      var subQueryModel = ExpressionHelper.CreateQueryModel<Cook> ();
      var referencedClause = ExpressionHelper.CreateMainFromClause_Int ();
      subQueryModel.SelectClause.Selector = new QuerySourceReferenceExpression (referencedClause);
      var expression = new SubQueryExpression (subQueryModel);

      var newReferenceExpression = new QuerySourceReferenceExpression (ExpressionHelper.CreateMainFromClause_Int ());
      _querySourceMapping.AddMapping (referencedClause, newReferenceExpression);

      var result = CloningExpressionVisitor.AdjustExpressionAfterCloning (expression, _querySourceMapping);
      var newSubQuerySelectClause = ((SubQueryExpression) result).QueryModel.SelectClause;
      Assert.That (newSubQuerySelectClause.Selector, Is.SameAs (newReferenceExpression));
    }

    [Test]
    public void Replaces_QuerySourceReferenceExpressions ()
    {
      var expression = new QuerySourceReferenceExpression (_oldFromClause);
      var result = CloningExpressionVisitor.AdjustExpressionAfterCloning (expression, _querySourceMapping);

      Assert.That (((QuerySourceReferenceExpression) result).ReferencedQuerySource, Is.SameAs (_newFromClause));
    }

    [Test]
    public void Replaces_NestedQuerySourceReferenceExpressions ()
    {
      var expression = Expression.Negate (new QuerySourceReferenceExpression (_oldFromClause));
      var result = (UnaryExpression) CloningExpressionVisitor.AdjustExpressionAfterCloning (expression, _querySourceMapping);

      Assert.That (((QuerySourceReferenceExpression) result.Operand).ReferencedQuerySource, Is.SameAs (_newFromClause));
    }

    [Test]
    public void Replaces_UnmappedQuerySourceReferenceExpressions_Ignored ()
    {
      var expression = new QuerySourceReferenceExpression (ExpressionHelper.CreateMainFromClause_Int ());
      var result = CloningExpressionVisitor.AdjustExpressionAfterCloning (expression, _querySourceMapping);

      Assert.That (result, Is.SameAs (expression));
    }

    [Test]
    public void VisitExtensionExpression_ChildrenAreProcessed ()
    {
      var extensionExpression = new ReducibleExtensionExpression (new QuerySourceReferenceExpression (_oldFromClause));

      var result = (ReducibleExtensionExpression) CloningExpressionVisitor.AdjustExpressionAfterCloning (extensionExpression, _querySourceMapping);

      var expectedExpression = new ReducibleExtensionExpression (new QuerySourceReferenceExpression (_newFromClause));
      ExpressionTreeComparer.CheckAreEqualTrees (expectedExpression, result);
    }

    [Test]
    public void VisitUnknownNonExtensionExpression_Ignored ()
    {
      var expression = new UnknownExpression (typeof (object));
      var result = CloningExpressionVisitor.AdjustExpressionAfterCloning (expression, _querySourceMapping);

      Assert.That (result, Is.SameAs (expression));
    }
  }
}
