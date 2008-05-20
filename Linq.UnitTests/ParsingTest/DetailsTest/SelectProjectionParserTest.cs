using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using NUnit.Framework;
using NUnit.Framework.SyntaxHelpers;
using Remotion.Collections;
using Remotion.Data.Linq.Clauses;
using Remotion.Data.Linq.DataObjectModel;
using Remotion.Data.Linq.Parsing;
using Remotion.Data.Linq.Parsing.Details;
using Remotion.Data.Linq.Parsing.Details.SelectProjectionParsing;
using Remotion.Data.Linq.Parsing.FieldResolving;
using Remotion.Data.Linq.Parsing.Structure;
using Remotion.Data.Linq.UnitTests.TestQueryGenerators;

namespace Remotion.Data.Linq.UnitTests.ParsingTest.DetailsTest
{
  [TestFixture]
  public class SelectProjectionParserTest
  {
    private IQueryable<Student> _source;
    private JoinedTableContext _context;

    [SetUp]
    public void SetUp ()
    {
      _source = ExpressionHelper.CreateQuerySource ();
      _context = new JoinedTableContext ();
    }

    [Test]
    public void Column_SelectClause ()
    {
      PropertyInfo member = typeof (Student).GetProperty ("s");
      ParameterExpression identifier = Expression.Parameter (typeof (Student), "s");
      IQueryable<Student> query = SelectTestQueryGenerator.CreateSimpleQuery (_source);
      MainFromClause fromClause = ExpressionHelper.CreateMainFromClause (identifier, query);
      QueryParser parser = new QueryParser (query.Expression);
      QueryModel model = parser.GetParsedQuery ();
      SelectClause selectClause = (SelectClause) model.SelectOrGroupClause;

      SelectProjectionParser selectParser = new SelectProjectionParser (model, selectClause.ProjectionExpression.Body, StubDatabaseInfo.Instance, _context, ParseContext.TopLevelQuery);

      Tuple<List<FieldDescriptor>, List<IEvaluation>> parseResult = selectParser.GetParseResult ();

      FieldDescriptor expectedField = ExpressionHelper.CreateFieldDescriptor (fromClause, member);

      //Assert.AreEqual (expectedField.Column, parseResult.B);
      Assert.That (parseResult.B, Is.EqualTo (new object[] { expectedField.Column }));
    }

    [Test]
    public void IdentityProjection_WithoutSelectExpression ()
    {
      PropertyInfo member = typeof (Student).GetProperty ("s");
      ParameterExpression identifier = Expression.Parameter (typeof (Student), "s");
      IQueryable<Student> query = WhereTestQueryGenerator.CreateSimpleWhereQuery (_source);
      MainFromClause fromClause = ExpressionHelper.CreateMainFromClause (identifier, query);

      QueryParser parser = new QueryParser (query.Expression);
      QueryModel model = parser.GetParsedQuery ();
      SelectClause selectClause = (SelectClause) model.SelectOrGroupClause;

      SelectProjectionParser selectParser = new SelectProjectionParser (model, model.MainFromClause.Identifier, StubDatabaseInfo.Instance, _context, ParseContext.TopLevelQuery);

      Tuple<List<FieldDescriptor>, List<IEvaluation>> parseResult = selectParser.GetParseResult ();

      FieldDescriptor expectedField = ExpressionHelper.CreateFieldDescriptor (fromClause, member);

      Assert.That (parseResult.B, Is.EqualTo (new object[] { expectedField.Column }));

    }

    [Test]
    public void MemberAccessProjection ()
    {
      IQueryable<string> query = SelectTestQueryGenerator.CreateSimpleQueryWithProjection (_source);
      QueryParser parser = new QueryParser (query.Expression);
      QueryModel model = parser.GetParsedQuery ();
      SelectClause selectClause = (SelectClause) model.SelectOrGroupClause;

      SelectProjectionParser selectParser = new SelectProjectionParser (model, selectClause.ProjectionExpression.Body, StubDatabaseInfo.Instance, _context, ParseContext.TopLevelQuery);

      Tuple<List<FieldDescriptor>, List<IEvaluation>> parseResult = selectParser.GetParseResult ();
      IEnumerable<FieldDescriptor> selectedFields = parseResult.A;

      Assert.That (selectedFields.ToArray (),
          Is.EqualTo (new object[] { ExpressionHelper.CreateFieldDescriptor (model.MainFromClause, typeof (Student).GetProperty ("First")) }));
    }

    [Test]
    [ExpectedException (typeof (FieldAccessResolveException), ExpectedMessage = "The member 'Remotion.Data.Linq.UnitTests.Student.NonDBProperty' does"
      + " not identify a queryable column.")]
    public void NonDbMemberAccessProjection ()
    {
      IQueryable<string> query = SelectTestQueryGenerator.CreateSimpleSelectWithNonDbProjection (_source);
      QueryParser parser = new QueryParser (query.Expression);
      QueryModel model = parser.GetParsedQuery ();
      SelectClause selectClause = (SelectClause) model.SelectOrGroupClause;

      SelectProjectionParser selectParser = new SelectProjectionParser (model, selectClause.ProjectionExpression.Body, StubDatabaseInfo.Instance, _context, ParseContext.TopLevelQuery);

      Tuple<List<FieldDescriptor>, List<IEvaluation>> parseResult = selectParser.GetParseResult ();
    }

    [Test]
    public void JoinSelectClause ()
    {
      IQueryable<string> query = JoinTestQueryGenerator.CreateSimpleImplicitSelectJoin (ExpressionHelper.CreateQuerySource_Detail ());
      QueryModel parsedQuery = ExpressionHelper.ParseQuery (query);

      SelectProjectionParser parser = new SelectProjectionParser (parsedQuery, ((SelectClause) parsedQuery.SelectOrGroupClause).ProjectionExpression.Body,
          StubDatabaseInfo.Instance,
          _context, ParseContext.TopLevelQuery);

      PropertyInfo relationMember = typeof (Student_Detail).GetProperty ("Student");
      IColumnSource studentDetailTable = parsedQuery.MainFromClause.GetFromSource (StubDatabaseInfo.Instance);
      Table studentTable = DatabaseInfoUtility.GetRelatedTable (StubDatabaseInfo.Instance, relationMember);
      Tuple<string, string> joinColumns = DatabaseInfoUtility.GetJoinColumnNames (StubDatabaseInfo.Instance, relationMember);

      SingleJoin join = new SingleJoin (new Column (studentDetailTable, joinColumns.A), new Column (studentTable, joinColumns.B));

      FieldSourcePath path = new FieldSourcePath (studentDetailTable, new[] { join });

      FieldDescriptor expectedField =
          new FieldDescriptor (typeof (Student).GetProperty ("First"), path, new Column (studentTable, "FirstColumn"));

      Assert.That (parser.GetParseResult ().A, Is.EqualTo (new object[] { expectedField }));
    }

    [Test]
    public void ParserUsesContext ()
    {
      Assert.AreEqual (0, _context.Count);
      JoinSelectClause ();
      Assert.AreEqual (1, _context.Count);
    }

    [Test]
    public void SelectRelationMember ()
    {
      IQueryable<Student> query = SelectTestQueryGenerator.CreateRelationMemberSelectQuery (ExpressionHelper.CreateQuerySource_Detail ());
      QueryParser parser = new QueryParser (query.Expression);
      QueryModel model = parser.GetParsedQuery ();
      SelectClause selectClause = (SelectClause) model.SelectOrGroupClause;

      SelectProjectionParser selectParser =
        new SelectProjectionParser (model, selectClause.ProjectionExpression.Body, StubDatabaseInfo.Instance, _context, ParseContext.TopLevelQuery);

      IEnumerable<FieldDescriptor> selectedFields = selectParser.GetParseResult ().A;
      PropertyInfo relationMember = typeof (Student_Detail).GetProperty ("Student");
      IColumnSource studentDetailTable = model.MainFromClause.GetFromSource (StubDatabaseInfo.Instance);
      FieldSourcePath path = new FieldSourcePathBuilder ().BuildFieldSourcePath (StubDatabaseInfo.Instance, _context, studentDetailTable,
          new[] { relationMember });
      FieldDescriptor expected = new FieldDescriptor (relationMember, path, new Column (path.LastSource, "*"));

      Assert.That (selectedFields.ToArray (), Is.EqualTo (new object[] { expected }));
    }

    [Test]
    public void NewExpression_IntegrationTest ()
    {
      var query = from s in ExpressionHelper.CreateQuerySource () select new { s.First, s.Last };
      QueryParser parser = new QueryParser (query.Expression);
      QueryModel model = parser.GetParsedQuery ();
      SelectClause selectClause = (SelectClause) model.SelectOrGroupClause;

      ParameterExpression parameter = Expression.Parameter (typeof (Student), "s");
      MainFromClause fromClause = ExpressionHelper.CreateMainFromClause (parameter, ExpressionHelper.CreateQuerySource ());
      var fromSource = fromClause.GetFromSource (StubDatabaseInfo.Instance);

      SelectProjectionParser selectParser =
        new SelectProjectionParser (model, selectClause.ProjectionExpression.Body, StubDatabaseInfo.Instance, _context, ParseContext.TopLevelQuery);

      //expectedResult
      Column column1 = new Column (fromSource, "FirstColumn");
      Column column2 = new Column (fromSource, "LastColumn");
      List<IEvaluation> expectedResult = new List<IEvaluation> { column1, column2 };

      List<IEvaluation> result = selectParser.GetParseResult ().B;

      Assert.That (result, Is.EqualTo (expectedResult));
    }

    [Test]
    public void NewExpressionInNewExpression_IntegrationTest ()
    {
      var query = from s in ExpressionHelper.CreateQuerySource () select new { s.First, pair = new { s.First, s.Last } };

      QueryParser parser = new QueryParser (query.Expression);
      QueryModel model = parser.GetParsedQuery ();
      SelectClause selectClause = (SelectClause) model.SelectOrGroupClause;

      ParameterExpression parameter = Expression.Parameter (typeof (Student), "s");
      MainFromClause fromClause = ExpressionHelper.CreateMainFromClause (parameter, ExpressionHelper.CreateQuerySource ());
      var fromSource = fromClause.GetFromSource (StubDatabaseInfo.Instance);

      SelectProjectionParser selectParser =
        new SelectProjectionParser (model, selectClause.ProjectionExpression.Body, StubDatabaseInfo.Instance, _context, ParseContext.TopLevelQuery);

      //expectedResult
      Column column1 = new Column (fromSource, "FirstColumn");
      Column column2 = new Column (fromSource, "LastColumn");
      List<IEvaluation> expectedResult = new List<IEvaluation> { column1, column1, column2 };

      List<IEvaluation> result = selectParser.GetParseResult ().B;

      Assert.That (result, Is.EqualTo (expectedResult));
    }

    [Test]
    public void ParseWithEncapsulatedBinaryExpressions ()
    {
      ParameterExpression parameter = Expression.Parameter (typeof (Student), "s");
      MainFromClause fromClause = ExpressionHelper.CreateMainFromClause (parameter, ExpressionHelper.CreateQuerySource ());
      IColumnSource fromSource = fromClause.GetFromSource (StubDatabaseInfo.Instance);
      QueryModel queryModel = ExpressionHelper.CreateQueryModel (fromClause);
      MemberExpression memberExpression1 = Expression.MakeMemberAccess (parameter, typeof (Student).GetProperty ("First"));
      MemberExpression memberExpression2 = Expression.MakeMemberAccess (parameter, typeof (Student).GetProperty ("Last"));
      MemberExpression memberExpression3 = Expression.MakeMemberAccess (parameter, typeof (Student).GetProperty ("Last"));
      List<FieldDescriptor> fieldDescriptors = new List<FieldDescriptor> ();


      MethodInfo methodInfo = typeof (string).GetMethod ("Concat", new Type[] { typeof (string), typeof (string) });
      BinaryExpression binaryExpression1 = Expression.Add (memberExpression1, memberExpression2, methodInfo);
      BinaryExpression binaryExpression2 = Expression.Add (binaryExpression1, memberExpression3, methodInfo);

      ////expectedResult
      Column column1 = new Column (fromSource, "FirstColumn");
      Column column2 = new Column (fromSource, "LastColumn");
      BinaryEvaluation expectedResult1 = new BinaryEvaluation (column1, column2, BinaryEvaluation.EvaluationKind.Add);
      BinaryEvaluation expectedResult2 = new BinaryEvaluation (expectedResult1, column2, BinaryEvaluation.EvaluationKind.Add);

      BinaryExpressionParser binaryExpressionParser = new BinaryExpressionParser (queryModel, e => e == memberExpression3 ? column2 : (IEvaluation) expectedResult1);
      IEvaluation result = binaryExpressionParser.Parse (binaryExpression2, fieldDescriptors);

      Assert.AreEqual (expectedResult2, result);
    }

    [Test]
    public void ParseWithAddAndDivide ()
    {
      var query = from s in ExpressionHelper.CreateQuerySource () select ((s.ID + s.ID) / s.ID);

      QueryParser parser = new QueryParser (query.Expression);
      QueryModel model = parser.GetParsedQuery ();
      SelectClause selectClause = (SelectClause) model.SelectOrGroupClause;

      ParameterExpression parameter = Expression.Parameter (typeof (Student), "s");
      MainFromClause fromClause = ExpressionHelper.CreateMainFromClause (parameter, ExpressionHelper.CreateQuerySource ());
      var fromSource = fromClause.GetFromSource (StubDatabaseInfo.Instance);

      SelectProjectionParser selectParser =
       new SelectProjectionParser (model, selectClause.ProjectionExpression.Body, StubDatabaseInfo.Instance, _context, ParseContext.TopLevelQuery);


      //expectedResult
      Column column = new Column (fromSource, "IDColumn");
      BinaryEvaluation expectedResult1 = new BinaryEvaluation (column, column, BinaryEvaluation.EvaluationKind.Add);
      BinaryEvaluation expectedResult2 = new BinaryEvaluation (expectedResult1, column, BinaryEvaluation.EvaluationKind.Divide);
      List<IEvaluation> expectedResult = new List<IEvaluation> { expectedResult2 };

      List<IEvaluation> result = selectParser.GetParseResult ().B;

      Assert.AreEqual (expectedResult, result);
    }

    [Test]
    public void ParseMethodCall ()
    {
      var query = from s in ExpressionHelper.CreateQuerySource () select (s.First).Remove(5);

      QueryParser parser = new QueryParser (query.Expression);
      QueryModel model = parser.GetParsedQuery ();
      SelectClause selectClause = (SelectClause) model.SelectOrGroupClause;

      ParameterExpression parameter = Expression.Parameter (typeof (Student), "s");
      MainFromClause fromClause = ExpressionHelper.CreateMainFromClause (parameter, ExpressionHelper.CreateQuerySource ());
      var fromSource = fromClause.GetFromSource (StubDatabaseInfo.Instance);

      SelectProjectionParser selectParser =
       new SelectProjectionParser (model, selectClause.ProjectionExpression.Body, StubDatabaseInfo.Instance, _context, ParseContext.TopLevelQuery);

      //expectedResult
      MethodInfo methodInfo = typeof (string).GetMethod ("Remove", new Type[] { typeof (int) });
      Column column = new Column (fromSource, "FirstColumn");
      Constant item = new Constant (5);
      List<IEvaluation> arguments = new List<IEvaluation> { item };
      MethodCallEvaluation expected = new MethodCallEvaluation (methodInfo, column, arguments);

      List<IEvaluation> result = selectParser.GetParseResult ().B;
      Assert.AreEqual (expected.EvaluationArguments, ((MethodCallEvaluation)result.First()).EvaluationArguments);
      Assert.AreEqual (expected.EvaluationMethodInfo, ((MethodCallEvaluation)result.First()).EvaluationMethodInfo);
      Assert.AreEqual (expected.EvaluationParameter, ((MethodCallEvaluation)result.First()).EvaluationParameter);
    }

    [Test]
    [Ignore]
    public void NewExpressionProjection ()
    {
      IQueryable<Tuple<string, string>> query = SelectTestQueryGenerator.CreateSimpleQueryWithFieldProjection (_source);
      QueryParser parser = new QueryParser (query.Expression);
      QueryModel model = parser.GetParsedQuery ();
      SelectClause selectClause = (SelectClause) model.SelectOrGroupClause;

      SelectProjectionParser selectParser = new SelectProjectionParser (model, selectClause.ProjectionExpression.Body, StubDatabaseInfo.Instance, _context, ParseContext.TopLevelQuery);

      Tuple<List<FieldDescriptor>, List<IEvaluation>> parseResult = selectParser.GetParseResult ();
      IEnumerable<FieldDescriptor> selectedFields = parseResult.A;

      Assert.That (selectedFields.ToArray (), Is.EqualTo (
          new object[]
            {
                ExpressionHelper.CreateFieldDescriptor (model.MainFromClause, typeof (Student).GetProperty ("First")),
                ExpressionHelper.CreateFieldDescriptor (model.MainFromClause, typeof (Student).GetProperty ("Last"))
            }));
    }

    [Test]
    [Ignore]
    public void NonEntityMemberAccessProjection ()
    {
      IQueryable<int> query = SelectTestQueryGenerator.CreateSimpleSelectWithNonEntityMemberAccess (_source);
      QueryParser parser = new QueryParser (query.Expression);
      QueryModel model = parser.GetParsedQuery ();
      SelectClause selectClause = (SelectClause) model.SelectOrGroupClause;

      new SelectProjectionParser (model, selectClause.ProjectionExpression.Body, StubDatabaseInfo.Instance, _context, ParseContext.TopLevelQuery).GetParseResult ();
    }

    [Test]
    [Ignore]
    public void MultiFromProjection ()
    {
      IQueryable<Tuple<string, string, int>> query = MixedTestQueryGenerator.CreateMultiFromQueryWithProjection (_source, _source, _source);
      QueryParser parser = new QueryParser (query.Expression);
      QueryModel model = parser.GetParsedQuery ();
      SelectClause selectClause = (SelectClause) model.SelectOrGroupClause;

      SelectProjectionParser selectParser = new SelectProjectionParser (model, selectClause.ProjectionExpression.Body, StubDatabaseInfo.Instance, _context, ParseContext.TopLevelQuery);

      Tuple<List<FieldDescriptor>, List<IEvaluation>> parseResult = selectParser.GetParseResult ();
      IEnumerable<FieldDescriptor> selectedFields = parseResult.A;

      Assert.That (selectedFields.ToArray (), Is.EqualTo (
          new object[]
            {
                ExpressionHelper.CreateFieldDescriptor (model.MainFromClause, typeof (Student).GetProperty ("First")),
                ExpressionHelper.CreateFieldDescriptor ((FromClauseBase) model.BodyClauses.First(),
                    typeof (Student).GetProperty ("Last")),
                ExpressionHelper.CreateFieldDescriptor ((FromClauseBase) model.BodyClauses.Last(), typeof (Student).GetProperty ("ID"))
            }));
    }

    [Test]
    [Ignore]
    [ExpectedException (typeof (ParserException), ExpectedMessage = "The select clause contains an expression that cannot be parsed",
        MatchType = MessageMatch.Contains)]
    public void SimpleQueryWithSpecialProjection ()
    {
      IQueryable<Tuple<Student, string, string, string>> query = SelectTestQueryGenerator.CreateSimpleQueryWithSpecialProjection (_source);
      QueryParser parser = new QueryParser (query.Expression);
      QueryModel model = parser.GetParsedQuery ();
      SelectClause selectClause = (SelectClause) model.SelectOrGroupClause;

      SelectProjectionParser selectParser = new SelectProjectionParser (model, selectClause.ProjectionExpression.Body, StubDatabaseInfo.Instance, _context, ParseContext.TopLevelQuery);

      Tuple<List<FieldDescriptor>, List<IEvaluation>> selectedFields = selectParser.GetParseResult ();

      Assert.That (selectedFields.A.ToArray (), Is.EqualTo (
          new object[]
            {
                ExpressionHelper.CreateFieldDescriptor (model.MainFromClause, null),
                ExpressionHelper.CreateFieldDescriptor (model.MainFromClause, typeof (Student).GetProperty ("Last")),
            }));
    }

    [Test]
    [Ignore]
    public void QueryWithUnaryBinaryLambdaInvocationConvertNewArrayExpression ()
    {
      IQueryable<string> query = SelectTestQueryGenerator.CreateUnaryBinaryLambdaInvocationConvertNewArrayExpressionQuery (_source);
      QueryParser parser = new QueryParser (query.Expression);
      QueryModel model = parser.GetParsedQuery ();
      SelectClause selectClause = (SelectClause) model.SelectOrGroupClause;

      SelectProjectionParser selectParser = new SelectProjectionParser (model, selectClause.ProjectionExpression.Body, StubDatabaseInfo.Instance, _context, ParseContext.TopLevelQuery);

      Tuple<List<FieldDescriptor>, List<IEvaluation>> selectedFields = selectParser.GetParseResult ();

      Assert.That (selectedFields.A.ToArray (), Is.EquivalentTo (
          new object[]
            {
                ExpressionHelper.CreateFieldDescriptor (model.MainFromClause, typeof (Student).GetProperty ("First")),
                ExpressionHelper.CreateFieldDescriptor (model.MainFromClause, typeof (Student).GetProperty ("Last")),
                ExpressionHelper.CreateFieldDescriptor (model.MainFromClause, null),
                ExpressionHelper.CreateFieldDescriptor (model.MainFromClause, typeof (Student).GetProperty ("ID"))
            }));
    }


  }

  public class DummyClass2
  {
    public DummyClass2 (string arg1, string arg2) { }
  }
}