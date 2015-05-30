// Copyright (c) rubicon IT GmbH, www.rubicon.eu
//
// See the NOTICE file distributed with this work for additional information
// regarding copyright ownership.  rubicon licenses this file to you under 
// the Apache License, Version 2.0 (the "License"); you may not use this 
// file except in compliance with the License.  You may obtain a copy of the 
// License at
//
//   http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software 
// distributed under the License is distributed on an "AS IS" BASIS, WITHOUT 
// WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.  See the 
// License for the specific language governing permissions and limitations
// under the License.
// 

using System;
using Remotion.Linq.Clauses.StreamedData;
using Remotion.Utilities;

namespace Remotion.Linq.Development.UnitTesting.Clauses.StreamedData
{
  public sealed class TestStreamedValueInfo : IStreamedDataInfo
  {
    private readonly Type _dataType;

    public TestStreamedValueInfo (Type dataType)
    {
      ArgumentUtility.CheckNotNull ("dataType", dataType);

      _dataType = dataType;
    }

    public Type DataType
    {
      get { return _dataType; }
    }

    public IStreamedData ExecuteQueryModel (QueryModel queryModel, IQueryExecutor executor)
    {
      throw new NotImplementedException();
    }

    public IStreamedDataInfo AdjustDataType (Type dataType)
    {
      return new TestStreamedValueInfo (dataType);
    }

    public override bool Equals (object obj)
    {
      return Equals (obj as IStreamedDataInfo);
    }

    public bool Equals (IStreamedDataInfo obj)
    {
      if (obj == null)
        return false;
    
      if (GetType () != obj.GetType ())
        return false;

      var other = (TestStreamedValueInfo) obj;
      return _dataType.Equals (other._dataType);
    }

    public override int GetHashCode ()
    {
      return _dataType.GetHashCode();
    }
  }
}
