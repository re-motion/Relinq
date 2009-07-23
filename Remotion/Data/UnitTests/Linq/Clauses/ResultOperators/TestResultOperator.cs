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

    public override object ExecuteInMemory (object input)
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

    public new IExecuteInMemoryData InvokeGenericExecuteMethod<TSource, TResult> (IExecuteInMemoryData input, Func<TSource, TResult> genericMethodCaller)
      where TSource : IExecuteInMemoryData
      where TResult : IExecuteInMemoryData 
    {
      return base.InvokeGenericExecuteMethod (input, genericMethodCaller);
    }

    public ExecuteInMemorySequenceData DistinctExecuteMethod<T> (ExecuteInMemorySequenceData arg)
    {
      var currentSequence = arg.GetCurrentSequence<T>();
      return new ExecuteInMemorySequenceData (currentSequence.A.Distinct (), currentSequence.B);
    }

    public ExecuteInMemorySequenceData ThrowingExecuteMethod<T> (ExecuteInMemorySequenceData arg)
    {
      throw new NotImplementedException ("Test");
    }

// ReSharper disable UnusedTypeParameter
    public ExecuteInMemoryValueData InvalidExecuteInMemory_TooManyGenericParameters<T1, T2> (ExecuteInMemorySequenceData input)
    {
      throw new NotImplementedException();
    }
// ReSharper restore UnusedTypeParameter

// ReSharper disable UnusedTypeParameter
    public ExecuteInMemoryValueData ExecuteMethodWithNonMatchingArgumentType<T> (ExecuteInMemoryValueData arg)
    {
      throw new NotImplementedException ("Test");
    }
// ReSharper restore UnusedTypeParameter

    public ExecuteInMemorySequenceData NonGenericExecuteMethod (ExecuteInMemorySequenceData arg)
    {
      throw new NotImplementedException ();
    }

    internal ExecuteInMemorySequenceData NonPublicExecuteMethod<T> (ExecuteInMemorySequenceData arg)
    {
      throw new NotImplementedException ();
    }
  }
}