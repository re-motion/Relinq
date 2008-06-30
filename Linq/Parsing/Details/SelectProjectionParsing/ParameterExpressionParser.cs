using System.Collections.Generic;
using System.Linq.Expressions;
using Remotion.Data.Linq.DataObjectModel;
using Remotion.Data.Linq.Parsing.FieldResolving;
using Remotion.Utilities;

namespace Remotion.Data.Linq.Parsing.Details.SelectProjectionParsing
{
  public class ParameterExpressionParser : ISelectProjectionParser
  {
    // parameter expression parsing is the same for where conditions and select projections, so delegate to that implementation
    private readonly WhereConditionParsing.ParameterExpressionParser _innerParser;

    public ParameterExpressionParser (ClauseFieldResolver resolver)
    {
      ArgumentUtility.CheckNotNull ("resolver", resolver);
      _innerParser = new WhereConditionParsing.ParameterExpressionParser (resolver);
    }

    public IEvaluation Parse (ParameterExpression parameterExpression, ParseContext parseContext)
    {
      ArgumentUtility.CheckNotNull ("parameterExpression", parameterExpression);
      ArgumentUtility.CheckNotNull ("parseContext", parseContext);
      return _innerParser.Parse (parameterExpression, parseContext);
    }

    IEvaluation ISelectProjectionParser.Parse (Expression expression, ParseContext parseContext)
    {
      ArgumentUtility.CheckNotNull ("expression", expression);
      ArgumentUtility.CheckNotNull ("parseContext", parseContext);
      return Parse ((ParameterExpression) expression, parseContext);
    }

    public bool CanParse(Expression expression)
    {
      ArgumentUtility.CheckNotNull ("expression", expression);
      return expression is ParameterExpression;
    }
  }
}