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
using System.Linq;
using System.Linq.Expressions;
using NUnit.Framework;
using NUnit.Framework.SyntaxHelpers;
using Remotion.Data.Linq;
using Remotion.Data.Linq.Backend;
using Remotion.Data.Linq.Clauses.Expressions;
using Remotion.Data.Linq.Backend.DataObjectModel;
using Remotion.Data.Linq.Backend.Details;
using Remotion.Data.Linq.Backend.Details.WhereConditionParsing;
using Remotion.Data.Linq.Backend.FieldResolving;

namespace Remotion.Data.UnitTests.Linq.Parsing.Details.WhereConditionParsing
{
  [TestFixture]
  public class SubQueryExpressionParserTest : DetailParserTestBase
  {
    [Test]
    public void CanParse_SubQueryExpression ()
    {
      var subQueryExpression = new SubQueryExpression (ExpressionHelper.CreateQueryModel());

      var subQueryExpressionParser = new SubQueryExpressionParser();
      Assert.That (subQueryExpressionParser.CanParse (subQueryExpression), Is.True);
    }

    [Test]
    public void ParseSubQuery ()
    {
      QueryModel subQueryModel = ExpressionHelper.CreateQueryModel();
      var subQueryExpression = new SubQueryExpression (subQueryModel);

      var resolver =
          new FieldResolver (StubDatabaseInfo.Instance, new WhereFieldAccessPolicy (StubDatabaseInfo.Instance));
      var parserRegistry = new WhereConditionParserRegistry (StubDatabaseInfo.Instance);
      parserRegistry.RegisterParser (typeof (ConstantExpression), new ConstantExpressionParser (StubDatabaseInfo.Instance));
      parserRegistry.RegisterParser (typeof (MemberExpression), new MemberExpressionParser (resolver));

      var subQueryExpressionParser = new SubQueryExpressionParser();

      var expectedSubQuery = new SubQuery (subQueryModel, ParseMode.SubQueryInSelect, null);

      ICriterion actualCriterion = subQueryExpressionParser.Parse (subQueryExpression, ParseContext);

      Assert.That (actualCriterion, Is.EqualTo (expectedSubQuery));
    }
  }
}