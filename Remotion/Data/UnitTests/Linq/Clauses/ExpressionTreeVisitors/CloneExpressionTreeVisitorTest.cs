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
using NUnit.Framework;
using NUnit.Framework.SyntaxHelpers;
using Remotion.Data.Linq.Clauses;
using Remotion.Data.Linq.Clauses.Expressions;
using Remotion.Data.Linq.Clauses.ExpressionTreeVisitors;
using System.Linq.Expressions;
using Remotion.Data.UnitTests.Linq.Parsing.ExpressionTreeVisitors;

namespace Remotion.Data.UnitTests.Linq.Clauses.ExpressionTreeVisitors
{
  [TestFixture]
  public class CloneExpressionTreeVisitorTest
  {
    private ClauseMapping _clauseMapping;
    private MainFromClause _oldFromClause;
    private MainFromClause _newFromClause;
    private CloneContext _cloneContext;

    [SetUp]
    public void SetUp ()
    {
      _oldFromClause = ExpressionHelper.CreateMainFromClause ();
      _newFromClause = ExpressionHelper.CreateMainFromClause ();

      _clauseMapping = new ClauseMapping ();
      _clauseMapping.AddMapping (_oldFromClause, new QuerySourceReferenceExpression(_newFromClause));

      _cloneContext = new CloneContext (_clauseMapping);
    }

    [Test]
    public void Replaces_QuerySourceReferenceExpressions ()
    {
      var expression = new QuerySourceReferenceExpression (_oldFromClause);
      var result = CloneExpressionTreeVisitor.ReplaceClauseReferences (expression, _cloneContext);

      Assert.That (((QuerySourceReferenceExpression) result).ReferencedClause, Is.SameAs (_newFromClause));
    }

    [Test]
    public void Replaces_NestedExpressions ()
    {
      var expression = Expression.Negate (new QuerySourceReferenceExpression (_oldFromClause));
      var result = (UnaryExpression) CloneExpressionTreeVisitor.ReplaceClauseReferences (expression, _cloneContext);

      Assert.That (((QuerySourceReferenceExpression) result.Operand).ReferencedClause, Is.SameAs (_newFromClause));
    }

    [Test]
    public void Replaces_SubQueryExpressions ()
    {
      var expression = new SubQueryExpression (ExpressionHelper.CreateQueryModel());
      var result = CloneExpressionTreeVisitor.ReplaceClauseReferences (expression, _cloneContext);

      Assert.That (((SubQueryExpression) result).QueryModel, Is.Not.SameAs (expression.QueryModel));
    }

    [Test]
    public void Replaces_SubQueryExpressions_WithCorrectCloneContext ()
    {
      var subQueryModel = ExpressionHelper.CreateQueryModel ();
      var referencedClause = ExpressionHelper.CreateMainFromClause();
      ((SelectClause)subQueryModel.SelectOrGroupClause).Selector = new QuerySourceReferenceExpression (referencedClause);
      var expression = new SubQueryExpression (subQueryModel);

      var newReferenceExpression = new QuerySourceReferenceExpression (ExpressionHelper.CreateMainFromClause ());
      _clauseMapping.AddMapping (referencedClause, newReferenceExpression);
      
      var result = CloneExpressionTreeVisitor.ReplaceClauseReferences (expression, _cloneContext);
      var newSubQuerySelectClause = (SelectClause) ((SubQueryExpression) result).QueryModel.SelectOrGroupClause;
      Assert.That (newSubQuerySelectClause.Selector, Is.SameAs (newReferenceExpression));
    }

    [Test]
    public void VisitUnknownExpression_Ignored ()
    {
      var expression = new UnknownExpression (typeof (object));
      var result = CloneExpressionTreeVisitor.ReplaceClauseReferences (expression, _cloneContext);

      Assert.That (result, Is.SameAs (expression));
    }
  }
}