using System;
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using System.Linq;
using Rubicon.Collections;
using Rubicon.Data.Linq.Clauses;
using Rubicon.Data.Linq.DataObjectModel;
using Rubicon.Data.Linq.Parsing;
using System.Linq.Expressions;
using Rubicon.Data.Linq.Parsing.Details;
using Rubicon.Data.Linq.Parsing.FieldResolving;
using NUnit.Framework.SyntaxHelpers;

namespace Rubicon.Data.Linq.UnitTests.ParsingTest.DetailsTest
{
  [TestFixture]
  public class WhereConditionParserTest
  {
    private IDatabaseInfo _databaseInfo;
    private ParameterExpression _parameter;
    private MainFromClause _fromClause;
    private QueryExpression _queryExpression;
    private JoinedTableContext _context;
    
    [SetUp]
    public void SetUp()
    {
      _databaseInfo = StubDatabaseInfo.Instance;
      _parameter = Expression.Parameter (typeof (Student), "s");
      _fromClause = new MainFromClause (_parameter, ExpressionHelper.CreateQuerySource ());
      _queryExpression = new QueryExpression (_fromClause, ExpressionHelper.CreateQueryBody());
      _context = new JoinedTableContext();
    }
    
    [Test]
    [ExpectedException (typeof (ParserException), ExpectedMessage = "Expected binary expression, constant expression,method call expression or member expression for "
        + "where condition, found ConditionalExpression (IIF(True, True, True)).")]
    public void Invalid ()
    {
      WhereClause whereClause =
          new WhereClause (ExpressionHelper.CreateMainFromClause(),
              Expression.Lambda (Expression.Condition (Expression.Constant (true), Expression.Constant (true), Expression.Constant (true))));
      WhereConditionParser parser = new WhereConditionParser (_queryExpression, whereClause, _databaseInfo, _context, false);
      parser.GetParseResult();
    }

    [Test]
    public void Column()
    {
      Expression condition = Expression.MakeMemberAccess(_parameter,typeof(Student).GetProperty("IsOld"));
      Tuple<List<FieldDescriptor>, ICriterion> parseResult = CreateAndParseWhereClause(condition);
      List<FieldDescriptor> fieldDescriptors = parseResult.A;
      ICriterion criterion = parseResult.B;

      Column expectedColumn = new Column (new Table ("studentTable", "s"), "IsOldColumn");
      FieldDescriptor expectedField = new FieldDescriptor (typeof (Student).GetProperty ("IsOld"), _fromClause, expectedColumn.Table, expectedColumn);

      Assert.AreEqual (expectedColumn, criterion);
      Assert.That (fieldDescriptors, Is.EqualTo (new object[] { expectedField }));
    }

    [Test]
    [ExpectedException (typeof (FieldAccessResolveException), ExpectedMessage = "The member 'Rubicon.Data.Linq.UnitTests.Student.NonDBBoolProperty' "
        + "does not identify a queryable column.")]
    public void NonDbField ()
    {
      Expression condition = Expression.MakeMemberAccess (_parameter, typeof (Student).GetProperty ("NonDBBoolProperty"));
      CreateAndParseWhereClause (condition);
    }

    [Test]
    [ExpectedException (typeof (FieldAccessResolveException), ExpectedMessage = "The field access expression "
        + "'value(Rubicon.Data.Linq.UnitTests.Student).IsOld' does not contain a from clause identifier.")]
    public void InvalidMemberAccess ()
    {
      Expression condition = Expression.MakeMemberAccess (Expression.Constant (new Student()), typeof (Student).GetProperty ("IsOld"));
      CreateAndParseWhereClause(condition);
    }

    [Test]
    public void Constant ()
    {
      Expression condition = Expression.Constant(true);
      Tuple<List<FieldDescriptor>, ICriterion> parseResult = CreateAndParseWhereClause(condition);
      List<FieldDescriptor> fieldDescriptors = parseResult.A;
      ICriterion criterion = parseResult.B;

      Assert.AreEqual (new Constant(true), criterion);
      Assert.That (fieldDescriptors, Is.Empty);
    }

    [Test]
    [ExpectedException (typeof (ParserException), ExpectedMessage = "Expected and, or, or comparison expression for binary expression in where "
        + "condition, found ExpressionType (ArrayIndex).")]
    public void InvalidBinary ()
    {
      Expression condition = Expression.ArrayIndex (Expression.Constant (new bool[0]), Expression.Constant(0));
      CreateAndParseWhereClause(condition);  
    }

    [Test]
    public void BinaryAnd ()
    {
      Expression condition = Expression.And(Expression.Constant(true),Expression.Constant(true));
      Tuple<List<FieldDescriptor>, ICriterion> parseResult = CreateAndParseWhereClause(condition);
      ICriterion criterion = parseResult.B;

      Assert.AreEqual (new ComplexCriterion (new Constant (true), new Constant (true),ComplexCriterion.JunctionKind.And), criterion);
    }

    [Test]
    public void BinaryOr ()
    {
      Expression condition = Expression.Or (Expression.Constant (true), Expression.Constant (true));
      Tuple<List<FieldDescriptor>, ICriterion> parseResult = CreateAndParseWhereClause(condition);
      ICriterion criterion = parseResult.B;

      Assert.AreEqual (new ComplexCriterion (new Constant (true), new Constant (true), ComplexCriterion.JunctionKind.Or), criterion);
    }

    [Test]
    public void BinaryEquals ()
    {
      Expression condition = Expression.Equal (Expression.Constant (true), Expression.Constant (true));
      Tuple<List<FieldDescriptor>, ICriterion> parseResult = CreateAndParseWhereClause(condition);
      ICriterion criterion = parseResult.B;

      Assert.AreEqual (new BinaryCondition (new Constant (true), new Constant (true), BinaryCondition.ConditionKind.Equal), criterion);
    }

    [Test]
    public void Binary_WithFields()
    {
      MemberExpression memberAccess1 = Expression.MakeMemberAccess (_parameter, typeof (Student).GetProperty ("First"));
      MemberExpression memberAccess2 = Expression.MakeMemberAccess (_parameter, typeof (Student).GetProperty ("Last"));

      Expression condition = Expression.Equal (memberAccess1, memberAccess2);
      Tuple<List<FieldDescriptor>, ICriterion> parseResult = CreateAndParseWhereClause(condition);
      List<FieldDescriptor> fieldDescriptors = parseResult.A;

      Table table = _fromClause.GetTable (_databaseInfo);
      Column column1 = DatabaseInfoUtility.GetColumn (_databaseInfo, table, memberAccess1.Member).Value;
      Column column2 = DatabaseInfoUtility.GetColumn (_databaseInfo, table, memberAccess2.Member).Value;

      FieldDescriptor expectedField1 = new FieldDescriptor (memberAccess1.Member, _fromClause, table, column1);
      FieldDescriptor expectedField2 = new FieldDescriptor (memberAccess2.Member, _fromClause, table, column2);

      Assert.That (fieldDescriptors, Is.EqualTo (new object[] {expectedField1, expectedField2}));
    }

    [Test]
    public void Complex_WithFields ()
    {
      MemberExpression memberAccess1 = Expression.MakeMemberAccess (_parameter, typeof (Student).GetProperty ("IsOld"));
      MemberExpression memberAccess2 = Expression.MakeMemberAccess (_parameter, typeof (Student).GetProperty ("HasDog"));

      Expression condition = Expression.And (memberAccess1, memberAccess2);
      Tuple<List<FieldDescriptor>, ICriterion> parseResult = CreateAndParseWhereClause(condition);
      List<FieldDescriptor> fieldDescriptors = parseResult.A;

      Table table = _fromClause.GetTable (_databaseInfo);
      Column column1 = DatabaseInfoUtility.GetColumn (_databaseInfo, table, memberAccess1.Member).Value;
      Column column2 = DatabaseInfoUtility.GetColumn (_databaseInfo, table, memberAccess2.Member).Value;

      FieldDescriptor expectedField1 = new FieldDescriptor (memberAccess1.Member, _fromClause, table, column1);
      FieldDescriptor expectedField2 = new FieldDescriptor (memberAccess2.Member, _fromClause, table, column2);

      Assert.That (fieldDescriptors, Is.EqualTo (new object[] { expectedField1, expectedField2 }));
    }

    [Test]
    public void MethodCallStartsWith ()
    {
      MemberExpression memberAccess = Expression.MakeMemberAccess (_parameter, typeof (Student).GetProperty ("First"));
      Expression condition = Expression.Call(
                      memberAccess,
                      typeof(string).GetMethod("StartsWith",new Type[] {typeof (string)}),
                      Expression.Constant("Garcia")
                      );
      Tuple<List<FieldDescriptor>, ICriterion> parseResult = CreateAndParseWhereClause(condition);
      ICriterion criterion = parseResult.B;

      Column expectedColumn = new Column (new Table ("studentTable", "s"), "FirstColumn");
      Assert.AreEqual (new BinaryCondition (expectedColumn, new Constant ("Garcia%"), 
          BinaryCondition.ConditionKind.Like), criterion);
    }

    [Test]
    public void MethodCallEndsWith ()
    {
      Expression condition = Expression.Call (
                      Expression.MakeMemberAccess (_parameter, typeof (Student).GetProperty ("First")),
                      typeof (string).GetMethod ("EndsWith", new Type[] { typeof (string) }),
                      Expression.Constant ("Garcia")
                      );
      Tuple<List<FieldDescriptor>, ICriterion> parseResult = CreateAndParseWhereClause(condition);
      ICriterion criterion = parseResult.B;

      Assert.AreEqual (new BinaryCondition (new Column (new Table ("studentTable", "s"), "FirstColumn"), new Constant ("%Garcia"), BinaryCondition.ConditionKind.Like), criterion);
    }

    [Test]
    public void CreateLike_WithField ()
    {
      MemberExpression memberAccess = Expression.MakeMemberAccess (_parameter, typeof (Student).GetProperty ("First"));
      Expression condition = Expression.Call (
                      memberAccess,
                      typeof (string).GetMethod ("StartsWith", new Type[] { typeof (string) }),
                      Expression.Constant ("Garcia")
                      );
      Tuple<List<FieldDescriptor>, ICriterion> parseResult = CreateAndParseWhereClause(condition);
      List<FieldDescriptor> fieldDescriptors = parseResult.A;
      ICriterion criterion = parseResult.B;

      Column expectedColumn = new Column (new Table ("studentTable", "s"), "FirstColumn");
      FieldDescriptor expectedField = new FieldDescriptor (memberAccess.Member, _fromClause, expectedColumn.Table, expectedColumn);

      Assert.That (fieldDescriptors, Is.EqualTo (new object[] { expectedField }));
      Assert.AreEqual (new BinaryCondition (expectedColumn, new Constant ("Garcia%"),
          BinaryCondition.ConditionKind.Like), criterion);
    }

    [Test]
    public void UnaryNot ()
    {
      Expression condition = Expression.Not (Expression.Constant (true));
      Tuple<List<FieldDescriptor>, ICriterion> parseResult = CreateAndParseWhereClause(condition);
      ICriterion criterion = parseResult.B;

      Assert.AreEqual (new NotCriterion(new Constant(true)), criterion);
    }

    [Test]
    public void UnaryIgnoredConvert ()
    {
      Expression condition = Expression.Convert (Expression.Constant (true), typeof (bool));
      Tuple<List<FieldDescriptor>, ICriterion> parseResult = CreateAndParseWhereClause (condition);
      ICriterion criterion = parseResult.B;

      Assert.AreEqual (new Constant (true), criterion);
    }

    [Test]
    public void Unary_WithField ()
    {
      MemberExpression memberAccess = Expression.MakeMemberAccess (_parameter, typeof (Student).GetProperty ("IsOld"));
      Expression condition = Expression.Not (memberAccess);
      Tuple<List<FieldDescriptor>, ICriterion> parseResult = CreateAndParseWhereClause(condition);
      List<FieldDescriptor> fieldDescriptors = parseResult.A;

      Column expectedColumn = new Column (new Table ("studentTable", "s"), "IsOldColumn");
      FieldDescriptor expectedField = new FieldDescriptor (typeof (Student).GetProperty ("IsOld"), _fromClause, expectedColumn.Table, expectedColumn);

      Assert.That (fieldDescriptors, Is.EqualTo (new object[] { expectedField }));
    }

    [Test]
    [ExpectedException (typeof (ParserException), ExpectedMessage = "Expected not or convert expression for unary expression in where condition, found "
        + "ExpressionType (ConvertChecked).")]
    public void InvalidUnary ()
    {
      Expression condition = Expression.ConvertChecked (Expression.Constant (true), typeof (bool));
      CreateAndParseWhereClause(condition);
    }

    [Test]
    public void AllComparisons()
    {
      IQueryable<Student> query = TestQueryGenerator.CreateWhereQueryWithDifferentComparisons (ExpressionHelper.CreateQuerySource());
      QueryExpression parsedQuery = ExpressionHelper.ParseQuery (query);
      WhereClause whereClause = ClauseFinder.FindClause<WhereClause> (parsedQuery.QueryBody.SelectOrGroupClause);
      WhereConditionParser parser = new WhereConditionParser (parsedQuery, whereClause, _databaseInfo, _context, false);
      Tuple<List<FieldDescriptor>, ICriterion> parseResult = parser.GetParseResult ();
      ICriterion result = parseResult.B;
      Column firstColumn = new Column(new Table("studentTable", "s"), "FirstColumn");
      Column idColumn = new Column (new Table ("studentTable", "s"), "IDColumn");
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
    public void Simplify_True()
    {
      IQueryable<Student> query = TestQueryGenerator.CreateWhereQueryWithEvaluatableSubExpression (ExpressionHelper.CreateQuerySource ());
      QueryExpression parsedQuery = ExpressionHelper.ParseQuery (query);
      WhereClause whereClause = ClauseFinder.FindClause<WhereClause> (parsedQuery.QueryBody.SelectOrGroupClause);
      WhereConditionParser parser = new WhereConditionParser (parsedQuery, whereClause, _databaseInfo, _context, true);
      Tuple<List<FieldDescriptor>, ICriterion> parseResult = parser.GetParseResult ();
      ICriterion result = parseResult.B;
      Assert.AreEqual (new BinaryCondition(new Column(new Table("studentTable", "s"), "LastColumn"), new Constant("Garcia"),
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
      WhereConditionParser parser = new WhereConditionParser (parsedQuery, whereClause, _databaseInfo, _context, false);
      parser.GetParseResult ();
    }

    [Test]
    public void JoinWhereConditions()
    {
      IQueryable<Student_Detail> query = TestQueryGenerator.CreateSimpleImplicitWhereJoin (ExpressionHelper.CreateQuerySource_Detail());
      
      QueryExpression parsedQuery = ExpressionHelper.ParseQuery (query);
      FromClauseBase fromClause = parsedQuery.MainFromClause;
      WhereClause whereClause = ClauseFinder.FindClause<WhereClause> (parsedQuery.QueryBody.SelectOrGroupClause);
      
      PropertyInfo relationMember = typeof (Student_Detail).GetProperty ("Student");
      Table leftSide = DatabaseInfoUtility.GetRelatedTable (StubDatabaseInfo.Instance, relationMember); // Student
      Table rightSide = fromClause.GetTable (StubDatabaseInfo.Instance); // Student_Detail
      Tuple<string, string> columns = DatabaseInfoUtility.GetJoinColumns (StubDatabaseInfo.Instance, relationMember);
      JoinTree joinTree = new JoinTree (leftSide, rightSide, new Column (leftSide, columns.B), new Column (rightSide, columns.A));

      PropertyInfo member = typeof (Student).GetProperty ("First");
      Column? column = DatabaseInfoUtility.GetColumn (StubDatabaseInfo.Instance, leftSide, member);
      FieldDescriptor fieldDescriptor = new FieldDescriptor (member, fromClause, joinTree, column);

      WhereConditionParser parser = new WhereConditionParser (parsedQuery, whereClause, _databaseInfo, _context, false);
      Tuple<List<FieldDescriptor>, ICriterion> parseResult = parser.GetParseResult ();
      Assert.AreEqual (fieldDescriptor, parseResult.A[0]);
    }

    [Test]
    public void ParserUsesContext ()
    {
      Assert.AreEqual (0, _context.Count);
      JoinWhereConditions ();
      Assert.AreEqual (1, _context.Count);
    }

    private Tuple<List<FieldDescriptor>, ICriterion> CreateAndParseWhereClause (Expression whereCondition)
    {
      WhereClause whereClause =
          new WhereClause (_fromClause,
              Expression.Lambda (
                  typeof (System.Func<Student, bool>),
                  whereCondition,
                  _parameter));

      WhereConditionParser parser = new WhereConditionParser (_queryExpression, whereClause, _databaseInfo, _context, false);
      return parser.GetParseResult ();
    }
  }
}