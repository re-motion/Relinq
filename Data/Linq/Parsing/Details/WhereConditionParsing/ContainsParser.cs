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
using Remotion.Data.Linq.Expressions;
using Remotion.Utilities;

namespace Remotion.Data.Linq.Parsing.Details.WhereConditionParsing
{
  public class ContainsParser : IWhereConditionParser
  {
    private readonly WhereConditionParserRegistry _parserRegistry;

    public ContainsParser (WhereConditionParserRegistry parserRegistry)
    {
      ArgumentUtility.CheckNotNull ("parserRegistry", parserRegistry);
      _parserRegistry = parserRegistry;
    }

    public ICriterion Parse (MethodCallExpression methodCallExpression, ParseContext parseContext)
    {
      if (methodCallExpression.Method.Name == "Contains" && methodCallExpression.Method.IsGenericMethod)
      {
        ParserUtility.CheckNumberOfArguments (methodCallExpression, "Contains", 2, parseContext.ExpressionTreeRoot);
        ParserUtility.CheckParameterType<SubQueryExpression> (methodCallExpression, "Contains", 0, parseContext.ExpressionTreeRoot);
        return CreateContains ((SubQueryExpression) methodCallExpression.Arguments[0], methodCallExpression.Arguments[1], parseContext);
      }
      throw ParserUtility.CreateParserException ("Contains with expression", methodCallExpression.Method.Name,
          "method call expression in where condition", parseContext.ExpressionTreeRoot);
    }

    ICriterion IWhereConditionParser.Parse (Expression expression, ParseContext parseContext)
    {
      return Parse ((MethodCallExpression) expression, parseContext);
    }

    public bool CanParse (Expression expression)
    {
      var methodCallExpression = expression as MethodCallExpression;
      if (methodCallExpression != null)
      {
        if (methodCallExpression.Method.Name == "Contains" && methodCallExpression.Method.IsGenericMethod)
          return true;
      }
      return false;
    }

    private BinaryCondition CreateContains (SubQueryExpression subQueryExpression, Expression itemExpression, ParseContext parseContext)
    {
      SubQuery subQuery = new SubQuery (subQueryExpression.QueryModel, null);
      return new BinaryCondition (subQuery, _parserRegistry.GetParser (itemExpression).Parse (itemExpression, parseContext),
          BinaryCondition.ConditionKind.Contains);
    }
  }
}
