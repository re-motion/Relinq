using System.Linq.Expressions;
using Remotion.Data.Linq.DataObjectModel;
using Remotion.Data.Linq.Expressions;
using Remotion.Utilities;

namespace Remotion.Data.Linq.Parsing.Details.WhereConditionParsing
{
  public class SubQueryExpressionParser : IWhereConditionParser
  {
    public ICriterion Parse (SubQueryExpression subQueryExpression, ParseContext parseContext)
    {
      ArgumentUtility.CheckNotNull ("subQueryExpression", subQueryExpression);
      return new SubQuery (subQueryExpression.QueryModel, ParseMode.SubQueryInWhere, null);
    }

    public bool CanParse (Expression expression)
    {
      return expression is SubQueryExpression;
    }

    ICriterion IWhereConditionParser.Parse (Expression expression, ParseContext parseContext)
    {
      return Parse ((SubQueryExpression) expression, parseContext);
    }

    
  }
}