/* Copyright (C) 2005 - 2008 rubicon informationstechnologie gmbh
 *
 * This program is free software: you can redistribute it and/or modify it under 
 * the terms of the re:motion license agreement in license.txt. If you did not 
 * receive it, please visit http://www.re-motion.org/licensing.
 * 
 * Unless otherwise provided, this software is distributed on an "AS IS" basis, 
 * WITHOUT WARRANTY OF ANY KIND, either express or implied. 
 */

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Remotion.Utilities;

namespace Remotion.Data.Linq.QueryProviderImplementation
{
  public abstract class QueryableBase<T> : IOrderedQueryable<T>
  {
    private readonly QueryProviderBase _queryProvider;
    
    public QueryableBase (QueryProviderBase provider)
    {
      ArgumentUtility.CheckNotNull ("provider", provider);

      _queryProvider = provider;
      Expression = Expression.Constant (this);
    }

    public QueryableBase (QueryProviderBase provider, Expression expression)
    {
      ArgumentUtility.CheckNotNull ("provider", provider);
      ArgumentUtility.CheckNotNull ("expression", expression);

      if (!typeof (IEnumerable<T>).IsAssignableFrom (expression.Type))
        throw new ArgumentTypeException ("expression", typeof (IEnumerable<T>), expression.Type);

      _queryProvider = provider;
      Expression = expression;
    }

    public Expression Expression { get; private set; }

    public IQueryProvider Provider { get
    {
      return _queryProvider;
    }
    }

    public Type ElementType
    {
      get { return typeof (T); }
    }

    public IEnumerator<T> GetEnumerator ()
    {
      return _queryProvider.ExecuteCollection<T> (Expression).GetEnumerator ();
    }

    IEnumerator IEnumerable.GetEnumerator ()
    {
      return _queryProvider.ExecuteCollection (Expression).GetEnumerator ();
    }
  }
}
