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

    public List<IEvaluation> Parse (ConstantExpression constantExpression, List<FieldDescriptor> fieldDescriptorCollection)
    {
      ArgumentUtility.CheckNotNull ("constantExpression", constantExpression);
      return new List<IEvaluation> { _innerParser.Parse (constantExpression, fieldDescriptorCollection) };
    }

    List<IEvaluation> ISelectProjectionParser.Parse (Expression expression, List<FieldDescriptor> fieldDescriptors)
    {
      return Parse ((ConstantExpression) expression, fieldDescriptors);
    }

    public bool CanParse(Expression expression)
    {
      return expression is ConstantExpression;
    }
  }
}