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
using System.Linq;
using Remotion.Data.Linq.Clauses;
using Remotion.Data.Linq.Clauses.ResultOperators;

namespace Remotion.Data.UnitTests.Linq.Clauses.ResultOperators
{
  public class TestResultOperator : ResultOperatorBase
  {
    public TestResultOperator (IExecutionStrategy executionStrategy)
        : base(executionStrategy)
    {
    }

    public override IStreamedData ExecuteInMemory (IStreamedData input)
    {
      throw new NotImplementedException();
    }

    public override Type GetResultType (Type inputResultType)
    {
      throw new NotImplementedException();
    }

    public override ResultOperatorBase Clone (CloneContext cloneContext)
    {
      throw new NotImplementedException();
    }

    public new IStreamedData InvokeGenericExecuteMethod<TSource, TResult> (IStreamedData input, Func<TSource, TResult> genericMethodCaller)
      where TSource : IStreamedData
      where TResult : IStreamedData 
    {
      return base.InvokeGenericExecuteMethod (input, genericMethodCaller);
    }

    public StreamedSequence DistinctExecuteMethod<T> (StreamedSequence arg)
    {
      var currentSequence = arg.GetCurrentSequenceInfo<T>();
      return new StreamedSequence (currentSequence.Sequence.Distinct (), currentSequence.ItemExpression);
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