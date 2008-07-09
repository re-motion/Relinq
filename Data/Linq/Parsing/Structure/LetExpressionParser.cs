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
using Remotion.Utilities;

namespace Remotion.Data.Linq.Parsing.Structure
{
  public class LetExpressionParser
  {
    private readonly SourceExpressionParser _sourceParser = new SourceExpressionParser (false);

    public LetExpressionParser ()
    {
    }

    public void Parse (ParseResultCollector resultCollector, MethodCallExpression letExpression)
    {
      ArgumentUtility.CheckNotNull ("resultCollector", resultCollector);
      ArgumentUtility.CheckNotNull ("letExpression", letExpression);

      ParserUtility.CheckMethodCallExpression (letExpression, resultCollector.ExpressionTreeRoot, "Select");

      if (letExpression.Arguments.Count != 2)
        throw ParserUtility.CreateParserException ("Let call with two arguments", letExpression, "Let expressions",
            resultCollector.ExpressionTreeRoot);

      ParseLet (resultCollector, letExpression);
    }

    private void ParseLet (ParseResultCollector resultCollector, MethodCallExpression letExpression)
    {
      UnaryExpression unaryExpression = ParserUtility.GetTypedExpression<UnaryExpression> (letExpression.Arguments[1],
          "second argument of Let expression", resultCollector.ExpressionTreeRoot);
      
      LambdaExpression ueLambda = ParserUtility.GetTypedExpression<LambdaExpression> (unaryExpression.Operand,
          "second argument of Let expression", resultCollector.ExpressionTreeRoot);
      string identifierName = ((NewExpression) ueLambda.Body).Constructor.GetParameters ()[1].Name;
      Type identifierType = ((NewExpression) ueLambda.Body).Constructor.GetParameters ()[1].ParameterType;
      ParameterExpression identifier = Expression.Parameter (identifierType, identifierName);
      Expression expression = ((NewExpression) ueLambda.Body).Arguments[1];
      _sourceParser.Parse (resultCollector, letExpression.Arguments[0], ueLambda.Parameters[0], "first argument of Select expression");

      resultCollector.AddBodyExpression (new LetExpressionData (identifier, expression));
      resultCollector.AddProjectionExpression (ueLambda);

    }
  }
}
