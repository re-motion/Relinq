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
using System.Linq.Expressions;
using Remotion.Data.Linq.Parsing.Structure;
using Remotion.Utilities;

namespace Remotion.Data.Linq.Parsing.Structure
{
  public class WhereExpressionParser
  {
    private readonly bool _isTopLevel;
    private readonly SourceExpressionParser _sourceParser = new SourceExpressionParser(false);

    public WhereExpressionParser (bool isTopLevel)
    {
      _isTopLevel = isTopLevel;
    }

    public void Parse(ParseResultCollector resultCollector, MethodCallExpression whereExpression)
    {
      ArgumentUtility.CheckNotNull ("resultCollector", resultCollector);
      ArgumentUtility.CheckNotNull ("whereExpression", whereExpression);

      ParserUtility.CheckMethodCallExpression (whereExpression, resultCollector.ExpressionTreeRoot, "Where");
      if (whereExpression.Arguments.Count != 2)
        throw ParserUtility.CreateParserException ("Where call with two arguments", whereExpression, "Where expressions",
            resultCollector.ExpressionTreeRoot);

      ParseWhere (resultCollector, whereExpression);
    }

    private void ParseWhere (ParseResultCollector resultCollector, MethodCallExpression sourceExpression)
    {
      UnaryExpression unaryExpression = ParserUtility.GetTypedExpression<UnaryExpression> (sourceExpression.Arguments[1],
          "second argument of Where expression", resultCollector.ExpressionTreeRoot);
      LambdaExpression ueLambda = ParserUtility.GetTypedExpression<LambdaExpression> (unaryExpression.Operand,
          "second argument of Where expression", resultCollector.ExpressionTreeRoot);

      _sourceParser.Parse (resultCollector, sourceExpression.Arguments[0], ueLambda.Parameters[0], "first argument of Where expression");

      resultCollector.AddBodyExpression (new WhereExpressionData (ueLambda));
      if (_isTopLevel)
        resultCollector.AddProjectionExpression (null);
    }
  }
}
