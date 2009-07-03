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
using Remotion.Data.Linq.Clauses.Expressions;
using Remotion.Data.Linq.Clauses.ExpressionTreeVisitors;
using Remotion.Data.Linq.StringBuilding;
using Remotion.Utilities;

namespace Remotion.Data.Linq.Clauses
{
  public class GroupClause : ISelectGroupClause
  {
    private Expression _groupExpression;
    private Expression _byExpression;

    public GroupClause (Expression groupExpression, Expression byExpression)
    {
      ArgumentUtility.CheckNotNull ("groupExpression", groupExpression);
      ArgumentUtility.CheckNotNull ("byExpression", byExpression);

      _groupExpression = groupExpression;
      _byExpression = byExpression;
    }

    [DebuggerDisplay ("{Remotion.Data.Linq.StringBuilding.FormattingExpressionTreeVisitor.Format (GroupExpression),nq}")]
    public Expression GroupExpression
    {
      get { return _groupExpression; }
      set { _groupExpression = ArgumentUtility.CheckNotNull("value", value); }
    }

    [DebuggerDisplay ("{Remotion.Data.Linq.StringBuilding.FormattingExpressionTreeVisitor.Format (ByExpression),nq}")]
    public Expression ByExpression
    {
      get { return _byExpression; }
      set { _byExpression = ArgumentUtility.CheckNotNull ("value", value); }
    }

    /// <summary>
    /// Accepts the specified visitor by calling one its <see cref="IQueryModelVisitor.VisitGroupClause"/> method.
    /// </summary>
    /// <param name="visitor">The visitor to accept.</param>
    /// <param name="queryModel">The query model in whose context this clause is visited.</param>
    public void Accept (IQueryModelVisitor visitor, QueryModel queryModel)
    {
      ArgumentUtility.CheckNotNull ("visitor", visitor);
      ArgumentUtility.CheckNotNull ("queryModel", queryModel);
      visitor.VisitGroupClause (this, queryModel);
    }

    /// <summary>
    /// Clones this clause, adjusting all <see cref="QuerySourceReferenceExpression"/> instances held by it as defined by
    /// <paramref name="cloneContext"/>.
    /// </summary>
    /// <param name="cloneContext">The clone context to use for replacing <see cref="QuerySourceReferenceExpression"/> objects.</param>
    /// <returns>A clone of this clause.</returns>
    public GroupClause Clone (CloneContext cloneContext)
    {
      ArgumentUtility.CheckNotNull ("cloneContext", cloneContext);

      var clone = new GroupClause (GroupExpression, ByExpression);
      clone.TransformExpressions (ex => ReferenceReplacingExpressionTreeVisitor.ReplaceClauseReferences (ex, cloneContext.ClauseMapping));
      return clone;
    }

    ISelectGroupClause ISelectGroupClause.Clone (CloneContext cloneContext)
    {
      return Clone (cloneContext);
    }

    public IExecutionStrategy GetExecutionStrategy ()
    {
      throw new NotImplementedException ();
    }

    /// <summary>
    /// Transforms all the expressions in this clause and its child objects via the given <paramref name="transformation"/> delegate.
    /// </summary>
    /// <param name="transformation">The transformation object. This delegate is called for each <see cref="Expression"/> within this
    /// clause, and those expressions will be replaced with what the delegate returns.</param>
    public void TransformExpressions (Func<Expression, Expression> transformation)
    {
      ArgumentUtility.CheckNotNull ("transformation", transformation);

      GroupExpression = transformation (GroupExpression);
      ByExpression = transformation (ByExpression);
    }

    public override string ToString ()
    {
      return string.Format (
          "group {0} by {1}", 
          FormattingExpressionTreeVisitor.Format (GroupExpression), 
          FormattingExpressionTreeVisitor.Format (ByExpression)); 
    }
  }
}
