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
using System.Linq;
using System.Linq.Expressions;
using Remotion.Data.Linq.Expressions;
using Remotion.Data.Linq.Parsing.Details.WhereConditionParsing;
using Remotion.Data.Linq.Parsing.FieldResolving;

namespace Remotion.Data.Linq.Parsing.Details
{
  public class WhereConditionParserRegistry
  {
    private readonly ParserRegistry _parserRegistry;

    public WhereConditionParserRegistry (IDatabaseInfo databaseInfo)
    {
      _parserRegistry = new ParserRegistry ();
      ClauseFieldResolver resolver = new ClauseFieldResolver (databaseInfo, new WhereFieldAccessPolicy (databaseInfo));

      RegisterParser (typeof (BinaryExpression), new BinaryExpressionParser (this));
      RegisterParser (typeof (MemberExpression), new MemberExpressionParser (resolver));
      RegisterParser (typeof (ParameterExpression), new ParameterExpressionParser (resolver));
      RegisterParser (typeof (ConstantExpression), new ConstantExpressionParser (databaseInfo));
      RegisterParser (typeof (MethodCallExpression), new MethodCallExpressionParser (this));
      RegisterParser (typeof (MethodCallExpression), new LikeParser (this));
      RegisterParser (typeof (SubQueryExpression), new SubQueryExpressionParser ());
      RegisterParser (typeof (MethodCallExpression), new ContainsParser (this));
      RegisterParser (typeof (MethodCallExpression), new ContainsFullTextParser (this));
      RegisterParser (typeof (UnaryExpression), new UnaryExpressionParser (this));
    }

    public IEnumerable<IWhereConditionParser> GetParsers (Type expressionType)
    {
      return _parserRegistry.GetParsers (expressionType).Cast<IWhereConditionParser> ();
    }
    
    public virtual IWhereConditionParser GetParser (Expression expression)
    {
      return (IWhereConditionParser) _parserRegistry.GetParser (expression);
    }

    public void RegisterParser (Type expressionType, IWhereConditionParser parser)
    {
      _parserRegistry.RegisterParser (expressionType, parser);
    }
  }
}
