// This file is part of the re-linq project (relinq.codeplex.com)
// Copyright (C) 2005-2009 rubicon informationstechnologie gmbh, www.rubicon.eu
// 
// re-linq is free software; you can redistribute it and/or modify it under 
// the terms of the GNU Lesser General Public License as published by the 
// Free Software Foundation; either version 2.1 of the License, 
// or (at your option) any later version.
// 
// re-linq is distributed in the hope that it will be useful, 
// but WITHOUT ANY WARRANTY; without even the implied warranty of 
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the 
// GNU Lesser General Public License for more details.
// 
// You should have received a copy of the GNU Lesser General Public License
// along with re-linq; if not, see http://www.gnu.org/licenses.
// 
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Remotion.Linq.Clauses.ExpressionTreeVisitors;
using Remotion.Linq.Utilities;

namespace Remotion.Linq.Clauses
{
  /// <summary>
  /// Maps <see cref="IQuerySource"/> instances to <see cref="Expression"/> instances. This is used by <see cref="QueryModel.Clone()"/>
  /// in order to be able to correctly update references to old clauses to point to the new clauses. Via 
  /// <see cref="ReferenceReplacingExpressionTreeVisitor"/> and <see cref="CloningExpressionTreeVisitor"/>, it can also be used manually.
  /// </summary>
  public class QuerySourceMapping
  {
    private readonly Dictionary<IQuerySource, Expression> _lookup = new Dictionary<IQuerySource, Expression> ();

    public bool ContainsMapping (IQuerySource querySource)
    {
      ArgumentUtility.CheckNotNull ("querySource", querySource);
      return _lookup.ContainsKey (querySource);
    }

    public void AddMapping (IQuerySource querySource, Expression expression)
    {
      ArgumentUtility.CheckNotNull ("querySource", querySource);
      ArgumentUtility.CheckNotNull ("expression", expression);

      try
      {
        _lookup.Add (querySource, expression);
      }
      catch (ArgumentException)
      {
        throw new InvalidOperationException ("Query source has already been associated with an expression.");
      }
    }

    public void ReplaceMapping (IQuerySource querySource, Expression expression)
    {
      ArgumentUtility.CheckNotNull ("querySource", querySource);
      ArgumentUtility.CheckNotNull ("expression", expression);

      if (!ContainsMapping (querySource))
        throw new InvalidOperationException ("Query source has not been associated with an expression, cannot replace its mapping.");

      _lookup[querySource] = expression;
    }

    public Expression GetExpression (IQuerySource querySource)
    {
      ArgumentUtility.CheckNotNull ("querySource", querySource);
      try
      {
        return _lookup[querySource];
      }
      catch (KeyNotFoundException)
      {
        throw new KeyNotFoundException ("Query source has not been associated with an expression.");
      }
    }
  }
}
