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

      ParseWhere(resultCollector, ueLambda);
    }

    private void ParseWhere (ParseResultCollector resultCollector, LambdaExpression whereConditionLambda)
    {
      resultCollector.AddBodyExpression (new WhereExpressionData (whereConditionLambda));
      if (_isTopLevel)
        resultCollector.AddIdentityProjectionExpression (whereConditionLambda.Parameters[0]);
    }
  }
}
