// This file is part of the re-linq project (relinq.codeplex.com)
// Copyright (C) 2005-2009 rubicon informationstechnologie gmbh, www.rubicon.eu
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
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using Remotion.Linq.Clauses.Expressions;
using Remotion.Linq.Clauses.ExpressionTreeVisitors;
using Remotion.Linq.Clauses.StreamedData;
using Remotion.Linq.Utilities;

namespace Remotion.Linq.Clauses
{
  /// <summary>
  /// Represents the select part of a query, projecting data items according to some <see cref="Selector"/>.
  /// </summary>
  /// <example>
  /// In C#, the "select" clause in the following sample corresponds to a <see cref="SelectClause"/>. "s" (a reference to the query source "s", see
  /// <see cref="QuerySourceReferenceExpression"/>) is the <see cref="Selector"/> expression:
  /// <code>
  /// var query = from s in Students
  ///             where s.First == "Hugo"
  ///             select s;
  /// </code>
  /// </example>
  public class SelectClause : IClause
  {
    private Expression _selector;

    /// <summary>
    /// Initializes a new instance of the <see cref="SelectClause"/> class.
    /// </summary>
    /// <param name="selector">The selector that projects the data items.</param>
    public SelectClause (Expression selector) // TODO 3207
    {
      ArgumentUtility.CheckNotNull ("selector", selector);

      _selector = selector;
    }

    /// <summary>
    /// Gets the selector defining what parts of the data items are returned by the query.
    /// </summary>
    [DebuggerDisplay ("{Remotion.Data.Linq.Clauses.ExpressionTreeVisitors.FormattingExpressionTreeVisitor.Format (Selector),nq}")]
    public Expression Selector
    {
      get { return _selector; }
      set { _selector = ArgumentUtility.CheckNotNull ("value", value); }
    }

    /// <summary>
    /// Accepts the specified visitor by calling its <see cref="IQueryModelVisitor.VisitSelectClause"/> method.
    /// </summary>
    /// <param name="visitor">The visitor to accept.</param>
    /// <param name="queryModel">The query model in whose context this clause is visited.</param>
    public virtual void Accept (IQueryModelVisitor visitor, QueryModel queryModel)
    {
      ArgumentUtility.CheckNotNull ("visitor", visitor);
      ArgumentUtility.CheckNotNull ("queryModel", queryModel);

      visitor.VisitSelectClause (this, queryModel);
    }

    /// <summary>
    /// Clones this clause.
    /// </summary>
    /// <param name="cloneContext">The clones of all query source clauses are registered with this <see cref="CloneContext"/>.</param>
    /// <returns>A clone of this clause.</returns>
    public SelectClause Clone (CloneContext cloneContext)
    {
      ArgumentUtility.CheckNotNull ("cloneContext", cloneContext);

      var result = new SelectClause (Selector);
      return result;
    }

    /// <summary>
    /// Transforms all the expressions in this clause and its child objects via the given <paramref name="transformation"/> delegate.
    /// </summary>
    /// <param name="transformation">The transformation object. This delegate is called for each <see cref="Expression"/> within this
    /// clause, and those expressions will be replaced with what the delegate returns.</param>
    public virtual void TransformExpressions (Func<Expression, Expression> transformation)
    {
      ArgumentUtility.CheckNotNull ("transformation", transformation);
      Selector = transformation (Selector);
    }

    public override string ToString ()
    {
      return "select " + FormattingExpressionTreeVisitor.Format (Selector);
    }

    /// <summary>
    /// Gets an <see cref="StreamedSequenceInfo"/> object describing the data streaming out of this <see cref="SelectClause"/>. If a query ends with
    /// the <see cref="SelectClause"/>, this corresponds to the query's output data. If a query has <see cref="QueryModel.ResultOperators"/>, the data
    /// is further modified by those operators. Use <see cref="QueryModel.GetOutputDataInfo"/> to obtain the real result type of
    /// a query model, including the <see cref="QueryModel.ResultOperators"/>.
    /// </summary>
    /// <returns>Gets a <see cref="StreamedSequenceInfo"/> object describing the data streaming out of this <see cref="SelectClause"/>.</returns>
    /// <remarks>
    /// The data streamed from a <see cref="SelectClause"/> is always of type <see cref="IQueryable{T}"/> instantiated
    /// with the type of <see cref="Selector"/> as its generic parameter. Its <see cref="StreamedSequenceInfo.ItemExpression"/> corresponds to the
    /// <see cref="Selector"/>.
    /// </remarks>
    public StreamedSequenceInfo GetOutputDataInfo ()
    {
      return new StreamedSequenceInfo (typeof (IQueryable<>).MakeGenericType (Selector.Type), Selector);
    }
  }
}
