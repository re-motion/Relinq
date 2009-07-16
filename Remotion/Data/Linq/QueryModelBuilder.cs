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
using System.Collections.ObjectModel;
using Remotion.Data.Linq.Clauses;
using Remotion.Utilities;

namespace Remotion.Data.Linq
{
  /// <summary>
  /// Collects clauses and creates a <see cref="QueryModel"/> from them. This provides a simple way to first add all the clauses and then
  /// create the <see cref="QueryModel"/> rather than the two-step approach (first <see cref="SelectClause"/> and <see cref="MainFromClause"/>,
  /// then the <see cref="IBodyClause"/>s) required by <see cref="QueryModel"/>'s constructor.
  /// </summary>
  public class QueryModelBuilder
  {
    private readonly List<ResultOperatorBase> _resultOperators = new List<ResultOperatorBase>();
    private readonly List<IBodyClause> _bodyClauses = new List<IBodyClause>();

    public MainFromClause MainFromClause { get; private set; }
    public SelectClause SelectClause { get; private set; }

    public ReadOnlyCollection<IBodyClause> BodyClauses
    {
      get { return _bodyClauses.AsReadOnly(); }
    }

    public ReadOnlyCollection<ResultOperatorBase> ResultOperators
    {
      get { return _resultOperators.AsReadOnly(); }
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
      throw new ArgumentTypeException (message, "clause", null, clause.GetType());
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

      var queryModel = new QueryModel (null, MainFromClause, SelectClause);

      foreach (var bodyClause in BodyClauses)
        queryModel.BodyClauses.Add (bodyClause);

      foreach (var resultOperator in ResultOperators)
        queryModel.ResultOperators.Add (resultOperator);

      return queryModel;
    }
  }
}