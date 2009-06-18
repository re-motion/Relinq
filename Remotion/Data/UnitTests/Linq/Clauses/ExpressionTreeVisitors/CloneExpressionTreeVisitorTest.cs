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
using System.Collections.Generic;
using NUnit.Framework;
using NUnit.Framework.SyntaxHelpers;
using Remotion.Data.Linq;
using Remotion.Data.Linq.Clauses;
using Remotion.Data.Linq.Clauses.Expressions;
using Remotion.Data.Linq.Clauses.ExpressionTreeVisitors;
using System.Linq.Expressions;

namespace Remotion.Data.UnitTests.Linq.Clauses.ExpressionTreeVisitors
{
  [TestFixture]
  public class CloneExpressionTreeVisitorTest
  {
    private ClonedClauseMapping _clonedClauseMapping;
    private MainFromClause _oldFromClause;
    private MainFromClause _newFromClause;
    private List<QueryModel> _subQueryRegistry;
    private CloneContext _cloneContext;

    [SetUp]
    public void SetUp ()
    {
      _oldFromClause = ExpressionHelper.CreateMainFromClause ();
      _newFromClause = ExpressionHelper.CreateMainFromClause ();

      _clonedClauseMapping = new ClonedClauseMapping ();
      _clonedClauseMapping.AddMapping (_oldFromClause, _newFromClause);

      _subQueryRegistry = new List<QueryModel> ();

      _cloneContext = new CloneContext (_clonedClauseMapping, _subQueryRegistry);
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
    public void Replaces_SubQueryExpressions_RegistersSubQueries ()
    {
      var expression = new SubQueryExpression (ExpressionHelper.CreateQueryModel ());
      var result = CloneExpressionTreeVisitor.ReplaceClauseReferences (expression, _cloneContext);

      Assert.That (_subQueryRegistry, List.Contains(((SubQueryExpression) result).QueryModel));
    }
  }
}