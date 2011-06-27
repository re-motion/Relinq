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
  /// Represents counting the number of items returned by a query.
  /// This is a result operator, operating on the whole result set of a query.
  /// </summary>
  /// <remarks>
  /// "Count" query methods taking a predicate are represented as a combination of a <see cref="WhereClause"/> and a <see cref="CountResultOperator"/>.
  /// </remarks>  /// <example>
  /// In C#, the "Count" call in the following example corresponds to a <see cref="CountResultOperator"/>.
  /// <code>
  /// var query = (from s in Students
  ///              select s).Count();
  /// </code>
  /// </example>
  public class CountResultOperator : ValueFromSequenceResultOperatorBase
  {
    public override ResultOperatorBase Clone (CloneContext cloneContext)
    {
      return new CountResultOperator();
    }

    public override StreamedValue ExecuteInMemory<T> (StreamedSequence input)
    {
      var sequence = input.GetTypedSequence<T> ();
      var result = sequence.Count ();
      return new StreamedValue (result, (StreamedValueInfo) GetOutputDataInfo (input.DataInfo));
    }

    public override IStreamedDataInfo GetOutputDataInfo (IStreamedDataInfo inputInfo)
    {
      ArgumentUtility.CheckNotNullAndType<StreamedSequenceInfo> ("inputInfo", inputInfo);
      return new StreamedScalarValueInfo (typeof (int));
    }

    /// <inheritdoc />
    public override void TransformExpressions (Func<Expression, Expression> transformation)
    {
      //nothing to do here
    }

    public override string ToString ()
    {
      return "Count()";
    }
  }
}
