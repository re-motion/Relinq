// This file is part of the re-motion Core Framework (www.re-motion.org)
// Copyright (C) 2005-2009 rubicon informationstechnologie gmbh, www.rubicon.eu
// 
// The re-motion Core Framework is free software; you can redistribute it 
// and/or modify it under the terms of the GNU Lesser General Public License 
// as published by the Free Software Foundation; either version 2.1 of the 
// License, or (at your option) any later version.
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
using Remotion.Linq.Clauses;
using Remotion.Linq.Clauses.StreamedData;
using Remotion.Linq.Utilities;

namespace Remotion.Linq.UnitTests.Linq.Core.Clauses.ResultOperators
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
      return new StreamedSequenceInfo (typeof (IEnumerable<>).MakeGenericType (inputSequenceInfo.ItemExpression.Type), inputSequenceInfo.ItemExpression);
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
