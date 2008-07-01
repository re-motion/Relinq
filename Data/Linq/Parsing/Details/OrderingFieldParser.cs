using System.Linq.Expressions;
using Remotion.Data.Linq.Clauses;
using Remotion.Data.Linq.DataObjectModel;
using Remotion.Data.Linq.Parsing.FieldResolving;
using Remotion.Utilities;

namespace Remotion.Data.Linq.Parsing.Details
{
  public class OrderingFieldParser
  {
    private readonly ClauseFieldResolver _resolver;

    public OrderingFieldParser (IDatabaseInfo databaseInfo)
    {
      ArgumentUtility.CheckNotNull ("databaseInfo", databaseInfo);

      _resolver = new ClauseFieldResolver (databaseInfo, new OrderingFieldAccessPolicy());
    }

    public OrderingField Parse (Expression expression, ParseContext parseContext, OrderDirection orderDirection)
    {
      FieldDescriptor fieldDescriptor = parseContext.QueryModel.ResolveField (_resolver, expression, parseContext.JoinedTableContext);
      OrderingField orderingField = new OrderingField (fieldDescriptor, orderDirection);
      return orderingField;
    }
  }
}