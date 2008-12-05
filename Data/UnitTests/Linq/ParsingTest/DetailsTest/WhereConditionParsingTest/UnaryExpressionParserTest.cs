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
using System.Linq.Expressions;
using NUnit.Framework;
using System.Collections.Generic;
using Remotion.Data.Linq;
using Remotion.Data.Linq.Clauses;
using Remotion.Data.Linq.DataObjectModel;
using Remotion.Data.Linq.Parsing.Details;
using Remotion.Data.Linq.Parsing.Details.WhereConditionParsing;
using Remotion.Data.Linq.Parsing.FieldResolving;

namespace Remotion.Data.UnitTests.Linq.ParsingTest.DetailsTest.WhereConditionParsingTest
{
  [TestFixture]
  public class UnaryExpressionParserTest : DetailParserTestBase
  {
    [Test]
    public void Parse ()
    {
      QueryModel queryModel = ExpressionHelper.CreateQueryModel();
      UnaryExpression unaryExpression = Expression.Not (Expression.Constant (5));
      ICriterion expectedCriterion = new NotCriterion (new Constant (5));

      WhereConditionParserRegistry parserRegistry = 
        new WhereConditionParserRegistry (StubDatabaseInfo.Instance);
      parserRegistry.RegisterParser (typeof(ConstantExpression), new ConstantExpressionParser (StubDatabaseInfo.Instance));

      UnaryExpressionParser parser = new UnaryExpressionParser(parserRegistry);

      ICriterion actualCriterion = parser.Parse (unaryExpression, ParseContext);
      Assert.AreEqual (expectedCriterion, actualCriterion);
    }
  }
}
