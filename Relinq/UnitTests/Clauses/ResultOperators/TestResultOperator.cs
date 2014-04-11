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
using System.Collections.Generic;
using System.Linq;
using Remotion.Linq.Clauses;
using Remotion.Linq.Clauses.StreamedData;
using Remotion.Utilities;

namespace Remotion.Linq.UnitTests.Clauses.ResultOperators
{
  public class TestResultOperator : ResultOperatorBase
  {
    public override IStreamedData ExecuteInMemory (IStreamedData input)
    {
      throw new NotImplementedException();
    }

    public override IStreamedDataInfo GetOutputDataInfo (IStreamedDataInfo inputInfo)
    {
      var inputSequenceInfo = ArgumentUtility.CheckNotNullAndType<StreamedSequenceInfo>("inputInfo", inputInfo);
      return new StreamedSequenceInfo (typeof (IEnumerable<>).MakeGenericType (inputSequenceInfo.ResultItemType), inputSequenceInfo.ItemExpression);
    }

    public override ResultOperatorBase Clone (CloneContext cloneContext)
    {
      throw new NotImplementedException();
    }

    public override void TransformExpressions (Func<System.Linq.Expressions.Expression, System.Linq.Expressions.Expression> transformation)
    {
      throw new NotImplementedException ();
    }

    public new TResult InvokeGenericExecuteMethod<TSource, TResult> (IStreamedData input, Func<TSource, TResult> genericMethodCaller)
      where TSource : IStreamedData
      where TResult : IStreamedData 
    {
      return base.InvokeGenericExecuteMethod (input, genericMethodCaller);
    }

    public new void CheckSequenceItemType (StreamedSequenceInfo sequenceInfo, Type expectedItemType)
    {
      base.CheckSequenceItemType (sequenceInfo, expectedItemType);
    }

    public StreamedSequence DistinctExecuteMethod<T> (StreamedSequence input)
    {
      var currentSequence = input.GetTypedSequence<T>();
      return new StreamedSequence (currentSequence.Distinct (), (StreamedSequenceInfo) GetOutputDataInfo (input.DataInfo));
    }

    // ReSharper disable UnusedTypeParameter
    public StreamedSequence ThrowingExecuteMethod<T> (StreamedSequence arg)
    {
      throw new NotImplementedException ("Test");
    }

    public StreamedValue InvalidExecuteInMemory_TooManyGenericParameters<T1, T2> (StreamedSequence input)
    {
      throw new NotImplementedException();
    }

    public StreamedValue ExecuteMethodWithNonMatchingArgumentType<T> (StreamedValue arg)
    {
      throw new NotImplementedException ("Test");
    }

    public StreamedSequence NonGenericExecuteMethod (StreamedSequence arg)
    {
      throw new NotImplementedException ();
    }

    internal StreamedSequence NonPublicExecuteMethod<T> (StreamedSequence arg)
    {
      throw new NotImplementedException ();
    }
    // ReSharper restore UnusedTypeParameter
  }
}
