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
using System.Linq.Expressions;
using Remotion.Data.Linq.DataObjectModel;
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
      ArgumentUtility.CheckNotNull ("methodCallExpression", methodCallExpression);
      ArgumentUtility.CheckNotNull ("parseContext", parseContext);

      if (CanParse (methodCallExpression))
      {
        ParserUtility.CheckNumberOfArguments (methodCallExpression, "Contains", 2);
        return CreateContains (methodCallExpression.Arguments[0], methodCallExpression.Arguments[1], parseContext);
      }
      else
      {
        throw ParserUtility.CreateParserException (
            "Contains with expression",
            methodCallExpression.Method.Name,
            "method call expression in where condition");
      }
    }

    ICriterion IWhereConditionParser.Parse (Expression expression, ParseContext parseContext)
    {
      return Parse ((MethodCallExpression) expression, parseContext);
    }

    public bool CanParse (Expression expression)
    {
      var methodCallExpression = expression as MethodCallExpression;
      return methodCallExpression != null && methodCallExpression.Method.Name == "Contains" && methodCallExpression.Method.IsGenericMethod;
    }

    private BinaryCondition CreateContains (Expression expression, Expression itemExpression, ParseContext parseContext)
    {
      return new BinaryCondition (
        _parserRegistry.GetParser (expression).Parse (expression, parseContext),
        _parserRegistry.GetParser (itemExpression).Parse (itemExpression, parseContext),
        BinaryCondition.ConditionKind.Contains);
    }
  }
}
