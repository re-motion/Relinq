// This file is part of the re-linq project (relinq.codeplex.com)
// Copyright (c) rubicon IT GmbH, www.rubicon.eu
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
using Remotion.Linq.Clauses;
using Remotion.Linq.Collections;

namespace Remotion.Linq.UnitTests.Linq.Core.TestDomain
{
  public class TestQueryModelVisitor : QueryModelVisitorBase
  {
    //public void VisitCollection<T> (ObservableCollection<T> collection, Action<T> acceptAction, Action<int> indexSetter)
    //{
    //  var method = typeof (QueryModelVisitorBase)
    //      .GetMethod ("VisitCollection", BindingFlags.NonPublic | BindingFlags.Instance)
    //      .MakeGenericMethod (typeof (T));
    //  try
    //  {
    //    method.Invoke (this, new object[] { collection, acceptAction, indexSetter });
    //  }
    //  catch (TargetInvocationException ex)
    //  {
    //    throw ex.InnerException.PreserveStackTrace();
    //  }
    //}

    public new void VisitBodyClauses (ObservableCollection<IBodyClause> bodyClauses, QueryModel queryModel)
    {
      base.VisitBodyClauses (bodyClauses, queryModel);
    }

    public new void VisitOrderings (ObservableCollection<Ordering> orderings, QueryModel queryModel, OrderByClause orderByClause)
    {
      base.VisitOrderings (orderings, queryModel, orderByClause);
    }

    public new void VisitResultOperators (ObservableCollection<ResultOperatorBase> resultOperators, QueryModel queryModel)
    {
      base.VisitResultOperators (resultOperators, queryModel);
    }
  }
}
