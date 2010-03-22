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
using System.Reflection;
using Remotion.Data.Linq.SqlBackend.SqlStatementModel.Resolved;
using Remotion.Data.Linq.Utilities;

namespace Remotion.Data.Linq.SqlBackend.SqlStatementModel.Unresolved
{
  /// <summary>
  /// <see cref="UnresolvedJoinInfo"/> represents the data source defined by a member access in a LINQ expression.
  /// </summary>
  public class UnresolvedJoinInfo : AbstractJoinInfo
  {
    private readonly MemberInfo _memberInfo;
    private readonly JoinCardinality _cardinality;
    private readonly Type _itemType;
    private readonly SqlTableBase _sqlTable;

    public UnresolvedJoinInfo (SqlTableBase sqlTable, MemberInfo memberInfo, JoinCardinality cardinality)
    {
      ArgumentUtility.CheckNotNull ("sqlTable", sqlTable);
      ArgumentUtility.CheckNotNull ("memberInfo", memberInfo);

      _sqlTable = sqlTable;
      _memberInfo = memberInfo;
      _cardinality = cardinality;

      var memberReturnType = ReflectionUtility.GetFieldOrPropertyType (memberInfo);
      if (Cardinality == JoinCardinality.One)
        _itemType = memberReturnType;
      else
        _itemType = ReflectionUtility.GetItemTypeOfIEnumerable (memberReturnType, "memberInfo");
    }

    public SqlTableBase SqlTable
    {
      get { return _sqlTable; }
    }

    public MemberInfo MemberInfo
    {
      get { return _memberInfo; }
    }

    public JoinCardinality Cardinality
    {
      get { return _cardinality; }
    }

    public override Type ItemType
    {
      get { return _itemType; }
    }

    public override AbstractJoinInfo Accept (IJoinInfoVisitor visitor)
    {
      ArgumentUtility.CheckNotNull ("visitor", visitor);
      return visitor.VisitUnresolvedJoinInfo (this);
    }

   public override IResolvedTableInfo GetResolvedTableInfo ()
    {
      throw new InvalidOperationException ("This join has not yet been resolved; call the resolution step first.");
    }
  }
}