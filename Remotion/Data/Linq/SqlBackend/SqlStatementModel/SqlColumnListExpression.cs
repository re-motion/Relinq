// This file is part of the re-motion Core Framework (www.re-motion.org)
// Copyright (C) 2005-2009 rubicon informationstechnologie gmbh, www.rubicon.eu
// 
// The re-motion Core Framework is free software; you can redistribute it 
// and/or modify it under the terms of the GNU Lesser General Public License 
// as published by the Free Software Foundation; either version 2.1 of the 
// License, or (at your option) any later version.
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
using System.Linq.Expressions;
using Remotion.Data.Linq.Clauses.Expressions;
using Remotion.Data.Linq.Parsing;
using Remotion.Data.Linq.Utilities;

namespace Remotion.Data.Linq.SqlBackend.SqlStatementModel
{
  /// <summary>
  /// <see cref="SqlColumnListExpression"/> holds a list of <see cref="SqlColumnExpression"/> instances.
  /// </summary>
  public class SqlColumnListExpression : ExtensionExpression
  {
    private readonly List<SqlColumnExpression> _columns; // TODO: Use SqlColumnExpression[] - the list will not be changed anyway.

    public SqlColumnListExpression (Type type, List<SqlColumnExpression> columns)
        : base (type)
    {
      ArgumentUtility.CheckNotNull ("columns", columns);

      _columns = columns;
    }

    // TODO: Do not expose the Columns as a List, use ReadOnlyCollection instead. (Expressions should be immutable.)
    public List<SqlColumnExpression> Columns
    {
      get { return _columns; }
    }

    // TODO: Implement and test - should call visitor.VisitExpression for all _columns.
    protected internal override Expression VisitChildren (ExpressionTreeVisitor visitor)
    {
      ArgumentUtility.CheckNotNull ("visitor", visitor);
      throw new NotImplementedException();
    }

    public override Expression Accept (ExpressionTreeVisitor visitor)
    {
      ArgumentUtility.CheckNotNull ("visitor", visitor);
      throw new NotImplementedException();
    }
  }
}