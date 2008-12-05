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
  public class SelectExpressionParser
  {
    private readonly SourceExpressionParser _sourceParser = new SourceExpressionParser (false);

    public SelectExpressionParser ()
    {
    }

    public void Parse (ParseResultCollector resultCollector, MethodCallExpression selectExpression)
    {
      ArgumentUtility.CheckNotNull ("selectExpression", selectExpression);
      ArgumentUtility.CheckNotNull ("resultCollector", resultCollector);

      ParserUtility.CheckMethodCallExpression (selectExpression, resultCollector.ExpressionTreeRoot, "Select");

      if (selectExpression.Arguments.Count != 2)
        throw ParserUtility.CreateParserException ("Select call with two arguments", selectExpression, "Select expressions",
            resultCollector.ExpressionTreeRoot);

      ParseSelect (resultCollector, selectExpression);
    }

    private void ParseSelect (ParseResultCollector resultCollector, MethodCallExpression sourceExpression)
    {
      UnaryExpression unaryExpression = ParserUtility.GetTypedExpression<UnaryExpression> (sourceExpression.Arguments[1],
          "second argument of Select expression", resultCollector.ExpressionTreeRoot);
      LambdaExpression ueLambda = ParserUtility.GetTypedExpression<LambdaExpression> (unaryExpression.Operand,
          "second argument of Select expression", resultCollector.ExpressionTreeRoot);

      CheckForSubQuery(resultCollector, sourceExpression, ueLambda);

      _sourceParser.Parse (resultCollector, sourceExpression.Arguments[0], ueLambda.Parameters[0],  "first argument of Select expression");
      resultCollector.AddProjectionExpression (ueLambda);
    }

    private void CheckForSubQuery (ParseResultCollector resultCollector, MethodCallExpression sourceExpression, LambdaExpression ueLambda)
    {
      var possibleSubQuery = ueLambda.Body as MethodCallExpression;
      if (possibleSubQuery != null && _sourceParser.CallDispatcher.CanParse (possibleSubQuery.Method))
        throw ParserUtility.CreateParserException(
            "no subqueries",
            sourceExpression,
            "Select expressions",
            resultCollector.ExpressionTreeRoot);
    }
  }
}
