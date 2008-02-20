using System;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using Rubicon.Collections;
using Rubicon.Data.Linq.Clauses;
using Rubicon.Data.Linq.DataObjectModel;
using Rubicon.Data.Linq.Parsing.Details;
using Rubicon.Data.Linq.Parsing.FieldResolving;

namespace Rubicon.Data.Linq.UnitTests.ParsingTest.DetailsTest
{
  [TestFixture]
  public class OrderingFieldParserTest
  {
    private JoinedTableContext _context;

    [SetUp]
    public void SetUp()
    {
      _context = new JoinedTableContext();  
    }

    [Test]
    public void SimpleOrderingClause ()
    {
      IQueryable<Student> query = TestQueryGenerator.CreateSimpleOrderByQuery (ExpressionHelper.CreateQuerySource ());
      QueryExpression parsedQuery = ExpressionHelper.ParseQuery (query);
      OrderByClause orderBy = (OrderByClause) parsedQuery.QueryBody.BodyClauses.First ();
      OrderingClause orderingClause = orderBy.OrderingList.First ();

      OrderingFieldParser parser = new OrderingFieldParser (parsedQuery, orderingClause, StubDatabaseInfo.Instance, _context);
      FieldDescriptor fieldDescriptor = ExpressionHelper.CreateFieldDescriptor (parsedQuery.MainFromClause, typeof(Student).GetProperty("First"));
      Assert.AreEqual (new OrderingField (fieldDescriptor, OrderDirection.Asc), parser.GetField ());
    }

    [Test]
    public void TwoOrderingClause_FirstClause ()
    {
      IQueryable<Student> query =
          TestQueryGenerator.CreateTwoOrderByQuery (ExpressionHelper.CreateQuerySource ());
      QueryExpression parsedQuery = ExpressionHelper.ParseQuery (query);
      OrderByClause orderBy1 = (OrderByClause) parsedQuery.QueryBody.BodyClauses.First ();
      OrderingClause orderingClause1 = orderBy1.OrderingList.First ();

      OrderingFieldParser parser = new OrderingFieldParser (parsedQuery, orderingClause1, StubDatabaseInfo.Instance, _context);

      FieldDescriptor fieldDescriptor1 = ExpressionHelper.CreateFieldDescriptor (parsedQuery.MainFromClause, typeof (Student).GetProperty ("First"));
      Assert.AreEqual (new OrderingField (fieldDescriptor1, OrderDirection.Asc), parser.GetField());
    }

    [Test]
    public void TwoOrderingClause_SecondClause ()
    {
      IQueryable<Student> query =
          TestQueryGenerator.CreateTwoOrderByQuery (ExpressionHelper.CreateQuerySource ());
      QueryExpression parsedQuery = ExpressionHelper.ParseQuery (query);
      OrderByClause orderBy2 = (OrderByClause) parsedQuery.QueryBody.BodyClauses.Last ();
      OrderingClause orderingClause2 = orderBy2.OrderingList.Last ();

      OrderingFieldParser parser = new OrderingFieldParser (parsedQuery, orderingClause2, StubDatabaseInfo.Instance, _context);

      FieldDescriptor fieldDescriptor2 = ExpressionHelper.CreateFieldDescriptor (parsedQuery.MainFromClause, typeof (Student).GetProperty ("Last"));
      Assert.AreEqual (new OrderingField (fieldDescriptor2, OrderDirection.Desc), parser.GetField());
    }

    [Test]
    public void ComplexOrderingClause_FirstOrdering ()
    {
      IQueryable<Student> query =
          TestQueryGenerator.CreateMultiFromWhereOrderByQuery (ExpressionHelper.CreateQuerySource (), ExpressionHelper.CreateQuerySource ());
      QueryExpression parsedQuery = ExpressionHelper.ParseQuery (query);
      OrderByClause orderBy1 = (OrderByClause) parsedQuery.QueryBody.BodyClauses.Skip (2).First ();
      OrderingClause orderingClause1 = orderBy1.OrderingList.First ();

      OrderingFieldParser parser = new OrderingFieldParser (parsedQuery, orderingClause1, StubDatabaseInfo.Instance, _context);
      FieldDescriptor fieldDescriptor = ExpressionHelper.CreateFieldDescriptor (parsedQuery.MainFromClause, typeof (Student).GetProperty ("First"));
      Assert.AreEqual (new OrderingField (fieldDescriptor, OrderDirection.Asc), parser.GetField ());
    }

    [Test]
    public void ComplexOrderingClause_SecondOrdering ()
    {
      IQueryable<Student> query =
          TestQueryGenerator.CreateMultiFromWhereOrderByQuery (ExpressionHelper.CreateQuerySource (), ExpressionHelper.CreateQuerySource ());
      QueryExpression parsedQuery = ExpressionHelper.ParseQuery (query);
      OrderByClause orderBy1 = (OrderByClause) parsedQuery.QueryBody.BodyClauses.Skip (2).First ();
      OrderingClause orderingClause2 = orderBy1.OrderingList.Last ();

      OrderingFieldParser parser = new OrderingFieldParser (parsedQuery, orderingClause2, StubDatabaseInfo.Instance, _context);
      FieldDescriptor fieldDescriptor = ExpressionHelper.CreateFieldDescriptor ((FromClauseBase) parsedQuery.QueryBody.BodyClauses[0],
          typeof (Student).GetProperty ("Last"));
      Assert.AreEqual (new OrderingField (fieldDescriptor, OrderDirection.Desc), parser.GetField ());
    }

    [Test]
    [ExpectedException (typeof (FieldAccessResolveException), ExpectedMessage = "The member 'Rubicon.Data.Linq.UnitTests.Student.NonDBProperty' "
        +"does not identify a queryable column.")]
    public void OrderingClause_WithNonDBField ()
    {
      IQueryable<Student> query = TestQueryGenerator.CreateOrderByNonDBPropertyQuery (ExpressionHelper.CreateQuerySource ());
      QueryExpression parsedQuery = ExpressionHelper.ParseQuery (query);
      OrderByClause orderBy = (OrderByClause) parsedQuery.QueryBody.BodyClauses.First ();
      OrderingClause orderingClause = orderBy.OrderingList.First ();

      OrderingFieldParser parser = new OrderingFieldParser (parsedQuery, orderingClause, StubDatabaseInfo.Instance, _context);
      parser.GetField ();
    }

    [Test]
    public void JoinOrderingClause ()
    {
      IQueryable<Student_Detail> query = TestQueryGenerator.CreateSimpleImplicitOrderByJoin (ExpressionHelper.CreateQuerySource_Detail ());
      QueryExpression parsedQuery = ExpressionHelper.ParseQuery (query);
      OrderByClause orderBy = (OrderByClause) parsedQuery.QueryBody.BodyClauses.First ();
      OrderingClause orderingClause = orderBy.OrderingList.First ();

      FromClauseBase fromClause = parsedQuery.MainFromClause;
      PropertyInfo relationMember = typeof (Student_Detail).GetProperty ("Student");
      Table leftSide = DatabaseInfoUtility.GetRelatedTable (StubDatabaseInfo.Instance, relationMember); // Student
      Table rightSide = fromClause.GetTable (StubDatabaseInfo.Instance); // Student_Detail
      Tuple<string, string> columns = DatabaseInfoUtility.GetJoinColumns (StubDatabaseInfo.Instance, relationMember);
      JoinTree joinTree = new JoinTree (leftSide, rightSide, new Column (leftSide, columns.B), new Column (rightSide, columns.A));
      PropertyInfo orderingMember = typeof (Student).GetProperty ("First");
      Column? column = DatabaseInfoUtility.GetColumn (StubDatabaseInfo.Instance, leftSide, orderingMember);
      FieldDescriptor fieldDescriptor = new FieldDescriptor (orderingMember, fromClause, joinTree, column);

      OrderingFieldParser parser = new OrderingFieldParser (parsedQuery, orderingClause, StubDatabaseInfo.Instance, _context);
      Assert.AreEqual (new OrderingField (fieldDescriptor, OrderDirection.Asc), parser.GetField ());
    }

    [Test]
    public void ParserUsesContext()
    {
      Assert.AreEqual (0, _context.Count);
      JoinOrderingClause();
      Assert.AreEqual (1, _context.Count);
    }
  }
}