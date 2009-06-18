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
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using NUnit.Framework;
using Remotion.Data.Linq.Clauses;
using Remotion.Data.Linq.DataObjectModel;
using Remotion.Data.Linq.EagerFetching;
using Remotion.Data.Linq.Parsing.Details;
using Remotion.Data.Linq.Parsing.FieldResolving;
using Remotion.Data.Linq.Parsing.Structure;
using Remotion.Data.Linq.SqlGeneration;
using Remotion.Data.Linq.Parsing;
using NUnit.Framework.SyntaxHelpers;
using Remotion.Data.Linq.ExtensionMethods;
using System.Diagnostics;

namespace Remotion.Data.UnitTests.Linq.Parsing.Structure
{
  // TODO 1096: Remove.
  [TestFixture]
  public class QueryParserNameHackTest
  {
    private StubDatabaseInfo _databaseInfo;
    private SelectProjectionParserRegistry _parserRegistry;

    [SetUp]
    public void SetUp ()
    {
      _databaseInfo = StubDatabaseInfo.Instance;
      _parserRegistry = new SelectProjectionParserRegistry (_databaseInfo, ParseMode.TopLevelQuery);
    }

    [Test]
    public void MissingSelect_AfterSelectMany_CanBeResolved ()
    {
      var query = from s1 in ExpressionHelper.CreateQuerySource()
                  from s2 in ExpressionHelper.CreateQuerySource()
                  select s1;

      var queryModel = new QueryParser().GetParsedQuery (query.Expression);
      var mainFromTable = queryModel.MainFromClause.GetColumnSource (_databaseInfo);

      var selectClause = ((SelectClause) queryModel.SelectOrGroupClause);
      
      var parser = _parserRegistry.GetParser (selectClause.Selector);
      var evaluation = parser.Parse (
          selectClause.Selector, new ParseContext (queryModel, query.Expression, new List<FieldDescriptor>(), new JoinedTableContext()));

      Assert.That (evaluation, Is.InstanceOfType (typeof (Column)));
      Assert.That (((Column) evaluation).ColumnSource,  Is.SameAs (mainFromTable));
      Assert.That (((Column) evaluation).Name, Is.EqualTo ("*"));
    }
    
    [Test]
    public void MissingSelect_AfterMainFromClause_CanBeResolved ()
    {
      var query = ExpressionHelper.CreateQuerySource ();

      var queryModel = new QueryParser ().GetParsedQuery (query.Expression);
      var mainFromTable = queryModel.MainFromClause.GetColumnSource (_databaseInfo);

      var selectClause = ((SelectClause) queryModel.SelectOrGroupClause);

      var parser = _parserRegistry.GetParser (selectClause.Selector);
      var evaluation = parser.Parse (
          selectClause.Selector, new ParseContext (queryModel, query.Expression, new List<FieldDescriptor> (), new JoinedTableContext ()));

      Assert.That (evaluation, Is.InstanceOfType (typeof (Column)));
      Assert.That (((Column) evaluation).ColumnSource, Is.SameAs (mainFromTable));
      Assert.That (((Column) evaluation).Name, Is.EqualTo ("*"));
    }

    [Test]
    public void MissingSelect_AfterSelectMany_WithSubQuery_CanBeResolved ()
    {
      var query = from s1 in ExpressionHelper.CreateQuerySource ()
                  from s2 in (from s3 in ExpressionHelper.CreateQuerySource () select s3)
                  select s1;

      var queryModel = new QueryParser ().GetParsedQuery (query.Expression);
      var mainFromTable = queryModel.MainFromClause.GetColumnSource (_databaseInfo);

      var selectClause = ((SelectClause) queryModel.SelectOrGroupClause);

      var parser = _parserRegistry.GetParser (selectClause.Selector);
      var evaluation = parser.Parse (
          selectClause.Selector, new ParseContext (queryModel, query.Expression, new List<FieldDescriptor> (), new JoinedTableContext ()));

      Assert.That (evaluation, Is.InstanceOfType (typeof (Column)));
      Assert.That (((Column) evaluation).ColumnSource, Is.SameAs (mainFromTable));
      Assert.That (((Column) evaluation).Name, Is.EqualTo ("*"));
    }
  }
}