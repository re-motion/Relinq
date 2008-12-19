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
using Remotion.Data.Linq.DataObjectModel;
using Remotion.Utilities;

namespace Remotion.Data.Linq.Parsing.Details.WhereConditionParsing
{
  public class BinaryExpressionParser : IWhereConditionParser
  {
    private readonly WhereConditionParserRegistry _parserRegistry;

    public BinaryExpressionParser (WhereConditionParserRegistry parserRegistry)
    {
      ArgumentUtility.CheckNotNull ("parserRegistry", parserRegistry);
      _parserRegistry = parserRegistry;
    }

    public ICriterion Parse (BinaryExpression binaryExpression, ParseContext parseContext)
    {
      switch (binaryExpression.NodeType)
      {
        case ExpressionType.And:
        case ExpressionType.AndAlso:
          return CreateComplexCriterion (binaryExpression, ComplexCriterion.JunctionKind.And, parseContext);
        case ExpressionType.Or:
        case ExpressionType.OrElse:
          return CreateComplexCriterion (binaryExpression, ComplexCriterion.JunctionKind.Or, parseContext);
        case ExpressionType.Equal:
          return CreateBinaryCondition (binaryExpression, BinaryCondition.ConditionKind.Equal, parseContext);
        case ExpressionType.NotEqual:
          return CreateBinaryCondition (binaryExpression, BinaryCondition.ConditionKind.NotEqual, parseContext);
        case ExpressionType.GreaterThanOrEqual:
          return CreateBinaryCondition (binaryExpression, BinaryCondition.ConditionKind.GreaterThanOrEqual, parseContext);
        case ExpressionType.GreaterThan:
          return CreateBinaryCondition (binaryExpression, BinaryCondition.ConditionKind.GreaterThan, parseContext);
        case ExpressionType.LessThanOrEqual:
          return CreateBinaryCondition (binaryExpression, BinaryCondition.ConditionKind.LessThanOrEqual, parseContext);
        case ExpressionType.LessThan:
          return CreateBinaryCondition (binaryExpression, BinaryCondition.ConditionKind.LessThan, parseContext);
        default:
          throw ParserUtility.CreateParserException ("and, or, or comparison expression", binaryExpression.NodeType, 
              "binary expression in where condition", parseContext.ExpressionTreeRoot);
      }
    }

    ICriterion IWhereConditionParser.Parse (Expression expression, ParseContext parseContext)
    {
      return Parse ((BinaryExpression) expression, parseContext);
    }

    public bool CanParse (Expression expression)
    {
      return expression is BinaryExpression;
    }

    private ComplexCriterion CreateComplexCriterion 
      (BinaryExpression expression, ComplexCriterion.JunctionKind kind, ParseContext parseContext)
    {
      return new ComplexCriterion (
        _parserRegistry.GetParser (expression.Left).Parse (expression.Left, parseContext),
        _parserRegistry.GetParser (expression.Right).Parse (expression.Right, parseContext),
        kind
        );
    }

    private BinaryCondition CreateBinaryCondition 
      (BinaryExpression expression, BinaryCondition.ConditionKind kind, ParseContext parseContext)
    {
      return new BinaryCondition (
        _parserRegistry.GetParser (expression.Left).Parse (expression.Left, parseContext),
        _parserRegistry.GetParser (expression.Right).Parse (expression.Right, parseContext),
        kind
        );
    }
  }
}
