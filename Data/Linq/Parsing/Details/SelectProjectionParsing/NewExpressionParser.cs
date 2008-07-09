/* Copyright (C) 2005 - 2008 rubicon informationstechnologie gmbh
 *
 * This program is free software: you can redistribute it and/or modify it under 
 * the terms of the re:motion license agreement in license.txt. If you did not 
 * receive it, please visit http://www.re-motion.org/licensing.
 * 
 * Unless otherwise provided, this software is distributed on an "AS IS" basis, 
 * WITHOUT WARRANTY OF ANY KIND, either express or implied. 
 */

using System.Collections.Generic;
using System.Linq.Expressions;
using Remotion.Data.Linq.DataObjectModel;
using Remotion.Utilities;

namespace Remotion.Data.Linq.Parsing.Details.SelectProjectionParsing
{
  public class NewExpressionParser : ISelectProjectionParser
  {
    private readonly SelectProjectionParserRegistry _parserRegistry;

    public NewExpressionParser (SelectProjectionParserRegistry parserRegistry)
    {
      ArgumentUtility.CheckNotNull ("parserRegistry", parserRegistry);

      _parserRegistry = parserRegistry;
    }

    public virtual IEvaluation Parse (NewExpression newExpression, ParseContext parseContext)
    {
      ArgumentUtility.CheckNotNull ("newExpression", newExpression);
      ArgumentUtility.CheckNotNull ("parseContext", parseContext);

      List<IEvaluation> argumentEvaluations = new List<IEvaluation> ();
      foreach (Expression exp in newExpression.Arguments)
      {
        argumentEvaluations.Add (_parserRegistry.GetParser (exp).Parse (exp, parseContext));
      }
      return new NewObject (newExpression.Constructor, argumentEvaluations.ToArray());
    }

    IEvaluation ISelectProjectionParser.Parse (Expression expression, ParseContext parseContext)
    {
      return Parse ((NewExpression) expression, parseContext);
    }

    public bool CanParse(Expression expression)
    {
      return expression is NewExpression;
    }
  }
}
