using System.Linq.Expressions;
using Remotion.Data.Linq.DataObjectModel;


namespace Remotion.Data.Linq.Parsing.Details.SelectProjectionParsing
{
  public class ConstantExpressionParser
  {
    private readonly WhereConditionParsing.ConstantExpressionParser _innerParser;


    public ConstantExpressionParser (IDatabaseInfo databaseInfo)
    {
      _innerParser = new WhereConditionParsing.ConstantExpressionParser (databaseInfo);
    }

    public IEvaluation Parse (ConstantExpression expression)
    {
      return _innerParser.Parse (expression);
    }
  }
}