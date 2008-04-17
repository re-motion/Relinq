using System.Linq.Expressions;
using Rubicon.Data.Linq.DataObjectModel;
using Rubicon.Data.Linq.Parsing.FieldResolving;
using Rubicon.Utilities;

namespace Rubicon.Data.Linq.Parsing.Details.SelectProjectionParsing
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