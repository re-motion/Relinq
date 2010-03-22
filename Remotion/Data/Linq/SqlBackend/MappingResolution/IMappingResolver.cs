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
  /// <see cref="IMappingResolver"/> provides methods to resolve expressions and return database-specific information.
  /// </summary>
  public interface IMappingResolver
  {
    /// <summary>
    /// The method takes an <see cref="UnresolvedTableInfo"/> and an <see cref="UniqueIdentifierGenerator"/> 
    /// to generate a <see cref="AbstractTableInfo"/>. The method has to return the sql table for the given <see cref="UnresolvedTableInfo"/>.
    /// </summary>
    /// <param name="tableInfo">The <see cref="UnresolvedTableInfo"/> which is resolved into a <see cref="ResolveTableInfo"/> or 
    /// a <see cref="ResolvedSubStatementTableInfo"/>.</param>
    /// <param name="generator">The <see cref="UniqueIdentifierGenerator"/> which is used to generate unique identifiers for a 
    /// resolved <see cref="AbstractTableInfo"/>.</param>
    /// <returns>The method returns <see cref="AbstractTableInfo"/> which represents a sql table with all needed information.</returns>
    AbstractTableInfo ResolveTableInfo (UnresolvedTableInfo tableInfo, UniqueIdentifierGenerator generator);

    /// <summary>
    /// The method takes a <see cref="SqlTableBase"/>, <see cref="UnresolvedJoinInfo"/> and an <see cref="UniqueIdentifierGenerator"/> to generate an
    /// <see cref="AbstractJoinInfo"/>. The method has to return a sql join between <see cref="SqlTableBase"/> and <see cref="UnresolvedJoinInfo"/>.
    /// </summary>
    /// <param name="joinInfo">The <see cref="UnresolvedTableInfo"/> which represents the sql table which holds the foreign key. 
    /// The <see cref="UnresolvedTableInfo"/> has to be resolved to get the appropriate sql table.</param>
    /// <param name="generator">The <see cref="UniqueIdentifierGenerator"/> which is used to generate unique identifiers for resolved <see cref="AbstractJoinInfo"/>.</param>
    /// <returns>The method returns <see cref="ResolvedJoinInfo"/> which represents a sql join between two sql tables.</returns>
    ResolvedJoinInfo ResolveJoinInfo (UnresolvedJoinInfo joinInfo, UniqueIdentifierGenerator generator);

    /// <summary>
    /// The method analyses the <see cref="SqlTableReferenceExpression"/> and returns the <see cref="SqlEntityExpression"/> which holds a list
    /// of <see cref="SqlColumnExpression"/>s. The methods returns all columns of a sql table.
    /// </summary>
    /// <param name="tableReferenceExpression">The <see cref="SqlTableReferenceExpression"/> which has to be analyzed. 
    /// The expression represents the reference to a sql table.</param>
    /// <param name="generator">TODO: generator is never used. remove? </param>
    /// <returns>The method returns a <see cref="SqlEntityExpression"/> which contains all columns of the referenced sql table.</returns>
    Expression ResolveTableReferenceExpression (SqlTableReferenceExpression tableReferenceExpression, UniqueIdentifierGenerator generator);
    
    /// <summary>
    /// The method takes a <see cref="SqlMemberExpression"/> and <see cref="UniqueIdentifierGenerator"/> to generate a <see cref="SqlColumnExpression"/>
    /// or a <see cref="SqlEntityExpression"/> after analyzing the <see cref="SqlMemberExpression"/>.
    /// </summary>
    /// <param name="memberExpression">The <see cref="SqlMemberExpression"/> which represents the sql specific member expression.</param>
    /// <param name="generator">TODO: generator is never used. remove? </param>
    /// <returns>The method returns a <see cref="SqlColumnExpression"/> for simple columns or a
    /// <see cref="SqlEntityExpression"/> for members representing an entity.</returns>
    Expression ResolveMemberExpression (SqlMemberExpression memberExpression, UniqueIdentifierGenerator generator);

    /// <summary>
    /// The method analyses the given <see cref="ConstantExpression"/> to return a <see cref="SqlEntityConstantExpression"/> if the given 
    /// <see cref="ConstantExpression"/> is an entity or the value of the <see cref="ConstantExpression"/> if not.
    /// </summary>
    /// <param name="constantExpression">The <see cref="ConstantExpression"/> to be analyzed.</param>
    /// <returns>The method returns a <see cref="SqlEntityConstantExpression"/> or a <see cref="ConstantExpression"/>.</returns>
    Expression ResolveConstantExpression (ConstantExpression constantExpression);
  }
}