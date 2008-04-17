using System.Collections.Generic;
using System.Linq.Expressions;
using Rubicon.Data.Linq.DataObjectModel;
using Rubicon.Data.Linq.Parsing.FieldResolving;

namespace Rubicon.Data.Linq.Parsing.Details.SelectProjectionParsing
{
  public class MemberExpressionParser
  {
    private readonly WhereConditionParsing.MemberExpressionParser _innerParser;

    public MemberExpressionParser (QueryModel queryModel, ClauseFieldResolver resolver)
    {
      _innerParser = new WhereConditionParsing.MemberExpressionParser (queryModel, resolver);
    }

    public virtual IEvaluation Parse (MemberExpression expression, List<FieldDescriptor> fieldDescriptorCollection)
    {
      return _innerParser.Parse (expression, fieldDescriptorCollection);
    }
  }
}