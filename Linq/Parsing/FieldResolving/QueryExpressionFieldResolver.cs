using System.Linq.Expressions;
using Rubicon.Data.Linq.DataObjectModel;
using Rubicon.Utilities;

namespace Rubicon.Data.Linq.Parsing.FieldResolving
{
  public class QueryExpressionFieldResolver
  {
    private readonly QueryExpression _queryExpression;

    public QueryExpressionFieldResolver (QueryExpression queryExpression)
    {
      ArgumentUtility.CheckNotNull ("queryExpression", queryExpression);
      _queryExpression = queryExpression;
    }

    public FieldDescriptor ResolveField (FromClauseFieldResolver resolver, Expression fieldAccessExpression)
    {
      ArgumentUtility.CheckNotNull ("resolver", resolver);
      ArgumentUtility.CheckNotNull ("fieldAccessExpression", fieldAccessExpression);

      QueryExpressionFieldResolverVisitor visitor = new QueryExpressionFieldResolverVisitor (_queryExpression);
      QueryExpressionFieldResolverVisitor.Result visitorResult = visitor.ParseAndReduce (fieldAccessExpression);

      return visitorResult.FromClause.ResolveField (resolver, visitorResult.ReducedExpression, fieldAccessExpression);
    }
  }
}