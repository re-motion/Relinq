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

    public ICriterion Parse (ConstantExpression constantExpression, List<FieldDescriptor> fieldDescriptorCollection)
    {
      object newValue = _databaseInfo.ProcessWhereParameter (constantExpression.Value);
      return new Constant (newValue);
    }

    public bool CanParse(Expression expression)
    {
      return expression is ConstantExpression;
    }

    public ICriterion Parse(Expression expression, List<FieldDescriptor> fieldDescriptors)
    {
      return Parse ((ConstantExpression) expression, fieldDescriptors);
    }
  }
}