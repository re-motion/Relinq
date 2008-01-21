using System;
using System.Linq;
using NUnit.Framework;
using NUnit.Framework.SyntaxHelpers;
using Rubicon.Data.Linq.Clauses;
using Rubicon.Data.Linq.DataObjectModel;
using Rubicon.Data.Linq.SqlGeneration;

namespace Rubicon.Data.Linq.UnitTests.ParsingTest
{
  [TestFixture]
  public class OrderingFieldParserTest
  {
    private SqlGeneratorVisitor _sqlGeneratorVisitor;

    [SetUp]
    public void SetUp ()
    {
      _sqlGeneratorVisitor = new SqlGeneratorVisitor (new StubDatabaseInfo ());
    }

    [Test]
    public void VisitSimpleOrderingClause ()
    {
      IQueryable<Student> query = TestQueryGenerator.CreateSimpleOrderByQuery (ExpressionHelper.CreateQuerySource ());
      QueryExpression parsedQuery = ExpressionHelper.ParseQuery (query);
      OrderByClause orderBy = (OrderByClause) parsedQuery.QueryBody.BodyClauses.First ();
      OrderingClause orderingClause = orderBy.OrderingList.First ();
      _sqlGeneratorVisitor.VisitOrderingClause (orderingClause);

      Assert.That (_sqlGeneratorVisitor.OrderingFields,
          Is.EqualTo (new object[] { new OrderingField (new Column (new Table ("sourceTable", "s1"), "FirstColumn"), OrderDirection.Asc) }));
    }

    [Test]
    public void VisitTwoOrderingClause ()
    {
      IQueryable<Student> query =
        TestQueryGenerator.CreateTwoOrderByQuery (ExpressionHelper.CreateQuerySource ());
      QueryExpression parsedQuery = ExpressionHelper.ParseQuery (query);
      OrderByClause orderBy1 = (OrderByClause) parsedQuery.QueryBody.BodyClauses.First ();
      OrderByClause orderBy2 = (OrderByClause) parsedQuery.QueryBody.BodyClauses.Last ();
      OrderingClause orderingClause1 = orderBy1.OrderingList.First ();
      _sqlGeneratorVisitor.VisitOrderingClause (orderingClause1);
      OrderingClause orderingClause2 = orderBy2.OrderingList.Last ();
      _sqlGeneratorVisitor.VisitOrderingClause (orderingClause2);
      Assert.That (_sqlGeneratorVisitor.OrderingFields,
          Is.EqualTo (new object[]
              {
                  new OrderingField (new Column (new Table ("sourceTable", "s1"), "FirstColumn"), OrderDirection.Asc),
                  new OrderingField (new Column (new Table ("sourceTable", "s1"), "LastColumn"), OrderDirection.Desc)
    }));

    }

    [Test]
    public void VisitMixedOrderingClause ()
    {
      IQueryable<Student> query =
        TestQueryGenerator.CreateThreeOrderByQuery (ExpressionHelper.CreateQuerySource ());
      QueryExpression parsedQuery = ExpressionHelper.ParseQuery (query);
      OrderByClause orderBy1 = (OrderByClause) parsedQuery.QueryBody.BodyClauses.First ();
      OrderByClause orderBy2 = (OrderByClause) parsedQuery.QueryBody.BodyClauses.Last ();
      OrderingClause orderingClause1 = orderBy1.OrderingList.First ();
      OrderingClause orderingClause2 = orderBy1.OrderingList.Last ();
      OrderingClause orderingClause3 = orderBy2.OrderingList.Last ();
      _sqlGeneratorVisitor.VisitOrderingClause (orderingClause1);
      _sqlGeneratorVisitor.VisitOrderingClause (orderingClause2);
      _sqlGeneratorVisitor.VisitOrderingClause (orderingClause3);
      Assert.That (_sqlGeneratorVisitor.OrderingFields,
          Is.EqualTo (new object[]
              {
                new OrderingField (new Column (new Table ("sourceTable", "s1"), "FirstColumn"), OrderDirection.Asc),
                new OrderingField (new Column (new Table ("sourceTable", "s1"), "LastColumn"), OrderDirection.Asc),
                new OrderingField (new Column (new Table ("sourceTable", "s1"), "LastColumn"), OrderDirection.Desc)
              }));
    }

    [Test]
    public void VisitComplexOrderingClause ()
    {
      IQueryable<Student> query =
        TestQueryGenerator.CreateMultiFromWhereOrderByQuery (ExpressionHelper.CreateQuerySource (), ExpressionHelper.CreateQuerySource ());
      QueryExpression parsedQuery = ExpressionHelper.ParseQuery (query);
      OrderByClause orderBy1 = (OrderByClause) parsedQuery.QueryBody.BodyClauses.Skip (2).First ();
      OrderingClause orderingClause1 = orderBy1.OrderingList.First ();
      OrderingClause orderingClause2 = orderBy1.OrderingList.Last ();
      _sqlGeneratorVisitor.VisitOrderingClause (orderingClause1);
      _sqlGeneratorVisitor.VisitOrderingClause (orderingClause2);
      Assert.That (_sqlGeneratorVisitor.OrderingFields,
          Is.EqualTo (new object[]
              {
                new OrderingField (new Column (new Table ("sourceTable", "s1"), "FirstColumn"), OrderDirection.Asc),
                new OrderingField (new Column (new Table ("sourceTable", "s2"), "LastColumn"), OrderDirection.Desc)
                  
    }));
      


    }
  }
}