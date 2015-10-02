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
#if !NET_3_5
using System.Collections.ObjectModel;
#endif
using System.Linq;
using Remotion.Linq.Clauses;
using Remotion.Linq.Clauses.Expressions;
using Remotion.Linq.Clauses.ExpressionVisitors;
using Remotion.Linq.Clauses.ResultOperators;
#if NET_3_5
using Remotion.Linq.Collections;
#endif
using Remotion.Linq.Parsing.Structure;
using Remotion.Utilities;

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

    protected virtual void FlattenSubQuery (SubQueryExpression subQueryExpression, IFromClause fromClause, QueryModel queryModel, int destinationIndex)
    {
      ArgumentUtility.CheckNotNull ("subQueryExpression", subQueryExpression);
      ArgumentUtility.CheckNotNull ("fromClause", fromClause);
      ArgumentUtility.CheckNotNull ("queryModel", queryModel);

      CheckFlattenable (subQueryExpression.QueryModel);

      var innerMainFromClause = subQueryExpression.QueryModel.MainFromClause;
      fromClause.CopyFromSource (innerMainFromClause);

      var innerSelectorMapping = new QuerySourceMapping();
      innerSelectorMapping.AddMapping (fromClause, subQueryExpression.QueryModel.SelectClause.Selector);
      queryModel.TransformExpressions (ex => ReferenceReplacingExpressionVisitor.ReplaceClauseReferences (ex, innerSelectorMapping, false));

      InsertBodyClauses (subQueryExpression.QueryModel.BodyClauses, queryModel, destinationIndex);

      var innerBodyClauseMapping = new QuerySourceMapping();
      innerBodyClauseMapping.AddMapping (innerMainFromClause, new QuerySourceReferenceExpression (fromClause));
      queryModel.TransformExpressions (ex => ReferenceReplacingExpressionVisitor.ReplaceClauseReferences (ex, innerBodyClauseMapping, false));
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
