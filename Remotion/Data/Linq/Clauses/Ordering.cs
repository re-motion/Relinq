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
  /// Represents one expression of a order by in a linq query.
  /// </summary>
  public class Ordering
  {
    private Expression _expression;
    
    /// <summary>
    /// Initialize a new instance of <see cref="Ordering"/>
    /// </summary>
    /// <param name="expression">The expression from one part of a order by in a linq query.</param>
    /// <param name="direction"></param>
    public Ordering (Expression expression, OrderingDirection direction)
    {
      ArgumentUtility.CheckNotNull ("expression", expression);

      _expression = expression;
      OrderingDirection = direction;
    }

    /// <summary>
    /// The previous clause of type <see cref="IClause"/> in the <see cref="QueryModel"/>.
    /// </summary>
    public OrderByClause OrderByClause { get; set; }

    /// <summary>
    /// The expression from one part of a order by in a linq query.
    /// </summary>
    [DebuggerDisplay ("{Remotion.Data.Linq.StringBuilding.FormattingExpressionTreeVisitor.Format (Expression),nq}")]
    public Expression Expression
    {
      get { return _expression; }
      set { _expression = ArgumentUtility.CheckNotNull ("value", value); }
    }

    public OrderingDirection OrderingDirection { get; set; }

    public virtual void Accept (IQueryModelVisitor visitor, QueryModel queryModel, OrderByClause orderByClause, int index)
    {
      ArgumentUtility.CheckNotNull ("visitor", visitor);
      ArgumentUtility.CheckNotNull ("queryModel", queryModel);
      ArgumentUtility.CheckNotNull ("orderByClause", orderByClause);
      
      visitor.VisitOrdering (this, queryModel, orderByClause, index);
    }

    public Ordering Clone (CloneContext cloneContext)
    {
      ArgumentUtility.CheckNotNull ("cloneContext", cloneContext);

      var clone = new Ordering (Expression, OrderingDirection);
      clone.TransformExpressions (ex => ReferenceReplacingExpressionTreeVisitor.ReplaceClauseReferences (ex, cloneContext.ClauseMapping));
      return clone;
    }

    public void TransformExpressions (Func<Expression, Expression> transformation)
    {
      ArgumentUtility.CheckNotNull ("transformation", transformation);
      Expression = transformation (Expression);
    }

    public override string ToString ()
    {
      switch (OrderingDirection)
      {
        case OrderingDirection.Asc:
          return string.Format ("{0} ascending ", FormatExpression (Expression));
        case OrderingDirection.Desc:
          return string.Format ("{0} descending ", FormatExpression (Expression));
      }
      return base.ToString ();
    }

    private string FormatExpression (Expression expression)
    {
      if (expression != null)
        return FormattingExpressionTreeVisitor.Format (expression);
      else
        return "<null>";
    }
  }
}
