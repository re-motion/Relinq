/* Copyright (C) 2005 - 2008 rubicon informationstechnologie gmbh
 *
 * This program is free software: you can redistribute it and/or modify it under 
 * the terms of the re:motion license agreement in license.txt. If you did not 
 * receive it, please visit http://www.re-motion.org/licensing.
 * 
 * Unless otherwise provided, this software is distributed on an "AS IS" basis, 
 * WITHOUT WARRANTY OF ANY KIND, either express or implied. 
 */

using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Remotion.Data.Linq.Parsing.Structure;

namespace Remotion.Data.UnitTests.Linq.ParsingTest.StructureTest.WhereExpressionParserTest
{
  public class BodyHelper
  {
    private readonly IEnumerable<BodyExpressionDataBase> _bodyExpressions;

    public BodyHelper (IEnumerable<BodyExpressionDataBase> bodyExpressions)
    {
      _bodyExpressions = bodyExpressions;
    }

    public List<Expression> FromExpressions
    {
      get
      {
        List<Expression> fromExpressions = new List<Expression>();
        foreach (BodyExpressionDataBase expression in _bodyExpressions)
        {
          FromExpressionData fromExpressionData = expression as FromExpressionData;
          if (fromExpressionData != null)
            fromExpressions.Add (fromExpressionData.TypedExpression);
        }
        return fromExpressions;
      }
    }

    public List<ParameterExpression> FromIdentifiers
    {
      get
      {
        List<ParameterExpression> fromIdentifiers = new List<ParameterExpression>();
        foreach (BodyExpressionDataBase expression in _bodyExpressions)
        {
          FromExpressionData fromExpressionData = expression as FromExpressionData;
          if (fromExpressionData != null)
            fromIdentifiers.Add (fromExpressionData.Identifier);
        }
        return fromIdentifiers;
      }
    }

    public List<LambdaExpression> WhereExpressions
    {
      get
      {
        List<LambdaExpression> fromExpressions = new List<LambdaExpression>();
        foreach (BodyExpressionDataBase expression in _bodyExpressions)
        {
          WhereExpressionData whereExpressionData = expression as WhereExpressionData;
          if (whereExpressionData != null)
            fromExpressions.Add (whereExpressionData.TypedExpression);
        }
        return fromExpressions;
      }
    }

    public List<OrderExpressionData> OrderingExpressions
    {
      get
      {
        List<OrderExpressionData> orderbyExpressions = new List<OrderExpressionData> ();
        foreach (BodyExpressionDataBase expression in _bodyExpressions)
        {
          OrderExpressionData orderExpressionData = expression as OrderExpressionData;
          if (orderExpressionData != null)
            orderbyExpressions.Add (orderExpressionData);
        }
        return orderbyExpressions;
      }
    }

    public List<Expression> LetExpressions
    {
      get 
      {
        List<Expression> letExpressions = new List<Expression> ();
        foreach (BodyExpressionDataBase expression in _bodyExpressions)
        {
          LetExpressionData letExpressionData = expression as LetExpressionData;
          if (letExpressionData != null)
            letExpressions.Add (letExpressionData.TypedExpression);
        }
        return letExpressions;
      }
    }

    public List<ParameterExpression> LetIdentifiers
    {
      get
      {
        List<ParameterExpression> letIdentifiers = new List<ParameterExpression> ();
        foreach (BodyExpressionDataBase expression in _bodyExpressions)
        {
          LetExpressionData letExpressionData = expression as LetExpressionData;
          if (letExpressionData != null)
            letIdentifiers.Add (letExpressionData.Identifier);
        }
        return letIdentifiers;
      }
    }

    


  }
}
