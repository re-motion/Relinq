// This file is part of the re-motion Core Framework (www.re-motion.org)
// Copyright (C) 2005-2008 rubicon informationstechnologie gmbh, www.rubicon.eu
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

    public IQueryProvider Provider
    {
      get { return _queryProvider; }
    }

    public Type ElementType
    {
      get { return typeof (T); }
    }

    public IEnumerator<T> GetEnumerator()
    {
      return _queryProvider.ExecuteCollection<T> (Expression).GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
      return _queryProvider.ExecuteCollection (Expression).GetEnumerator();
    }
  }
}
