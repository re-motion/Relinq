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
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Remotion.Linq.Clauses.StreamedData;
using Remotion.Linq.Utilities;

namespace Remotion.Linq.Clauses.ResultOperators
{
  /// <summary>
  /// Represents a calculation of an average value from the items returned by a query.
  /// This is a result operator, operating on the whole result set of a query.
  /// </summary>
  /// <example>
  /// In C#, the "Average" call in the following example corresponds to an <see cref="AverageResultOperator"/>.
  /// <code>
  /// var query = (from s in Students
  ///              select s.ID).Average();
  /// </code>
  /// </example>
  public class AverageResultOperator : ValueFromSequenceResultOperatorBase
  {
    public override ResultOperatorBase Clone (CloneContext cloneContext)
    {
      return new AverageResultOperator ();
    }

    public override StreamedValue ExecuteInMemory<T> (StreamedSequence input)
    {
      ArgumentUtility.CheckNotNull ("input", input);

      var method = typeof (Enumerable).GetMethod ("Average", BindingFlags.Public | BindingFlags.Static, null, new[] { typeof (IEnumerable<T>) }, null);
      if (method == null)
      {
        var message = string.Format ("Cannot calculate the average of objects of type '{0}' in memory.", typeof (T).FullName);
        throw new NotSupportedException (message);
      }

      Debug.Assert (GetOutputDataInfo (input.DataInfo).DataType == method.ReturnType, "ReturnType of method matches return type of this operator");

      var result = method.Invoke (null, new[] { input.GetTypedSequence<T>() });
      return new StreamedValue (result, (StreamedValueInfo) GetOutputDataInfo (input.DataInfo));
    }

    public override IStreamedDataInfo GetOutputDataInfo (IStreamedDataInfo inputInfo)
    {
      var sequenceInfo = ArgumentUtility.CheckNotNullAndType<StreamedSequenceInfo> ("inputInfo", inputInfo);

      Type resultType = GetResultType (sequenceInfo.ResultItemType);
      return new StreamedScalarValueInfo (resultType);
    }

    /// <inheritdoc />
    public override void TransformExpressions (Func<Expression, Expression> transformation)
    {
      //nothing to do here
    }

    public override string ToString ()
    {
      return "Average()";
    }

    private Type GetResultType (Type inputItemType)
    {
      if (inputItemType == typeof (int) || inputItemType == typeof (long))
        return typeof (double);
      else if (inputItemType == typeof (int?) || inputItemType == typeof (long?))
        return typeof (double?);
      else
        return inputItemType;
    }
  }
}
