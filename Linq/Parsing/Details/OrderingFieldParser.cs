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
    private readonly QueryModel _queryModel;
    private readonly OrderingClause _orderingClause;
    private readonly FromClauseFieldResolver _resolver;

    public OrderingFieldParser (QueryModel queryModel, OrderingClause orderingClause, IDatabaseInfo databaseInfo, JoinedTableContext context)
    {
      ArgumentUtility.CheckNotNull ("queryExpression", queryModel);
      ArgumentUtility.CheckNotNull ("orderingClause", orderingClause);
      ArgumentUtility.CheckNotNull ("databaseInfo", databaseInfo);
      ArgumentUtility.CheckNotNull ("context", context);

      _queryModel = queryModel;
      _orderingClause = orderingClause;

      _resolver = new FromClauseFieldResolver (databaseInfo, context, new OrderingFieldAccessPolicy());
    }

    public OrderingField GetField ()
    {
      return ParseExpression (_orderingClause.Expression.Body);
    }

    private OrderingField ParseExpression (Expression expression)
    {
      FieldDescriptor fieldDescriptor = _queryModel.ResolveField (_resolver, expression);
      OrderingField orderingField = new OrderingField (fieldDescriptor, _orderingClause.OrderDirection);
      return orderingField;
    }
  }
}