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
using Remotion.Data.Linq.Parsing.Structure.IntermediateModel;
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
      Type nodeType = _nodeTypeRegistry.GetNodeType (expressionToParse.Method);
      return ExpressionNodeFactory.CreateExpressionNode (nodeType, source, expressionToParse.Arguments.Skip (1).Select (expr => StripUnaryExpression (expr)).ToArray());
    }

    private Expression StripUnaryExpression (Expression expression)
    {
      throw new NotImplementedException();
    }
  }
}