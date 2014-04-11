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
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Remotion.Linq.Development.UnitTesting;
using Remotion.Linq.UnitTests.TestDomain;

namespace Remotion.Linq.UnitTests.Parsing.Structure.TestDomain
{
  public class QueryableFakeWithCount<T> : IQueryable<T>, IQueryProvider
  {
    public IQueryable<Cook> Field = ExpressionHelper.CreateQueryable<Cook> ();

    public QueryableFakeWithCount ()
    {
      Expression = Expression.Constant (this);
    }

    public QueryableFakeWithCount (Expression expression)
    {
      Expression = expression;
    }

    public Expression Expression { get; private set; }

    public Type ElementType
    {
      get { return typeof (T); }
    }

    public IQueryProvider Provider 
    { 
      get { return this; }
    }

    public int Count
    {
      get { throw new NotImplementedException(); }
    }

    internal IQueryable<Cook> InternalProperty
    {
      get { throw new NotImplementedException (); }
    }

    public IQueryable CreateQuery (Expression expression)
    {
      throw new NotImplementedException ();
    }

    public IQueryable<TElement> CreateQuery<TElement> (Expression expression)
    {
      return new QueryableFakeWithCount<TElement> (expression);
    }

    public object Execute (Expression expression)
    {
      throw new NotImplementedException();
    }

    public TResult Execute<TResult> (Expression expression)
    {
      throw new NotImplementedException();
    }

    public IEnumerator<T> GetEnumerator ()
    {
      throw new NotImplementedException();
    }

    IEnumerator IEnumerable.GetEnumerator ()
    {
      return GetEnumerator();
    }
  }
}
