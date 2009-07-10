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

    public override ResultOperatorBase Clone (CloneContext cloneContext)
    {
      throw new NotImplementedException();
    }

    public object InvokeGenericOnEnumerable (object input, Func<IEnumerable<object>, object> genericMethodCaller)
    {
      return base.InvokeGenericOnEnumerable (input, genericMethodCaller);
    }

    public IEnumerable<T> DistinctExecuteMethod<T> (IEnumerable<T> arg)
    {
      return arg.Distinct ();
    }

    public IEnumerable<T> ThrowingExecuteMethod<T> (IEnumerable<T> arg)
    {
      throw new NotImplementedException ("Test");
    }

    public object ExecuteMethodWithNonMatchingArgumentType<T> (IEnumerable<object> arg)
    {
      throw new NotImplementedException ("Test");
    }

    public object NonGenericExecuteMethod (IEnumerable<object> arg)
    {
      throw new NotImplementedException ();
    }

    internal object NonPublicExecuteMethod<T> (IEnumerable<T> arg)
    {
      throw new NotImplementedException ();
    }
  }
}