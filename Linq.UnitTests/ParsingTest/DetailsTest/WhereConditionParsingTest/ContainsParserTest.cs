using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using NUnit.Framework;
using Remotion.Data.Linq.Clauses;
using Remotion.Data.Linq.DataObjectModel;
using Remotion.Data.Linq.Parsing;
using Remotion.Data.Linq.Parsing.Details;
using Remotion.Data.Linq.Parsing.Details.WhereConditionParsing;
using Remotion.Data.Linq.Parsing.FieldResolving;

namespace Remotion.Data.Linq.UnitTests.ParsingTest.DetailsTest.WhereConditionParsingTest
{
  [TestFixture]
  public class ContainsParserTest
  {
    [Test]
    public void ParseContainsWithSubQuery ()
    {
      IQueryable<Student> querySource = ExpressionHelper.CreateQuerySource ();
      QueryModel queryModel = ExpressionHelper.CreateQueryModel ();
      SubQueryExpression subQueryExpression = new SubQueryExpression (queryModel);
      Student item = new Student ();
      ConstantExpression checkedExpression = Expression.Constant (item);
      ParameterExpression parameter = Expression.Parameter (typeof (Student), "s");
      MemberExpression memberAccess = Expression.MakeMemberAccess (parameter, typeof (Student).GetProperty ("First"));

      MethodInfo containsMethod = ParserUtility.GetMethod (() => querySource.Contains (item));
      MethodCallExpression methodCallExpression = Expression.Call (
          memberAccess,
          containsMethod,
          subQueryExpression,
          checkedExpression
          );

      ClauseFieldResolver resolver =
          new ClauseFieldResolver (StubDatabaseInfo.Instance, new JoinedTableContext (), new WhereFieldAccessPolicy (StubDatabaseInfo.Instance));
      WhereConditionParserRegistry parserRegistry = new WhereConditionParserRegistry (queryModel, StubDatabaseInfo.Instance, new JoinedTableContext ());
      parserRegistry.RegisterParser (typeof (ConstantExpression), new ConstantExpressionParser (StubDatabaseInfo.Instance));
      parserRegistry.RegisterParser (typeof (ParameterExpression), new ParameterExpressionParser (queryModel, resolver));
      parserRegistry.RegisterParser (typeof (MemberExpression), new MemberExpressionParser (queryModel, resolver));

      //MethodCallExpressionParser parser = new MethodCallExpressionParser (queryModel.GetExpressionTree (), parserRegistry);
      ContainsParser parser = new ContainsParser (queryModel.GetExpressionTree(), parserRegistry);

      List<FieldDescriptor> fieldCollection = new List<FieldDescriptor> ();
      ICriterion actualCriterion = parser.Parse (methodCallExpression, fieldCollection);
      SubQuery expectedSubQuery = new SubQuery (queryModel, null);
      IValue expectedCheckedItem = new Constant (0);
      ICriterion expectedCriterion = new BinaryCondition (expectedSubQuery, expectedCheckedItem, BinaryCondition.ConditionKind.Contains);

      Assert.AreEqual (expectedCriterion, actualCriterion);
    }

    [Test]
    [ExpectedException (typeof (ParserException), ExpectedMessage = "Expected SubQueryExpression for argument 0 of Contains method call, found "
        + "ConstantExpression (null).")]
    public void ParseContains_NoSubQueryExpression ()
    {
      WhereClause whereClause = ExpressionHelper.CreateWhereClause ();

      IQueryable<Student> querySource = ExpressionHelper.CreateQuerySource ();
      Student item = new Student ();
      ConstantExpression checkedExpression = Expression.Constant (item);
      QueryModel queryModel = ExpressionHelper.CreateQueryModel ();
      ParameterExpression parameter = Expression.Parameter (typeof (Student), "s");
      MainFromClause fromClause = ExpressionHelper.CreateMainFromClause (parameter, ExpressionHelper.CreateQuerySource ());
      ClauseFieldResolver resolver =
          new ClauseFieldResolver (StubDatabaseInfo.Instance, new JoinedTableContext (), new WhereFieldAccessPolicy (StubDatabaseInfo.Instance));

      MethodInfo containsMethod = ParserUtility.GetMethod (() => querySource.Contains (item));
      MethodCallExpression methodCallExpression = Expression.Call (
          null,
          containsMethod,
          Expression.Constant (null, typeof (IQueryable<Student>)),
          checkedExpression
          );

      WhereConditionParserRegistry parserRegistry = new WhereConditionParserRegistry (queryModel, StubDatabaseInfo.Instance, new JoinedTableContext ());
      parserRegistry.RegisterParser (typeof (ConstantExpression), new ConstantExpressionParser (StubDatabaseInfo.Instance));
      parserRegistry.RegisterParser (typeof (ParameterExpression), new ParameterExpressionParser (queryModel, resolver));
      parserRegistry.RegisterParser (typeof (MemberExpression), new MemberExpressionParser (queryModel, resolver));

      ContainsParser parser = new ContainsParser (queryModel.GetExpressionTree (), parserRegistry);
      
      List<FieldDescriptor> fieldCollection = new List<FieldDescriptor> ();
      parser.Parse (methodCallExpression, fieldCollection);
    }

    [Test]
    [ExpectedException (typeof (ParserException), ExpectedMessage = "Expected Contains with expression for method call expression in where "
      + "condition, found Contains.")]
    public void ParseContains_NoArguments ()
    {
      WhereClause whereClause = ExpressionHelper.CreateWhereClause ();

      MethodInfo containsMethod = typeof (ContainsParserTest).GetMethod ("Contains");
      MethodCallExpression methodCallExpression = Expression.Call (
          null,
          containsMethod
          );
      QueryModel queryModel = ExpressionHelper.CreateQueryModel ();
      ParameterExpression parameter = Expression.Parameter (typeof (Student), "s");
      MainFromClause fromClause = ExpressionHelper.CreateMainFromClause (parameter, ExpressionHelper.CreateQuerySource ());
      ClauseFieldResolver resolver =
          new ClauseFieldResolver (StubDatabaseInfo.Instance, new JoinedTableContext (), new WhereFieldAccessPolicy (StubDatabaseInfo.Instance));


      WhereConditionParserRegistry parserRegistry = new WhereConditionParserRegistry (queryModel, StubDatabaseInfo.Instance, new JoinedTableContext ());
      parserRegistry.RegisterParser (typeof (ConstantExpression), new ConstantExpressionParser (StubDatabaseInfo.Instance));
      parserRegistry.RegisterParser (typeof (ParameterExpression), new ParameterExpressionParser (queryModel, resolver));
      parserRegistry.RegisterParser (typeof (MemberExpression), new MemberExpressionParser (queryModel, resolver));

      ContainsParser parser = new ContainsParser (queryModel.GetExpressionTree (), parserRegistry);
      
      List<FieldDescriptor> fieldCollection = new List<FieldDescriptor> ();
      parser.Parse (methodCallExpression, fieldCollection);
    }

    public static bool Contains () { return true; }
  }
}