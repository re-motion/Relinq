using System.Collections.Generic;
using System.Linq.Expressions;
using Remotion.Data.Linq.DataObjectModel;
using Remotion.Utilities;

namespace Remotion.Data.Linq.Parsing.Details.SelectProjectionParsing
{
  public class ConstantExpressionParser : ISelectProjectionParser
  {
    private readonly WhereConditionParsing.ConstantExpressionParser _innerParser;

    public ConstantExpressionParser (IDatabaseInfo databaseInfo)
    {
      _innerParser = new WhereConditionParsing.ConstantExpressionParser (databaseInfo);
    }

    public List<IEvaluation> Parse (ConstantExpression constantExpression, ParseContext parseContext)
    {
      ArgumentUtility.CheckNotNull ("constantExpression", constantExpression);
      ArgumentUtility.CheckNotNull ("parseContext", parseContext);
      return new List<IEvaluation> { _innerParser.Parse (constantExpression, parseContext) };
    }

    List<IEvaluation> ISelectProjectionParser.Parse (Expression expression, ParseContext parseContext)
    {
      ArgumentUtility.CheckNotNull ("expression", expression);
      ArgumentUtility.CheckNotNull ("parseContext", parseContext);
      return Parse ((ConstantExpression) expression, parseContext);
    }

    public bool CanParse(Expression expression)
    {
      ArgumentUtility.CheckNotNull ("expression", expression);
      return expression is ConstantExpression;
    }
  }
}