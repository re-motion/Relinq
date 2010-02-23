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
using System.Linq.Expressions;
using Remotion.Data.Linq.Clauses.Expressions;
using Remotion.Data.Linq.Parsing;
using Remotion.Data.Linq.Utilities;

namespace Remotion.Data.Linq.SqlBackend.SqlStatementModel
{
  /// <summary>
  /// <see cref="SqlTableReferenceExpression"/> points to the corresponding <see cref="SqlStatementModel.SqlTableExpression"/>.
  /// </summary>
  public class SqlTableReferenceExpression : ExtensionExpression
  {
    private readonly SqlTableExpression _sqlTableExpression;
    
    public SqlTableReferenceExpression (Type type, SqlTableExpression sqlTableExpression)
        : base(type)
    {
      ArgumentUtility.CheckNotNull ("sqlTableExpression", sqlTableExpression);

      _sqlTableExpression = sqlTableExpression;
    }

    public SqlTableExpression SqlTableExpression
    {
      get { return _sqlTableExpression; }
    }

    protected internal override Expression VisitChildren (ExpressionTreeVisitor visitor)
    {
      return this;
    }

    public override Expression Accept (ExpressionTreeVisitor visitor)
    {
      throw new NotImplementedException();
    }


  }
}