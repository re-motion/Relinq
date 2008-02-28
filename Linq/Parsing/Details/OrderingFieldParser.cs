using System;
using System.Linq.Expressions;
using Rubicon.Data.Linq.Clauses;
using Rubicon.Data.Linq.DataObjectModel;
using Rubicon.Data.Linq.Parsing.FieldResolving;
using Rubicon.Utilities;

namespace Rubicon.Data.Linq.Parsing.Details
{
  public class OrderingFieldParser
  {
    private readonly QueryExpression _queryExpression;
    private readonly OrderingClause _orderingClause;
    private readonly FromClauseFieldResolver _resolver;

    public OrderingFieldParser (QueryExpression queryExpression, OrderingClause orderingClause, IDatabaseInfo databaseInfo, JoinedTableContext context)
    {
      ArgumentUtility.CheckNotNull ("queryExpression", queryExpression);
      ArgumentUtility.CheckNotNull ("orderingClause", orderingClause);
      ArgumentUtility.CheckNotNull ("databaseInfo", databaseInfo);
      ArgumentUtility.CheckNotNull ("context", context);

      _queryExpression = queryExpression;
      _orderingClause = orderingClause;

      _resolver = new FromClauseFieldResolver (databaseInfo, context, new OrderingFieldAccessPolicy());
    }

    public OrderingField GetField ()
    {
      return ParseExpression (_orderingClause.Expression.Body);
    }

    private OrderingField ParseExpression (Expression expression)
    {
      FieldDescriptor fieldDescriptor = _queryExpression.ResolveField (_resolver, expression);
      OrderingField orderingField = new OrderingField (fieldDescriptor, _orderingClause.OrderDirection);
      return orderingField;
    }
  }
}