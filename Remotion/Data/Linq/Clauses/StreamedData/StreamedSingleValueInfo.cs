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
using System.Reflection;
using Remotion.Data.Linq.Clauses.ResultOperators;
using Remotion.Data.Linq.EagerFetching;
using Remotion.Utilities;
using System.Linq;

namespace Remotion.Data.Linq.Clauses.StreamedData
{
  /// <summary>
  /// Describes a single value streamed out of a <see cref="QueryModel"/> or <see cref="ResultOperatorBase"/>. A single value corresponds to one
  /// item from the result set, as produced by <see cref="FirstResultOperator"/> or <see cref="SingleResultOperator"/>, for instance.
  /// </summary>
  public class StreamedSingleValueInfo : StreamedValueInfo
  {
    private static readonly MethodInfo s_executeMethod = (typeof (StreamedSingleValueInfo).GetMethod ("ExecuteSingleQueryModel"));

    private readonly bool _returnDefaultWhenEmpty;

    public StreamedSingleValueInfo (Type dataType, bool returnDefaultWhenEmpty)
        : base(dataType)
    {
      _returnDefaultWhenEmpty = returnDefaultWhenEmpty;
    }

    public bool ReturnDefaultWhenEmpty
    {
      get { return _returnDefaultWhenEmpty; }
    }

    public override IStreamedData ExecuteQueryModel (QueryModel queryModel, FetchRequestBase[] fetchRequests, IQueryExecutor executor)
    {
      ArgumentUtility.CheckNotNull ("queryModel", queryModel);
      ArgumentUtility.CheckNotNull ("fetchRequests", fetchRequests);
      ArgumentUtility.CheckNotNull ("executor", executor);

      var executeMethod = s_executeMethod.MakeGenericMethod (DataType);
      // wrap executeMethod into a delegate instead of calling Invoke in order to allow for exceptions that are bubbled up correctly
      var func = (Func<QueryModel, FetchRequestBase[], IQueryExecutor, object>)
          Delegate.CreateDelegate (typeof (Func<QueryModel, FetchRequestBase[], IQueryExecutor, object>), this, executeMethod);
      var result = func (queryModel, fetchRequests, executor);

      return new StreamedValue (result, this);
    }

    public T ExecuteSingleQueryModel<T> (QueryModel queryModel, FetchRequestBase[] fetchRequests, IQueryExecutor executor)
    {
      ArgumentUtility.CheckNotNull ("queryModel", queryModel);
      ArgumentUtility.CheckNotNull ("fetchRequests", fetchRequests);
      ArgumentUtility.CheckNotNull ("executor", executor);

      return executor.ExecuteSingle<T> (queryModel, fetchRequests, _returnDefaultWhenEmpty);
    }
  }
}