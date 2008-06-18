using System.Collections.Generic;
using System.Linq.Expressions;
using Remotion.Data.Linq.DataObjectModel;
using Remotion.Data.Linq.Parsing.FieldResolving;
using Remotion.Utilities;

namespace Remotion.Data.Linq.Parsing.Details.WhereConditionParsing
{
  public class MemberExpressionParser : IWhereConditionParser
  {
    private readonly QueryModel _queryModel;
    private readonly ClauseFieldResolver _resolver;

    public MemberExpressionParser (QueryModel queryModel, ClauseFieldResolver resolver)
    {
      ArgumentUtility.CheckNotNull ("queryExpression", queryModel);
      ArgumentUtility.CheckNotNull ("resolver", resolver);
      _queryModel = queryModel;
      _resolver = resolver;
    }

    public virtual ICriterion Parse (MemberExpression memberExpression, List<FieldDescriptor> fieldDescriptorCollection)
    {
      FieldDescriptor fieldDescriptor = _queryModel.ResolveField (_resolver, memberExpression);
      fieldDescriptorCollection.Add (fieldDescriptor);
      return fieldDescriptor.GetMandatoryColumn ();
    }

    ICriterion IWhereConditionParser.Parse (Expression expression, List<FieldDescriptor> fieldDescriptors)
    {
      return Parse ((MemberExpression) expression, fieldDescriptors);
    }

    public bool CanParse(Expression expression)
    {
      return expression is MemberExpression;
    }
  }
}