using System.Collections.Generic;
using System.Linq.Expressions;
using Rubicon.Data.Linq.DataObjectModel;
using Rubicon.Data.Linq.Parsing.FieldResolving;
using Rubicon.Utilities;

namespace Rubicon.Data.Linq.Parsing.Details.WhereParser
{
  public class MemberExpressionParser
  {
    private readonly QueryExpression _queryExpression;
    private readonly FromClauseFieldResolver _resolver;

    public MemberExpressionParser (QueryExpression queryExpression, FromClauseFieldResolver resolver)
    {
      ArgumentUtility.CheckNotNull ("queryExpression", queryExpression);
      ArgumentUtility.CheckNotNull ("resolver", resolver);
      _queryExpression = queryExpression;
      _resolver = resolver;
    }

    public virtual ICriterion Parse (MemberExpression expression, List<FieldDescriptor> fieldDescriptorCollection)
    {
      FieldDescriptor fieldDescriptor = _queryExpression.ResolveField (_resolver, expression);
      fieldDescriptorCollection.Add (fieldDescriptor);
      return fieldDescriptor.GetMandatoryColumn ();
    }
  }
}