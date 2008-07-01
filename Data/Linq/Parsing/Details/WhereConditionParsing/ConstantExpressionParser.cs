using System.Collections.Generic;
using System.Linq.Expressions;
using Remotion.Data.Linq.DataObjectModel;
using Remotion.Utilities;

namespace Remotion.Data.Linq.Parsing.Details.WhereConditionParsing
{
  public class ConstantExpressionParser : IWhereConditionParser
  {
    private readonly IDatabaseInfo _databaseInfo;

    public ConstantExpressionParser(IDatabaseInfo databaseInfo)
    {
      ArgumentUtility.CheckNotNull ("databaseInfo", databaseInfo);
      _databaseInfo = databaseInfo;
    }

    public ICriterion Parse (ConstantExpression constantExpression, ParseContext parseContext)
    {
      object newValue = _databaseInfo.ProcessWhereParameter (constantExpression.Value);
      return new Constant (newValue);
    }

    public bool CanParse(Expression expression)
    {
      return expression is ConstantExpression;
    }

    ICriterion IWhereConditionParser.Parse(Expression expression, ParseContext parseContext)
    {
      return Parse ((ConstantExpression) expression, parseContext);
    }
  }
}