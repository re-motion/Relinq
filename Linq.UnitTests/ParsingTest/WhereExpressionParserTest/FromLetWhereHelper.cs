using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Rubicon.Data.Linq.Parsing;

namespace Rubicon.Data.Linq.UnitTests.ParsingTest.WhereExpressionParserTest
{
  public class FromLetWhereHelper
  {
    private readonly IEnumerable<FromLetWhereExpressionBase> _fromLetWhereExpressions;

    public FromLetWhereHelper (IEnumerable<FromLetWhereExpressionBase> fromLetWhereExpressions)
    {
      _fromLetWhereExpressions = fromLetWhereExpressions;
    }

    public List<Expression> FromExpressions
    {
      get
      {
        List<Expression> fromExpressions = new List<Expression>();
        foreach (FromLetWhereExpressionBase expression in _fromLetWhereExpressions)
        {
          FromExpression fromExpression = expression as FromExpression;
          if (fromExpression != null)
            fromExpressions.Add (fromExpression.Expression);
        }
        return fromExpressions;
      }
    }

    public List<ParameterExpression> FromIdentifiers
    {
      get
      {
        List<ParameterExpression> fromIdentifiers = new List<ParameterExpression>();
        foreach (FromLetWhereExpressionBase expression in _fromLetWhereExpressions)
        {
          FromExpression fromExpression = expression as FromExpression;
          if (fromExpression != null)
            fromIdentifiers.Add (fromExpression.Identifier);
        }
        return fromIdentifiers;
      }
    }

    public List<LambdaExpression> WhereExpressions
    {
      get
      {
        List<LambdaExpression> fromExpressions = new List<LambdaExpression>();
        foreach (FromLetWhereExpressionBase expression in _fromLetWhereExpressions)
        {
          WhereExpression whereExpression = expression as WhereExpression;
          if (whereExpression != null)
            fromExpressions.Add (whereExpression.Expression);
        }
        return fromExpressions;
      }
    }
  }
}
