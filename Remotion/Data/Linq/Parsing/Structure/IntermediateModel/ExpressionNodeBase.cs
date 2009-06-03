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
using System.Reflection;
using Remotion.Data.Linq.Clauses;
using Remotion.Data.Linq.Clauses.ResultModifications;
using Remotion.Utilities;

namespace Remotion.Data.Linq.Parsing.Structure.IntermediateModel
{
  /// <summary>
  /// Base class for classes representing instantiations of <see cref="MethodCallExpression"/> for specific methods.
  /// </summary>
  public abstract class ExpressionNodeBase : IExpressionNode
  {
    protected static MethodInfo GetSupportedMethod<T> (Expression<Func<T>> methodCall)
    {
      var method = ParserUtility.GetMethod (methodCall);
      return method.IsGenericMethod ? method.GetGenericMethodDefinition() : method;
    }

    public abstract Expression Resolve (ParameterExpression inputParameter, Expression expressionToBeResolved);
    public abstract ParameterExpression CreateParameterForOutput ();
    public virtual IClause CreateClause (IClause previousClause) // TODO: Make abstract.
    {
      throw new NotImplementedException();
    }

    protected InvalidOperationException CreateResolveNotSupportedException ()
    {
      return
          new InvalidOperationException (
              GetType().Name + " does not support resolving of expressions, because it does not stream any data to the following node.");
    }

    protected InvalidOperationException CreateOutputParameterNotSupportedException ()
    {
      return
          new InvalidOperationException (
              GetType().Name + " does not support creating a parameter for its output because it does not stream any data to the following node.");
    }

    protected void CreateWhereClauseForResultModification (SelectClause selectClause, LambdaExpression optionalPredicate1)
    {
      LambdaExpression optionalPredicate = optionalPredicate1;
      if (optionalPredicate != null)
      {
        var whereClause = new WhereClause (selectClause.PreviousClause, optionalPredicate);
        selectClause.PreviousClause = whereClause;
      }
    }

    protected SelectClause GetSelectClauseForResultModification (IExpressionNode source, IClause previousClause)
    {
      ArgumentUtility.CheckNotNull ("previousClause", previousClause);
      ArgumentUtility.CheckNotNull ("source", source);

      var selectClause = previousClause as SelectClause;

      if (selectClause == null)
      {
        var selectorParameter = source.CreateParameterForOutput();
        selectClause = new SelectClause (previousClause, Expression.Lambda (selectorParameter, selectorParameter));
      }

      return selectClause;
    }
  }
}