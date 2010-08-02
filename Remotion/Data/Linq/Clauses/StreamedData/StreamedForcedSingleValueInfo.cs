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
using Remotion.Data.Linq.Clauses.ResultOperators;

namespace Remotion.Data.Linq.Clauses.StreamedData
{
  /// <summary>
  /// Describes a single value streamed out of a <see cref="QueryModel"/> or <see cref="ResultOperatorBase"/>. A single value corresponds to one
  /// item from the result set, as produced by <see cref="FirstResultOperator"/> or <see cref="SingleResultOperator"/>, for instance.
  /// The <see cref="StreamedForcedSingleValueInfo"/> is set by the handler for the single result operator.
  /// </summary>
  public class StreamedForcedSingleValueInfo : StreamedSingleValueInfo
  {
    public StreamedForcedSingleValueInfo (Type dataType, bool returnDefaultWhenEmpty)
        : base(dataType, returnDefaultWhenEmpty)
    {

    }


  }
}