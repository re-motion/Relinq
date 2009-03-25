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
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;
using Remotion.Collections;
using Remotion.Data.Linq.DataObjectModel;
using Remotion.Utilities;

namespace Remotion.Data.Linq.Parsing.Details.SelectProjectionParsing
{
  public class ResultModifierParser
  {
    private readonly SelectProjectionParserRegistry _selectParserRegistry;
    private readonly WhereConditionParserRegistry _whereParserRegistry;

    public ResultModifierParser (SelectProjectionParserRegistry selectParserRegistry, WhereConditionParserRegistry whereParserRegistry)
    {
      ArgumentUtility.CheckNotNull ("selectParserRegistry", selectParserRegistry);
      ArgumentUtility.CheckNotNull ("whereParserRegistry", whereParserRegistry);

      _selectParserRegistry = selectParserRegistry;
      _whereParserRegistry = whereParserRegistry;
    }

    public Tuple<MethodCall, ICriterion> Parse (MethodCallExpression methodCallExpression, ParseContext parseContext)
    {
      ArgumentUtility.CheckNotNull ("methodCallExpression", methodCallExpression);
      ArgumentUtility.CheckNotNull ("parseContext", parseContext);

      var methodArguments = methodCallExpression.Arguments.Skip (1).Where (argument => argument.NodeType != ExpressionType.Quote);
      var filteringArgument = methodCallExpression.Arguments.Skip (1).Where (argument => argument.NodeType == ExpressionType.Quote).SingleOrDefault();
      

      var methodCallExpressionParser = new MethodCallExpressionParser (_selectParserRegistry);
      var methodCall = methodCallExpressionParser.Parse (methodCallExpression, parseContext, methodArguments);

      ICriterion criterion = null;
      if (filteringArgument != null)
      {
        UnaryExpression filteringUnaryExpression = ParserUtility.GetTypedExpression<UnaryExpression> (
            filteringArgument,
            "filtering argument of " + methodCallExpression.Method.Name,
            parseContext.ExpressionTreeRoot);
        LambdaExpression ueLambda = ParserUtility.GetTypedExpression<LambdaExpression> (
            filteringUnaryExpression.Operand,
            "filtering argument of " + methodCallExpression.Method.Name,
            parseContext.ExpressionTreeRoot);

        criterion = _whereParserRegistry.GetParser (ueLambda.Body).Parse (ueLambda.Body, parseContext);
      }
      
      return Tuple.NewTuple (methodCall, criterion);
    }
  }
}
