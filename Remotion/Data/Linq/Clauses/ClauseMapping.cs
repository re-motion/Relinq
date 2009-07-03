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
using System.Linq.Expressions;
using Remotion.Data.Linq.Clauses.ExpressionTreeVisitors;
using Remotion.Utilities;

namespace Remotion.Data.Linq.Clauses
{
  /// <summary>
  /// Maps <see cref="IClause"/> instances to <see cref="Expression"/> instances. This is used by <see cref="QueryModel.Clone()"/>
  /// and the clauses' Clone methods in order to be able to correctly update references to old clauses to point to the new clauses. Via
  /// <see cref="ReferenceReplacingExpressionTreeVisitor"/>, it can also be used manually.
  /// </summary>
  public class ClauseMapping
  {
    private readonly Dictionary<IClause, Expression> _lookup = new Dictionary<IClause, Expression> ();

    public bool ContainsMapping (IClause clause)
    {
      ArgumentUtility.CheckNotNull ("clause", clause);
      return _lookup.ContainsKey (clause);
    }

    public void AddMapping (IClause clause, Expression expression)
    {
      ArgumentUtility.CheckNotNull ("clause", clause);
      ArgumentUtility.CheckNotNull ("expression", expression);

      try
      {
        _lookup.Add (clause, expression);
      }
      catch (ArgumentException)
      {
        throw new InvalidOperationException ("Clause has already been associated with an expression.");
      }
    }

    public void ReplaceMapping (IClause clause, Expression expression)
    {
      ArgumentUtility.CheckNotNull ("clause", clause);
      ArgumentUtility.CheckNotNull ("expression", expression);

      if (!ContainsMapping (clause))
        throw new InvalidOperationException ("Clause has not been associated with an expression, cannot replace its mapping.");

      _lookup[clause] = expression;
    }

    public Expression GetExpression (IClause clause)
    {
      ArgumentUtility.CheckNotNull ("clause", clause);
      try
      {
        return _lookup[clause];
      }
      catch (KeyNotFoundException)
      {
        throw new KeyNotFoundException ("Clause has not been associated with an expression.");
      }
    }

  }
}