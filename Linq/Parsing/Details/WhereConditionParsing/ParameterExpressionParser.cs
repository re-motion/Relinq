using System.Collections.Generic;
using System.Linq.Expressions;
using Rubicon.Data.Linq.DataObjectModel;
using Rubicon.Data.Linq.Parsing.FieldResolving;
using Rubicon.Utilities;

namespace Rubicon.Data.Linq.Parsing.Details.WhereConditionParsing
{
  public class ParameterExpressionParser
  {
    private readonly QueryModel _queryModel;
    private readonly ClauseFieldResolver _resolver;

    public ParameterExpressionParser (QueryModel queryModel, ClauseFieldResolver resolver)
    {
      _queryModel = queryModel;
      _resolver = resolver;
      ArgumentUtility.CheckNotNull ("queryModel", queryModel);
      ArgumentUtility.CheckNotNull ("resolver", resolver);
    }

    public ICriterion Parse (ParameterExpression expression, List<FieldDescriptor> fieldDescriptorCollection)
    {
      //MemberExpression primaryKeyExpression = Expression.MakeMemberAccess (expression,
      //    DatabaseInfoUtility.GetPrimaryKeyMember (_databaseInfo, expression.Type));
      //return _parsingCall (primaryKeyExpression);

      //MemberExpression primaryKeyExpression = Expression.MakeMemberAccess (expression,
      //    DatabaseInfoUtility.GetPrimaryKeyMember (_resolver.DatabaseInfo, expression.Type));
      //return new MemberExpressionParser (_queryModel, _resolver).Parse (primaryKeyExpression, fieldDescriptorCollection);

      FieldDescriptor fieldDescriptor = _queryModel.ResolveField (_resolver, expression);
      fieldDescriptorCollection.Add (fieldDescriptor);
      return fieldDescriptor.GetMandatoryColumn ();
    }
  }
}