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
using System.Collections.ObjectModel;
using Remotion.Linq.Clauses;
using Remotion.Utilities;

namespace Remotion.Linq
{
  /// <summary>
  /// Collects clauses and creates a <see cref="QueryModel"/> from them. This provides a simple way to first add all the clauses and then
  /// create the <see cref="QueryModel"/> rather than the two-step approach (first <see cref="SelectClause"/> and <see cref="MainFromClause"/>,
  /// then the <see cref="IBodyClause"/>s) required by <see cref="QueryModel"/>'s constructor.
  /// </summary>
  public sealed class QueryModelBuilder
  {
    private readonly List<ResultOperatorBase> _resultOperators = new List<ResultOperatorBase>();
    private readonly List<IBodyClause> _bodyClauses = new List<IBodyClause>();

    public MainFromClause MainFromClause { get; private set; }
    public SelectClause SelectClause { get; private set; }

    public ReadOnlyCollection<IBodyClause> BodyClauses
    {
      get { return new ReadOnlyCollection<IBodyClause> (_bodyClauses); }
    }

    public ReadOnlyCollection<ResultOperatorBase> ResultOperators
    {
      get { return new ReadOnlyCollection<ResultOperatorBase> (_resultOperators); }
    }

    public void AddClause (IClause clause)
    {
      ArgumentUtility.CheckNotNull ("clause", clause);

      var clauseAsMainFromClause = clause as MainFromClause;
      if (clauseAsMainFromClause != null)
      {
        if (MainFromClause != null)
          throw new InvalidOperationException ("Builder already has a MainFromClause.");

        MainFromClause = clauseAsMainFromClause;
        return;
      }

      var clauseAsSelectClause = clause as SelectClause;
      if (clauseAsSelectClause != null)
      {
        if (SelectClause != null)
          throw new InvalidOperationException ("Builder already has a SelectClause.");

        SelectClause = clauseAsSelectClause;
        return;
      }

      var clauseAsBodyClause = clause as IBodyClause;
      if (clauseAsBodyClause != null)
      {
        _bodyClauses.Add (clauseAsBodyClause);
        return;
      }

      var message = string.Format (
          "Cannot add clause of type '{0}' to a query model. Only instances of IBodyClause, MainFromClause, or ISelectGroupClause are supported.",
          clause.GetType());
      throw new ArgumentException (message, "clause");
    }

    public void AddResultOperator (ResultOperatorBase resultOperator)
    {
      ArgumentUtility.CheckNotNull ("resultOperator", resultOperator);
      _resultOperators.Add (resultOperator);
    }

    public QueryModel Build ()
    {
      if (MainFromClause == null)
        throw new InvalidOperationException ("No MainFromClause was added to the builder.");

      if (SelectClause == null)
        throw new InvalidOperationException ("No SelectOrGroupClause was added to the builder.");

      var queryModel = new QueryModel (MainFromClause, SelectClause);

      foreach (var bodyClause in BodyClauses)
        queryModel.BodyClauses.Add (bodyClause);

      foreach (var resultOperator in ResultOperators)
        queryModel.ResultOperators.Add (resultOperator);

      return queryModel;
    }
  }
}
