using System.Collections.Generic;
using System.Linq.Expressions;
using Rubicon.Data.Linq.DataObjectModel;
using Rubicon.Data.Linq.Parsing.FieldResolving;
using Rubicon.Utilities;

namespace Rubicon.Data.Linq.Parsing.Details.WhereConditionParsing
{
  public class MemberExpressionParser
  {
    private readonly QueryModel _queryModel;
    private readonly FromClauseFieldResolver _resolver;

    public MemberExpressionParser (QueryModel queryModel, FromClauseFieldResolver resolver)
    {
      ArgumentUtility.CheckNotNull ("queryExpression", queryModel);
      ArgumentUtility.CheckNotNull ("resolver", resolver);
      _queryModel = queryModel;
      _resolver = resolver;
    }

    public virtual ICriterion Parse (MemberExpression expression, List<FieldDescriptor> fieldDescriptorCollection)
    {
      FieldDescriptor fieldDescriptor = _queryModel.ResolveField (_resolver, expression);
      fieldDescriptorCollection.Add (fieldDescriptor);
      return fieldDescriptor.GetMandatoryColumn ();
    }
  }
}