using System;
using System.Collections.Generic;
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

    [SetUp]
    public void SetUp()
    {
      _source = ExpressionHelper.CreateQuerySource ();
      _root = ExpressionHelper.CreateExpression ();
      _mainFromClause = new MainFromClause (ExpressionHelper.CreateParameterExpression (), _source);
    }

    [Test]
    public void FirstBodyExpressionIsIgnored()
    {
      List<LambdaExpression> projectionExpressions = new List<LambdaExpression> ();
      projectionExpressions.Add (ExpressionHelper.CreateLambdaExpression ());

      List<BodyExpressionBase> bodyExpressions = new List<BodyExpressionBase> ();
      bodyExpressions.Add (new FromExpression (ExpressionHelper.CreateLambdaExpression (), ExpressionHelper.CreateParameterExpression ()));

      QueryBodyCreator bodyCreator = new QueryBodyCreator (_root, _mainFromClause, projectionExpressions, bodyExpressions);
      QueryBody body = bodyCreator.GetQueryBody();
      Assert.AreEqual (0, body.BodyClauses.Count, "no body clause from first body expression - this is reserved for the main from clause");
    }

    [Test]
    public void LastProjectionExpresion_TranslatedIntoSelectClause_NoFromClauses ()
    {
      List<LambdaExpression> projectionExpressions = new List<LambdaExpression> ();
      projectionExpressions.Add (ExpressionHelper.CreateLambdaExpression ());

      List<BodyExpressionBase> bodyExpressions = new List<BodyExpressionBase> ();

      QueryBodyCreator bodyCreator = new QueryBodyCreator (_root, _mainFromClause, projectionExpressions, bodyExpressions);
      QueryBody body = bodyCreator.GetQueryBody ();
      Assert.AreEqual (0, body.BodyClauses.Count);

      SelectClause selectClause = body.SelectOrGroupClause as SelectClause;
      Assert.IsNotNull (selectClause);
      Assert.AreSame (projectionExpressions[0], selectClause.ProjectionExpression);
    }

    [Test]
    public void SubsequentBodyExpression_TranslatedIntoFromClause ()
    {
      List<LambdaExpression> projectionExpressions = new List<LambdaExpression> ();
      projectionExpressions.Add (ExpressionHelper.CreateLambdaExpression ());
      projectionExpressions.Add (ExpressionHelper.CreateLambdaExpression ());

      FromExpression fromExpression = new FromExpression (ExpressionHelper.CreateLambdaExpression (), ExpressionHelper.CreateParameterExpression ());

      List<BodyExpressionBase> bodyExpressions = new List<BodyExpressionBase> ();
      bodyExpressions.Add (new FromExpression (ExpressionHelper.CreateLambdaExpression (), ExpressionHelper.CreateParameterExpression ()));
      bodyExpressions.Add (fromExpression);

      QueryBodyCreator bodyCreator = new QueryBodyCreator (_root, _mainFromClause, projectionExpressions, bodyExpressions);
      QueryBody body = bodyCreator.GetQueryBody ();
      Assert.AreEqual (1, body.BodyClauses.Count);
      
      AdditionalFromClause additionalFromClause = body.BodyClauses.First() as AdditionalFromClause;
      Assert.IsNotNull (additionalFromClause);
      Assert.AreSame (fromExpression.Expression, additionalFromClause.FromExpression);
      Assert.AreSame (fromExpression.Identifier, additionalFromClause.Identifier);
      Assert.AreSame (projectionExpressions[0], additionalFromClause.ProjectionExpression);
    }

    [Test]
    public void LastProjectionExpresion_TranslatedIntoSelectClause_WithFromClauses ()
    {
      List<LambdaExpression> projectionExpressions = new List<LambdaExpression> ();
      projectionExpressions.Add (ExpressionHelper.CreateLambdaExpression ());
      projectionExpressions.Add (ExpressionHelper.CreateLambdaExpression ());

      FromExpression fromExpression = new FromExpression (ExpressionHelper.CreateLambdaExpression (), ExpressionHelper.CreateParameterExpression ());

      List<BodyExpressionBase> bodyExpressions = new List<BodyExpressionBase> ();
      bodyExpressions.Add (new FromExpression (ExpressionHelper.CreateLambdaExpression (), ExpressionHelper.CreateParameterExpression ()));
      bodyExpressions.Add (fromExpression);

      QueryBodyCreator bodyCreator = new QueryBodyCreator (_root, _mainFromClause, projectionExpressions, bodyExpressions);
      QueryBody body = bodyCreator.GetQueryBody ();
      Assert.AreEqual (1, body.BodyClauses.Count);

      SelectClause selectClause = body.SelectOrGroupClause as SelectClause;
      Assert.IsNotNull (selectClause);
      Assert.AreSame (projectionExpressions[1], selectClause.ProjectionExpression);
    }

    [Test]
    public void SubsequentBodyExpression_TranslatedIntoWhereClause()
    {
      List<LambdaExpression> projectionExpressions = new List<LambdaExpression> ();
      projectionExpressions.Add (ExpressionHelper.CreateLambdaExpression ());
      
      WhereExpression whereExpression = new WhereExpression (ExpressionHelper.CreateLambdaExpression ());

      List<BodyExpressionBase> bodyExpressions = new List<BodyExpressionBase> ();
      bodyExpressions.Add (new FromExpression (ExpressionHelper.CreateLambdaExpression (), ExpressionHelper.CreateParameterExpression ()));
      bodyExpressions.Add (whereExpression);

      QueryBodyCreator bodyCreator = new QueryBodyCreator (_root, _mainFromClause, projectionExpressions, bodyExpressions);
      QueryBody body = bodyCreator.GetQueryBody ();
      Assert.AreEqual (1, body.BodyClauses.Count);

      WhereClause whereClause = body.BodyClauses.First () as WhereClause;
      Assert.IsNotNull (whereClause);
      Assert.AreSame (whereExpression.Expression, whereClause.BoolExpression);
    }      

    [Test]
    public void SubsequentBodyExpression_TranslatedIntoOrderByClause()
    {
      List<LambdaExpression> projectionExpressions = new List<LambdaExpression> ();
      projectionExpressions.Add (ExpressionHelper.CreateLambdaExpression ());

      OrderExpression orderExpression = new OrderExpression (true, OrderDirection.Asc, ExpressionHelper.CreateLambdaExpression());
      List<BodyExpressionBase> bodyExpressions = new List<BodyExpressionBase> ();
      bodyExpressions.Add (new FromExpression (ExpressionHelper.CreateLambdaExpression (), ExpressionHelper.CreateParameterExpression ()));
      bodyExpressions.Add (orderExpression);

      QueryBodyCreator bodyCreator = new QueryBodyCreator (_root, _mainFromClause, projectionExpressions, bodyExpressions);
      QueryBody body = bodyCreator.GetQueryBody ();
      Assert.AreEqual (1, body.BodyClauses.Count);

      OrderByClause orderByClause = body.BodyClauses.First() as OrderByClause;
      Assert.IsNotNull (orderByClause);
      Assert.AreEqual (1,orderByClause.OrderingList.Count);
      Assert.AreSame (orderExpression.Expression, orderByClause.OrderingList.First().Expression);
      Assert.AreEqual (orderExpression.OrderDirection, orderByClause.OrderingList.First().OrderDirection);
      Assert.AreSame (_mainFromClause, orderByClause.OrderingList.First ().PreviousClause);
    }

    [Test]
    public void OrderByThenBy_TranslatedIntoOrderByClause()
    {
      List<LambdaExpression> projectionExpressions = new List<LambdaExpression> ();
      projectionExpressions.Add (ExpressionHelper.CreateLambdaExpression ());


      OrderExpression orderExpression1 = new OrderExpression (true, OrderDirection.Asc, ExpressionHelper.CreateLambdaExpression ());
      OrderExpression orderExpression2 = new OrderExpression (false, OrderDirection.Desc, ExpressionHelper.CreateLambdaExpression ());
      OrderExpression orderExpression3 = new OrderExpression (true, OrderDirection.Asc, ExpressionHelper.CreateLambdaExpression ());
      List<BodyExpressionBase> bodyExpressions = new List<BodyExpressionBase> ();
      bodyExpressions.Add (new FromExpression (ExpressionHelper.CreateLambdaExpression (), ExpressionHelper.CreateParameterExpression ()));
      bodyExpressions.Add (orderExpression1);
      bodyExpressions.Add (orderExpression2);
      bodyExpressions.Add (orderExpression3);

      QueryBodyCreator bodyCreator = new QueryBodyCreator (_root, _mainFromClause, projectionExpressions, bodyExpressions);
      QueryBody body = bodyCreator.GetQueryBody ();
      Assert.AreEqual (2, body.BodyClauses.Count);

      OrderByClause orderByClause1 = body.BodyClauses.First () as OrderByClause;
      OrderByClause orderByClause2 = body.BodyClauses.Last () as OrderByClause;

      Assert.IsNotNull (orderByClause1);
      Assert.IsNotNull (orderByClause2);

      Assert.AreEqual(2,orderByClause1.OrderingList.Count);
      Assert.AreEqual (1, orderByClause2.OrderingList.Count);

      Assert.AreSame (orderExpression1.Expression, orderByClause1.OrderingList.First().Expression);
      Assert.AreEqual (orderExpression1.OrderDirection, orderByClause1.OrderingList.First().OrderDirection);
      Assert.AreSame (_mainFromClause, orderByClause1.OrderingList.First().PreviousClause);
      Assert.AreSame (orderExpression2.Expression, orderByClause1.OrderingList.Last().Expression);
      Assert.AreEqual (orderExpression2.OrderDirection, orderByClause1.OrderingList.Last().OrderDirection);
      Assert.AreSame (orderByClause1, orderByClause1.OrderingList.Last().PreviousClause);
      Assert.AreSame (orderExpression3.Expression, orderByClause2.OrderingList.First().Expression);
      Assert.AreEqual (orderExpression3.OrderDirection, orderByClause2.OrderingList.First ().OrderDirection);
      Assert.AreSame (orderByClause1, orderByClause2.OrderingList.First ().PreviousClause);

    }

    [Test]
    public void MultiExpression_IntegrationTest ()
    {
      List<LambdaExpression> projectionExpressions = new List<LambdaExpression>();
      projectionExpressions.Add (ExpressionHelper.CreateLambdaExpression());
      projectionExpressions.Add (ExpressionHelper.CreateLambdaExpression());
      projectionExpressions.Add (ExpressionHelper.CreateLambdaExpression());

      FromExpression fromExpression1 = new FromExpression (ExpressionHelper.CreateLambdaExpression (), Expression.Parameter (typeof (Student), "s1"));
      FromExpression fromExpression2 = new FromExpression (ExpressionHelper.CreateLambdaExpression (), Expression.Parameter (typeof (Student), "s2"));
      WhereExpression whereExpression1 = new WhereExpression (ExpressionHelper.CreateLambdaExpression ());
      WhereExpression whereExpression2 = new WhereExpression (ExpressionHelper.CreateLambdaExpression ());

      OrderExpression orderExpression1 = new OrderExpression (true, OrderDirection.Asc, ExpressionHelper.CreateLambdaExpression ());
      OrderExpression orderExpression2 = new OrderExpression (false, OrderDirection.Desc, ExpressionHelper.CreateLambdaExpression ());
      OrderExpression orderExpression3 = new OrderExpression (true, OrderDirection.Asc, ExpressionHelper.CreateLambdaExpression ());

      List<BodyExpressionBase> bodyExpressions = new List<BodyExpressionBase> ();
      bodyExpressions.Add (new FromExpression (ExpressionHelper.CreateLambdaExpression (), ExpressionHelper.CreateParameterExpression ())); // main from
      bodyExpressions.Add (fromExpression1);
      bodyExpressions.Add (fromExpression2);
      bodyExpressions.Add (whereExpression1);
      bodyExpressions.Add (whereExpression2);
      bodyExpressions.Add (orderExpression1);
      bodyExpressions.Add (orderExpression2);
      bodyExpressions.Add (orderExpression3);

      
      QueryBodyCreator bodyCreator = new QueryBodyCreator (_root, _mainFromClause, projectionExpressions, bodyExpressions);
      QueryBody body = bodyCreator.GetQueryBody();
      
      OrderByClause orderByClause1 = body.BodyClauses.Skip (4).First() as OrderByClause;
      OrderByClause orderByClause2 = body.BodyClauses.Skip (5).First () as OrderByClause;

      AdditionalFromClause fromClause1 = body.BodyClauses.First () as AdditionalFromClause;
      Assert.IsNotNull (fromClause1);
      Assert.AreSame (fromExpression1.Identifier, fromClause1.Identifier);
      Assert.AreSame (fromExpression1.Expression, fromClause1.FromExpression);
      Assert.AreSame (projectionExpressions[0], fromClause1.ProjectionExpression);
      Assert.AreSame (_mainFromClause, fromClause1.PreviousClause);

      AdditionalFromClause fromClause2 = body.BodyClauses.Skip(1).First () as AdditionalFromClause;
      Assert.IsNotNull (fromClause2);
      Assert.AreSame (fromExpression2.Identifier, fromClause2.Identifier);
      Assert.AreSame (fromExpression2.Expression, fromClause2.FromExpression);
      Assert.AreSame (projectionExpressions[1], fromClause2.ProjectionExpression);
      Assert.AreSame (fromClause1, fromClause2.PreviousClause);

      WhereClause whereClause1 = body.BodyClauses.Skip (2).First () as WhereClause;
      Assert.IsNotNull (whereClause1);
      Assert.AreSame (whereExpression1.Expression, whereClause1.BoolExpression);
      Assert.AreSame (fromClause2, whereClause1.PreviousClause);

      WhereClause whereClause2 = body.BodyClauses.Skip (3).First () as WhereClause;
      Assert.IsNotNull (whereClause2);
      Assert.AreSame (whereExpression2.Expression, whereClause2.BoolExpression);
      Assert.AreSame (whereClause1, whereClause2.PreviousClause);
      
      Assert.IsNotNull (orderByClause1);
      Assert.AreSame (orderExpression1.Expression, orderByClause1.OrderingList.First().Expression);
      Assert.AreSame (whereClause2, orderByClause1.OrderingList.First().PreviousClause);
      Assert.AreSame (whereClause2, orderByClause1.PreviousClause);

      Assert.AreSame (orderExpression2.Expression, orderByClause1.OrderingList.Last().Expression);
      Assert.AreSame (orderByClause1,orderByClause1.OrderingList.Last().PreviousClause);

      
      Assert.IsNotNull (orderByClause2);
      Assert.AreSame (orderExpression3.Expression, orderByClause2.OrderingList.First ().Expression);
      Assert.AreSame (orderByClause1, orderByClause2.OrderingList.First().PreviousClause);
      Assert.AreSame (orderByClause1, orderByClause2.PreviousClause);

      SelectClause selectClause = body.SelectOrGroupClause as SelectClause;
      Assert.IsNotNull (selectClause);
      Assert.AreSame (projectionExpressions[2], selectClause.ProjectionExpression);
      Assert.AreSame (orderByClause2, selectClause.PreviousClause);

    }

  }
}