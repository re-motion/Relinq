using System;
using System.Linq;
using System.Linq.Expressions;
using NUnit.Framework;
using Remotion.Data.Linq.Clauses;
using Remotion.Data.Linq.DataObjectModel;
using Remotion.Data.Linq.Parsing;
using Remotion.Data.Linq.Parsing.Details;
using Remotion.Data.Linq.Parsing.Details.WhereConditionParsing;
using System.Reflection;
using System.Collections.Generic;
using NUnit.Framework.SyntaxHelpers;
using Remotion.Data.Linq.Parsing.FieldResolving;

namespace Remotion.Data.Linq.UnitTests.ParsingTest.DetailsTest.WhereConditionParsingTest
{
  [TestFixture]
  public class MethodCallExpressionParserTest
  {
    [Test]
    public void ParseStartsWith ()
    {
      var methodName = "StartsWith";
      var pattern = "Test%";
      CheckParsingOfLikeVariant (methodName, pattern);
    }

    [Test]
    [ExpectedException (typeof (ParserException), ExpectedMessage = "Expected ConstantExpression for argument 0 of StartsWith method call, "
        + "found ParameterExpression (Test).")]
    public void ParseStartsWith_NoConstantExpression ()
    {
      var methodName = "StartsWith";
      CheckParsingOfLikeVariant_NoConstantExpression (methodName);
    }

    [Test]
    [ExpectedException (typeof (ParserException), ExpectedMessage = "Expected at least 1 argument for StartsWith method call, found 0 arguments.")]
    public void ParseStartsWith_NoArguments ()
    {
      var methodName = "StartsWith";
      CheckParsingOfLikeVariant_NoArguments (methodName);
    }

    [Test]
    public void ParseEndsWith ()
    {
      var methodName = "EndsWith";
      var pattern = "%Test";
      CheckParsingOfLikeVariant (methodName, pattern);
    }

    [Test]
    [Ignore("TODO: Check comparision")]
    public void ParseContainsWithConstantExpression ()
    {
      WhereClause whereClause = ExpressionHelper.CreateWhereClause ();
      ParameterExpression parameter = Expression.Parameter (typeof (Student), "s");
      MemberExpression memberAccess = Expression.MakeMemberAccess (parameter, typeof (Student).GetProperty ("First"));

      ConstantExpression checkedExpression = Expression.Constant ("Test");

      MethodCallExpression methodCallExpression = Expression.Call (
          memberAccess,
          typeof (string).GetMethod ("Contains", new Type[] { typeof (string) }),
          Expression.Constant ("Test")
          );

      ICriterion criterion = new Column (new Table ("Student", "s"), "First");

      MainFromClause fromClause = ExpressionHelper.CreateMainFromClause (parameter, ExpressionHelper.CreateQuerySource ());
      QueryModel queryModel = ExpressionHelper.CreateQueryModel (fromClause);
      ClauseFieldResolver resolver =
          new ClauseFieldResolver (StubDatabaseInfo.Instance, new JoinedTableContext (), new WhereFieldAccessPolicy (StubDatabaseInfo.Instance));


      ParserRegistry parserRegistry = new ParserRegistry ();
      parserRegistry.RegisterParser (new ConstantExpressionParser (StubDatabaseInfo.Instance));
      parserRegistry.RegisterParser (new ParameterExpressionParser (queryModel, resolver));
      parserRegistry.RegisterParser (new MemberExpressionParser (queryModel, resolver));

      MethodCallExpressionParser parser = new MethodCallExpressionParser (queryModel, parserRegistry);
      
      List<FieldDescriptor> fieldCollection = new List<FieldDescriptor> ();
      ICriterion actualCriterion = parser.Parse (methodCallExpression, fieldCollection);
      ICriterion expectedCriterion = new BinaryCondition (new Column (new Table ("Student", "s"), "FirstColumn"), new Constant ("%Test%"), BinaryCondition.ConditionKind.Like);
      Assert.AreEqual (expectedCriterion, actualCriterion);
    }


    [Test]
    [ExpectedException (typeof (ParserException), ExpectedMessage = "Expected ConstantExpression for argument 0 of EndsWith method call, "
        + "found ParameterExpression (Test).")]
    public void ParseEndsWith_NoConstantExpression ()
    {
      var methodName = "EndsWith";
      CheckParsingOfLikeVariant_NoConstantExpression (methodName);
    }

    [Test]
    [ExpectedException (typeof (ParserException), ExpectedMessage = "Expected at least 1 argument for EndsWith method call, found 0 arguments.")]
    public void ParseEndsWith_NoArguments ()
    {
      var methodName = "EndsWith";
      CheckParsingOfLikeVariant_NoArguments (methodName);
    }

    public static bool StartsWith () { return true; }
    public static bool EndsWith () { return true; }


    [Test]
    [ExpectedException (typeof (Remotion.Data.Linq.Parsing.ParserException), ExpectedMessage = "Expected StartsWith, EndsWith, Contains, ContainsFulltext "
        + "for method call expression in where condition, found Equals.")]
    public void Parse_WithException ()
    {
      WhereClause whereClause = ExpressionHelper.CreateWhereClause ();

      ParameterExpression parameter = Expression.Parameter (typeof (Student), "s");
      MemberExpression memberAccess = Expression.MakeMemberAccess (parameter, typeof (Student).GetProperty ("First"));
      MethodCallExpression methodCallExpression = Expression.Call (
          memberAccess,
          typeof (string).GetMethod ("Equals", new Type[] { typeof (object) }),
          Expression.Constant ("Test")
          );

      ICriterion criterion = new Constant ("Test");

      MainFromClause fromClause = ExpressionHelper.CreateMainFromClause (parameter, ExpressionHelper.CreateQuerySource ());
      QueryModel queryModel = ExpressionHelper.CreateQueryModel (fromClause);
      ClauseFieldResolver resolver =
          new ClauseFieldResolver (StubDatabaseInfo.Instance, new JoinedTableContext (), new WhereFieldAccessPolicy (StubDatabaseInfo.Instance));
      ParserRegistry parserRegistry = new ParserRegistry ();
      parserRegistry.RegisterParser (new ConstantExpressionParser (StubDatabaseInfo.Instance));
      parserRegistry.RegisterParser (new ParameterExpressionParser (queryModel, resolver));
      parserRegistry.RegisterParser (new MemberExpressionParser (queryModel, resolver));

      MethodCallExpressionParser parser = new MethodCallExpressionParser (queryModel, parserRegistry);

      List<FieldDescriptor> fieldCollection = new List<FieldDescriptor> ();
      parser.Parse (methodCallExpression, fieldCollection);
    }

    [Test]
    public void ParseContainsWithSubQuery ()
    {
      WhereClause whereClause = ExpressionHelper.CreateWhereClause ();

      IQueryable<Student> querySource = ExpressionHelper.CreateQuerySource ();
      QueryModel queryModel = ExpressionHelper.CreateQueryModel ();
      SubQueryExpression subQueryExpression = new SubQueryExpression (queryModel);
      Student item = new Student ();
      ConstantExpression checkedExpression = Expression.Constant (item);
      ParameterExpression parameter = Expression.Parameter (typeof (Student), "s");
      MemberExpression memberAccess = Expression.MakeMemberAccess (parameter, typeof (Student).GetProperty ("First"));

      MethodInfo containsMethod = ExpressionHelper.GetMethod (() => querySource.Contains (item));
      MethodCallExpression methodCallExpression = Expression.Call (
          memberAccess,
          containsMethod,
          subQueryExpression,
          checkedExpression
          );

      MainFromClause fromClause = ExpressionHelper.CreateMainFromClause (parameter, ExpressionHelper.CreateQuerySource ());
      ClauseFieldResolver resolver =
          new ClauseFieldResolver (StubDatabaseInfo.Instance, new JoinedTableContext (), new WhereFieldAccessPolicy (StubDatabaseInfo.Instance));
      ParserRegistry parserRegistry = new ParserRegistry ();
      parserRegistry.RegisterParser (new ConstantExpressionParser (StubDatabaseInfo.Instance));
      parserRegistry.RegisterParser (new ParameterExpressionParser (queryModel, resolver));
      parserRegistry.RegisterParser (new MemberExpressionParser (queryModel, resolver));

      MethodCallExpressionParser parser = new MethodCallExpressionParser (queryModel, parserRegistry);

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
      
      MethodInfo containsMethod = ExpressionHelper.GetMethod (() => querySource.Contains (item));
      MethodCallExpression methodCallExpression = Expression.Call (
          null,
          containsMethod,
          Expression.Constant (null, typeof (IQueryable<Student>)),
          checkedExpression
          );

      ParserRegistry parserRegistry = new ParserRegistry ();
      parserRegistry.RegisterParser (new ConstantExpressionParser (StubDatabaseInfo.Instance));
      parserRegistry.RegisterParser (new ParameterExpressionParser (queryModel, resolver));
      parserRegistry.RegisterParser (new MemberExpressionParser (queryModel, resolver));

      MethodCallExpressionParser parser = new MethodCallExpressionParser (queryModel, parserRegistry);

      List<FieldDescriptor> fieldCollection = new List<FieldDescriptor> ();
      parser.Parse (methodCallExpression, fieldCollection);
    }

    [Test]
    [ExpectedException (typeof (ParserException), ExpectedMessage = "Expected at least 1 argument for Contains method call, found 0 arguments.")]
    public void ParseContains_NoArguments ()
    {
      WhereClause whereClause = ExpressionHelper.CreateWhereClause ();

      MethodInfo containsMethod = typeof (MethodCallExpressionParserTest).GetMethod ("Contains");
      MethodCallExpression methodCallExpression = Expression.Call (
          null,
          containsMethod
          );
      QueryModel queryModel = ExpressionHelper.CreateQueryModel ();
      ParameterExpression parameter = Expression.Parameter (typeof (Student), "s");
      MainFromClause fromClause = ExpressionHelper.CreateMainFromClause (parameter, ExpressionHelper.CreateQuerySource ());
      ClauseFieldResolver resolver =
          new ClauseFieldResolver (StubDatabaseInfo.Instance, new JoinedTableContext (), new WhereFieldAccessPolicy (StubDatabaseInfo.Instance));
      

      ParserRegistry parserRegistry = new ParserRegistry ();
      parserRegistry.RegisterParser (new ConstantExpressionParser (StubDatabaseInfo.Instance));
      parserRegistry.RegisterParser (new ParameterExpressionParser (queryModel, resolver));
      parserRegistry.RegisterParser (new MemberExpressionParser (queryModel, resolver));

      MethodCallExpressionParser parser = new MethodCallExpressionParser (queryModel, parserRegistry);
      //MethodCallExpressionParser parser = new MethodCallExpressionParser (whereClause, delegate { return null; });
      List<FieldDescriptor> fieldCollection = new List<FieldDescriptor> ();
      parser.Parse (methodCallExpression, fieldCollection);
    }

    [Test]
    public void ParseContainsFulltext ()
    {
      var methodName = "ContainsFulltext";
      var pattern = "Test";
      CheckParsingOfContainsFulltext (methodName, pattern);
    }

    public static bool Contains () { return true; }

    private static void CheckParsingOfLikeVariant (string methodName, string pattern)
    {
      WhereClause whereClause = ExpressionHelper.CreateWhereClause ();

      ParameterExpression parameter = Expression.Parameter (typeof (Student), "s");
      MemberExpression memberAccess = Expression.MakeMemberAccess (parameter, typeof (Student).GetProperty ("First"));

      MainFromClause fromClause = ExpressionHelper.CreateMainFromClause (parameter, ExpressionHelper.CreateQuerySource ());
      QueryModel queryModel = ExpressionHelper.CreateQueryModel (fromClause);
      ClauseFieldResolver resolver =
          new ClauseFieldResolver (StubDatabaseInfo.Instance, new JoinedTableContext (), new WhereFieldAccessPolicy (StubDatabaseInfo.Instance));


      ParserRegistry parserRegistry = new ParserRegistry ();
      parserRegistry.RegisterParser (new ConstantExpressionParser (StubDatabaseInfo.Instance));
      parserRegistry.RegisterParser (new ParameterExpressionParser (queryModel, resolver));
      parserRegistry.RegisterParser (new MemberExpressionParser (queryModel, resolver));
      
      MethodCallExpression methodCallExpression = Expression.Call (
          memberAccess,
          typeof (string).GetMethod (methodName, new Type[] { typeof (string) }),
          Expression.Constant ("Test")
          );

      ICriterion criterion = new Constant ("Test");
      MethodCallExpressionParser parser = new MethodCallExpressionParser (queryModel, parserRegistry);

      List<FieldDescriptor> fieldCollection = new List<FieldDescriptor> ();
      ICriterion actualCriterion = parser.Parse (methodCallExpression, fieldCollection);
      ICriterion expectedCriterion = new BinaryCondition (new Column(new Table("studentTable","s"),"FirstColumn"), new Constant (pattern), BinaryCondition.ConditionKind.Like);
      Assert.AreEqual (expectedCriterion, actualCriterion);
    }

    private void CheckParsingOfLikeVariant_NoConstantExpression (string methodName)
    {
      WhereClause whereClause = ExpressionHelper.CreateWhereClause ();

      ParameterExpression parameter = Expression.Parameter (typeof (Student), "s");
      MemberExpression memberAccess = Expression.MakeMemberAccess (parameter, typeof (Student).GetProperty ("First"));

      MainFromClause fromClause = ExpressionHelper.CreateMainFromClause (parameter, ExpressionHelper.CreateQuerySource ());
      QueryModel queryModel = ExpressionHelper.CreateQueryModel (fromClause);
      ClauseFieldResolver resolver =
          new ClauseFieldResolver (StubDatabaseInfo.Instance, new JoinedTableContext (), new WhereFieldAccessPolicy (StubDatabaseInfo.Instance));


      ParserRegistry parserRegistry = new ParserRegistry ();
      parserRegistry.RegisterParser (new ConstantExpressionParser (StubDatabaseInfo.Instance));
      parserRegistry.RegisterParser (new ParameterExpressionParser (queryModel, resolver));
      parserRegistry.RegisterParser (new MemberExpressionParser (queryModel, resolver));
      
      MethodCallExpression methodCallExpression = Expression.Call (
          memberAccess,
          typeof (string).GetMethod (methodName, new Type[] { typeof (string) }),
          Expression.Parameter (typeof (string), "Test")
          );
      
      //MethodCallExpressionParser parser = new MethodCallExpressionParser (whereClause, delegate { return null; });
      MethodCallExpressionParser parser = new MethodCallExpressionParser (queryModel, parserRegistry);
      List<FieldDescriptor> fieldCollection = new List<FieldDescriptor> ();
      parser.Parse (methodCallExpression, fieldCollection);
    }

    private void CheckParsingOfLikeVariant_NoArguments (string methodName)
    {
      WhereClause whereClause = ExpressionHelper.CreateWhereClause ();

      ParameterExpression parameter = Expression.Parameter (typeof (Student), "s");
      MemberExpression memberAccess = Expression.MakeMemberAccess (parameter, typeof (Student).GetProperty ("First"));

      MainFromClause fromClause = ExpressionHelper.CreateMainFromClause (parameter, ExpressionHelper.CreateQuerySource ());
      QueryModel queryModel = ExpressionHelper.CreateQueryModel (fromClause);
      ClauseFieldResolver resolver =
          new ClauseFieldResolver (StubDatabaseInfo.Instance, new JoinedTableContext (), new WhereFieldAccessPolicy (StubDatabaseInfo.Instance));

      
      ParserRegistry parserRegistry = new ParserRegistry ();
      parserRegistry.RegisterParser (new ConstantExpressionParser (StubDatabaseInfo.Instance));
      parserRegistry.RegisterParser (new ParameterExpressionParser (queryModel, resolver));
      parserRegistry.RegisterParser (new MemberExpressionParser (queryModel, resolver));
      
      MethodCallExpression methodCallExpression = Expression.Call (
          memberAccess,
          typeof (MethodCallExpressionParserTest).GetMethod (methodName)
          );

      //MethodCallExpressionParser parser = new MethodCallExpressionParser (whereClause, delegate { return null; });
      MethodCallExpressionParser parser = new MethodCallExpressionParser (queryModel, parserRegistry);
      List<FieldDescriptor> fieldCollection = new List<FieldDescriptor> ();
      parser.Parse (methodCallExpression, fieldCollection);
    }

    private void CheckParsingOfContainsFulltext (string methodName, string pattern)
    {
      WhereClause whereClause = ExpressionHelper.CreateWhereClause ();

      ParameterExpression parameter = Expression.Parameter (typeof (Student), "s");
      MemberExpression memberAccess = Expression.MakeMemberAccess (parameter, typeof (Student).GetProperty ("First"));

      MethodCallExpression methodCallExpression = Expression.Call (
          memberAccess,
          typeof (Remotion.Data.Linq.ExtensionMethods.ExtensionMethods).GetMethod (methodName),
          Expression.MakeMemberAccess (Expression.Parameter (typeof (Student), "s"), typeof (Student).GetProperty ("First")),
          Expression.Constant ("Test")
          );

      ICriterion criterion = new Constant ("Test");

      MainFromClause fromClause = ExpressionHelper.CreateMainFromClause (parameter, ExpressionHelper.CreateQuerySource ());
      QueryModel queryModel = ExpressionHelper.CreateQueryModel (fromClause);
      ClauseFieldResolver resolver =
          new ClauseFieldResolver (StubDatabaseInfo.Instance, new JoinedTableContext (), new WhereFieldAccessPolicy (StubDatabaseInfo.Instance));
      ParserRegistry parserRegistry = new ParserRegistry ();
      parserRegistry.RegisterParser (new ConstantExpressionParser (StubDatabaseInfo.Instance));
      parserRegistry.RegisterParser (new ParameterExpressionParser (queryModel, resolver));
      parserRegistry.RegisterParser (new MemberExpressionParser (queryModel, resolver));

      MethodCallExpressionParser parser = new MethodCallExpressionParser (queryModel, parserRegistry);

      //MethodCallExpressionParser parser = new MethodCallExpressionParser (whereClause, delegate (Expression expression)
      //{
      //  Expression.Constant (5);
      //  return criterion;
      //});

      List<FieldDescriptor> fieldCollection = new List<FieldDescriptor> ();
      ICriterion actualCriterion = parser.Parse (methodCallExpression, fieldCollection);
      ICriterion expectedCriterion = new BinaryCondition (new Column (new Table ("studentTable", "s"), "FirstColumn"), new Constant (pattern), BinaryCondition.ConditionKind.ContainsFulltext);
      Assert.AreEqual (expectedCriterion, actualCriterion);
    }

  }
}