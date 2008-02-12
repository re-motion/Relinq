using System;
using System.Linq;
using NUnit.Framework;
using NUnit.Framework.SyntaxHelpers;
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
      Assert.AreEqual (new OrderingField (new Column (new Table ("sourceTable", "s1"), "FirstColumn"), OrderDirection.Asc), parser.GetField ());
    }

    [Test]
    public void ComplexOrderingClause ()
    {
      IQueryable<Student> query =
        TestQueryGenerator.CreateMultiFromWhereOrderByQuery (ExpressionHelper.CreateQuerySource (), ExpressionHelper.CreateQuerySource ());
      QueryExpression parsedQuery = ExpressionHelper.ParseQuery (query);
      OrderByClause orderBy1 = (OrderByClause) parsedQuery.QueryBody.BodyClauses.Skip (2).First ();
      OrderingClause orderingClause1 = orderBy1.OrderingList.First ();
      OrderingClause orderingClause2 = orderBy1.OrderingList.Last ();

      OrderingFieldParser parser = new OrderingFieldParser (parsedQuery, orderingClause1, StubDatabaseInfo.Instance);
      Assert.AreEqual (new OrderingField (new Column (new Table ("sourceTable", "s1"), "FirstColumn"), OrderDirection.Asc), parser.GetField ());

      parser = new OrderingFieldParser (parsedQuery, orderingClause2, StubDatabaseInfo.Instance);
      Assert.AreEqual (new OrderingField (new Column (new Table ("sourceTable", "s2"), "LastColumn"), OrderDirection.Desc), parser.GetField ());
    }

    [Test]
    [ExpectedException (typeof (FieldAccessResolveException), ExpectedMessage = "The member 'Rubicon.Data.Linq.UnitTests.Student.NonDBProperty' "
      +"does not identify a queryable column in table 'sourceTable'.")]
    public void OrderingClause_WithNonDBField ()
    {
      IQueryable<Student> query = TestQueryGenerator.CreateOrderByNonDBPropertyQuery (ExpressionHelper.CreateQuerySource ());
      QueryExpression parsedQuery = ExpressionHelper.ParseQuery (query);
      OrderByClause orderBy = (OrderByClause) parsedQuery.QueryBody.BodyClauses.First ();
      OrderingClause orderingClause = orderBy.OrderingList.First ();

      OrderingFieldParser parser = new OrderingFieldParser (parsedQuery, orderingClause, StubDatabaseInfo.Instance);
      parser.GetField ();
    }
  }
}