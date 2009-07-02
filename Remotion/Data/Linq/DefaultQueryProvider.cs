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
using System.Linq;
using System.Linq.Expressions;
using Remotion.Data.Linq.Parsing.Structure;
using Remotion.Utilities;

namespace Remotion.Data.Linq
{
  /// <summary>
  /// Represents a default implementation of <see cref="QueryProviderBase"/> that is automatically used by <see cref="QueryableBase{T}"/>
  /// unless a custom <see cref="IQueryProvider"/> is specified.
  /// </summary>
  public class DefaultQueryProvider : QueryProviderBase
  {
    private readonly Type _queryableType;

    public DefaultQueryProvider (Type queryableType, IQueryExecutor executor)
        : base (ArgumentUtility.CheckNotNull ("executor", executor))
    {
      ArgumentUtility.CheckNotNull ("queryableType", queryableType);
      CheckQueryableType (queryableType);

      _queryableType = queryableType;
    }

    public DefaultQueryProvider (Type queryableType, IQueryExecutor executor, MethodCallExpressionNodeTypeRegistry nodeTypeRegistry)
        : base (ArgumentUtility.CheckNotNull ("executor", executor), ArgumentUtility.CheckNotNull ("nodeTypeRegistry", nodeTypeRegistry))
    {
      ArgumentUtility.CheckNotNull ("queryableType", queryableType);
      CheckQueryableType (queryableType);

      _queryableType = queryableType;
    }

    private void CheckQueryableType (Type queryableType)
    {
      ArgumentUtility.CheckTypeIsAssignableFrom ("queryableType", queryableType, typeof (IQueryable));
      
      if (!queryableType.IsGenericTypeDefinition)
      {
        var message = string.Format (
            "Expected the generic type definition of an implementation of IQueryable<T>, but was '{0}'.", 
            queryableType.FullName);
        throw new ArgumentTypeException (message, "queryableType", typeof (IQueryable<>), queryableType);
      }
      var genericArgumentCount = queryableType.GetGenericArguments().Length;
      if (genericArgumentCount != 1)
      {
        var message = string.Format (
            "Expected the generic type definition of an implementation of IQueryable<T> with exactly one type argument, but found {0} arguments.",
            genericArgumentCount);
        throw new ArgumentTypeException (message, "queryableType", typeof (IQueryable<>), queryableType);
      }
    }

    /// <summary>
    /// Gets the type of queryable created by this provider. This is the generic type definition of an implementation of <see cref="IQueryable{T}"/>
    /// (usually a subclass of <see cref="QueryableBase{T}"/>) with exactly one type argument.
    /// </summary>
    public Type QueryableType
    {
      get { return _queryableType; }
    }

    protected override IQueryable<T> CreateQueryable<T> (Expression expression)
    {
      return (IQueryable<T>) Activator.CreateInstance (QueryableType.MakeGenericType (typeof (T)), this, expression);
    }
  }
}