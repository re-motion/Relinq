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
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.Serialization;
using Remotion.Data.Linq.Parsing.Structure.IntermediateModel;
using Remotion.Data.Linq.Parsing.TreeEvaluation;
using Remotion.Utilities;

namespace Remotion.Data.Linq.Parsing.Structure
{
  public class MethodCallExpressionParser
  {
    private readonly ExpressionNodeTypeRegistry _nodeTypeRegistry;

    public MethodCallExpressionParser (ExpressionNodeTypeRegistry nodeTypeRegistry)
    {
      ArgumentUtility.CheckNotNull ("nodeTypeRegistry", nodeTypeRegistry);
      _nodeTypeRegistry = nodeTypeRegistry;
    }

    public IExpressionNode Parse (IExpressionNode source, MethodCallExpression expressionToParse)
    {
      ArgumentUtility.CheckNotNull ("source", source);
      ArgumentUtility.CheckNotNull ("expressionToParse", expressionToParse);

      Type nodeType = GetNodeType(expressionToParse);
      var additionalConstructorParameters = expressionToParse.Arguments
            .Skip (1) // skip the expression corresponding to the source argument
            .Select (expr => ConvertExpressionToParameterValue (expr)).ToArray();
      return CreateExpressionNode(nodeType, source, additionalConstructorParameters);
    }

    private Type GetNodeType (MethodCallExpression expressionToParse)
    {
      try
      {
        return _nodeTypeRegistry.GetNodeType (expressionToParse.Method);
      }
      catch (KeyNotFoundException ex)
      {
        string message = string.Format (
            "Could not parse expression '{0}': This overload of the method '{1}.{2}' is currently not supported, but you can register your own parser if needed.",
            expressionToParse, expressionToParse.Method.DeclaringType.FullName, expressionToParse.Method.Name);
        throw new ParserException (message, ex);
      }
    }

    private object ConvertExpressionToParameterValue (Expression expression)
    {
      var evaluatedExpression = PartialTreeEvaluator.EvaluateSubtree (expression);
      return evaluatedExpression.Value;
    }

    private IExpressionNode CreateExpressionNode (Type nodeType, IExpressionNode source, object[] additionalConstructorParameters)
    {
      return ExpressionNodeFactory.CreateExpressionNode (nodeType, source, additionalConstructorParameters);
    }
  }
}