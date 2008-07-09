/* Copyright (C) 2005 - 2008 rubicon informationstechnologie gmbh
 *
 * This program is free software: you can redistribute it and/or modify it under 
 * the terms of the re:motion license agreement in license.txt. If you did not 
 * receive it, please visit http://www.re-motion.org/licensing.
 * 
 * Unless otherwise provided, this software is distributed on an "AS IS" basis, 
 * WITHOUT WARRANTY OF ANY KIND, either express or implied. 
 */

using System.Linq.Expressions;
using Remotion.Data.Linq.DataObjectModel;
using Remotion.Utilities;

namespace Remotion.Data.Linq.Parsing.Details.WhereConditionParsing
{
  public class UnaryExpressionParser : IWhereConditionParser
  {
    private readonly WhereConditionParserRegistry _parserRegistry;
    
    public UnaryExpressionParser (WhereConditionParserRegistry parserRegistry)
    {
      ArgumentUtility.CheckNotNull ("parserRegistry", parserRegistry);
      _parserRegistry = parserRegistry;
    }

    public ICriterion Parse (UnaryExpression unaryExpression, ParseContext parseContext)
    {
      switch (unaryExpression.NodeType)
      {
        case ExpressionType.Not:
          return new NotCriterion (_parserRegistry.GetParser (unaryExpression.Operand).Parse (unaryExpression.Operand, parseContext));
        case ExpressionType.Convert: // Convert is simply ignored ATM, change to more sophisticated logic when needed
          return (_parserRegistry.GetParser (unaryExpression.Operand).Parse (unaryExpression.Operand, parseContext));
        default:
          throw ParserUtility.CreateParserException ("not or convert expression", unaryExpression.NodeType, "unary expression in where condition",
              parseContext.ExpressionTreeRoot);
      }
    }

    public bool CanParse(Expression expression)
    {
      return expression is UnaryExpression;
    }

    public ICriterion Parse(Expression expression, ParseContext parseContext)
    {
      return Parse ((UnaryExpression) expression, parseContext);
    }
  }
}
