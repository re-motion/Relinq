using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using NUnit.Framework;
using Remotion.Data.Linq.Clauses;
using Remotion.Data.Linq.DataObjectModel;
using Remotion.Data.Linq.Parsing.Details;
using Remotion.Data.Linq.Parsing.Details.WhereConditionParsing;
using Remotion.Data.Linq.Parsing.FieldResolving;

namespace Remotion.Data.Linq.UnitTests.ParsingTest.DetailsTest.WhereConditionParsingTest
{
  [TestFixture]
  public class ContainsFulltextParserTest : DetailParserTestBase
  {
    [Test]
    public void ParseContainsFulltext ()
    {
      var methodName = "ContainsFulltext";
      var pattern = "Test";
      CheckParsingOfContainsFulltext (methodName, pattern);
    }

    public static bool Contains () { return true; }

    private void CheckParsingOfContainsFulltext (string methodName, string pattern)
    {
      ParameterExpression parameter = Expression.Parameter (typeof (Student), "s");
      MemberExpression memberAccess = Expression.MakeMemberAccess (parameter, typeof (Student).GetProperty ("First"));

      MethodCallExpression methodCallExpression = Expression.Call (
          memberAccess,
          typeof (Remotion.Data.Linq.ExtensionMethods.ExtensionMethods).GetMethod (methodName),
          Expression.MakeMemberAccess (Expression.Parameter (typeof (Student), "s"), typeof (Student).GetProperty ("First")),
          Expression.Constant ("Test")
          );

      MainFromClause fromClause = ExpressionHelper.CreateMainFromClause (parameter, ExpressionHelper.CreateQuerySource ());
      QueryModel queryModel = ExpressionHelper.CreateQueryModel (fromClause);
      ClauseFieldResolver resolver =
          new ClauseFieldResolver (StubDatabaseInfo.Instance, new WhereFieldAccessPolicy (StubDatabaseInfo.Instance));

      WhereConditionParserRegistry parserRegistry = new WhereConditionParserRegistry (StubDatabaseInfo.Instance);
      parserRegistry.RegisterParser (typeof (ConstantExpression), new ConstantExpressionParser (StubDatabaseInfo.Instance));
      parserRegistry.RegisterParser (typeof (ParameterExpression), new ParameterExpressionParser (resolver));
      parserRegistry.RegisterParser (typeof (MemberExpression), new MemberExpressionParser (resolver));

      //MethodCallExpressionParser parser = new MethodCallExpressionParser (queryModel.GetExpressionTree (), parserRegistry);
      ContainsFullTextParser parser = new ContainsFullTextParser (parserRegistry);
      

      List<FieldDescriptor> fieldCollection = new List<FieldDescriptor> ();
      ICriterion actualCriterion = parser.Parse (methodCallExpression, ParseContext);
      ICriterion expectedCriterion = new BinaryCondition (new Column (new Table ("studentTable", "s"), "FirstColumn"), new Constant (pattern), BinaryCondition.ConditionKind.ContainsFulltext);
      Assert.AreEqual (expectedCriterion, actualCriterion);
    }
  }
}