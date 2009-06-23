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
using System.Reflection;
using NUnit.Framework;
using NUnit.Framework.SyntaxHelpers;
using Remotion.Data.Linq;
using Remotion.Data.Linq.Clauses.Expressions;
using Remotion.Data.Linq.DataObjectModel;
using Remotion.Data.Linq.Parsing;
using Remotion.Data.Linq.Parsing.Details;
using Remotion.Data.Linq.Parsing.Details.WhereConditionParsing;
using Remotion.Data.Linq.Parsing.FieldResolving;

namespace Remotion.Data.UnitTests.Linq.Parsing.Details.WhereConditionParsing
{
  [TestFixture]
  public class ContainsParserTest : DetailParserTestBase
  {
    [Test]
    public void ParseContainsWithSubQuery ()
    {
      IQueryable<Student> querySource = ExpressionHelper.CreateQuerySource();
      QueryModel subQueryModel = ExpressionHelper.CreateQueryModel();
      var subQueryExpression = new SubQueryExpression (subQueryModel);
      var item = new Student();
      ConstantExpression checkedExpression = Expression.Constant (item);

      MethodInfo containsMethod = ParserUtility.GetMethod (() => querySource.Contains (item));
      MethodCallExpression methodCallExpression = Expression.Call (
          Student_First_Expression,
          containsMethod,
          subQueryExpression,
          checkedExpression
          );

      var resolver =
          new ClauseFieldResolver (StubDatabaseInfo.Instance, new WhereFieldAccessPolicy (StubDatabaseInfo.Instance));
      var parserRegistry = new WhereConditionParserRegistry (StubDatabaseInfo.Instance);
      parserRegistry.RegisterParser (typeof (ConstantExpression), new ConstantExpressionParser (StubDatabaseInfo.Instance));
      parserRegistry.RegisterParser (typeof (MemberExpression), new MemberExpressionParser (resolver));

      var parser = new ContainsParser (parserRegistry);

      ICriterion actualCriterion = parser.Parse (methodCallExpression, ParseContext);
      var expectedSubQuery = new SubQuery (subQueryModel, ParseMode.SubQueryInSelect, null);
      IValue expectedCheckedItem = new Constant (0);
      ICriterion expectedCriterion = new BinaryCondition (expectedSubQuery, expectedCheckedItem, BinaryCondition.ConditionKind.Contains);

      Assert.That (actualCriterion, Is.EqualTo (expectedCriterion));
    }

    [Test]
    [ExpectedException (typeof (ParserException), ExpectedMessage = "Expected Contains with expression for method call expression in where "
                                                                    + "condition, found 'Contains'.")]
    public void ParseContains_NoArguments ()
    {
      MethodInfo containsMethod = typeof (ContainsParserTest).GetMethod ("Contains");
      MethodCallExpression methodCallExpression = Expression.Call (
          null,
          containsMethod);
      var resolver =
          new ClauseFieldResolver (StubDatabaseInfo.Instance, new WhereFieldAccessPolicy (StubDatabaseInfo.Instance));

      var parserRegistry = new WhereConditionParserRegistry (StubDatabaseInfo.Instance);
      parserRegistry.RegisterParser (typeof (ConstantExpression), new ConstantExpressionParser (StubDatabaseInfo.Instance));
      parserRegistry.RegisterParser (typeof (MemberExpression), new MemberExpressionParser (resolver));

      var parser = new ContainsParser (parserRegistry);

      parser.Parse (methodCallExpression, ParseContext);
    }

    public static bool Contains ()
    {
      return true;
    }
  }
}
