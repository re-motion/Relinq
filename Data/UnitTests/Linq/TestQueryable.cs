/* Copyright (C) 2005 - 2008 rubicon informationstechnologie gmbh
 *
 * This program is free software: you can redistribute it and/or modify it under 
 * the terms of the re:motion license agreement in license.txt. If you did not 
 * receive it, please visit http://www.re-motion.org/licensing.
 * 
 * Unless otherwise provided, this software is distributed on an "AS IS" basis, 
 * WITHOUT WARRANTY OF ANY KIND, either express or implied. 
 */

using System.Linq.Expressions;
using Remotion.Data.Linq;
using Remotion.Data.Linq.QueryProviderImplementation;

namespace Remotion.Data.UnitTests.Linq
{
  public class TestQueryable<T> : QueryableBase<T>
  {
    public TestQueryable (QueryProviderBase provider, Expression expression)
        : base (provider, expression)
    {
    }

    public TestQueryable (IQueryExecutor executor)
        : base (new TestQueryProvider (executor))
    {
    }

    public override string ToString ()
    {
      return "TestQueryable<" + typeof (T).Name + ">()";
    }
  }
}
