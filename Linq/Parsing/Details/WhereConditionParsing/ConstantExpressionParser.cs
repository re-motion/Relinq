using System.Linq.Expressions;
using Rubicon.Data.Linq.DataObjectModel;
using Rubicon.Utilities;

namespace Rubicon.Data.Linq.Parsing.Details.WhereConditionParsing
{
  public class ConstantExpressionParser
  {
    private readonly IDatabaseInfo _databaseInfo;

    public ConstantExpressionParser(IDatabaseInfo databaseInfo)
    {
      ArgumentUtility.CheckNotNull ("databaseInfo", databaseInfo);
      _databaseInfo = databaseInfo;
    }

    public ICriterion Parse (ConstantExpression expression)
    {
      object newValue = _databaseInfo.ProcessWhereParameter (expression.Value);
      return new Constant (newValue);
    }
  }
}