// This file is part of the re-linq project (relinq.codeplex.com)
// Copyright (C) 2005-2009 rubicon informationstechnologie gmbh, www.rubicon.eu
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
using Remotion.Linq.Clauses.StreamedData;
using Remotion.Linq.Utilities;

namespace Remotion.Linq.Clauses.ResultOperators
{
  /// <summary>
  /// Represents a check whether any items are returned by a query.
  /// This is a result operator, operating on the whole result set of a query.
  /// </summary>
  /// <remarks>
  /// "Any" query methods taking a predicate are represented as into a combination of a <see cref="WhereClause"/> and an 
  /// <see cref="AnyResultOperator"/>.
  /// </remarks>
  /// <example>
  /// In C#, the "Any" call in the following example corresponds to an <see cref="AnyResultOperator"/>.
  /// <code>
  /// var result = (from s in Students
  ///              select s).Any();
  /// </code>
  /// </example>
  public class AnyResultOperator : ValueFromSequenceResultOperatorBase
  {
    /// <inheritdoc />
    public override StreamedValue ExecuteInMemory<T> (StreamedSequence input)
    {
      ArgumentUtility.CheckNotNull ("input", input);

      var sequence = input.GetTypedSequence<T> ();
      var result = sequence.Any ();
      return new StreamedValue (result, (StreamedValueInfo) GetOutputDataInfo (input.DataInfo));
    }

    /// <inheritdoc />
    public override ResultOperatorBase Clone (CloneContext cloneContext)
    {
      return new AnyResultOperator ();
    }

    /// <inheritdoc />
    public override IStreamedDataInfo GetOutputDataInfo (IStreamedDataInfo inputInfo)
    {
      ArgumentUtility.CheckNotNullAndType<StreamedSequenceInfo> ("inputInfo", inputInfo);
      return new StreamedScalarValueInfo (typeof (bool));
    }

    /// <inheritdoc />
    public override void TransformExpressions (Func<Expression, Expression> transformation)
    {
      //nothing to do here
    }

    /// <inheritdoc />
    public override string ToString ()
    {
      return "Any()";
    }
  }
}
