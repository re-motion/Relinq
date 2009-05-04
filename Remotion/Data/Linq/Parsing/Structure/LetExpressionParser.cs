// This file is part of the re-motion Core Framework (www.re-motion.org)
// Copyright (C) 2005-2009 rubicon informationstechnologie gmbh, www.rubicon.eu
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
      if (ueLambda.Body is ParameterExpression)
      {
        resultCollector.AddBodyExpression (new MainFromExpressionData (letExpression.Arguments[0])); 
      }
      else
      {
        string identifierName = ((NewExpression) ueLambda.Body).Constructor.GetParameters ()[1].Name; //if ueLambda == Parameter Expression => SubSelect
        Type identifierType = ((NewExpression) ueLambda.Body).Constructor.GetParameters ()[1].ParameterType;
        ParameterExpression identifier = Expression.Parameter (identifierType, identifierName);
        Expression expression = ((NewExpression) ueLambda.Body).Arguments[1];
        _sourceParser.Parse (resultCollector, letExpression.Arguments[0], ueLambda.Parameters[0], "first argument of Select expression");

        resultCollector.AddBodyExpression (new LetExpressionData (identifier, expression));
        resultCollector.AddProjectionExpression (ueLambda);
      }

    }
  }
}
