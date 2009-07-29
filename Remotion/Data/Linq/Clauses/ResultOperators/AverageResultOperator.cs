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
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Remotion.Data.Linq.Clauses.StreamedData;
using Remotion.Utilities;

namespace Remotion.Data.Linq.Clauses.ResultOperators
{
  /// <summary>
  /// Represents the average part of a query. This is a result operator, operating on the whole result set of a query.
  /// </summary>
  /// <example>
  /// In C#, the "average" clause in the following example corresponds to a <see cref="AverageResultOperator"/>.
  /// <code>
  /// var query = (from s in Students
  ///              select s.ID).Average();
  /// </code>
  /// </example>
  public class AverageResultOperator : ValueFromSequenceResultOperatorBase
  {
    private Type _resultType;

    /// <summary>
    /// Initializes a new instance of the <see cref="AverageResultOperator"/>.
    /// </summary>
    /// <param name="resultType">The type of the value returned by this <see cref="AverageResultOperator"/>.</param>
    public AverageResultOperator (Type resultType)
    {
      ArgumentUtility.CheckNotNull ("resultType", resultType);
      ResultType = resultType;
    }

    /// <summary>
    /// Gets or sets the type of the value returned by this <see cref="AverageResultOperator"/>.
    /// </summary>
    /// <value>The result type of this <see cref="AverageResultOperator"/>.</value>
    public Type ResultType
    {
      get { return _resultType; }
      set { _resultType = ArgumentUtility.CheckNotNull ("value", value); }
    }

    public override ResultOperatorBase Clone (CloneContext cloneContext)
    {
      return new AverageResultOperator (ResultType);
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

      if (!ResultType.IsAssignableFrom (method.ReturnType))
      {
        var message = string.Format (
            "Cannot calculate the average of items of type '{0}' in memory so "
            + "that a value of type '{1}' is returned. Instead, a value of type '{2}' would be returned. This does not match the "
            + "ResultType of the AverageResultOperator.",
            typeof (T),
            ResultType,
            method.ReturnType);
        throw new NotSupportedException (message);
      }

      var result = method.Invoke (null, new[] { input.GetTypedSequence<T>() });
      return new StreamedValue (result, (StreamedValueInfo) GetOutputDataInfo (input.DataInfo));
    }

    public override IStreamedDataInfo GetOutputDataInfo (IStreamedDataInfo inputInfo)
    {
      ArgumentUtility.CheckNotNullAndType<StreamedSequenceInfo> ("inputInfo", inputInfo);
      return new StreamedScalarValueInfo (ResultType);
    }

    public override string ToString ()
    {
      return "Average()";
    }

  }
}