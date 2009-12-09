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
using System.Linq;
using Remotion.Data.Linq.Clauses.StreamedData;

namespace Remotion.Data.Linq.Clauses.ResultOperators
{
  /// <summary>
  /// Represents taking the single item returned by a query.
  /// This is a result operator, operating on the whole result set of a query.
  /// </summary>
  /// <example>
  /// In C#, the "Single" call in the following example corresponds to a <see cref="SingleResultOperator"/>.
  /// <code>
  /// var query = (from s in Students
  ///              select s).Single();
  /// </code>
  /// </example>
  public class SingleResultOperator : ChoiceResultOperatorBase
  {
    /// <summary>
    /// Initializes a new instance of the <see cref="SingleResultOperator"/>.
    /// </summary>
    /// <param name="returnDefaultWhenEmpty">The flag defines if a default expression should be regarded.</param>
    public SingleResultOperator (bool returnDefaultWhenEmpty)
      : base (returnDefaultWhenEmpty)
    {
    }

    public override ResultOperatorBase Clone (CloneContext cloneContext)
    {
      return new SingleResultOperator (ReturnDefaultWhenEmpty);
    }

    public override StreamedValue ExecuteInMemory<T> (StreamedSequence input)
    {
      var sequence = input.GetTypedSequence<T> ();
      T result = ReturnDefaultWhenEmpty ? sequence.SingleOrDefault () : sequence.Single ();
      return new StreamedValue (result, (StreamedValueInfo) GetOutputDataInfo (input.DataInfo));

    }

    public override string ToString ()
    {
      if (ReturnDefaultWhenEmpty)
        return "SingleOrDefault()";
      else
        return "Single()";
    }
  }
}
