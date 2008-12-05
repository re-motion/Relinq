// This file is part of the re-motion Core Framework (www.re-motion.org)
// Copyright (C) 2005-2008 rubicon informationstechnologie gmbh, www.rubicon.eu
//
// The re-motion Core Framework is free software; you can redistribute it 
// and/or modify it under the terms of the GNU Lesser General Public License 
// version 3.0 as published by the Free Software Foundation.
//
// re-motion is distributed in the hope that it will be useful, 
// but WITHOUT ANY WARRANTY; without even the implied warranty of 
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the 
// GNU Lesser General Public License for more details.
//
// You should have received a copy of the GNU Lesser General Public License
// along with re-motion; if not, see http://www.gnu.org/licenses.
//
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
