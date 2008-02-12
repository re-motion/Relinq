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
    private ParameterExpression _parameter;
    private MainFromClause _fromClause;
    private QueryExpression _queryExpression;


    [SetUp]
    public void SetUp()
    {
      _databaseInfo = StubDatabaseInfo.Instance;
      _parameter = Expression.Parameter (typeof (Student), "s");
      _fromClause = new MainFromClause (_parameter, ExpressionHelper.CreateQuerySource ());
      _queryExpression = new QueryExpression (_fromClause, ExpressionHelper.CreateQueryBody());
    }

    

    [Test]
    [ExpectedException (typeof (ParserException), ExpectedMessage = "Expected binary expression, constant expression,method call expression or member expression for "
        + "where condition, found ConditionalExpression (IIF(True, True, True)).")]
    public void Invalid ()
    {
      WhereClause whereClause =
          new WhereClause (ExpressionHelper.CreateMainFromClause(),
              Expression.Lambda (Expression.Condition (Expression.Constant (true), Expression.Constant (true), Expression.Constant (true))));
      WhereConditionParser parser = new WhereConditionParser (_queryExpression, whereClause, _databaseInfo, false);
      parser.GetCriterion();
    }

    [Test]
    public void Column()
    {
  
      WhereClause whereClause =
          new WhereClause (_fromClause,
              Expression.Lambda (
              typeof (System.Func<Student, bool>),
              Expression.MakeMemberAccess(_parameter,typeof(Student).GetProperty("IsOld")),
              _parameter));
      WhereConditionParser parser = new WhereConditionParser (_queryExpression, whereClause, _databaseInfo, false);
      ICriterion criterion = parser.GetCriterion ();
      Assert.AreEqual (new Column (new Table ("sourceTable", "s"), "IsOldColumn"), criterion);
    }

    [Test]
    [ExpectedException (typeof (FieldAccessResolveException), ExpectedMessage = "The member 'Rubicon.Data.Linq.UnitTests.Student.NonDBBoolProperty' "
      + "does not identify a queryable column in table 'sourceTable'.")]
    public void NonDbField ()
    {

      WhereClause whereClause =
          new WhereClause (_fromClause,
              Expression.Lambda (
              typeof (System.Func<Student, bool>),
              Expression.MakeMemberAccess (_parameter, typeof (Student).GetProperty ("NonDBBoolProperty")),
              _parameter));
      WhereConditionParser parser = new WhereConditionParser (_queryExpression, whereClause, _databaseInfo, false);
      parser.GetCriterion ();
    }

    [Test]
    [Ignore]
    [ExpectedException (typeof (ParserException), ExpectedMessage = "Expected table identifier for member access in where condition, found "
        + "ConstantExpression (value(Rubicon.Data.Linq.UnitTests.Student)).")]
    public void InvalidMemberAccess ()
    {
      
      WhereClause whereClause =
          new WhereClause (_fromClause,
              Expression.Lambda (
              typeof (System.Func<Student, bool>),
              Expression.MakeMemberAccess (Expression.Constant (new Student()), typeof (Student).GetProperty ("IsOld")),
              _parameter));
      WhereConditionParser parser = new WhereConditionParser (_queryExpression, whereClause, _databaseInfo, false);
      parser.GetCriterion ();
    }

    [Test]
    public void Constant ()
    {
      
      WhereClause whereClause =
          new WhereClause (_fromClause,
              Expression.Lambda (
              typeof (System.Func<Student, bool>),
              Expression.Constant(true),
              _parameter));
      WhereConditionParser parser = new WhereConditionParser (_queryExpression, whereClause, _databaseInfo, false);
      ICriterion criterion = parser.GetCriterion ();
      Assert.AreEqual (new Constant(true), criterion);
    }

    [Test]
    [ExpectedException (typeof (ParserException), ExpectedMessage = "Expected and, or, or comparison expression for binary expression in where "
        + "condition, found ExpressionType (ArrayIndex).")]
    public void InvalidBinary ()
    {
  
      WhereClause whereClause =
          new WhereClause (_fromClause,
              Expression.Lambda (
              typeof (System.Func<Student, bool>),
              Expression.ArrayIndex (Expression.Constant (new bool[0]), Expression.Constant(0)),
              _parameter));
      WhereConditionParser parser = new WhereConditionParser (_queryExpression, whereClause, _databaseInfo, false);
      parser.GetCriterion ();
    }

    [Test]
    public void BinaryAnd ()
    {
      WhereClause whereClause =
          new WhereClause (_fromClause,
              Expression.Lambda (
              typeof (System.Func<Student, bool>),
              Expression.And(Expression.Constant(true),Expression.Constant(true)),
              _parameter));
      WhereConditionParser parser = new WhereConditionParser (_queryExpression, whereClause, _databaseInfo, false);
      ICriterion criterion = parser.GetCriterion ();
      Assert.AreEqual (new ComplexCriterion (new Constant (true), new Constant (true),ComplexCriterion.JunctionKind.And), criterion);
    }

    [Test]
    public void BinaryOr ()
    { 
      WhereClause whereClause =
          new WhereClause (_fromClause,
              Expression.Lambda (
              typeof (System.Func<Student, bool>),
              Expression.Or (Expression.Constant (true), Expression.Constant (true)),
              _parameter));
      WhereConditionParser parser = new WhereConditionParser (_queryExpression, whereClause, _databaseInfo, false);
      ICriterion criterion = parser.GetCriterion ();
      Assert.AreEqual (new ComplexCriterion (new Constant (true), new Constant (true), ComplexCriterion.JunctionKind.Or), criterion);
    }

    [Test]
    public void BinaryEquals ()
    {
      WhereClause whereClause =
          new WhereClause (_fromClause,
              Expression.Lambda (
              typeof (System.Func<Student, bool>),
              Expression.Equal (Expression.Constant (true), Expression.Constant (true)),
              _parameter));
      WhereConditionParser parser = new WhereConditionParser (_queryExpression, whereClause, _databaseInfo, false);
      ICriterion criterion = parser.GetCriterion ();
      Assert.AreEqual (new BinaryCondition (new Constant (true), new Constant (true), BinaryCondition.ConditionKind.Equal), criterion);
    }

    [Test]
    public void MethodCallStartsWith ()
    {
  
      WhereClause whereClause =
          new WhereClause (_fromClause,
              Expression.Lambda (
                  typeof (System.Func<Student, bool> ),
                  Expression.Call(
                    Expression.MakeMemberAccess (_parameter, typeof (Student).GetProperty ("First")),
                    typeof(string).GetMethod("StartsWith",new Type[] {typeof (string)}),
                    Expression.Constant("Garcia")
                   ),
                  _parameter
                  ));
      WhereConditionParser parser = new WhereConditionParser (_queryExpression, whereClause, _databaseInfo, false);
      ICriterion criterion = parser.GetCriterion ();
      Assert.AreEqual (new BinaryCondition (new Column (new Table("sourceTable","s"),"FirstColumn"), new Constant ("Garcia%"), BinaryCondition.ConditionKind.Like), criterion);
    }

    [Test]
    public void MethodCallEndsWith ()
    {
      WhereClause whereClause =
          new WhereClause (_fromClause,
              Expression.Lambda (
                  typeof (System.Func<Student, bool>),
                  Expression.Call (
                    Expression.MakeMemberAccess (_parameter, typeof (Student).GetProperty ("First")),
                    typeof (string).GetMethod ("EndsWith", new Type[] { typeof (string) }),
                    Expression.Constant ("Garcia")
                   ),
                  _parameter
                  ));
      WhereConditionParser parser = new WhereConditionParser (_queryExpression, whereClause, _databaseInfo, false);
      ICriterion criterion = parser.GetCriterion ();
      Assert.AreEqual (new BinaryCondition (new Column (new Table ("sourceTable", "s"), "FirstColumn"), new Constant ("%Garcia"), BinaryCondition.ConditionKind.Like), criterion);
    }

    [Test]
    public void UnaryNot ()
    {
      WhereClause whereClause =
          new WhereClause (_fromClause,
              Expression.Lambda (
              typeof (System.Func<Student, bool>),
              Expression.Not (Expression.Constant (true)),
              _parameter));
      WhereConditionParser parser = new WhereConditionParser (_queryExpression, whereClause, _databaseInfo, false);
      ICriterion criterion = parser.GetCriterion ();
      Assert.AreEqual (new NotCriterion(new Constant(true)), criterion);
    }

    [Test]
    [ExpectedException (typeof (ParserException), ExpectedMessage = "Expected not expression for unary expression in where condition, found "
        + "ExpressionType (Convert).")]
    public void InvalidUnary ()
    {
      WhereClause whereClause =
          new WhereClause (_fromClause,
              Expression.Lambda (
              typeof (System.Func<Student, bool>),
              Expression.Convert (Expression.Constant (true), typeof (bool)),
              _parameter));
      WhereConditionParser parser = new WhereConditionParser (_queryExpression, whereClause, _databaseInfo, false);
      parser.GetCriterion ();
    }

    [Test]
    public void AllComparisons()
    {
      IQueryable<Student> query = TestQueryGenerator.CreateWhereQueryWithDifferentComparisons (ExpressionHelper.CreateQuerySource());
      QueryExpression parsedQuery = ExpressionHelper.ParseQuery (query);
      WhereClause whereClause = ClauseFinder.FindClause<WhereClause> (parsedQuery.QueryBody.SelectOrGroupClause);
      WhereConditionParser parser = new WhereConditionParser (parsedQuery, whereClause, _databaseInfo, false);
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

    [Test]
    public void ComplexWhereCondition()
    {
      IQueryable<Student> query =
        TestQueryGenerator.CreateMultiFromWhereOrderByQuery (ExpressionHelper.CreateQuerySource (), ExpressionHelper.CreateQuerySource ());
      QueryExpression parsedQuery = ExpressionHelper.ParseQuery (query);
      WhereClause whereClause1 = ClauseFinder.FindClause<WhereClause> (parsedQuery.QueryBody.BodyClauses.Skip (1).First());
      WhereClause whereClause2 = ClauseFinder.FindClause<WhereClause> (parsedQuery.QueryBody.BodyClauses.Last ());

      WhereConditionParser parser1 = new WhereConditionParser (parsedQuery, whereClause1, _databaseInfo, false);
      ICriterion result1 = parser1.GetCriterion();

      WhereConditionParser parser2 = new WhereConditionParser (parsedQuery, whereClause2, _databaseInfo, false);
      ICriterion result2 = parser2.GetCriterion ();

      Column c1 = new Column(new Table("sourceTable","s1"),"LastColumn" );
      BinaryCondition comp1 = new BinaryCondition(c1,new Constant("Garcia"),BinaryCondition.ConditionKind.Equal);
      Assert.AreEqual (comp1, result1);
      
    }

    [Test]
    public void Simplify_True()
    {
      IQueryable<Student> query = TestQueryGenerator.CreateWhereQueryWithEvaluatableSubExpression (ExpressionHelper.CreateQuerySource ());
      QueryExpression parsedQuery = ExpressionHelper.ParseQuery (query);
      WhereClause whereClause = ClauseFinder.FindClause<WhereClause> (parsedQuery.QueryBody.SelectOrGroupClause);
      WhereConditionParser parser = new WhereConditionParser (parsedQuery, whereClause, _databaseInfo, true);
      ICriterion result = parser.GetCriterion ();
      Assert.AreEqual (new BinaryCondition(new Column(new Table("sourceTable", "s"), "LastColumn"), new Constant("Garcia"),
          BinaryCondition.ConditionKind.Equal), result);
    }

    [Test]
    [ExpectedException (typeof (ParserException), ExpectedMessage = "Expected and, or, or comparison expression for binary expression in where "
        + "condition, found ExpressionType (Add).")]
    public void Simplify_False ()
    {
      IQueryable<Student> query = TestQueryGenerator.CreateWhereQueryWithEvaluatableSubExpression (ExpressionHelper.CreateQuerySource ());
      QueryExpression parsedQuery = ExpressionHelper.ParseQuery (query);
      WhereClause whereClause = ClauseFinder.FindClause<WhereClause> (parsedQuery.QueryBody.SelectOrGroupClause);
      WhereConditionParser parser = new WhereConditionParser (parsedQuery, whereClause, _databaseInfo, false);
      parser.GetCriterion ();
    }



    


  }
}