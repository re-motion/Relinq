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
using System.Reflection;
using Remotion.Linq.Clauses.ResultOperators;
using Remotion.Linq.Utilities;

namespace Remotion.Linq.Clauses.StreamedData
{
  /// <summary>
  /// Describes a scalar value streamed out of a <see cref="QueryModel"/> or <see cref="ResultOperatorBase"/>. A scalar value corresponds to a
  /// value calculated from the result set, as produced by <see cref="CountResultOperator"/> or <see cref="ContainsResultOperator"/>, for instance.
  /// </summary>
  public class StreamedScalarValueInfo : StreamedValueInfo
  {
    private static readonly MethodInfo s_executeMethod = (typeof (StreamedScalarValueInfo).GetMethod ("ExecuteScalarQueryModel"));

    public StreamedScalarValueInfo (Type dataType)
        : base(dataType)
    {
    }

    public override IStreamedData ExecuteQueryModel (QueryModel queryModel, IQueryExecutor executor)
    {
      ArgumentUtility.CheckNotNull ("queryModel", queryModel);
      ArgumentUtility.CheckNotNull ("executor", executor);

      var executeMethod = s_executeMethod.MakeGenericMethod (DataType);
      
      // wrap executeMethod into a delegate instead of calling Invoke in order to allow for exceptions that are bubbled up correctly
      var func = (Func<QueryModel, IQueryExecutor, object>) 
          Delegate.CreateDelegate (typeof (Func<QueryModel, IQueryExecutor, object>), this, executeMethod);
      var result = func (queryModel, executor);

      return new StreamedValue (result, this);
    }

    protected override StreamedValueInfo CloneWithNewDataType (Type dataType)
    {
      ArgumentUtility.CheckNotNull ("dataType", dataType);
      return new StreamedScalarValueInfo (dataType);
    }

    public object ExecuteScalarQueryModel<T> (QueryModel queryModel, IQueryExecutor executor)
    {
      ArgumentUtility.CheckNotNull ("queryModel", queryModel);
      ArgumentUtility.CheckNotNull ("executor", executor);

      return executor.ExecuteScalar<T> (queryModel);
    }
  }
}
