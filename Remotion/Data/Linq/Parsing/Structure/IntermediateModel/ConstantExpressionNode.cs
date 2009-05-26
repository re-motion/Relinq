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

namespace Remotion.Data.Linq.Parsing.Structure.IntermediateModel
{
  /// <summary>
  /// Represents a <see cref="ConstantExpression"/> which acts as a query source.
  /// </summary>
  public class ConstantExpressionNode : ExpressionNodeBase, IQuerySourceExpressionNode
  {
    public ConstantExpressionNode (Type querySourceType, object value)
    {
      ArgumentUtility.CheckNotNull ("querySourceType", querySourceType);
      ArgumentUtility.CheckNotNull ("value", value);
      
      QuerySourceType = querySourceType;
      Value = value;
    }

    public Type QuerySourceType { get; private set; }
    public object Value { get; private set; }

    public override Expression Resolve (ParameterExpression inputParameter, Expression expressionToBeResolved)
    {
      ArgumentUtility.CheckNotNull ("inputParameter", inputParameter);
      ArgumentUtility.CheckNotNull ("expressionToBeResolved", expressionToBeResolved);

      var identifierReference = new IdentifierReferenceExpression (this);
      return ReplacingExpressionTreeVisitor.Replace (inputParameter, identifierReference, expressionToBeResolved);
    }
  }
}