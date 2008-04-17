using System.Collections.Generic;
using System.Linq.Expressions;
using Rubicon.Data.Linq.DataObjectModel;
using Rubicon.Data.Linq.Parsing.FieldResolving;
using Rubicon.Utilities;

namespace Rubicon.Data.Linq.Parsing.Details.SelectProjectionParsing
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