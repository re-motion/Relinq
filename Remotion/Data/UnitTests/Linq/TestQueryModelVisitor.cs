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
using System.Reflection;
using Remotion.Collections;
using Remotion.Data.Linq;
using Remotion.Data.Linq.Clauses;
using Remotion.Utilities;

namespace Remotion.Data.UnitTests.Linq
{
  public class TestQueryModelVisitor : QueryModelVisitorBase
  {
    public void VisitCollection<T> (ObservableCollection<T> collection, Action<T> acceptAction, Action<int> indexSetter)
    {
      var method = typeof (QueryModelVisitorBase)
          .GetMethod ("VisitCollection", BindingFlags.NonPublic | BindingFlags.Instance)
          .MakeGenericMethod (typeof (T));
      try
      {
        method.Invoke (this, new object[] { collection, acceptAction, indexSetter });
      }
      catch (TargetInvocationException ex)
      {
        throw ex.InnerException.PreserveStackTrace();
      }
    }

    public new void VisitBodyClauses (QueryModel queryModel, ObservableCollection<IBodyClause> bodyClauses)
    {
      base.VisitBodyClauses (queryModel, bodyClauses);
    }

    public new void VisitJoinClauses (FromClauseBase fromClause, ObservableCollection<JoinClause> joinClauses)
    {
      base.VisitJoinClauses (fromClause, joinClauses);
    }

    public new void VisitOrderings (OrderByClause orderByClause, ObservableCollection<Ordering> orderings)
    {
      base.VisitOrderings (orderByClause, orderings);
    }

    public new void VisitResultModifications (QueryModel queryModel, SelectClause selectClause, ObservableCollection<ResultModificationBase> resultModifications)
    {
      base.VisitResultModifications (queryModel, selectClause, resultModifications);
    }
  }
}