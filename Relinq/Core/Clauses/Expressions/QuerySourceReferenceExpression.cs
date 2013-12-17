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
using System.Linq.Expressions;
using Remotion.Linq.Utilities;

namespace Remotion.Linq.Clauses.Expressions
{
  /// <summary>
  /// Represents an expression tree node that points to a query source represented by a <see cref="FromClauseBase"/>. These expressions should always
  /// point back, to a clause defined prior to the clause holding a <see cref="QuerySourceReferenceExpression"/>. Otherwise, exceptions might be 
  /// thrown at runtime.
  /// </summary>
  /// <remarks>
  /// This particular expression overrides <see cref="Equals"/>, i.e. it can be compared to another <see cref="QuerySourceReferenceExpression"/> based
  /// on the <see cref="ReferencedQuerySource"/>.
  /// </remarks>
  public class QuerySourceReferenceExpression : Expression
  {
    private readonly Type _type;
    public const ExpressionType ExpressionType = (ExpressionType) 100001;

    public QuerySourceReferenceExpression (IQuerySource querySource)
    {
      ArgumentUtility.CheckNotNull ("querySource", querySource);

      _type = querySource.ItemType;
      ReferencedQuerySource = querySource;
    }

    public override ExpressionType NodeType
    {
      get { return ExpressionType; }
    }

    public override Type Type
    {
      get { return _type; }
    }
    
    /// <summary>
    /// Gets the query source referenced by this expression.
    /// </summary>
    /// <value>The referenced query source.</value>
    public IQuerySource ReferencedQuerySource { get; private set; }

    /// <summary>
    /// Determines whether the specified <see cref="T:System.Object"/> is equal to the current <see cref="QuerySourceReferenceExpression"/> by 
    /// comparing the <see cref="ReferencedQuerySource"/> properties for reference equality.
    /// </summary>
    /// <param name="obj">The <see cref="T:System.Object"/> to compare with the current <see cref="QuerySourceReferenceExpression"/>.</param>
    /// <returns>
    /// <see langword="true" /> if the specified <see cref="T:System.Object"/> is a <see cref="QuerySourceReferenceExpression"/> that points to the 
    /// same <see cref="ReferencedQuerySource"/>; otherwise, false.
    /// </returns>
    public override bool Equals (object obj)
    {
      var other = obj as QuerySourceReferenceExpression;
      return other != null && ReferencedQuerySource == other.ReferencedQuerySource;
    }

    public override int GetHashCode ()
    {
      return ReferencedQuerySource.GetHashCode ();
    }
  }
}
