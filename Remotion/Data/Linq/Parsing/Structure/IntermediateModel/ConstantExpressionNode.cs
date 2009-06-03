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
using System.Linq.Expressions;
using Remotion.Data.Linq.Clauses;
using Remotion.Data.Linq.Parsing.ExpressionTreeVisitors;
using Remotion.Utilities;

namespace Remotion.Data.Linq.Parsing.Structure.IntermediateModel
{
  /// <summary>
  /// Represents a <see cref="ConstantExpression"/> which acts as a query source.
  /// </summary>
  public class ConstantExpressionNode : ExpressionNodeBase, IQuerySourceExpressionNode
  {
    public ConstantExpressionNode (Type querySourceType, object value, string fromIdentifierName)
    {
      ArgumentUtility.CheckNotNull ("querySourceType", querySourceType);
      ArgumentUtility.CheckNotNullOrEmpty ("fromIdentifierName", fromIdentifierName);

      QuerySourceType = querySourceType;
      QuerySourceElementType = GetQuerySourceElementType (querySourceType);
      Value = value;
      FromIdentifierName = fromIdentifierName;
    }

    private Type GetQuerySourceElementType (Type enumerableType)
    {
      try
      {
        return ReflectionUtility.GetAscribedGenericArguments (enumerableType, typeof (IEnumerable<>))[0];
      }
      catch (ArgumentTypeException)
      {
        throw new ArgumentTypeException ("expression", typeof (IEnumerable<>), enumerableType);
      }
    }

    public Type QuerySourceElementType { get; private set; }
    public Type QuerySourceType { get; set; }
    public object Value { get; private set; }
    public string FromIdentifierName { get; set; }

    public override Expression Resolve (ParameterExpression inputParameter, Expression expressionToBeResolved)
    {
      ArgumentUtility.CheckNotNull ("inputParameter", inputParameter);
      ArgumentUtility.CheckNotNull ("expressionToBeResolved", expressionToBeResolved);

      var identifierReference = new IdentifierReferenceExpression (this);
      return ReplacingVisitor.Replace (inputParameter, identifierReference, expressionToBeResolved);
    }

    public override IClause CreateClause (IClause previousClause)
    {
      if (previousClause != null)
      {
        throw new InvalidOperationException (
            "A ConstantExpressionNode cannot create a clause with a previous clause because it represents the end "
            + "of a query call chain. Set previousClause to null.");
      }

      return CreateClause ();
    }

    public MainFromClause CreateClause ()
    {
      return new MainFromClause (Expression.Parameter (QuerySourceElementType, FromIdentifierName), Expression.Constant (Value, QuerySourceType));
    }

    public override ParameterExpression CreateParameterForOutput ()
    {
      return Expression.Parameter (QuerySourceElementType, FromIdentifierName);
    }
  }
}