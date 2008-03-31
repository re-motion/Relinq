using System;
using System.Linq.Expressions;

namespace Rubicon.Data.Linq.DataObjectModel
{
  public class SubQueryExpression : Expression
  {

    public SubQueryExpression (QueryModel queryModel)
      : base ((ExpressionType) int.MaxValue, queryModel.ResultType)
    {
      
    }


  }
}