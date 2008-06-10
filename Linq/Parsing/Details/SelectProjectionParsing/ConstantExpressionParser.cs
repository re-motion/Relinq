using System.Collections.Generic;
using System.Linq.Expressions;
using Remotion.Data.Linq.DataObjectModel;
using Remotion.Utilities;


namespace Remotion.Data.Linq.Parsing.Details.SelectProjectionParsing
{
  public class ConstantExpressionParser : ISelectProjectionParser<ConstantExpression>, ISelectProjectionParser
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

    public bool CanParse(ConstantExpression constantExpression)
    {
      return true;
    }

    public List<IEvaluation> Parse(Expression expression, List<FieldDescriptor> fieldDescriptors)
    {
      return Parse ((ConstantExpression) expression, fieldDescriptors);
    }
  }
}