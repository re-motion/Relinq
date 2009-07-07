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
using Remotion.Data.Linq.Backend.DataObjectModel;
using Remotion.Utilities;

namespace Remotion.Data.Linq.Parsing.Details.WhereConditionParsing
{
  public class LikeParser : IWhereConditionParser
  {
    private readonly WhereConditionParserRegistry _parserRegistry;

    public LikeParser (WhereConditionParserRegistry parserRegistry)
    {
      ArgumentUtility.CheckNotNull ("parserRegistry", parserRegistry);
      _parserRegistry = parserRegistry;
    }

    public ICriterion Parse (MethodCallExpression methodCallExpression, ParseContext parseContext)
    {
      if (methodCallExpression.Method.Name == "StartsWith")
      {
        ParserUtility.CheckNumberOfArguments (methodCallExpression, "StartsWith", 1);
        ParserUtility.CheckParameterType<ConstantExpression> (methodCallExpression, "StartsWith", 0);
        return CreateLike (methodCallExpression, ((ConstantExpression) methodCallExpression.Arguments[0]).Value + "%", parseContext);
      }
      else if (methodCallExpression.Method.Name == "EndsWith")
      {
        ParserUtility.CheckNumberOfArguments (methodCallExpression, "EndsWith", 1);
        ParserUtility.CheckParameterType<ConstantExpression> (methodCallExpression, "EndsWith", 0);
        return CreateLike (methodCallExpression, "%" + ((ConstantExpression) methodCallExpression.Arguments[0]).Value, parseContext);
      }
      else if (methodCallExpression.Method.Name == "Contains" && !methodCallExpression.Method.IsGenericMethod)
      {
        ParserUtility.CheckNumberOfArguments (methodCallExpression, "Contains", 1);
        ParserUtility.CheckParameterType<ConstantExpression> (methodCallExpression, "Contains", 0);
        return CreateLike (methodCallExpression, "%" + ((ConstantExpression) methodCallExpression.Arguments[0]).Value + "%", parseContext);
      }
      throw ParserUtility.CreateParserException (
          "StartsWith, EndsWith, Contains with no expression", methodCallExpression.Method.Name, "method call expression in where condition");
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
        if (methodCallExpression.Method.Name == "StartsWith" ||
          methodCallExpression.Method.Name == "EndsWith" ||
          (methodCallExpression.Method.Name == "Contains" && !methodCallExpression.Method.IsGenericMethod))
          return true;
      }
      return false;
    }

    private BinaryCondition CreateLike (MethodCallExpression expression, string pattern, ParseContext parseContext)
    {
      return new BinaryCondition (_parserRegistry.GetParser (expression.Object).Parse (expression.Object, parseContext), new Constant (pattern), BinaryCondition.ConditionKind.Like);
    }
  }
}
