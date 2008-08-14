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

namespace Remotion.Data.Linq.Parsing.Details.SelectProjectionParsing
{
  public class ResultModifierParser : MethodCallExpressionParser
  {
    public ResultModifierParser (SelectProjectionParserRegistry parserRegistry)
        : base(parserRegistry)
    {
    }

    protected override List<IEvaluation> ParseEvaluationArguments (MethodCallExpression methodCallExpression, ParseContext parseContext)
    {
      SourceMarkerEvaluation sourceMarkerEvaluation = SourceMarkerEvaluation.Instance;
      return new List<IEvaluation> { sourceMarkerEvaluation };
    }
    
  }
}