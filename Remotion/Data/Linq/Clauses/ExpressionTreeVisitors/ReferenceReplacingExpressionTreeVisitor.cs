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
using System.Linq.Expressions;
using Remotion.Data.Linq.Clauses.Expressions;
using Remotion.Data.Linq.Parsing;
using Remotion.Utilities;

namespace Remotion.Data.Linq.Clauses.ExpressionTreeVisitors
{
  /// <summary>
  /// Takes an expression and replaces all <see cref="QuerySourceReferenceExpression"/>s to clauses with references to their cloned counterparts.
  /// Also replaces <see cref="SubQueryExpression"/>s to hold a cloned version of the <see cref="SubQueryExpression.QueryModel"/>.
  /// This is used when a <see cref="QueryModel"/> is cloned in order to ensure that all expressions its clauses hold correctly refer to the other
  /// cloned clauses afterwards.
  /// </summary>
  public class ReferenceReplacingExpressionTreeVisitor : ExpressionTreeVisitor
  {
    public static Expression ReplaceClauseReferences (Expression expression, ClauseMapping clauseMapping)
    {
      ArgumentUtility.CheckNotNull ("expression", expression);
      ArgumentUtility.CheckNotNull ("clauseMapping", clauseMapping);

      return ReplaceClauseReferences (expression, clauseMapping, false);
    }

    public static Expression ReplaceClauseReferences (Expression expression, ClauseMapping clauseMapping, bool ignoreUnmappedReferences)
    {
      ArgumentUtility.CheckNotNull ("expression", expression);
      ArgumentUtility.CheckNotNull ("clauseMapping", clauseMapping);

      return new ReferenceReplacingExpressionTreeVisitor (clauseMapping, ignoreUnmappedReferences).VisitExpression (expression);
    }

    private readonly ClauseMapping _clauseMapping;
    private readonly bool _ignoreUnmappedReferences;

    private ReferenceReplacingExpressionTreeVisitor (ClauseMapping clauseMapping, bool ignoreUnmappedReferences)
    {
      ArgumentUtility.CheckNotNull ("clauseMapping", clauseMapping);
      _clauseMapping = clauseMapping;
      _ignoreUnmappedReferences = ignoreUnmappedReferences;
    }

    protected override Expression VisitQuerySourceReferenceExpression (QuerySourceReferenceExpression expression)
    {
      ArgumentUtility.CheckNotNull ("expression", expression);

      if (!_ignoreUnmappedReferences || _clauseMapping.ContainsMapping (expression.ReferencedClause))
      {
        try
        {
          return _clauseMapping.GetExpression (expression.ReferencedClause);
        }
        catch (KeyNotFoundException)
        {
          var message = "Cannot replace reference to clause '" + expression.ReferencedClause.ItemName + "', there is no mapped expression.";
          throw new InvalidOperationException (message);
        }
      }
      else
        return expression;
    }

    protected override Expression VisitSubQueryExpression (SubQueryExpression expression)
    {
      ArgumentUtility.CheckNotNull ("expression", expression);
      
      var clonedQueryModel = expression.QueryModel.Clone (_clauseMapping);
      return new SubQueryExpression (clonedQueryModel);
    }

    protected override Expression VisitUnknownExpression (Expression expression)
    {
      //ignore
      return expression;
    }
  }
}