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
using Remotion.Data.Linq.Clauses;
using Remotion.Data.Linq.Parsing.Structure;
using Remotion.Utilities;

namespace Remotion.Data.Linq.Parsing.Structure
{
  public class OrderByExpressionParser
  {
    private readonly bool _isTopLevel;
    private readonly SourceExpressionParser _sourceParser = new SourceExpressionParser (false);

    public OrderByExpressionParser (bool isTopLevel)
    {
      _isTopLevel = isTopLevel;
    }

    public void Parse (ParseResultCollector resultCollector, MethodCallExpression orderExpression)
    {
      ArgumentUtility.CheckNotNull ("orderExpression", orderExpression);
      ArgumentUtility.CheckNotNull ("resultCollector", resultCollector);
      
      if (orderExpression.Arguments.Count != 2)
        throw ParserUtility.CreateParserException ("OrderBy call with two arguments", orderExpression, "OrderBy expressions",
            resultCollector.ExpressionTreeRoot);

      switch (ParserUtility.CheckMethodCallExpression (orderExpression, resultCollector.ExpressionTreeRoot,
          "OrderBy", "OrderByDescending", "ThenBy", "ThenByDescending"))
      {
        case "OrderBy":
          ParseOrderBy (resultCollector, orderExpression, OrderDirection.Asc, true);
          break;
        case "ThenBy":
          ParseOrderBy (resultCollector, orderExpression, OrderDirection.Asc, false);
          break;
        case "OrderByDescending":
          ParseOrderBy (resultCollector, orderExpression, OrderDirection.Desc, true);
          break;
        case "ThenByDescending":
          ParseOrderBy (resultCollector, orderExpression, OrderDirection.Desc, false);
          break;
      }
    }

    private void ParseOrderBy (ParseResultCollector resultCollector, MethodCallExpression sourceExpression, OrderDirection direction, bool orderBy)
    {
      UnaryExpression unaryExpression = ParserUtility.GetTypedExpression<UnaryExpression> (sourceExpression.Arguments[1],
          "second argument of OrderBy expression", resultCollector.ExpressionTreeRoot);
      LambdaExpression ueLambda = ParserUtility.GetTypedExpression<LambdaExpression> (unaryExpression.Operand,
          "second argument of OrderBy expression", resultCollector.ExpressionTreeRoot);

      _sourceParser.Parse (resultCollector, sourceExpression.Arguments[0], ueLambda.Parameters[0], "first argument of OrderBy expression");

      resultCollector.AddBodyExpression (new OrderExpressionData (orderBy, direction, ueLambda));
      if (_isTopLevel)
        resultCollector.AddProjectionExpression (null);
    }
  }
}
