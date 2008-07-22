/* Copyright (C) 2005 - 2008 rubicon informationstechnologie gmbh
 *
 * This program is free software: you can redistribute it and/or modify it under 
 * the terms of the re:motion license agreement in license.txt. If you did not 
 * receive it, please visit http://www.re-motion.org/licensing.
 * 
 * Unless otherwise provided, this software is distributed on an "AS IS" basis, 
 * WITHOUT WARRANTY OF ANY KIND, either express or implied. 
 */

using System.Linq;
using System.Linq.Expressions;
using Remotion.Data.Linq;

namespace Remotion.Data.UnitTests.Linq
{
  public class TestQueryProvider : QueryProviderBase
  {
    public TestQueryProvider (IQueryExecutor executor)
        : base (executor)
    {
    }

    protected override IQueryable<T> CreateQueryable<T> (Expression expression)
    {
      return new TestQueryable<T> (this, expression);
    }
  }
}
