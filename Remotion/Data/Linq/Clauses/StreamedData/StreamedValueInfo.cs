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
using System.Reflection;
using Remotion.Utilities;

namespace Remotion.Data.Linq.Clauses.StreamedData
{
  /// <summary>
  /// Describes a single or scalar value streamed out of a <see cref="QueryModel"/> or <see cref="ResultOperatorBase"/>.
  /// </summary>
  public abstract class StreamedValueInfo : IStreamedDataInfo
  {
    protected StreamedValueInfo (Type dataType)
    {
      ArgumentUtility.CheckNotNull ("dataType", dataType);
      DataType = dataType;
    }

    /// <summary>
    /// Gets the type of the data described by this <see cref="IStreamedDataInfo"/> instance. This is the type of the streamed value, or 
    /// <see cref="object"/> if the value is <see langword="null" />.
    /// </summary>
    public Type DataType { get; private set; }

    public abstract IStreamedData ExecuteQueryModel (QueryModel queryModel, IQueryExecutor executor);

    /// <summary>
    /// Takes the given <paramref name="genericMethodDefinition"/> and instantiates it, substituting its generic parameter with the value
    /// type of the value held by this object. The method must have exactly one generic parameter.
    /// </summary>
    /// <param name="genericMethodDefinition">The generic method definition to instantiate.</param>
    /// <returns>
    /// A closed generic instantiation of <paramref name="genericMethodDefinition"/> with this object's value type substituted for
    /// the generic parameter.
    /// </returns>
    public MethodInfo MakeClosedGenericExecuteMethod (MethodInfo genericMethodDefinition)
    {
      ArgumentUtility.CheckNotNull ("genericMethodDefinition", genericMethodDefinition);

      if (!genericMethodDefinition.IsGenericMethodDefinition)
        throw new ArgumentException ("GenericMethodDefinition must be a generic method definition.", "genericMethodDefinition");

      if (genericMethodDefinition.GetGenericArguments ().Length != 1)
        throw new ArgumentException ("GenericMethodDefinition must have exactly one generic parameter.", "genericMethodDefinition");

      return genericMethodDefinition.MakeGenericMethod (DataType);
    }
  }
}
