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
using Remotion.Data.Linq.SqlBackend.SqlStatementModel;
using Remotion.Data.Linq.SqlBackend.SqlStatementModel.Resolved;
using Remotion.Data.Linq.SqlBackend.SqlStatementModel.Unresolved;

namespace Remotion.Data.Linq.SqlBackend.MappingResolution
{
  /// <summary>
  /// Provides entry points for all transformations that occur during the mapping resolution phase.
  /// </summary>
  public interface IMappingResolutionStage
  {
    Expression ResolveSelectExpression (Expression expression);
    Expression ResolveWhereExpression (Expression expression);
    Expression ResolveOrderingExpression (Expression expression);
    Expression ResolveTopExpression (Expression expression);
    AbstractTableInfo ResolveTableInfo (AbstractTableInfo tableInfo);
    ResolvedJoinInfo ResolveJoinInfo (UnresolvedJoinInfo joinInfo);
    void ResolveSqlStatement (SqlStatement sqlStatement);
  }
}