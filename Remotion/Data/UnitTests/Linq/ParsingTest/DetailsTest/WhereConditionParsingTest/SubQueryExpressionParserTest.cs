// This file is part of the re-motion Core Framework (www.re-motion.org)
// Copyright (C) 2005-2008 rubicon informationstechnologie gmbh, www.rubicon.eu
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
using System.Linq;
using System.Linq.Expressions;
using NUnit.Framework;
using Remotion.Data.Linq.DataObjectModel;
using Remotion.Data.Linq.Expressions;
using Remotion.Data.Linq.Parsing;
using Remotion.Data.Linq.Parsing.Details;
using Remotion.Data.Linq.Parsing.Details.WhereConditionParsing;
using Remotion.Data.Linq.Parsing.FieldResolving;

namespace Remotion.Data.UnitTests.Linq.ParsingTest.DetailsTest.WhereConditionParsingTest
{
  [TestFixture]
  public class SubQueryExpressionParserTest : DetailParserTestBase
  {
    [Test]
    public void CanParse_SubQueryExpression ()
    {
      Data.Linq.QueryModel queryModel = ExpressionHelper.CreateQueryModel ();
      SubQueryExpression subQueryExpression = new SubQueryExpression (queryModel);

      SubQueryExpressionParser subQueryExpressionParser = new SubQueryExpressionParser ();

      Assert.IsTrue (subQueryExpressionParser.CanParse (subQueryExpression));
    }

    [Test]
    public void ParseSubQuery ()
    {
      IQueryable<Student> querySource = ExpressionHelper.CreateQuerySource ();
      Data.Linq.QueryModel queryModel = ExpressionHelper.CreateQueryModel ();
      SubQueryExpression subQueryExpression = new SubQueryExpression (queryModel);
      
      ClauseFieldResolver resolver =
          new ClauseFieldResolver (StubDatabaseInfo.Instance, new WhereFieldAccessPolicy (StubDatabaseInfo.Instance));
      WhereConditionParserRegistry parserRegistry = new WhereConditionParserRegistry (StubDatabaseInfo.Instance);
      parserRegistry.RegisterParser (typeof (ConstantExpression), new ConstantExpressionParser (StubDatabaseInfo.Instance));
      parserRegistry.RegisterParser (typeof (ParameterExpression), new ParameterExpressionParser (resolver));
      parserRegistry.RegisterParser (typeof (MemberExpression), new MemberExpressionParser (resolver));

      SubQueryExpressionParser subQueryExpressionParser = new SubQueryExpressionParser();

      SubQuery expectedSubQuery = new SubQuery (queryModel, ParseMode.SubQueryInSelect, null);

      ICriterion actualCriterion = subQueryExpressionParser.Parse (subQueryExpression, ParseContext);


      Assert.AreEqual (expectedSubQuery, actualCriterion);
    }

    
  }
}