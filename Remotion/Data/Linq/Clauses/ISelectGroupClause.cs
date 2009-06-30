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
namespace Remotion.Data.Linq.Clauses
{
  /// <summary>
  /// Represents the <see cref="SelectClause"/> or <see cref="GroupClause"/> of a query. This is the end point of the query, it defines what is 
  /// atually returned for each of the  items coming from the <see cref="QueryModel.MainFromClause"/> and passing the 
  /// <see cref="QueryModel.BodyClauses"/>.
  /// </summary>
  public interface ISelectGroupClause : IClause
  {
    ISelectGroupClause Clone (CloneContext cloneContext);
    void Accept (IQueryModelVisitor visitor, QueryModel queryModel);
    
    /// <summary>
    /// Gets the execution strategy to use for the given select or group clause. The execution strategy defines how to dispatch a query
    /// to an implementation of <see cref="IQueryExecutor"/> when the <see cref="QueryProviderBase"/> needs to execute a query.
    /// </summary>
    IExecutionStrategy GetExecutionStrategy ();
  }
}
