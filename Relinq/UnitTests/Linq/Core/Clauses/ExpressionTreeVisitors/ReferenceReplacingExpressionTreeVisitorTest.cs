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
using System.Linq.Expressions;
using NUnit.Framework;
using Remotion.Linq.Clauses;
using Remotion.Linq.Clauses.Expressions;
using Remotion.Linq.Clauses.ExpressionTreeVisitors;
using Remotion.Linq.Development.UnitTesting;
using Remotion.Linq.UnitTests.Linq.Core.Parsing;
using Remotion.Linq.UnitTests.Linq.Core.Parsing.ExpressionTreeVisitors;
using Remotion.Linq.UnitTests.Linq.Core.TestDomain;

namespace Remotion.Linq.UnitTests.Linq.Core.Clauses.ExpressionTreeVisitors
{
  [TestFixture]
  public class ReferenceReplacingExpressionTreeVisitorTest
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
    public void Replaces_QuerySourceReferenceExpressions ()
    {
      var expression = new QuerySourceReferenceExpression (_oldFromClause);
      var result = ReferenceReplacingExpressionTreeVisitor.ReplaceClauseReferences (expression, _querySourceMapping, true);

      Assert.That (((QuerySourceReferenceExpression) result).ReferencedQuerySource, Is.SameAs (_newFromClause));
    }

    [Test]
    public void Replaces_NestedExpressions ()
    {
      var expression = Expression.Negate (new QuerySourceReferenceExpression (_oldFromClause));
      var result = (UnaryExpression) ReferenceReplacingExpressionTreeVisitor.ReplaceClauseReferences (expression, _querySourceMapping, true);

      Assert.That (((QuerySourceReferenceExpression) result.Operand).ReferencedQuerySource, Is.SameAs (_newFromClause));
    }

    [Test]
    public void VisitSubQuery_ExpressionUnchanged ()
    {
      var expression = new SubQueryExpression (ExpressionHelper.CreateQueryModel<Cook> ());
      var result = ReferenceReplacingExpressionTreeVisitor.ReplaceClauseReferences (expression, _querySourceMapping, false);

      Assert.That (((SubQueryExpression) result).QueryModel, Is.SameAs (expression.QueryModel));
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

      var result = ReferenceReplacingExpressionTreeVisitor.ReplaceClauseReferences (expression, _querySourceMapping, false);
      var newSubQuerySelectClause = ((SubQueryExpression) result).QueryModel.SelectClause;
      Assert.That (newSubQuerySelectClause.Selector, Is.SameAs (newReferenceExpression));
    }
    
    [Test]
    [ExpectedException (typeof (InvalidOperationException))]
    public void VisitSubQuery_PassesFlagToInner_Throw ()
    {
      var expression = new SubQueryExpression (ExpressionHelper.CreateQueryModel<Cook> ());
      expression.QueryModel.SelectClause.Selector = new QuerySourceReferenceExpression (expression.QueryModel.MainFromClause);
      ReferenceReplacingExpressionTreeVisitor.ReplaceClauseReferences (expression, _querySourceMapping, true);
    }
    

    [Test]
    public void VisitSubQuery_PassesFlagToInner_Ignore ()
    {
      var expression = new SubQueryExpression (ExpressionHelper.CreateQueryModel<Cook> ());
      expression.QueryModel.SelectClause.Selector = new QuerySourceReferenceExpression (expression.QueryModel.MainFromClause);
      ReferenceReplacingExpressionTreeVisitor.ReplaceClauseReferences (expression, _querySourceMapping, false);
    }

    [Test]
    public void VisitUnknownNonExtensionExpression_Ignored ()
    {
      var expression = new UnknownExpression (typeof (object));
      var result = ReferenceReplacingExpressionTreeVisitor.ReplaceClauseReferences (expression, _querySourceMapping, true);

      Assert.That (result, Is.SameAs (expression));
    }

    [Test]
    public void VisitExtensionExpression_ChildrenAreProcessed ()
    {
      var extensionExpression = new TestExtensionExpression (new QuerySourceReferenceExpression (_oldFromClause));

      var result = (TestExtensionExpression) ReferenceReplacingExpressionTreeVisitor.ReplaceClauseReferences (extensionExpression, _querySourceMapping, true);

      var expectedExpression = new TestExtensionExpression (new QuerySourceReferenceExpression (_newFromClause));
      ExpressionTreeComparer.CheckAreEqualTrees (expectedExpression, result);
    }

    [Test]
    [ExpectedException (typeof (InvalidOperationException))]
    public void VisitUnmappedReference_Throws ()
    {
      var expression = new QuerySourceReferenceExpression (ExpressionHelper.CreateMainFromClause_Int ());
      ReferenceReplacingExpressionTreeVisitor.ReplaceClauseReferences (expression, _querySourceMapping, true);
    }

    [Test]
    [ExpectedException (typeof (InvalidOperationException))]
    public void VisitUnmappedReference_IgnoreFalse_Throws ()
    {
      var expression = new QuerySourceReferenceExpression (ExpressionHelper.CreateMainFromClause_Int ());
      ReferenceReplacingExpressionTreeVisitor.ReplaceClauseReferences (expression, _querySourceMapping, true);
    }

    [Test]
    public void VisitUnmappedReference_IgnoreTrue_Ignored ()
    {
      var expression = new QuerySourceReferenceExpression (ExpressionHelper.CreateMainFromClause_Int ());
      var result = ReferenceReplacingExpressionTreeVisitor.ReplaceClauseReferences (expression, _querySourceMapping, false);

      Assert.That (result, Is.SameAs (expression));
    }

  }
}
