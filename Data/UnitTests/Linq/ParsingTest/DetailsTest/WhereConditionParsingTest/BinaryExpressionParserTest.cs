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
using System.Collections.Generic;
using System.Linq.Expressions;
using NUnit.Framework;
using Remotion.Data.Linq.Clauses;
using Remotion.Data.Linq.DataObjectModel;
using Remotion.Data.Linq.Parsing.Details;
using Remotion.Data.Linq.Parsing.Details.WhereConditionParsing;
using Remotion.Data.Linq.Parsing.FieldResolving;

namespace Remotion.Data.UnitTests.Linq.ParsingTest.DetailsTest.WhereConditionParsingTest
{
  [TestFixture]
  public class BinaryExpressionParserTest : DetailParserTestBase
  {
    [Test]
    public void ParseAnd ()
    {
      BinaryExpression binaryExpression = Expression.And (Expression.Constant (5), Expression.Constant (5));

      WhereConditionParserRegistry parserRegistry = new WhereConditionParserRegistry (StubDatabaseInfo.Instance);
      parserRegistry.RegisterParser (typeof(ConstantExpression), new ConstantExpressionParser (StubDatabaseInfo.Instance));

      BinaryExpressionParser binaryExpressionParser = new BinaryExpressionParser (parserRegistry);
      parserRegistry.RegisterParser (typeof (BinaryExpression),binaryExpressionParser);
      ICriterion actualCriterion = binaryExpressionParser.Parse (binaryExpression, ParseContext);

      ICriterion expectedCriterion = new ComplexCriterion (new Constant (5), new Constant (5), ComplexCriterion.JunctionKind.And);
      
      Assert.AreEqual (expectedCriterion, actualCriterion);
    }

    [Test]
    public void ParseOr ()
    {
      WhereClause whereClause = ExpressionHelper.CreateWhereClause ();
      BinaryExpression binaryExpression = Expression.Or (Expression.Constant (5), Expression.Constant (5));
      ICriterion criterion = new Constant (5);

      WhereConditionParserRegistry parserRegistry = new WhereConditionParserRegistry (StubDatabaseInfo.Instance);
      parserRegistry.RegisterParser (typeof(ConstantExpression), new ConstantExpressionParser (StubDatabaseInfo.Instance));

      BinaryExpressionParser parser = new BinaryExpressionParser (parserRegistry);

      ICriterion actualCriterion = parser.Parse (binaryExpression, ParseContext);
      ICriterion expectedCriterion = new ComplexCriterion (new Constant (5), new Constant (5), ComplexCriterion.JunctionKind.Or);
      Assert.AreEqual (expectedCriterion, actualCriterion);
    }

    [Test]
    public void ParseEqual ()
    {
      WhereClause whereClause = ExpressionHelper.CreateWhereClause ();
      BinaryExpression binaryExpression = Expression.Equal (Expression.Constant (5), Expression.Constant (5));
      ICriterion criterion = new Constant (5);
      WhereConditionParserRegistry parserRegistry = new WhereConditionParserRegistry (StubDatabaseInfo.Instance);
      parserRegistry.RegisterParser (typeof (ConstantExpression), new ConstantExpressionParser (StubDatabaseInfo.Instance));

      BinaryExpressionParser parser = new BinaryExpressionParser (parserRegistry);

      ICriterion actualCriterion = parser.Parse (binaryExpression, ParseContext);
      ICriterion expectedCriterion = new BinaryCondition (new Constant (5), new Constant (5), BinaryCondition.ConditionKind.Equal);
      Assert.AreEqual (expectedCriterion, actualCriterion);
    }

    [Test]
    public void ParseGreaterThan ()
    {
      WhereClause whereClause = ExpressionHelper.CreateWhereClause ();
      BinaryExpression binaryExpression = Expression.GreaterThan (Expression.Constant (5), Expression.Constant (5));
      ICriterion criterion = new Constant (5);
      WhereConditionParserRegistry parserRegistry = new WhereConditionParserRegistry (StubDatabaseInfo.Instance);
      parserRegistry.RegisterParser (typeof(ConstantExpression), new ConstantExpressionParser (StubDatabaseInfo.Instance));

      BinaryExpressionParser parser = new BinaryExpressionParser (parserRegistry);

      ICriterion actualCriterion = parser.Parse (binaryExpression, ParseContext);
      ICriterion expectedCriterion = new BinaryCondition (new Constant (5), new Constant (5), BinaryCondition.ConditionKind.GreaterThan);
      Assert.AreEqual (expectedCriterion, actualCriterion);
    }

    [Test]
    public void ParseLessThan ()
    {
      WhereClause whereClause = ExpressionHelper.CreateWhereClause ();
      BinaryExpression binaryExpression = Expression.LessThan (Expression.Constant (5), Expression.Constant (5));
      ICriterion criterion = new Constant (5);
      WhereConditionParserRegistry parserRegistry = new WhereConditionParserRegistry (StubDatabaseInfo.Instance);
      parserRegistry.RegisterParser (typeof (ConstantExpression), new ConstantExpressionParser (StubDatabaseInfo.Instance));

      BinaryExpressionParser parser = new BinaryExpressionParser (parserRegistry);

      ICriterion actualCriterion = parser.Parse (binaryExpression, ParseContext);
      ICriterion expectedCriterion = new BinaryCondition (new Constant (5), new Constant (5), BinaryCondition.ConditionKind.LessThan);
      Assert.AreEqual (expectedCriterion, actualCriterion);
    }
  }
}
