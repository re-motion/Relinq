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
    private readonly IDatabaseInfo _databaseInfo;

    public OrderingFieldParser (QueryExpression queryExpression, OrderingClause orderingClause, IDatabaseInfo databaseInfo)
    {
      ArgumentUtility.CheckNotNull ("queryExpression", queryExpression);
      ArgumentUtility.CheckNotNull ("orderingClause", orderingClause);
      ArgumentUtility.CheckNotNull ("databaseInfo", databaseInfo);

      _queryExpression = queryExpression;
      _orderingClause = orderingClause;
      _databaseInfo = databaseInfo;

    }

    public OrderingField GetField ()
    {
      return ParseExpression (_orderingClause.Expression.Body);
    }

    private OrderingField ParseExpression (Expression expression)
    {
      JoinedTableContext context = new JoinedTableContext ();
      FieldDescriptor fieldDescriptor = _queryExpression.ResolveField (_databaseInfo, context, expression);
      OrderingField orderingField = new OrderingField (fieldDescriptor, _orderingClause.OrderDirection);
      return orderingField;
    }
  }
}