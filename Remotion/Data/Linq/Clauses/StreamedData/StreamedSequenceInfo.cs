// This file is part of the re-motion Core Framework (www.re-motion.org)
// Copyright (C) 2005-2009 rubicon informationstechnologie gmbh, www.rubicon.eu
// 
// The re-motion Core Framework is free software; you can redistribute it 
// and/or modify it under the terms of the GNU Lesser General Public License 
// as published by the Free Software Foundation; either version 2.1 of the 
// License, or (at your option) any later version.
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
using System.Reflection;
using Remotion.Data.Linq.Utilities;
using System.Collections;
using System.Linq;

namespace Remotion.Data.Linq.Clauses.StreamedData
{
  /// <summary>
  /// Describes sequence data streamed out of a <see cref="QueryModel"/> or <see cref="ResultOperatorBase"/>. Sequence data can be held by an object
  /// implementing <see cref="IEnumerable{T}"/>, and its items are described via a <see cref="ItemExpression"/>.
  /// </summary>
  public class StreamedSequenceInfo : IStreamedDataInfo
  {
    private static readonly MethodInfo s_executeMethod = (typeof (StreamedSequenceInfo).GetMethod ("ExecuteCollectionQueryModel"));

    public StreamedSequenceInfo (Type dataType, Expression itemExpression)
    {
      ArgumentUtility.CheckNotNull ("dataType", dataType);
      ArgumentUtility.CheckNotNull ("itemExpression", itemExpression);

      if (!typeof (IEnumerable).IsAssignableFrom (dataType))
        throw new InvalidOperationException (string.Format ("Data type '{0}' does not implement IEnumerable.", dataType.Name));

      var resultItemType = ReflectionUtility.TryGetItemTypeOfIEnumerable (dataType);
      if (itemExpression.Type != resultItemType)
      {
        var message = string.Format ("ItemExpression is of type {0} but should be {1}.", itemExpression.Type, resultItemType);
        throw new ArgumentTypeException (message, "itemExpression", resultItemType, itemExpression.Type);
      }

      DataType = dataType;
      ItemExpression = itemExpression;
    }

    /// <summary>
    /// Gets the type of the data described by this <see cref="StreamedSequenceInfo"/> instance. This is a type implementing
    /// <see cref="IEnumerable{T}"/>, where <c>T</c> is instantiated with a concrete type.
    /// </summary>
    public Type DataType { get; private set; }

    /// <summary>
    /// Gets an expression that describes the structure of the items held by the sequence described by this object.
    /// </summary>
    /// <value>The expression for the sequence's items.</value>
    public Expression ItemExpression { get; private set; }

    /// <summary>
    /// Takes the given <paramref name="genericMethodDefinition"/> and instantiates it, substituting its generic parameter with the 
    /// item type of the sequence described by this object. The method must have exactly one generic parameter.
    /// </summary>
    /// <param name="genericMethodDefinition">The generic method definition to instantiate.</param>
    /// <returns>
    /// A closed generic instantiation of <paramref name="genericMethodDefinition"/> with this object's item type substituted for
    /// the generic parameter.
    /// </returns>
    public MethodInfo MakeClosedGenericExecuteMethod (MethodInfo genericMethodDefinition)
    {
      ArgumentUtility.CheckNotNull ("genericMethodDefinition", genericMethodDefinition);

      if (!genericMethodDefinition.IsGenericMethodDefinition)
        throw new ArgumentException ("GenericMethodDefinition must be a generic method definition.", "genericMethodDefinition");

      if (genericMethodDefinition.GetGenericArguments ().Length != 1)
        throw new ArgumentException ("GenericMethodDefinition must have exactly one generic parameter.", "genericMethodDefinition");

      return genericMethodDefinition.MakeGenericMethod (ItemExpression.Type);
    }

    public IStreamedData ExecuteQueryModel (QueryModel queryModel, IQueryExecutor executor)
    {
      ArgumentUtility.CheckNotNull ("queryModel", queryModel);
      ArgumentUtility.CheckNotNull ("executor", executor);

      var executeMethod = s_executeMethod.MakeGenericMethod (ItemExpression.Type);

      // wrap executeMethod into a delegate instead of calling Invoke in order to allow for exceptions that are bubbled up correctly
      var func = (Func<QueryModel, IQueryExecutor, IEnumerable>)
          Delegate.CreateDelegate (typeof (Func<QueryModel, IQueryExecutor, IEnumerable>), this, executeMethod);
      var result = func (queryModel, executor).AsQueryable ();

      return new StreamedSequence (result, new StreamedSequenceInfo (result.GetType(), ItemExpression));
    }

    public IEnumerable ExecuteCollectionQueryModel<T> (QueryModel queryModel, IQueryExecutor executor)
    {
      ArgumentUtility.CheckNotNull ("queryModel", queryModel);
      ArgumentUtility.CheckNotNull ("executor", executor);

      return executor.ExecuteCollection<T> (queryModel);
    }
  }
}
