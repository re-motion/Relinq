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
using Remotion.Data.Linq.StringBuilding;
using Remotion.Utilities;

namespace Remotion.Data.Linq.Clauses
{
  /// <summary>
  /// Represents the where part of a linq query.
  /// example: where a.A = "something useful"
  /// </summary>
  public class WhereClause : IBodyClause
  {
    private Expression _predicate;
    
    /// <summary>
    /// Initialize a new instance of <see cref="WhereClause"/>
    /// </summary>
    /// <param name="predicate">The expression which represents the where conditions.</param>
    public WhereClause (Expression predicate)
    {
      ArgumentUtility.CheckNotNull ("predicate", predicate);
      _predicate = predicate;
    }

    /// <summary>
    /// The expression which represents the where conditions.
    /// </summary>
    [DebuggerDisplay ("{Remotion.Data.Linq.StringBuilding.FormattingExpressionTreeVisitor.Format (Predicate),nq}")]
    public Expression Predicate
    {
      get { return _predicate; }
      set { _predicate = ArgumentUtility.CheckNotNull ("value", value); }
    }

    public virtual void Accept (IQueryModelVisitor visitor, QueryModel queryModel, int index)
    {
      ArgumentUtility.CheckNotNull ("visitor", visitor);
      ArgumentUtility.CheckNotNull ("queryModel", queryModel);

      visitor.VisitWhereClause (this, queryModel, index);
    }

    public void TransformExpressions (Func<Expression, Expression> transformation)
    {
      ArgumentUtility.CheckNotNull ("transformation", transformation);
      Predicate = transformation (Predicate);
    }

    public WhereClause Clone (CloneContext cloneContext)
    {
      ArgumentUtility.CheckNotNull ("cloneContext", cloneContext);

      var clone = new WhereClause (Predicate);
      clone.TransformExpressions (ex => ReferenceReplacingExpressionTreeVisitor.ReplaceClauseReferences (ex, cloneContext.ClauseMapping));
      return clone;
    }

    IBodyClause IBodyClause.Clone (CloneContext cloneContext)
    {
      return Clone (cloneContext);
    }

    public override string ToString ()
    {
      return "where " + FormattingExpressionTreeVisitor.Format (Predicate);
    }
  }
}
