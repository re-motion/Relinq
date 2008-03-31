using System;
using System.Linq.Expressions;

namespace Rubicon.Data.Linq.DataObjectModel
{
  public class SubQueryExpression : Expression
  {
    public QueryModel QueryModel { get; private set; }

    public SubQueryExpression (QueryModel queryModel)
      : base ((ExpressionType) int.MaxValue, queryModel.ResultType)
    {
      QueryModel = queryModel;
    }
  }
}