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

namespace Remotion.Data.Linq.Clauses.StreamedData
{
  /// <summary>
  /// Holds the data needed to represent the output or input of a part of a query in memory. This is mainly used for 
  /// <see cref="ResultOperatorBase.ExecuteInMemory"/>. The data held by implementations of this interface can be either a value or a sequence.
  /// </summary>
  public interface IStreamedData
  {
    /// <summary>
    /// Gets an object describing the data held by this <see cref="IStreamedData"/> instance.
    /// </summary>
    /// <value>An <see cref="IStreamedDataInfo"/> object describing the data held by this <see cref="IStreamedData"/> instance.</value>
    IStreamedDataInfo DataInfo { get; }

    /// <summary>
    /// Gets the value held by this <see cref="IStreamedData"/> instance.
    /// </summary>
    /// <value>The value.</value>
    object Value { get; }
  }
}
