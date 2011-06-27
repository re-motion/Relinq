// This file is part of the re-linq project (relinq.codeplex.com)
// Copyright (C) 2005-2009 rubicon informationstechnologie gmbh, www.rubicon.eu
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
using System.Linq;
using Remotion.Linq.Clauses;
using Remotion.Linq.Clauses.Expressions;
using Remotion.Linq.Clauses.ExpressionTreeVisitors;
using Remotion.Linq.Clauses.ResultOperators;
using Remotion.Linq.Collections;
using Remotion.Linq.Parsing.Structure;
using Remotion.Linq.Utilities;

namespace Remotion.Linq.Transformations
{
  /// <summary>
  /// Takes a <see cref="QueryModel"/> and transforms it by replacing its <see cref="FromClauseBase"/> instances (<see cref="MainFromClause"/> and
  /// <see cref="AdditionalFromClause"/>) that contain subqueries with equivalent flattened clauses. Subqueries that contain a 
  /// <see cref="ResultOperatorBase"/> (such as <see cref="DistinctResultOperator"/> or <see cref="TakeResultOperator"/>) cannot be
  /// flattened.
  /// </summary>
  /// <example>
  /// As an example, take the following query:
  /// <code>
  /// from c in Customers
  /// from o in (from oi in OrderInfos where oi.Customer == c orderby oi.OrderDate select oi.Order)
  /// orderby o.Product.Name
  /// select new { c, o }
  /// </code>
  /// This will be transformed into:
  /// <code>
  /// from c in Customers
  /// from oi in OrderInfos
  /// where oi.Customer == c
  /// orderby oi.OrderDate
  /// orderby oi.Order.Product.Name
  /// select new { c, oi.Order }
  /// </code>
  /// As another example, take the following query:
  /// <code>
  /// from c in (from o in Orders select o.Customer)
  /// where c.Name.StartsWith ("Miller")
  /// select c
  /// </code>
  /// (This query is never produced by the <see cref="QueryParser"/>, the only way to construct it is via manually building a 
  /// <see cref="MainFromClause"/>.)
  /// This will be transforemd into:
  /// <code>
  /// from o in Orders
  /// where o.Customer.Name.StartsWith ("Miller")
  /// select o
  /// </code>
  /// </example>
  public class SubQueryFromClauseFlattener : QueryModelVisitorBase
  {
    public override void VisitMainFromClause (MainFromClause fromClause, QueryModel queryModel)
    {
      ArgumentUtility.CheckNotNull ("fromClause", fromClause);
      ArgumentUtility.CheckNotNull ("queryModel", queryModel);

      var subQueryExpression = fromClause.FromExpression as SubQueryExpression;
      if (subQueryExpression != null)
        FlattenSubQuery (subQueryExpression, fromClause, queryModel, 0);

      base.VisitMainFromClause (fromClause, queryModel);
    }

    public override void VisitAdditionalFromClause (AdditionalFromClause fromClause, QueryModel queryModel, int index)
    {
      ArgumentUtility.CheckNotNull ("fromClause", fromClause);
      ArgumentUtility.CheckNotNull ("queryModel", queryModel);

      var subQueryExpression = fromClause.FromExpression as SubQueryExpression;
      if (subQueryExpression != null)
        FlattenSubQuery (subQueryExpression, fromClause, queryModel, index + 1);

      base.VisitAdditionalFromClause (fromClause, queryModel, index);
    }

    protected virtual void FlattenSubQuery (
        SubQueryExpression subQueryExpression, FromClauseBase fromClause, QueryModel queryModel, int destinationIndex)
    {
      ArgumentUtility.CheckNotNull ("subQueryExpression", subQueryExpression);
      ArgumentUtility.CheckNotNull ("fromClause", fromClause);
      ArgumentUtility.CheckNotNull ("queryModel", queryModel);

      CheckFlattenable (subQueryExpression.QueryModel);

      var innerMainFromClause = subQueryExpression.QueryModel.MainFromClause;
      CopyFromClauseData (innerMainFromClause, fromClause);

      var innerSelectorMapping = new QuerySourceMapping();
      innerSelectorMapping.AddMapping (fromClause, subQueryExpression.QueryModel.SelectClause.Selector);
      queryModel.TransformExpressions (ex => ReferenceReplacingExpressionTreeVisitor.ReplaceClauseReferences (ex, innerSelectorMapping, false));

      InsertBodyClauses (subQueryExpression.QueryModel.BodyClauses, queryModel, destinationIndex);

      var innerBodyClauseMapping = new QuerySourceMapping();
      innerBodyClauseMapping.AddMapping (innerMainFromClause, new QuerySourceReferenceExpression (fromClause));
      queryModel.TransformExpressions (ex => ReferenceReplacingExpressionTreeVisitor.ReplaceClauseReferences (ex, innerBodyClauseMapping, false));
    }

    protected virtual void CheckFlattenable (QueryModel subQueryModel)
    {
      ArgumentUtility.CheckNotNull ("subQueryModel", subQueryModel);

      if (subQueryModel.ResultOperators.Count > 0)
      {
        var message = string.Format (
            "The subquery '{0}' cannot be flattened and pulled out of the from clause because it contains result operators.",
            subQueryModel);
        throw new NotSupportedException (message);
      }

      if (subQueryModel.BodyClauses.Any (bc => bc is OrderByClause))
      {
        var message = string.Format (
            "The subquery '{0}' cannot be flattened and pulled out of the from clause because it contains an OrderByClause.",
            subQueryModel);
        throw new NotSupportedException (message);
      }
    }

    protected void CopyFromClauseData (FromClauseBase source, FromClauseBase destination)
    {
      ArgumentUtility.CheckNotNull ("source", source);
      ArgumentUtility.CheckNotNull ("destination", destination);

      destination.FromExpression = source.FromExpression;
      destination.ItemName = source.ItemName;
      destination.ItemType = source.ItemType;
    }

    protected void InsertBodyClauses (ObservableCollection<IBodyClause> bodyClauses, QueryModel destinationQueryModel, int destinationIndex)
    {
      ArgumentUtility.CheckNotNull ("bodyClauses", bodyClauses);
      ArgumentUtility.CheckNotNull ("destinationQueryModel", destinationQueryModel);

      foreach (var bodyClause in bodyClauses)
      {
        destinationQueryModel.BodyClauses.Insert (destinationIndex, bodyClause);
        ++destinationIndex;
      }
    }
  }
}
