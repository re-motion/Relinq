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
using System.Collections;
using System.Linq.Expressions;
using Remotion.Data.Linq.Parsing.ExpressionTreeVisitors;
using Remotion.Data.Linq.Parsing.Structure.IntermediateModel;
using Remotion.Utilities;
using System.Collections.Generic;

namespace Remotion.Data.Linq.Parsing.Structure
{
  /// <summary>
  /// Parses an expression tree into a chain of <see cref="IExpressionNode"/> objects.
  /// </summary>
  public class ExpressionTreeParser
  {
    private readonly ExpressionNodeTypeRegistry _nodeTypeRegistry;

    public ExpressionTreeParser (ExpressionNodeTypeRegistry nodeTypeRegistry)
    {
      ArgumentUtility.CheckNotNull ("nodeTypeRegistry", nodeTypeRegistry);
      _nodeTypeRegistry = nodeTypeRegistry;
    }

    public IExpressionNode Parse (Expression expression)
    {
      ArgumentUtility.CheckNotNull ("expression", expression);

      var methodCallExpression = expression as MethodCallExpression;
      if (methodCallExpression != null)
        return ParseMethodCallExpression (methodCallExpression);
      else
      {
        var constantExpression = PartialTreeEvaluatingVisitor.EvaluateSubtree (expression);
        return new ConstantExpressionNode (constantExpression.Type, constantExpression.Value, "TODO"); // TODO: Implement algorithm and test
      }
    }


    private IExpressionNode ParseMethodCallExpression (MethodCallExpression methodCallExpression)
    {
      var parser = new MethodCallExpressionParser (_nodeTypeRegistry);
      var source = Parse (methodCallExpression.Arguments[0]);
      return parser.Parse (source, methodCallExpression);
    }
  }
}