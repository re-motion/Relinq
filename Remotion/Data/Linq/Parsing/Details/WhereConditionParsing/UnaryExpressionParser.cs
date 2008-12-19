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
using Remotion.Data.Linq.DataObjectModel;
using Remotion.Utilities;

namespace Remotion.Data.Linq.Parsing.Details.WhereConditionParsing
{
  public class UnaryExpressionParser : IWhereConditionParser
  {
    private readonly WhereConditionParserRegistry _parserRegistry;
    
    public UnaryExpressionParser (WhereConditionParserRegistry parserRegistry)
    {
      ArgumentUtility.CheckNotNull ("parserRegistry", parserRegistry);
      _parserRegistry = parserRegistry;
    }

    public ICriterion Parse (UnaryExpression unaryExpression, ParseContext parseContext)
    {
      switch (unaryExpression.NodeType)
      {
        case ExpressionType.Not:
          return new NotCriterion (_parserRegistry.GetParser (unaryExpression.Operand).Parse (unaryExpression.Operand, parseContext));
        case ExpressionType.Convert: // Convert is simply ignored ATM, change to more sophisticated logic when needed
          return (_parserRegistry.GetParser (unaryExpression.Operand).Parse (unaryExpression.Operand, parseContext));
        default:
          throw ParserUtility.CreateParserException ("not or convert expression", unaryExpression.NodeType, "unary expression in where condition",
              parseContext.ExpressionTreeRoot);
      }
    }

    public bool CanParse(Expression expression)
    {
      return expression is UnaryExpression;
    }

    public ICriterion Parse(Expression expression, ParseContext parseContext)
    {
      return Parse ((UnaryExpression) expression, parseContext);
    }
  }
}
