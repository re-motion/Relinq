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
using System.Reflection;
using Remotion.Data.Linq.Clauses.Expressions;
using Remotion.Data.Linq.Parsing;
using Remotion.Data.Linq.Utilities;

namespace Remotion.Data.Linq.SqlBackend.SqlStatementModel.Unresolved
{
  /// <summary>
  /// Describes a member reference representing an entity rather than a simple column.
  /// </summary>
  public class SqlEntityRefMemberExpression : ExtensionExpression
  {
    private readonly SqlTableBase _sqlTable;
    private readonly MemberInfo _memberInfo;

    public SqlEntityRefMemberExpression (SqlTableBase sqlTable, MemberInfo memberInfo)
      : base (ReflectionUtility.GetFieldOrPropertyType (ArgumentUtility.CheckNotNull ("memberInfo", memberInfo)))
    {
      ArgumentUtility.CheckNotNull ("sqlTable", sqlTable);
      ArgumentUtility.CheckNotNull ("memberInfo", memberInfo);

      _sqlTable = sqlTable;
      _memberInfo = memberInfo;
    }

    public SqlTableBase SqlTable
    {
      get { return _sqlTable; }
    }

    public MemberInfo MemberInfo
    {
      get { return _memberInfo; }
    }

    protected override Expression VisitChildren (ExpressionTreeVisitor visitor)
    {
      ArgumentUtility.CheckNotNull ("visitor", visitor);
      return this;
    }

    public override Expression Accept (ExpressionTreeVisitor visitor)
    {
      ArgumentUtility.CheckNotNull ("visitor", visitor);

      var specificVisitor = visitor as IUnresolvedSqlExpressionVisitor;
      if (specificVisitor != null)
        return specificVisitor.VisitSqlEntityRefMemberExpression(this);
      else
        return base.Accept (visitor);
    }
  }
}