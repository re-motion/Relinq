using System.Collections.Generic;
using System.Linq.Expressions;
using Rubicon.Data.Linq.DataObjectModel;
using Rubicon.Data.Linq.Parsing.FieldResolving;
using Rubicon.Utilities;

namespace Rubicon.Data.Linq.Parsing.Details.WhereConditionParsing
{
  public class ParameterExpressionParser
  {
    private readonly IDatabaseInfo _databaseInfo;
    private readonly Func<Expression, ICriterion> _parsingCall;

    public ParameterExpressionParser (IDatabaseInfo databaseInfo, Func<Expression, ICriterion> parsingCall)
    {
      ArgumentUtility.CheckNotNull ("databaseInfo", databaseInfo);
      ArgumentUtility.CheckNotNull ("parsingCall", parsingCall);
      _databaseInfo = databaseInfo;
      _parsingCall = parsingCall;
    }

    public ICriterion Parse (ParameterExpression expression, List<FieldDescriptor> fieldDescriptorCollection)
    {
      MemberExpression primaryKeyExpression = Expression.MakeMemberAccess (expression,
          DatabaseInfoUtility.GetPrimaryKeyMember (_databaseInfo, expression.Type));
      return _parsingCall (primaryKeyExpression);
    }
  }
}