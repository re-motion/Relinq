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
      return new StreamedSingleValueInfo (dataType, _returnDefaultWhenEmpty);
    }

    public object ExecuteSingleQueryModel<T> (QueryModel queryModel, IQueryExecutor executor)
    {
      ArgumentUtility.CheckNotNull ("queryModel", queryModel);
      ArgumentUtility.CheckNotNull ("executor", executor);

      return executor.ExecuteSingle<T> (queryModel, _returnDefaultWhenEmpty);
    }

    public override bool Equals (IStreamedDataInfo obj)
    {
      return base.Equals (obj) && ((StreamedSingleValueInfo)obj)._returnDefaultWhenEmpty == _returnDefaultWhenEmpty;
    }

    public override int GetHashCode ()
    {
      return base.GetHashCode () ^ _returnDefaultWhenEmpty.GetHashCode();
    }
  }
}
