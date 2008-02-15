using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Rubicon.Data.Linq.Parsing.Structure;

namespace Rubicon.Data.Linq.UnitTests.ParsingTest.StructureTest.WhereExpressionParserTest
{
  public class BodyHelper
  {
    private readonly IEnumerable<BodyExpressionBase> _bodyExpressions;

    public BodyHelper (IEnumerable<BodyExpressionBase> bodyExpressions)
    {
      _bodyExpressions = bodyExpressions;
    }

    public List<Expression> FromExpressions
    {
      get
      {
        List<Expression> fromExpressions = new List<Expression>();
        foreach (BodyExpressionBase expression in _bodyExpressions)
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
        foreach (BodyExpressionBase expression in _bodyExpressions)
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
        foreach (BodyExpressionBase expression in _bodyExpressions)
        {
          WhereExpression whereExpression = expression as WhereExpression;
          if (whereExpression != null)
            fromExpressions.Add (whereExpression.Expression);
        }
        return fromExpressions;
      }
    }

    public List<OrderExpression> OrderingExpressions
    {
      get
      {
        List<OrderExpression> orderbyExpressions = new List<OrderExpression> ();
        foreach (BodyExpressionBase expression in _bodyExpressions)
        {
          OrderExpression orderExpression = expression as OrderExpression;
          if (orderExpression != null)
            orderbyExpressions.Add (orderExpression);
        }
        return orderbyExpressions;
      }
    }

    


  }
}