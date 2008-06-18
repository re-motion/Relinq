using System.Collections.Generic;
using System.Linq.Expressions;
using Remotion.Data.Linq.DataObjectModel;
using Remotion.Data.Linq.Parsing.FieldResolving;

namespace Remotion.Data.Linq.Parsing.Details.SelectProjectionParsing
{
  public class ParameterExpressionParser : ISelectProjectionParser
  {
    // parameter expression parsing is the same for where conditions and select projections, so delegate to that implementation
    private readonly WhereConditionParsing.ParameterExpressionParser _innerParser;

    public ParameterExpressionParser (QueryModel queryModel, ClauseFieldResolver resolver)
    {
      _innerParser = new WhereConditionParsing.ParameterExpressionParser (queryModel, resolver);
    }

    public List<IEvaluation> Parse (ParameterExpression parameterExpression, List<FieldDescriptor> fieldDescriptorCollection)
    {
      return new List<IEvaluation> { _innerParser.Parse (parameterExpression, fieldDescriptorCollection) };
    }

    List<IEvaluation> ISelectProjectionParser.Parse (Expression expression, List<FieldDescriptor> fieldDescriptors)
    {
      return Parse ((ParameterExpression) expression, fieldDescriptors);
    }

    public bool CanParse(Expression expression)
    {
      return expression is ParameterExpression;
    }
  }
}