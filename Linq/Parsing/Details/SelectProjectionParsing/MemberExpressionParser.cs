using System.Collections.Generic;
using System.Linq.Expressions;
using Remotion.Data.Linq.DataObjectModel;
using Remotion.Data.Linq.Parsing.FieldResolving;

namespace Remotion.Data.Linq.Parsing.Details.SelectProjectionParsing
{
  public class MemberExpressionParser : ISelectProjectionParser
  {
    // member expression parsing is the same for where conditions and select projections, so delegate to that implementation
    private readonly WhereConditionParsing.MemberExpressionParser _innerParser;

    public MemberExpressionParser (QueryModel queryModel, ClauseFieldResolver resolver)
    {
      _innerParser = new WhereConditionParsing.MemberExpressionParser (queryModel, resolver);
    }

    public virtual List<IEvaluation> Parse (MemberExpression memberExpression, List<FieldDescriptor> fieldDescriptorCollection)
    {
      return new List<IEvaluation> { _innerParser.Parse (memberExpression, fieldDescriptorCollection) };
    }

    List<IEvaluation> ISelectProjectionParser.Parse (Expression expression, List<FieldDescriptor> fieldDescriptors)
    {
      return Parse ((MemberExpression) expression, fieldDescriptors);
    }

    public bool CanParse(Expression expression)
    {
      return expression is MemberExpression;
    }
  }
}