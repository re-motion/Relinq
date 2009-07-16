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
using System.Linq.Expressions;
using Remotion.Utilities;

namespace Remotion.Data.Linq.Clauses.Expressions
{
  /// <summary>
  /// Represents an expression tree node that points to a query source represented by a <see cref="FromClauseBase"/>. These expressions should always
  /// point back, to a clause defined prior to the clause holding a <see cref="QuerySourceReferenceExpression"/>. Otherwise, exceptions might be 
  /// thrown at runtime.
  /// </summary>
  public class QuerySourceReferenceExpression : Expression
  {
    public QuerySourceReferenceExpression (IQuerySourceWithItemType querySource)
        : base ((ExpressionType) (-1), ArgumentUtility.CheckNotNull ("querySource", querySource).ItemType)
    {
      ReferencedQuerySource = querySource;
    }

    public QuerySourceReferenceExpression (IQuerySource querySource, Type expressionType)
      : base ((ExpressionType) (-1), ArgumentUtility.CheckNotNull ("expressionType", expressionType))
    {
      ReferencedQuerySource = querySource;
    }

    public IQuerySource ReferencedQuerySource { get; private set; }
  }
}