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
using System.Linq;
using Remotion.Linq.Clauses.StreamedData;
using Remotion.Utilities;

namespace Remotion.Linq.Clauses.ResultOperators
{
  /// <summary>
  /// Represents a <see cref="SequenceFromSequenceResultOperatorBase"/> that is executed on a sequence, returning a new sequence with the same
  /// item type as its result.
  /// </summary>
  public abstract class SequenceTypePreservingResultOperatorBase : SequenceFromSequenceResultOperatorBase
  {
    public sealed override IStreamedDataInfo GetOutputDataInfo (IStreamedDataInfo inputInfo)
    {
      var inputSequenceInfo = ArgumentUtility.CheckNotNullAndType<StreamedSequenceInfo> ("inputInfo", inputInfo);
      return GetOutputDataInfo(inputSequenceInfo);
    }

    protected StreamedSequenceInfo GetOutputDataInfo (StreamedSequenceInfo inputSequenceInfo)
    {
      ArgumentUtility.CheckNotNull ("inputSequenceInfo", inputSequenceInfo);
      return new StreamedSequenceInfo (typeof (IQueryable<>).MakeGenericType (inputSequenceInfo.ResultItemType), inputSequenceInfo.ItemExpression);
    }
  }
}
