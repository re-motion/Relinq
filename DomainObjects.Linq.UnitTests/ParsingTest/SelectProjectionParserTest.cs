using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using NUnit.Framework;
using NUnit.Framework.SyntaxHelpers;
using Rubicon.Collections;
using Rubicon.Data.DomainObjects.Linq.Clauses;
using Rubicon.Data.DomainObjects.Linq.Parsing;

namespace Rubicon.Data.DomainObjects.Linq.UnitTests.ParsingTest
{
  [TestFixture]
  public class SelectProjectionParserTest
  {
    private IQueryable<Student> _source;

    [SetUp]
    public void SetUp()
    {
      _source = ExpressionHelper.CreateQuerySource();
    }

    [Test]
    public void IdentityProjection ()
    {
      IQueryable<Student> query = TestQueryGenerator.CreateSimpleQuery (_source);
      QueryParser parser = new QueryParser (query.Expression);
      QueryExpression expression = parser.GetParsedQuery();
      SelectClause selectClause = (SelectClause) expression.QueryBody.SelectOrGroupClause;

      SelectProjectionParser selectParser = new SelectProjectionParser (selectClause, new StubDatabaseInfo());

      IEnumerable<Tuple<FromClauseBase, MemberInfo>> selectedFields = selectParser.SelectedFields;

      Assert.That (selectedFields.ToArray(), Is.EqualTo (new object[] {new Tuple<FromClauseBase, MemberInfo> (expression.FromClause, null)}));
    }

    [Test]
    public void MemberAccessProjection ()
    {
      IQueryable<string> query = TestQueryGenerator.CreateSimpleQueryWithProjection (_source);
      QueryParser parser = new QueryParser (query.Expression);
      QueryExpression expression = parser.GetParsedQuery();
      SelectClause selectClause = (SelectClause) expression.QueryBody.SelectOrGroupClause;

      SelectProjectionParser selectParser = new SelectProjectionParser (selectClause, new StubDatabaseInfo());

      IEnumerable<Tuple<FromClauseBase, MemberInfo>> selectedFields = selectParser.SelectedFields;

      Assert.That (selectedFields.ToArray(),
          Is.EqualTo (new object[] {new Tuple<FromClauseBase, MemberInfo> (expression.FromClause, typeof (Student).GetProperty ("First"))}));
    }

    [Test]
    public void NewExpressionProjection ()
    {
      IQueryable<Tuple<string, string>> query = TestQueryGenerator.CreateSimpleQueryWithFieldProjection (_source);
      QueryParser parser = new QueryParser (query.Expression);
      QueryExpression expression = parser.GetParsedQuery ();
      SelectClause selectClause = (SelectClause) expression.QueryBody.SelectOrGroupClause;

      SelectProjectionParser selectParser = new SelectProjectionParser (selectClause, new StubDatabaseInfo ());

      IEnumerable<Tuple<FromClauseBase, MemberInfo>> selectedFields = selectParser.SelectedFields;

      Assert.That (selectedFields.ToArray (), Is.EqualTo (
          new object[]
              {
                  new Tuple<FromClauseBase, MemberInfo> (expression.FromClause, typeof (Student).GetProperty ("First")),
                  new Tuple<FromClauseBase, MemberInfo> (expression.FromClause, typeof (Student).GetProperty ("Last"))
              }));

    }


    [Test]
    public void NonDbMemberAccessProjection ()
    {
      IQueryable<string> query = TestQueryGenerator.CreateSimpleSelectWithNonDbProjection(_source);
      QueryParser parser = new QueryParser (query.Expression);
      QueryExpression expression = parser.GetParsedQuery ();
      SelectClause selectClause = (SelectClause) expression.QueryBody.SelectOrGroupClause;

      SelectProjectionParser selectParser = new SelectProjectionParser (selectClause, new StubDatabaseInfo ());

      IEnumerable<Tuple<FromClauseBase, MemberInfo>> selectedFields = selectParser.SelectedFields;

      Assert.IsEmpty (selectedFields.ToArray ());
    }


    [Test]
    public void NonEntityMemberAccessProjection ()
    {
      IQueryable<Expression> query = TestQueryGenerator.CreateSimpleSelectWithNonEntityMemberAccess (_source);
      QueryParser parser = new QueryParser (query.Expression);
      QueryExpression expression = parser.GetParsedQuery ();
      SelectClause selectClause = (SelectClause) expression.QueryBody.SelectOrGroupClause;

      SelectProjectionParser selectParser = new SelectProjectionParser (selectClause, new StubDatabaseInfo ());

      IEnumerable<Tuple<FromClauseBase, MemberInfo>> selectedFields = selectParser.SelectedFields;

      Assert.IsEmpty (selectedFields.ToArray ());
    }


    [Test]
    public void MultiFromProjection ()
    {
      IQueryable<Tuple<string,string,int>> query = TestQueryGenerator.CreateMultiFromQueryWithProjection (_source,_source,_source);
      QueryParser parser = new QueryParser (query.Expression);
      QueryExpression expression = parser.GetParsedQuery ();
      SelectClause selectClause = (SelectClause) expression.QueryBody.SelectOrGroupClause;

      SelectProjectionParser selectParser = new SelectProjectionParser (selectClause, new StubDatabaseInfo ());

      IEnumerable<Tuple<FromClauseBase, MemberInfo>> selectedFields = selectParser.SelectedFields;

      Assert.That (selectedFields.ToArray (), Is.EqualTo (
          new object[]
              {
                  new Tuple<FromClauseBase, MemberInfo> (expression.FromClause, typeof (Student).GetProperty ("First")),
                  new Tuple<FromClauseBase, MemberInfo> ((FromClauseBase)expression.QueryBody.FromLetWhereClauses.First(), typeof (Student).GetProperty ("Last")),
                  new Tuple<FromClauseBase, MemberInfo> ((FromClauseBase)expression.QueryBody.FromLetWhereClauses.Last(), typeof (Student).GetProperty ("ID"))
              }));
    }

    [Test]
    public void SimpleQueryWithSpecialProjection ()
    {
      IQueryable<Tuple<Student, string, string, string>> query = TestQueryGenerator.CreateSimpleQueryWithSpecialProjection (_source);
      QueryParser parser = new QueryParser (query.Expression);
      QueryExpression expression = parser.GetParsedQuery ();
      SelectClause selectClause = (SelectClause) expression.QueryBody.SelectOrGroupClause;

      SelectProjectionParser selectParser = new SelectProjectionParser (selectClause, new StubDatabaseInfo ());

      IEnumerable<Tuple<FromClauseBase, MemberInfo>> selectedFields = selectParser.SelectedFields;

      Assert.That (selectedFields.ToArray (), Is.EqualTo (
          new object[]
              {
                  new Tuple<FromClauseBase, MemberInfo> (expression.FromClause, null),
                  new Tuple<FromClauseBase, MemberInfo> (expression.FromClause, typeof (Student).GetProperty ("Last")),                
              }));
    }


  }
}