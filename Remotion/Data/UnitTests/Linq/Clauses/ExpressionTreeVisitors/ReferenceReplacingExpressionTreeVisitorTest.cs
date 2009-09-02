// This file is part of the re-motion Core Framework (www.re-motion.org)
// Copyright (C) 2005-2009 rubicon informationstechnologie gmbh, www.rubicon.eu
// 
// The re-motion Core Framework is free software; you can redistribute it 
// and/or modify it under the terms of the GNU Lesser General Public License 
// version 3.0 as published by the Free Software Foundation.
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
using System.Linq.Expressions;
using NUnit.Framework;
using NUnit.Framework.SyntaxHelpers;
using Remotion.Data.Linq.Clauses;
using Remotion.Data.Linq.Clauses.Expressions;
using Remotion.Data.Linq.Clauses.ExpressionTreeVisitors;
using Remotion.Data.UnitTests.Linq.Parsing.ExpressionTreeVisitors;

namespace Remotion.Data.UnitTests.Linq.Clauses.ExpressionTreeVisitors
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
      _oldFromClause = ExpressionHelper.CreateMainFromClause ();
      _newFromClause = ExpressionHelper.CreateMainFromClause ();

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
      var expression = new SubQueryExpression (ExpressionHelper.CreateQueryModel_Student ());
      var result = ReferenceReplacingExpressionTreeVisitor.ReplaceClauseReferences (expression, _querySourceMapping, false);

      Assert.That (((SubQueryExpression) result).QueryModel, Is.SameAs (expression.QueryModel));
    }

    [Test]
    public void Replaces_SubQueryExpressions_WithCorrectCloneContext ()
    {
      var subQueryModel = ExpressionHelper.CreateQueryModel_Student ();
      var referencedClause = ExpressionHelper.CreateMainFromClause ();
      subQueryModel.SelectClause.Selector = new QuerySourceReferenceExpression (referencedClause);
      var expression = new SubQueryExpression (subQueryModel);

      var newReferenceExpression = new QuerySourceReferenceExpression (ExpressionHelper.CreateMainFromClause ());
      _querySourceMapping.AddMapping (referencedClause, newReferenceExpression);

      var result = ReferenceReplacingExpressionTreeVisitor.ReplaceClauseReferences (expression, _querySourceMapping, false);
      var newSubQuerySelectClause = ((SubQueryExpression) result).QueryModel.SelectClause;
      Assert.That (newSubQuerySelectClause.Selector, Is.SameAs (newReferenceExpression));
    }
    
    [Test]
    [ExpectedException (typeof (InvalidOperationException))]
    public void VisitSubQuery_PassesFlagToInner_Throw ()
    {
      var expression = new SubQueryExpression (ExpressionHelper.CreateQueryModel_Student ());
      expression.QueryModel.SelectClause.Selector = new QuerySourceReferenceExpression (expression.QueryModel.MainFromClause);
      ReferenceReplacingExpressionTreeVisitor.ReplaceClauseReferences (expression, _querySourceMapping, true);
    }
    

    [Test]
    public void VisitSubQuery_PassesFlagToInner_Ignore ()
    {
      var expression = new SubQueryExpression (ExpressionHelper.CreateQueryModel_Student ());
      expression.QueryModel.SelectClause.Selector = new QuerySourceReferenceExpression (expression.QueryModel.MainFromClause);
      ReferenceReplacingExpressionTreeVisitor.ReplaceClauseReferences (expression, _querySourceMapping, false);
    }

    [Test]
    public void VisitUnknownExpression_Ignored ()
    {
      var expression = new UnknownExpression (typeof (object));
      var result = ReferenceReplacingExpressionTreeVisitor.ReplaceClauseReferences (expression, _querySourceMapping, true);

      Assert.That (result, Is.SameAs (expression));
    }

    [Test]
    [ExpectedException (typeof (InvalidOperationException))]
    public void VisitUnmappedReference_Throws ()
    {
      var expression = new QuerySourceReferenceExpression (ExpressionHelper.CreateMainFromClause ());
      ReferenceReplacingExpressionTreeVisitor.ReplaceClauseReferences (expression, _querySourceMapping, true);
    }

    [Test]
    [ExpectedException (typeof (InvalidOperationException))]
    public void VisitUnmappedReference_IgnoreFalse_Throws ()
    {
      var expression = new QuerySourceReferenceExpression (ExpressionHelper.CreateMainFromClause ());
      ReferenceReplacingExpressionTreeVisitor.ReplaceClauseReferences (expression, _querySourceMapping, true);
    }

    [Test]
    public void VisitUnmappedReference_IgnoreTrue_Ignored ()
    {
      var expression = new QuerySourceReferenceExpression (ExpressionHelper.CreateMainFromClause ());
      var result = ReferenceReplacingExpressionTreeVisitor.ReplaceClauseReferences (expression, _querySourceMapping, false);

      Assert.That (result, Is.SameAs (expression));
    }

  }
}