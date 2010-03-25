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
using Remotion.Data.Linq.SqlBackend.SqlStatementModel;
using Remotion.Data.Linq.SqlBackend.SqlStatementModel.Resolved;
using Remotion.Data.Linq.SqlBackend.SqlStatementModel.Unresolved;
using Remotion.Data.Linq.Utilities;

namespace Remotion.Data.Linq.SqlBackend.MappingResolution
{
  /// <summary>
  /// <see cref="ResolvingTableInfoVisitor"/> modifies <see cref="UnresolvedTableInfo"/>s and generates <see cref="ResolvedSimpleTableInfo"/>s.
  /// </summary>
  public class ResolvingTableInfoVisitor : ITableInfoVisitor
  {
    private readonly IMappingResolver _resolver;
    private readonly UniqueIdentifierGenerator _generator;
    private readonly IMappingResolutionStage _stage;

    public static AbstractTableInfo ResolveTableInfo (AbstractTableInfo tableInfo, IMappingResolver resolver, UniqueIdentifierGenerator generator, IMappingResolutionStage stage)
    {
      ArgumentUtility.CheckNotNull ("tableInfo", tableInfo);
      ArgumentUtility.CheckNotNull ("resolver", resolver);
      ArgumentUtility.CheckNotNull ("stage", stage);

      var visitor = new ResolvingTableInfoVisitor (resolver, generator, stage);
      return tableInfo.Accept (visitor);
    }

    protected ResolvingTableInfoVisitor (IMappingResolver resolver, UniqueIdentifierGenerator generator, IMappingResolutionStage stage)
    {
      ArgumentUtility.CheckNotNull ("generator", generator);
      ArgumentUtility.CheckNotNull ("resolver", resolver);
      ArgumentUtility.CheckNotNull ("stage", stage);

      _resolver = resolver;
      _generator = generator;
      _stage= stage;
    }

    public AbstractTableInfo VisitUnresolvedTableInfo (UnresolvedTableInfo tableInfo)
    {
      ArgumentUtility.CheckNotNull ("tableInfo", tableInfo);
      var result =  _resolver.ResolveTableInfo (tableInfo, _generator); 
      if (result == tableInfo)
        return result;
      else
        return result.Accept (this);
    }

    public AbstractTableInfo VisitSimpleTableInfo (ResolvedSimpleTableInfo tableInfo)
    {
      ArgumentUtility.CheckNotNull ("tableInfo", tableInfo);
      return tableInfo;
    }

    public AbstractTableInfo VisitSubStatementTableInfo (ResolvedSubStatementTableInfo tableInfo)
    {
      ArgumentUtility.CheckNotNull ("tableInfo", tableInfo);

      _stage.ResolveSqlStatement (tableInfo.SqlStatement);
      return tableInfo;
    }
  }
}