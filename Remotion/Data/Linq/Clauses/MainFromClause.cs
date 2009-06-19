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
using System.Diagnostics;
using System.Linq.Expressions;
using Remotion.Data.Linq.Clauses.ExpressionTreeVisitors;
using Remotion.Utilities;


namespace Remotion.Data.Linq.Clauses
{
  /// <summary>
  /// Extends <see cref="FromClauseBase"/>. <see cref="MainFromClause"/> is used for the first from clause of a linq query.
  /// A <see cref="MainFromClause"/> does not have a previous clause.
  /// example: from c in queryable
  /// </summary>
  public class MainFromClause : FromClauseBase
  {
    /// <summary>
    /// Initialize a new instance of <see cref="MainFromClause"/>.
    /// </summary>
    /// <param name="identifier">The identifier of the clause.</param>
    /// <param name="querySource">The source of the clause.</param>
    public MainFromClause (ParameterExpression identifier, Expression querySource): base(null,identifier)
    {
      ArgumentUtility.CheckNotNull ("querySource", querySource);
      QuerySource = querySource;
    }

    /// <summary>
    /// The query (data) source which is requested
    /// </summary>
    // TODO 1158: Replace by QuerySourceType
    [DebuggerDisplay ("{Remotion.Data.Linq.StringBuilding.FormattingExpressionTreeVisitor.Format (QuerySource),nq}")]
    public Expression QuerySource { get; private set; }

    public override void Accept (IQueryVisitor visitor)
    {
      ArgumentUtility.CheckNotNull ("visitor", visitor);
      visitor.VisitMainFromClause (this);
    }

    public override Type GetQuerySourceType ()
    {
      return QuerySource.Type;
    }

    public MainFromClause Clone (CloneContext cloneContext)
    {
      ArgumentUtility.CheckNotNull ("cloneContext", cloneContext);

      var newQuerySource = CloneExpressionTreeVisitor.ReplaceClauseReferences (QuerySource, cloneContext);
      var result = new MainFromClause (Identifier, newQuerySource);
      cloneContext.ClonedClauseMapping.AddMapping (this, result);
      result.AddClonedJoinClauses (JoinClauses, cloneContext);
      return result;
    }
  }
}
