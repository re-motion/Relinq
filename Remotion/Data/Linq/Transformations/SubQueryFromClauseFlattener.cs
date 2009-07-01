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
using Remotion.Collections;
using Remotion.Data.Linq.Clauses;
using Remotion.Data.Linq.Clauses.Expressions;
using Remotion.Data.Linq.Clauses.ExpressionTreeVisitors;
using Remotion.Data.Linq.Clauses.ResultModifications;
using Remotion.Data.Linq.Parsing.Structure;

namespace Remotion.Data.Linq.Transformations
{
  /// <summary>
  /// Takes a <see cref="QueryModel"/> and transforms it by replacing its <see cref="FromClauseBase"/> instances (<see cref="MainFromClause"/> and
  /// <see cref="AdditionalFromClause"/>) that contain subqueries with equivalent flattened clauses. Subqueries that contain a 
  /// <see cref="ResultModificationBase"/> (such as <see cref="DistinctResultModification"/> or <see cref="TakeResultModification"/>) cannot be
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
    public override void VisitAdditionalFromClause (AdditionalFromClause fromClause, QueryModel queryModel, int index)
    {
      var subQueryExpression = fromClause.FromExpression as SubQueryExpression;
      if (subQueryExpression != null)
      {
        var innerMainFromClause = subQueryExpression.QueryModel.MainFromClause;
        CopyFromClauseData (innerMainFromClause, fromClause);
        var clauseMapping = new ClauseMapping ();
        clauseMapping.AddMapping (innerMainFromClause, new QuerySourceReferenceExpression (fromClause));
        InsertBodyClauses(subQueryExpression.QueryModel.BodyClauses, queryModel, index + 1, clauseMapping);
      }

      base.VisitAdditionalFromClause (fromClause, queryModel, index);
    }

    private void CopyFromClauseData (FromClauseBase source, FromClauseBase destination)
    {
      destination.FromExpression = source.FromExpression;
      destination.ItemName = source.ItemName;
      destination.ItemType = source.ItemType;
    }

    private void InsertBodyClauses (ObservableCollection<IBodyClause> bodyClauses, QueryModel destinationQueryModel, int destinationIndex, ClauseMapping clauseMapping)
    {
      foreach (var bodyClause in bodyClauses)
      {
        destinationQueryModel.BodyClauses.Insert (destinationIndex, bodyClause);
        bodyClause.TransformExpressions(ex => ReferenceReplacingExpressionTreeVisitor.ReplaceClauseReferences(ex,clauseMapping));
        ++destinationIndex;
      }
    }
  }
}