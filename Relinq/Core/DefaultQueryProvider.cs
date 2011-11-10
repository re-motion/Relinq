// This file is part of the re-linq project (relinq.codeplex.com)
// Copyright (c) rubicon IT GmbH, www.rubicon.eu
// 
// re-linq is free software; you can redistribute it and/or modify it under 
// the terms of the GNU Lesser General Public License as published by the 
// Free Software Foundation; either version 2.1 of the License, 
// or (at your option) any later version.
// 
// re-linq is distributed in the hope that it will be useful, 
// but WITHOUT ANY WARRANTY; without even the implied warranty of 
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the 
// GNU Lesser General Public License for more details.
// 
// You should have received a copy of the GNU Lesser General Public License
// along with re-linq; if not, see http://www.gnu.org/licenses.
// 
using System;
using System.Linq;
using System.Linq.Expressions;
using Remotion.Linq.Parsing.Structure;
using Remotion.Linq.Utilities;

namespace Remotion.Linq
{
  /// <summary>
  /// Represents a default implementation of <see cref="QueryProviderBase"/> that is automatically used by <see cref="QueryableBase{T}"/>
  /// unless a custom <see cref="IQueryProvider"/> is specified. The <see cref="DefaultQueryProvider"/> executes queries by parsing them into
  /// an instance of type <see cref="QueryModel"/>, which is then passed to an implementation of <see cref="IQueryExecutor"/> to obtain the
  /// result set.
  /// </summary>
  public class DefaultQueryProvider : QueryProviderBase
  {
    private readonly Type _queryableType;

    /// <summary>
    /// Initializes a new instance of <see cref="DefaultQueryProvider"/> using a custom <see cref="IQueryParser"/>.
    /// </summary>
    /// <param name="queryableType">
    ///   A type implementing <see cref="IQueryable{T}"/>. This type is used to construct the chain of query operators. Must be a generic type
    ///   definition.
    /// </param>
    /// <param name="queryParser">The <see cref="IQueryParser"/> used to parse queries. Specify an instance of 
    ///   <see cref="Parsing.Structure.QueryParser"/> for default behavior. See also <see cref="QueryParser.CreateDefault"/>.</param>
    /// <param name="executor">The <see cref="IQueryExecutor"/> used to execute queries against a specific query backend.</param>
    public DefaultQueryProvider (Type queryableType, IQueryParser queryParser, IQueryExecutor executor)
      : base (ArgumentUtility.CheckNotNull ("queryParser", queryParser), ArgumentUtility.CheckNotNull ("executor", executor))
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

    /// <summary>
    /// Creates a new <see cref="IQueryable"/> (of type <see cref="QueryableType"/> with <typeparamref name="T"/> as its generic argument) that
    /// represents the query defined by <paramref name="expression"/> and is able to enumerate its results.
    /// </summary>
    /// <typeparam name="T">The type of the data items returned by the query.</typeparam>
    /// <param name="expression">An expression representing the query for which a <see cref="IQueryable{T}"/> should be created.</param>
    /// <returns>An <see cref="IQueryable{T}"/> that represents the query defined by <paramref name="expression"/>.</returns>
    public override IQueryable<T> CreateQuery<T> (Expression expression)
    {
      return (IQueryable<T>) Activator.CreateInstance (QueryableType.MakeGenericType (typeof (T)), this, expression);
    }
  }
}
