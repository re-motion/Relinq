using System;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using NUnit.Framework.SyntaxHelpers;
using Rubicon.Collections;
using Rubicon.Data.Linq.Clauses;
using Rubicon.Data.Linq.DataObjectModel;
using Rubicon.Data.Linq.Parsing;
using Rubicon.Data.Linq.SqlGeneration;

namespace Rubicon.Data.Linq.UnitTests.ParsingTest
{
  [TestFixture]
  public class OrderingFieldParserTest
  {
    [Test]
    public void SimpleOrderingClause ()
    {
      IQueryable<Student> query = TestQueryGenerator.CreateSimpleOrderByQuery (ExpressionHelper.CreateQuerySource ());
      QueryExpression parsedQuery = ExpressionHelper.ParseQuery (query);
      OrderByClause orderBy = (OrderByClause) parsedQuery.QueryBody.BodyClauses.First ();
      OrderingClause orderingClause = orderBy.OrderingList.First ();

      OrderingFieldParser parser = new OrderingFieldParser (parsedQuery, orderingClause, StubDatabaseInfo.Instance);
      FieldDescriptor fieldDescriptor = ExpressionHelper.CreateFieldDescriptor (parsedQuery.MainFromClause, typeof(Student).GetProperty("First"));
      Assert.AreEqual (new OrderingField (fieldDescriptor, OrderDirection.Asc), parser.GetField ());
    }

    [Test]
    public void ComplexOrderingClause_FirstOrdering ()
    {
      IQueryable<Student> query =
        TestQueryGenerator.CreateMultiFromWhereOrderByQuery (ExpressionHelper.CreateQuerySource (), ExpressionHelper.CreateQuerySource ());
      QueryExpression parsedQuery = ExpressionHelper.ParseQuery (query);
      OrderByClause orderBy1 = (OrderByClause) parsedQuery.QueryBody.BodyClauses.Skip (2).First ();
      OrderingClause orderingClause1 = orderBy1.OrderingList.First ();

      OrderingFieldParser parser = new OrderingFieldParser (parsedQuery, orderingClause1, StubDatabaseInfo.Instance);
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

      OrderingFieldParser parser = new OrderingFieldParser (parsedQuery, orderingClause2, StubDatabaseInfo.Instance);
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

      OrderingFieldParser parser = new OrderingFieldParser (parsedQuery, orderingClause, StubDatabaseInfo.Instance);
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
      Join join = new Join (leftSide, rightSide, new Column (leftSide, columns.B), new Column (rightSide, columns.A));
      PropertyInfo orderingMember = typeof (Student).GetProperty ("First");
      Column? column = DatabaseInfoUtility.GetColumn (StubDatabaseInfo.Instance, leftSide, orderingMember);
      FieldDescriptor fieldDescriptor = new FieldDescriptor (orderingMember, fromClause, join, column);

      OrderingFieldParser parser = new OrderingFieldParser (parsedQuery, orderingClause, StubDatabaseInfo.Instance);
      Assert.AreEqual (new OrderingField (fieldDescriptor, OrderDirection.Asc), parser.GetField ());
    }
  }
}