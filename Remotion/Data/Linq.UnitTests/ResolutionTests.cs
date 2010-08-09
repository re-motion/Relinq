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
using System.Data.Linq.Mapping;
using System.Linq.Expressions;
using System.Reflection;
using NUnit.Framework;
using Remotion.Data.Linq.IntegrationTests.TestDomain.Northwind;
using Remotion.Data.Linq.SqlBackend.MappingResolution;
using Remotion.Data.Linq.SqlBackend.SqlStatementModel;
using Remotion.Data.Linq.SqlBackend.SqlStatementModel.Resolved;
using Remotion.Data.Linq.SqlBackend.SqlStatementModel.Unresolved;
using Remotion.Data.Linq.UnitTests.Linq.SqlBackend.SqlStatementModel;

namespace Remotion.Data.Linq.UnitTests
{
  [TestFixture]
  public class ResolutionTests
  {
    private UniqueIdentifierGenerator _uniqueIdentifierGenerator;
    private UnresolvedTableInfo _unresolvedTableInfo;
    private SqlTable _sqlTable;
    private DefaultMappingResolutionStage _stage;
    private IMappingResolutionContext _mappingResolutionContext;

    [SetUp]
    public void SetUp ()
    {
      _uniqueIdentifierGenerator = new UniqueIdentifierGenerator();

      _unresolvedTableInfo = SqlStatementModelObjectMother.CreateUnresolvedTableInfo (typeof (Customer));
      _sqlTable = SqlStatementModelObjectMother.CreateSqlTable (_unresolvedTableInfo);

      _stage = new DefaultMappingResolutionStage (new MappingResolver(), _uniqueIdentifierGenerator);

      _mappingResolutionContext = new MappingResolutionContext();
    }

    [Test]
    public void MappingResolverResolveTableInfoTest ()
    {
      var result = _stage.ResolveTableInfo (_sqlTable.TableInfo, _mappingResolutionContext);
      var excpectedResult = new ResolvedSimpleTableInfo (typeof (Customer), "dbo.Customers", "t0");

      Assert.AreEqual (result.ToString(), excpectedResult.ToString());
    }
  }

  public class MappingResolver : IMappingResolver
  {
    private Northwind _db;
    private Northwind DB
    {
      get
      {
        if(_db==null)
          _db = new Northwind ("Data Source=localhost;Initial Catalog=Northwind; Integrated Security=SSPI;");

         return _db;
      }
    }

    public IResolvedTableInfo ResolveTableInfo (UnresolvedTableInfo tableInfo, UniqueIdentifierGenerator generator)
    {
      MetaTable resolvedTable = DB.Mapping.GetTable (tableInfo.ItemType); 
      return new ResolvedSimpleTableInfo (tableInfo.ItemType, resolvedTable.TableName, generator.GetUniqueIdentifier ("t"));
    }

    public ResolvedJoinInfo ResolveJoinInfo (UnresolvedJoinInfo joinInfo, UniqueIdentifierGenerator generator)
    {
      throw new NotImplementedException();
    }

    public SqlEntityDefinitionExpression ResolveSimpleTableInfo (IResolvedTableInfo tableInfo, UniqueIdentifierGenerator generator)
    {
      throw new NotImplementedException();
    }

    public Expression ResolveMemberExpression (SqlEntityExpression originatingEntity, MemberInfo memberInfo)
    {
      throw new NotImplementedException();
    }

    public Expression ResolveMemberExpression (SqlColumnExpression sqlColumnExpression, MemberInfo memberInfo)
    {
      throw new NotImplementedException();
    }

    public Expression ResolveConstantExpression (ConstantExpression constantExpression)
    {
      throw new NotImplementedException();
    }

    public Expression ResolveTypeCheck (Expression expression, Type desiredType)
    {
      throw new NotImplementedException();
    }
  }
} 