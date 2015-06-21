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
using System.Collections.Generic;
using System.Linq.Expressions;
using Remotion.Linq.Clauses.ExpressionVisitors;
using Remotion.Utilities;

namespace Remotion.Linq.Clauses
{
  /// <summary>
  /// Maps <see cref="IQuerySource"/> instances to <see cref="Expression"/> instances. This is used by <see cref="QueryModel.Clone()"/>
  /// in order to be able to correctly update references to old clauses to point to the new clauses. Via 
  /// <see cref="ReferenceReplacingExpressionVisitor"/>, it can also be used manually.
  /// </summary>
  public sealed class QuerySourceMapping
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
