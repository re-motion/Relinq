using System;
using System.Linq;
using System.Linq.Expressions;
using NUnit.Framework;
using NUnit.Framework.SyntaxHelpers;
using Rubicon.Collections;
using Rubicon.Data.DomainObjects.Linq.Clauses;
using Rubicon.Data.DomainObjects.Linq.Parsing;
using Rubicon.Data.DomainObjects.Linq.UnitTests.Parsing;
using Rubicon.Data.DomainObjects.Linq.Visitor;

namespace Rubicon.Data.DomainObjects.Linq.UnitTests.VisitorTest
{
  [TestFixture]
  [Ignore]
  public class SqlGeneratorVisitorTest
  {
    private SqlGeneratorVisitor _sqlGeneratorVisitor;

    [SetUp]
    public void SetUp()
    {
      _sqlGeneratorVisitor = new SqlGeneratorVisitor (new StubDatabaseInfo());
    }

    [Test]
    public void VisitSelectClause_IdentityProjection()
    {    
      IQueryable<Student> query = TestQueryGenerator.CreateSimpleQuery (ExpressionHelper.CreateQuerySource ());
      QueryExpression parsedQuery = ExpressionHelper.ParseQuery (query);
      SelectClause selectClause = (SelectClause) parsedQuery.QueryBody.SelectOrGroupClause;
      _sqlGeneratorVisitor.VisitSelectClause (selectClause);
      Assert.That (_sqlGeneratorVisitor.Columns, Is.EqualTo (new object[] {Tuple.NewTuple ("s", "*")}));
    }

    [Test]
    public void VisitSelectClause_FieldProjection ()
    {
      IQueryable<Tuple<string, string>> query = TestQueryGenerator.CreateSimpleQueryWithFieldProjection (ExpressionHelper.CreateQuerySource ());
      QueryExpression parsedQuery = ExpressionHelper.ParseQuery (query);
      SelectClause selectClause = (SelectClause) parsedQuery.QueryBody.SelectOrGroupClause;
      _sqlGeneratorVisitor.VisitSelectClause (selectClause);
      Assert.That (_sqlGeneratorVisitor.Columns, Is.EqualTo (new object[] {Tuple.NewTuple ("s", "First"), Tuple.NewTuple ("s", "Last")}));
    }

    [Test]
    public void VisitSelectClause_SpecialProjection()
    {
      IQueryable<Tuple<Student, string, string, string>> query = TestQueryGenerator.CreateSimpleQueryWithSpecialProjection (ExpressionHelper.CreateQuerySource ());
      QueryExpression parsedQuery = ExpressionHelper.ParseQuery (query);
      SelectClause selectClause = (SelectClause) parsedQuery.QueryBody.SelectOrGroupClause;
      _sqlGeneratorVisitor.VisitSelectClause (selectClause);
      Assert.That (_sqlGeneratorVisitor.Columns, Is.EqualTo (new object[]
          {
              Tuple.NewTuple ("s", "*"), Tuple.NewTuple ("s", "Last")
          }));
    }

    [Test]
    public void VisitMainFromClause()
    {
      IQueryable<Student> query = TestQueryGenerator.CreateSimpleQuery (ExpressionHelper.CreateQuerySource ());
      QueryExpression parsedQuery = ExpressionHelper.ParseQuery (query);
      MainFromClause fromClause = parsedQuery.FromClause;
      _sqlGeneratorVisitor.VisitMainFromClause (fromClause);
      Assert.That (_sqlGeneratorVisitor.Tables, Is.EqualTo (new object[] { Tuple.NewTuple ("sourceTable", "s") }));
    }

    [Test]
    public void VisitAdditionalFromClause ()
    {
      IQueryable<Student> query = TestQueryGenerator.CreateMultiFromWhereQuery (ExpressionHelper.CreateQuerySource (), ExpressionHelper.CreateQuerySource ());
      QueryExpression parsedQuery = ExpressionHelper.ParseQuery (query);
      AdditionalFromClause fromClause = (AdditionalFromClause)parsedQuery.QueryBody.FromLetWhereClauses.First();
      _sqlGeneratorVisitor.VisitAdditionalFromClause (fromClause);
      Assert.That (_sqlGeneratorVisitor.Tables, Is.EqualTo (new object[] { Tuple.NewTuple ("sourceTable", "s2") }));
    }
    
  }
}