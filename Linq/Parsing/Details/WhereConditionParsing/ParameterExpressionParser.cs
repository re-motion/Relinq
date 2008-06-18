using System.Collections.Generic;
using System.Linq.Expressions;
using Remotion.Data.Linq.DataObjectModel;
using Remotion.Data.Linq.Parsing.FieldResolving;
using Remotion.Utilities;

namespace Remotion.Data.Linq.Parsing.Details.WhereConditionParsing
{
  public class ParameterExpressionParser : IWhereConditionParser
  {
    private readonly QueryModel _queryModel;
    private readonly ClauseFieldResolver _resolver;

    public ParameterExpressionParser (QueryModel queryModel, ClauseFieldResolver resolver)
    {
      ArgumentUtility.CheckNotNull ("queryModel", queryModel);
      ArgumentUtility.CheckNotNull ("resolver", resolver);

      _queryModel = queryModel;
      _resolver = resolver;
    }

    public ICriterion Parse (ParameterExpression parameterExpression, List<FieldDescriptor> fieldDescriptorCollection)
    {
      FieldDescriptor fieldDescriptor = _queryModel.ResolveField (_resolver, parameterExpression);
      fieldDescriptorCollection.Add (fieldDescriptor);
      return fieldDescriptor.GetMandatoryColumn ();
    }

    ICriterion IWhereConditionParser.Parse (Expression expression, List<FieldDescriptor> fieldDescriptors)
    {
      return Parse ((ParameterExpression) expression, fieldDescriptors);
    }

    public bool CanParse(Expression expression)
    {
      return expression is ParameterExpression;
    }
  }
}