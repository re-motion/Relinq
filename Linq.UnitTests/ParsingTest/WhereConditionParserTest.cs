using System;
using NUnit.Framework;
using System.Linq;
using Rubicon.Data.Linq.Clauses;
using Rubicon.Data.Linq.DataObjectModel;
using Rubicon.Data.Linq.Parsing;
using System.Linq.Expressions;

namespace Rubicon.Data.Linq.UnitTests.ParsingTest
{
  [TestFixture]
  public class WhereConditionParserTest
  {
    private IDatabaseInfo _databaseInfo;

    [SetUp]
    public void SetUp()
    {
      _databaseInfo = new StubDatabaseInfo();
    }

    [Test]
    [ExpectedException (typeof (QueryParserException), ExpectedMessage = "Expected binary expression, constrant expression, or member expression for "
        + "where condition, found ConditionalExpression (IIF(True, True, True)).")]
    public void Invalid ()
    {
      WhereClause whereClause =
          new WhereClause (ExpressionHelper.CreateMainFromClause(),
              Expression.Lambda (Expression.Condition (Expression.Constant (true), Expression.Constant (true), Expression.Constant (true))));
      WhereConditionParser parser = new WhereConditionParser (whereClause, _databaseInfo);
      parser.GetCriterion();
    }

    [Test]
    public void Column()
    {
      ParameterExpression parameter = Expression.Parameter (typeof (Student), "s");
      MainFromClause fromClause = new MainFromClause(parameter,ExpressionHelper.CreateQuerySource());
      WhereClause whereClause =
          new WhereClause (fromClause,
              Expression.Lambda (
              typeof (System.Func<Student, bool>),
              Expression.MakeMemberAccess(parameter,typeof(Student).GetProperty("IsOld")),
              parameter));
      WhereConditionParser parser = new WhereConditionParser (whereClause, _databaseInfo);
      ICriterion criterion = parser.GetCriterion ();
      Assert.AreEqual (new Column (new Table ("sourceTable", "s"), "IsOldColumn"), criterion);
    }

    [Test]
    [ExpectedException (typeof (QueryParserException), ExpectedMessage = "Expected table identifier for member access in where condition, found "
        + "ConstantExpression (value(Rubicon.Data.Linq.UnitTests.Student)).")]
    public void InvalidMemberAccess ()
    {
      ParameterExpression parameter = Expression.Parameter (typeof (Student), "s");
      MainFromClause fromClause = new MainFromClause (parameter, ExpressionHelper.CreateQuerySource ());
      WhereClause whereClause =
          new WhereClause (fromClause,
              Expression.Lambda (
              typeof (System.Func<Student, bool>),
              Expression.MakeMemberAccess (Expression.Constant (new Student()), typeof (Student).GetProperty ("IsOld")),
              parameter));
      WhereConditionParser parser = new WhereConditionParser (whereClause, _databaseInfo);
      parser.GetCriterion ();
    }

    [Test]
    public void Constant ()
    {
      ParameterExpression parameter = Expression.Parameter (typeof (Student), "s");
      MainFromClause fromClause = new MainFromClause (parameter, ExpressionHelper.CreateQuerySource ());
      WhereClause whereClause =
          new WhereClause (fromClause,
              Expression.Lambda (
              typeof (System.Func<Student, bool>),
              Expression.Constant(true),
              parameter));
      WhereConditionParser parser = new WhereConditionParser (whereClause, _databaseInfo);
      ICriterion criterion = parser.GetCriterion ();
      Assert.AreEqual (new Constant(true), criterion);
    }

    [Test]
    [ExpectedException (typeof (QueryParserException), ExpectedMessage = "Expected and, or, or comparison expression for binary expression in where "
        + "condition, found ExpressionType (ArrayIndex).")]
    public void InvalidBinary ()
    {
      ParameterExpression parameter = Expression.Parameter (typeof (Student), "s");
      MainFromClause fromClause = new MainFromClause (parameter, ExpressionHelper.CreateQuerySource ());
      WhereClause whereClause =
          new WhereClause (fromClause,
              Expression.Lambda (
              typeof (System.Func<Student, bool>),
              Expression.ArrayIndex (Expression.Constant (new bool[0]), Expression.Constant(0)),
              parameter));
      WhereConditionParser parser = new WhereConditionParser (whereClause, _databaseInfo);
      parser.GetCriterion ();
    }

    [Test]
    public void BinaryAnd ()
    {
      ParameterExpression parameter = Expression.Parameter (typeof (Student), "s");
      MainFromClause fromClause = new MainFromClause (parameter, ExpressionHelper.CreateQuerySource ());
      WhereClause whereClause =
          new WhereClause (fromClause,
              Expression.Lambda (
              typeof (System.Func<Student, bool>),
              Expression.And(Expression.Constant(true),Expression.Constant(true)),
              parameter));
      WhereConditionParser parser = new WhereConditionParser (whereClause, _databaseInfo);
      ICriterion criterion = parser.GetCriterion ();
      Assert.AreEqual (new ComplexCriterion (new Constant (true), new Constant (true),ComplexCriterion.JunctionKind.And), criterion);
    }

    [Test]
    public void BinaryOr ()
    {
      ParameterExpression parameter = Expression.Parameter (typeof (Student), "s");
      MainFromClause fromClause = new MainFromClause (parameter, ExpressionHelper.CreateQuerySource ());
      WhereClause whereClause =
          new WhereClause (fromClause,
              Expression.Lambda (
              typeof (System.Func<Student, bool>),
              Expression.Or (Expression.Constant (true), Expression.Constant (true)),
              parameter));
      WhereConditionParser parser = new WhereConditionParser (whereClause, _databaseInfo);
      ICriterion criterion = parser.GetCriterion ();
      Assert.AreEqual (new ComplexCriterion (new Constant (true), new Constant (true), ComplexCriterion.JunctionKind.Or), criterion);
    }

    [Test]
    public void BinaryEquals ()
    {
      ParameterExpression parameter = Expression.Parameter (typeof (Student), "s");
      MainFromClause fromClause = new MainFromClause (parameter, ExpressionHelper.CreateQuerySource ());
      WhereClause whereClause =
          new WhereClause (fromClause,
              Expression.Lambda (
              typeof (System.Func<Student, bool>),
              Expression.Equal (Expression.Constant (true), Expression.Constant (true)),
              parameter));
      WhereConditionParser parser = new WhereConditionParser (whereClause, _databaseInfo);
      ICriterion criterion = parser.GetCriterion ();
      Assert.AreEqual (new BinaryCondition (new Constant (true), new Constant (true), BinaryCondition.ConditionKind.Equal), criterion);
    }

    [Test]
    public void UnaryNot ()
    {
      ParameterExpression parameter = Expression.Parameter (typeof (Student), "s");
      MainFromClause fromClause = new MainFromClause (parameter, ExpressionHelper.CreateQuerySource ());
      WhereClause whereClause =
          new WhereClause (fromClause,
              Expression.Lambda (
              typeof (System.Func<Student, bool>),
              Expression.Not (Expression.Constant (true)),
              parameter));
      WhereConditionParser parser = new WhereConditionParser (whereClause, _databaseInfo);
      ICriterion criterion = parser.GetCriterion ();
      Assert.AreEqual (new NotCriterion(new Constant(true)), criterion);
    }

    [Test]
    [ExpectedException (typeof (QueryParserException), ExpectedMessage = "Expected not expression for unary expression in where condition, found "
        + "ExpressionType (Convert).")]
    public void InvalidUnary ()
    {
      ParameterExpression parameter = Expression.Parameter (typeof (Student), "s");
      MainFromClause fromClause = new MainFromClause (parameter, ExpressionHelper.CreateQuerySource ());
      WhereClause whereClause =
          new WhereClause (fromClause,
              Expression.Lambda (
              typeof (System.Func<Student, bool>),
              Expression.Convert (Expression.Constant (true), typeof (bool)),
              parameter));
      WhereConditionParser parser = new WhereConditionParser (whereClause, _databaseInfo);
      parser.GetCriterion ();
    }

    [Test]
    public void AllComparisons()
    {
      IQueryable<Student> query = TestQueryGenerator.CreateWhereQueryWithDifferentComparisons (ExpressionHelper.CreateQuerySource());
      QueryExpression parsedQuery = ExpressionHelper.ParseQuery (query);
      WhereClause whereClause = ClauseFinder.FindClause<WhereClause> (parsedQuery.QueryBody.SelectOrGroupClause);
      WhereConditionParser parser = new WhereConditionParser (whereClause, _databaseInfo);
      ICriterion result = parser.GetCriterion();
      Column firstColumn = new Column(new Table("sourceTable", "s"), "FirstColumn");
      Column idColumn = new Column (new Table ("sourceTable", "s"), "IDColumn");
      BinaryCondition comparison1 = new BinaryCondition(firstColumn, new Constant("Garcia"), BinaryCondition.ConditionKind.NotEqual);
      BinaryCondition comparison2 = new BinaryCondition(idColumn, new Constant(5), BinaryCondition.ConditionKind.GreaterThan);
      BinaryCondition comparison3 = new BinaryCondition(idColumn, new Constant(6), BinaryCondition.ConditionKind.GreaterThanOrEqual);
      BinaryCondition comparison4 = new BinaryCondition(idColumn, new Constant(7), BinaryCondition.ConditionKind.LessThan);
      BinaryCondition comparison5 = new BinaryCondition(idColumn, new Constant(6), BinaryCondition.ConditionKind.LessThanOrEqual);
      BinaryCondition comparison6 = new BinaryCondition(idColumn, new Constant(6), BinaryCondition.ConditionKind.Equal);
      ICriterion expected =
          new ComplexCriterion (
              new ComplexCriterion (
                  new ComplexCriterion (
                      new ComplexCriterion (
                          new ComplexCriterion (comparison1, comparison2, ComplexCriterion.JunctionKind.And),
                          comparison3, ComplexCriterion.JunctionKind.And),
                      comparison4, ComplexCriterion.JunctionKind.And),
                  comparison5, ComplexCriterion.JunctionKind.And),
              comparison6, ComplexCriterion.JunctionKind.And);
      Assert.AreEqual (expected, result);
    }
  }
}