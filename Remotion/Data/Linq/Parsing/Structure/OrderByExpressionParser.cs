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
          ParseOrderBy (resultCollector, orderExpression, OrderingDirection.Asc, true);
          break;
        case "ThenBy":
          ParseOrderBy (resultCollector, orderExpression, OrderingDirection.Asc, false);
          break;
        case "OrderByDescending":
          ParseOrderBy (resultCollector, orderExpression, OrderingDirection.Desc, true);
          break;
        case "ThenByDescending":
          ParseOrderBy (resultCollector, orderExpression, OrderingDirection.Desc, false);
          break;
      }
    }

    private void ParseOrderBy (ParseResultCollector resultCollector, MethodCallExpression sourceExpression, OrderingDirection direction, bool isOrderBy)
    {
      UnaryExpression unaryExpression = ParserUtility.GetTypedExpression<UnaryExpression> (sourceExpression.Arguments[1],
          "second argument of OrderBy expression", resultCollector.ExpressionTreeRoot);
      LambdaExpression ueLambda = ParserUtility.GetTypedExpression<LambdaExpression> (unaryExpression.Operand,
          "second argument of OrderBy expression", resultCollector.ExpressionTreeRoot);

      _sourceParser.Parse (resultCollector, sourceExpression.Arguments[0], ueLambda.Parameters[0], "first argument of OrderBy expression");

      resultCollector.AddBodyExpression (new OrderExpressionData (isOrderBy, direction, ueLambda));
      if (_isTopLevel)
        resultCollector.AddIdentityProjectionExpression (ueLambda.Parameters[0]);
    }
  }
}
