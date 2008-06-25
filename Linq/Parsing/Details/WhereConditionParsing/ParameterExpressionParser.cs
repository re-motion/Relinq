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

    public ICriterion Parse (ParameterExpression parameterExpression, ParseContext parseContext)
    {
      FieldDescriptor fieldDescriptor = _queryModel.ResolveField (_resolver, parameterExpression);
      parseContext.FieldDescriptors.Add (fieldDescriptor);
      return fieldDescriptor.GetMandatoryColumn ();
    }

    ICriterion IWhereConditionParser.Parse (Expression expression, ParseContext parseContext)
    {
      return Parse ((ParameterExpression) expression, parseContext);
    }

    public bool CanParse(Expression expression)
    {
      return expression is ParameterExpression;
    }
  }
}