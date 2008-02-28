using System;
using System.Linq;
using System.Linq.Expressions;
using NUnit.Framework;
using Rubicon.Data.Linq.Clauses;
using Rubicon.Data.Linq.Parsing.Structure;

namespace Rubicon.Data.Linq.UnitTests.ParsingTest.StructureTest
{
  [TestFixture]
  public class QueryBodyCreatorTest
  {
    private IQueryable<Student> _source;
    private Expression _root;
    private MainFromClause _mainFromClause;
    private ParseResultCollector _result;

    [SetUp]
    public void SetUp ()
    {
      _source = ExpressionHelper.CreateQuerySource();
      _root = ExpressionHelper.CreateExpression();
      _mainFromClause = new MainFromClause (ExpressionHelper.CreateParameterExpression(), _source);
      _result = new ParseResultCollector (_root);
    }

    [Test]
    public void LastProjectionExpresion_TranslatedIntoSelectClause_NoFromClauses ()
    {
      _result.AddProjectionExpression (ExpressionHelper.CreateLambdaExpression());

      QueryBodyCreator bodyCreator = new QueryBodyCreator (_root, _mainFromClause, _result);
      QueryBody body = bodyCreator.GetQueryBody();
      Assert.AreEqual (0, body.BodyClauses.Count);

      SelectClause selectClause = body.SelectOrGroupClause as SelectClause;
      Assert.IsNotNull (selectClause);
      Assert.AreSame (_result.ProjectionExpressions[0], selectClause.ProjectionExpression);
    }

    [Test]
    public void SelectClause_Distinct_True ()
    {
      _result.AddProjectionExpression (ExpressionHelper.CreateLambdaExpression ());
      _result.SetDistinct();

      QueryBodyCreator bodyCreator = new QueryBodyCreator (_root, _mainFromClause, _result);
      QueryBody body = bodyCreator.GetQueryBody ();
      
      SelectClause selectClause = body.SelectOrGroupClause as SelectClause;
      Assert.IsTrue (selectClause.Distinct);
    }

    [Test]
    public void SelectClause_Distinct_False ()
    {
      _result.AddProjectionExpression (ExpressionHelper.CreateLambdaExpression ());

      QueryBodyCreator bodyCreator = new QueryBodyCreator (_root, _mainFromClause, _result);
      QueryBody body = bodyCreator.GetQueryBody ();

      SelectClause selectClause = body.SelectOrGroupClause as SelectClause;
      Assert.IsFalse (selectClause.Distinct);
    }

    [Test]
    public void BodyExpressions_TranslatedIntoFromClauses ()
    {
      _result.AddProjectionExpression (ExpressionHelper.CreateLambdaExpression());
      _result.AddProjectionExpression (ExpressionHelper.CreateLambdaExpression());

      FromExpression fromExpression1 = new FromExpression (ExpressionHelper.CreateLambdaExpression (), ExpressionHelper.CreateParameterExpression ());
      FromExpression fromExpression2 = new FromExpression (ExpressionHelper.CreateLambdaExpression (), Expression.Parameter (typeof (int), "j"));

      _result.AddBodyExpression (fromExpression1);
      _result.AddBodyExpression (fromExpression2);

      QueryBodyCreator bodyCreator = new QueryBodyCreator (_root, _mainFromClause, _result);
      QueryBody body = bodyCreator.GetQueryBody();
      Assert.AreEqual (2, body.BodyClauses.Count);

      AdditionalFromClause additionalFromClause1 = body.BodyClauses.First() as AdditionalFromClause;
      Assert.IsNotNull (additionalFromClause1);
      Assert.AreSame (fromExpression1.Expression, additionalFromClause1.FromExpression);
      Assert.AreSame (fromExpression1.Identifier, additionalFromClause1.Identifier);
      Assert.AreSame (_result.ProjectionExpressions[0], additionalFromClause1.ProjectionExpression);

      AdditionalFromClause additionalFromClause2 = body.BodyClauses.Last () as AdditionalFromClause;
      Assert.IsNotNull (additionalFromClause2);
      Assert.AreSame (fromExpression2.Expression, additionalFromClause2.FromExpression);
      Assert.AreSame (fromExpression2.Identifier, additionalFromClause2.Identifier);
      Assert.AreSame (_result.ProjectionExpressions[1], additionalFromClause2.ProjectionExpression);
    }

    [Test]
    public void LastProjectionExpresion_TranslatedIntoSelectClause_WithFromClauses ()
    {
      _result.AddProjectionExpression (ExpressionHelper.CreateLambdaExpression());
      _result.AddProjectionExpression (ExpressionHelper.CreateLambdaExpression());

      FromExpression fromExpression = new FromExpression (ExpressionHelper.CreateLambdaExpression(), ExpressionHelper.CreateParameterExpression());

      _result.AddBodyExpression (fromExpression);

      QueryBodyCreator bodyCreator = new QueryBodyCreator (_root, _mainFromClause, _result);
      QueryBody body = bodyCreator.GetQueryBody();
      Assert.AreEqual (1, body.BodyClauses.Count);

      SelectClause selectClause = body.SelectOrGroupClause as SelectClause;
      Assert.IsNotNull (selectClause);
      Assert.AreSame (_result.ProjectionExpressions[1], selectClause.ProjectionExpression);
    }

    [Test]
    public void BodyExpression_TranslatedIntoWhereClause ()
    {
      _result.AddProjectionExpression (ExpressionHelper.CreateLambdaExpression());

      WhereExpression whereExpression = new WhereExpression (ExpressionHelper.CreateLambdaExpression());

      _result.AddBodyExpression (whereExpression);

      QueryBodyCreator bodyCreator = new QueryBodyCreator (_root, _mainFromClause, _result);
      QueryBody body = bodyCreator.GetQueryBody();
      Assert.AreEqual (1, body.BodyClauses.Count);

      WhereClause whereClause = body.BodyClauses.First() as WhereClause;
      Assert.IsNotNull (whereClause);
      Assert.AreSame (whereExpression.Expression, whereClause.BoolExpression);
    }

    [Test]
    public void BodyExpression_TranslatedIntoOrderByClause ()
    {
      _result.AddProjectionExpression (ExpressionHelper.CreateLambdaExpression());

      OrderExpression orderExpression = new OrderExpression (true, OrderDirection.Asc, ExpressionHelper.CreateLambdaExpression());

      _result.AddBodyExpression (orderExpression);

      QueryBodyCreator bodyCreator = new QueryBodyCreator (_root, _mainFromClause, _result);
      QueryBody body = bodyCreator.GetQueryBody();
      Assert.AreEqual (1, body.BodyClauses.Count);

      OrderByClause orderByClause = body.BodyClauses.First() as OrderByClause;
      Assert.IsNotNull (orderByClause);
      Assert.AreEqual (1, orderByClause.OrderingList.Count);
      Assert.AreSame (orderExpression.Expression, orderByClause.OrderingList.First().Expression);
      Assert.AreEqual (orderExpression.OrderDirection, orderByClause.OrderingList.First().OrderDirection);
      Assert.AreSame (_mainFromClause, orderByClause.OrderingList.First().PreviousClause);
    }

    [Test]
    public void OrderByThenBy_TranslatedIntoOrderByClause ()
    {
      _result.AddProjectionExpression (ExpressionHelper.CreateLambdaExpression());

      OrderExpression orderExpression1 = new OrderExpression (true, OrderDirection.Asc, ExpressionHelper.CreateLambdaExpression());
      OrderExpression orderExpression2 = new OrderExpression (false, OrderDirection.Desc, ExpressionHelper.CreateLambdaExpression());
      OrderExpression orderExpression3 = new OrderExpression (true, OrderDirection.Asc, ExpressionHelper.CreateLambdaExpression());

      _result.AddBodyExpression (orderExpression1);
      _result.AddBodyExpression (orderExpression2);
      _result.AddBodyExpression (orderExpression3);

      QueryBodyCreator bodyCreator = new QueryBodyCreator (_root, _mainFromClause, _result);
      QueryBody body = bodyCreator.GetQueryBody();
      Assert.AreEqual (2, body.BodyClauses.Count);

      OrderByClause orderByClause1 = body.BodyClauses.First() as OrderByClause;
      OrderByClause orderByClause2 = body.BodyClauses.Last() as OrderByClause;

      Assert.IsNotNull (orderByClause1);
      Assert.IsNotNull (orderByClause2);

      Assert.AreEqual (2, orderByClause1.OrderingList.Count);
      Assert.AreEqual (1, orderByClause2.OrderingList.Count);

      Assert.AreSame (orderExpression1.Expression, orderByClause1.OrderingList.First().Expression);
      Assert.AreEqual (orderExpression1.OrderDirection, orderByClause1.OrderingList.First().OrderDirection);
      Assert.AreSame (_mainFromClause, orderByClause1.OrderingList.First().PreviousClause);
      Assert.AreSame (orderExpression2.Expression, orderByClause1.OrderingList.Last().Expression);
      Assert.AreEqual (orderExpression2.OrderDirection, orderByClause1.OrderingList.Last().OrderDirection);
      Assert.AreSame (orderByClause1, orderByClause1.OrderingList.Last().PreviousClause);
      Assert.AreSame (orderExpression3.Expression, orderByClause2.OrderingList.First().Expression);
      Assert.AreEqual (orderExpression3.OrderDirection, orderByClause2.OrderingList.First().OrderDirection);
      Assert.AreSame (orderByClause1, orderByClause2.OrderingList.First().PreviousClause);
    }

    [Test]
    public void MultiExpression_IntegrationTest ()
    {
      _result.AddProjectionExpression (ExpressionHelper.CreateLambdaExpression());
      _result.AddProjectionExpression (ExpressionHelper.CreateLambdaExpression());
      _result.AddProjectionExpression (ExpressionHelper.CreateLambdaExpression());

      FromExpression fromExpression1 = new FromExpression (ExpressionHelper.CreateLambdaExpression(), Expression.Parameter (typeof (Student), "s1"));
      FromExpression fromExpression2 = new FromExpression (ExpressionHelper.CreateLambdaExpression(), Expression.Parameter (typeof (Student), "s2"));
      WhereExpression whereExpression1 = new WhereExpression (ExpressionHelper.CreateLambdaExpression());
      WhereExpression whereExpression2 = new WhereExpression (ExpressionHelper.CreateLambdaExpression());

      OrderExpression orderExpression1 = new OrderExpression (true, OrderDirection.Asc, ExpressionHelper.CreateLambdaExpression());
      OrderExpression orderExpression2 = new OrderExpression (false, OrderDirection.Desc, ExpressionHelper.CreateLambdaExpression());
      OrderExpression orderExpression3 = new OrderExpression (true, OrderDirection.Asc, ExpressionHelper.CreateLambdaExpression());

      _result.AddBodyExpression (fromExpression1);
      _result.AddBodyExpression (fromExpression2);
      _result.AddBodyExpression (whereExpression1);
      _result.AddBodyExpression (whereExpression2);
      _result.AddBodyExpression (orderExpression1);
      _result.AddBodyExpression (orderExpression2);
      _result.AddBodyExpression (orderExpression3);


      QueryBodyCreator bodyCreator = new QueryBodyCreator (_root, _mainFromClause, _result);
      QueryBody body = bodyCreator.GetQueryBody();

      OrderByClause orderByClause1 = body.BodyClauses.Skip (4).First() as OrderByClause;
      OrderByClause orderByClause2 = body.BodyClauses.Skip (5).First() as OrderByClause;

      AdditionalFromClause fromClause1 = body.BodyClauses.First() as AdditionalFromClause;
      Assert.IsNotNull (fromClause1);
      Assert.AreSame (fromExpression1.Identifier, fromClause1.Identifier);
      Assert.AreSame (fromExpression1.Expression, fromClause1.FromExpression);
      Assert.AreSame (_result.ProjectionExpressions[0], fromClause1.ProjectionExpression);
      Assert.AreSame (_mainFromClause, fromClause1.PreviousClause);

      AdditionalFromClause fromClause2 = body.BodyClauses.Skip (1).First() as AdditionalFromClause;
      Assert.IsNotNull (fromClause2);
      Assert.AreSame (fromExpression2.Identifier, fromClause2.Identifier);
      Assert.AreSame (fromExpression2.Expression, fromClause2.FromExpression);
      Assert.AreSame (_result.ProjectionExpressions[1], fromClause2.ProjectionExpression);
      Assert.AreSame (fromClause1, fromClause2.PreviousClause);

      WhereClause whereClause1 = body.BodyClauses.Skip (2).First() as WhereClause;
      Assert.IsNotNull (whereClause1);
      Assert.AreSame (whereExpression1.Expression, whereClause1.BoolExpression);
      Assert.AreSame (fromClause2, whereClause1.PreviousClause);

      WhereClause whereClause2 = body.BodyClauses.Skip (3).First() as WhereClause;
      Assert.IsNotNull (whereClause2);
      Assert.AreSame (whereExpression2.Expression, whereClause2.BoolExpression);
      Assert.AreSame (whereClause1, whereClause2.PreviousClause);

      Assert.IsNotNull (orderByClause1);
      Assert.AreSame (orderExpression1.Expression, orderByClause1.OrderingList.First().Expression);
      Assert.AreSame (whereClause2, orderByClause1.OrderingList.First().PreviousClause);
      Assert.AreSame (whereClause2, orderByClause1.PreviousClause);

      Assert.AreSame (orderExpression2.Expression, orderByClause1.OrderingList.Last().Expression);
      Assert.AreSame (orderByClause1, orderByClause1.OrderingList.Last().PreviousClause);


      Assert.IsNotNull (orderByClause2);
      Assert.AreSame (orderExpression3.Expression, orderByClause2.OrderingList.First().Expression);
      Assert.AreSame (orderByClause1, orderByClause2.OrderingList.First().PreviousClause);
      Assert.AreSame (orderByClause1, orderByClause2.PreviousClause);

      SelectClause selectClause = body.SelectOrGroupClause as SelectClause;
      Assert.IsNotNull (selectClause);
      Assert.AreSame (_result.ProjectionExpressions[2], selectClause.ProjectionExpression);
      Assert.AreSame (orderByClause2, selectClause.PreviousClause);
    }
  }
}