using System.Collections.Generic;
using System.Linq.Expressions;
using Remotion.Data.Linq.DataObjectModel;
using Remotion.Data.Linq.Parsing.FieldResolving;

namespace Remotion.Data.Linq.Parsing.Details.SelectProjectionParsing
{
  public class ParameterExpressionParser
  {
    private readonly WhereConditionParsing.ParameterExpressionParser _innerParser;

    public ParameterExpressionParser (QueryModel queryModel, ClauseFieldResolver resolver)
    {
      _innerParser = new WhereConditionParsing.ParameterExpressionParser (queryModel, resolver);
    }

    public IEvaluation Parse (ParameterExpression expression, List<FieldDescriptor> fieldDescriptorCollection)
    {
      return _innerParser.Parse (expression, fieldDescriptorCollection);
    }
  }
}