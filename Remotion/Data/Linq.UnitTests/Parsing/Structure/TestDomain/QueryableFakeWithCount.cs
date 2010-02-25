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
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Remotion.Data.Linq.UnitTests.TestDomain;

namespace Remotion.Data.Linq.UnitTests.Parsing.Structure.TestDomain
{
  public class QueryableFakeWithCount<T> : IQueryable<T>, IQueryProvider
  {
    public IQueryable<Chef> Field = ExpressionHelper.CreateStudentQueryable ();

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

    internal IQueryable<Chef> InternalProperty
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
