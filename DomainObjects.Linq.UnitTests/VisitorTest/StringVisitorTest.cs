using System;
using System.Linq.Expressions;
using NUnit.Framework;
using Rhino.Mocks;
using Rubicon.Data.DomainObjects.Linq.Clauses;
using Rubicon.Data.DomainObjects.Linq.Visitor;

namespace Rubicon.Data.DomainObjects.Linq.UnitTests.VisitorTest
{
  using Text = NUnit.Framework.SyntaxHelpers.Text;

  [TestFixture]
  public class StringVisitorTest
  {
    [Test]
    public void StringVisitorForMainFromClause ()
    {
      MainFromClause fromClause = ExpressionHelper.CreateMainFromClause();

      StringVisitor sv = new StringVisitor();

      sv.VisitMainFromClause (fromClause);

      Assert.That (sv.ToString(), Text.Contains ("from"));
    }

    [Test]
    public void StringVisitorForFromClauseWithJoins ()
    {
      MainFromClause fromClause = ExpressionHelper.CreateMainFromClause();

      MockRepository repository = new MockRepository();
      JoinClause joinClause1 = repository.CreateMock<JoinClause> (ExpressionHelper.CreateParameterExpression(),
                                                                  ExpressionHelper.CreateExpression(), ExpressionHelper.CreateExpression(), ExpressionHelper.CreateExpression());
      JoinClause joinClause2 = repository.CreateMock<JoinClause> (ExpressionHelper.CreateParameterExpression(),
                                                                  ExpressionHelper.CreateExpression(), ExpressionHelper.CreateExpression(), ExpressionHelper.CreateExpression());

      fromClause.Add (joinClause1);
      fromClause.Add (joinClause2);

      StringVisitor sv = new StringVisitor();

      // expectations
      using (repository.Ordered())
      {
        joinClause1.Accept (sv);
        joinClause2.Accept (sv);
      }

      repository.ReplayAll();

      sv.VisitMainFromClause (fromClause);

      repository.VerifyAll();
    }

    [Test]
    public void StringVisitorForJoins()
    {
      JoinClause joinClause = ExpressionHelper.CreateJoinClause();

      StringVisitor sv = new StringVisitor();

      sv.VisitJoinClause (joinClause);

      Assert.That (sv.ToString(), Text.Contains ("join"));
    }


    [Test]
    public void StringVisitorForSelectClause()
    {
      SelectClause selectClause = ExpressionHelper.CreateSelectClause();
      StringVisitor sv = new StringVisitor();

      sv.VisitSelectClause (selectClause);

      Assert.That (sv.ToString (), Text.Contains ("select"));
    }

    [Test]
    public void StringVisitorForGroupClause()
    {
      GroupClause groupClause = new GroupClause (ExpressionHelper.CreateExpression(), ExpressionHelper.CreateExpression());
      StringVisitor sv = new StringVisitor ();

      sv.VisitGroupClause (groupClause);

      Assert.That (sv.ToString(), Text.Contains ("group"));
    }

    [Test]
    public void StringVisitorForWhereClause()
    {
      WhereClause whereClasue = ExpressionHelper.CreateWhereClause();
      StringVisitor sv = new StringVisitor();

      sv.VisitWhereClause (whereClasue);

      Assert.That (sv.ToString(), Text.Contains ("where"));
    }

    [Test]
    public void StringVisitorForLetClause()
    {
      LetClause letClause = ExpressionHelper.CreateLetClause();
      StringVisitor sv = new StringVisitor();

      sv.VisitLetClause (letClause);

      Assert.That (sv.ToString(), Text.Contains ("let"));
    }

    [Test]
    public void StringVisitorForOrderingClauseAsc()
    {
      OrderingClause orderingClause = ExpressionHelper.CreateOrderingClause();
      StringVisitor sv = new StringVisitor();

      sv.VisitOrderingClause (orderingClause);

      Assert.That (sv.ToString(), Text.Contains("ascending"));
    }


    [Test]
    public void StringVisitorForOrderingClauseDesc ()
    {
      OrderingClause orderingClause = new OrderingClause (ExpressionHelper.CreateExpression(), OrderDirection.Desc);
      StringVisitor sv = new StringVisitor ();

      sv.VisitOrderingClause (orderingClause);

      Assert.That (sv.ToString (), Text.Contains ("descending"));
    }



    [Test]
    public void StringVisitorForOrderByClauseOrderDirectionAsc()
    {
      OrderByClause orderByClause = ExpressionHelper.CreateOrderByClause();

      MockRepository repository = new MockRepository();

      OrderingClause ordering1 = repository.CreateMock<OrderingClause> (ExpressionHelper.CreateExpression(), OrderDirection.Asc);
      OrderingClause ordering2 = repository.CreateMock<OrderingClause> (ExpressionHelper.CreateExpression (), OrderDirection.Asc);

      orderByClause.Add (ordering1);
      orderByClause.Add (ordering2);

      StringVisitor sv = new StringVisitor ();

      //expectations
      using(repository.Ordered())
      {
        ordering1.Accept (sv);
        ordering2.Accept (sv);
      }

      repository.ReplayAll();

      sv.VisitOrderByClause (orderByClause);

      repository.VerifyAll();
    }


    [Test]
    public void StringVisitorForOrderByClauseOrderDirectionDesc ()
    {
      OrderByClause orderByClause = ExpressionHelper.CreateOrderByClause ();

      MockRepository repository = new MockRepository ();

      OrderingClause ordering1 = repository.CreateMock<OrderingClause> (ExpressionHelper.CreateExpression (), OrderDirection.Desc);
      OrderingClause ordering2 = repository.CreateMock<OrderingClause> (ExpressionHelper.CreateExpression (), OrderDirection.Desc);

      orderByClause.Add (ordering1);
      orderByClause.Add (ordering2);

      StringVisitor sv = new StringVisitor ();

      //expectations
      using (repository.Ordered ())
      {
        ordering1.Accept (sv);
        ordering2.Accept (sv);
      }

      repository.ReplayAll ();

      sv.VisitOrderByClause (orderByClause);

      repository.VerifyAll ();
    }

    [Test]
    public void StringVisitorForOrderByClauseOrderDirectionMixedAscDesc ()
    {
      OrderByClause orderByClause = ExpressionHelper.CreateOrderByClause ();

      MockRepository repository = new MockRepository ();

      OrderingClause ordering1 = repository.CreateMock<OrderingClause> (ExpressionHelper.CreateExpression (), OrderDirection.Desc);
      OrderingClause ordering2 = repository.CreateMock<OrderingClause> (ExpressionHelper.CreateExpression (), OrderDirection.Asc);

      orderByClause.Add (ordering1);
      orderByClause.Add (ordering2);

      StringVisitor sv = new StringVisitor ();

      //expectations
      using (repository.Ordered ())
      {
        ordering1.Accept (sv);
        ordering2.Accept (sv);
      }

      repository.ReplayAll ();

      sv.VisitOrderByClause (orderByClause);

      repository.VerifyAll ();
    }


    [Test]
    public void StringVisitorQueryBody()
    {
      MockRepository repository = new MockRepository();

      SelectClause selectClause1 = 
        repository.CreateMock<SelectClause> (new object[] {ExpressionHelper.CreateLambdaExpression()});

      OrderByClause orderByClause1 = 
        repository.CreateMock<OrderByClause> (ExpressionHelper.CreateOrderingClause());

      AdditionalFromClause fromClause1 = 
        repository.CreateMock<AdditionalFromClause> (ExpressionHelper.CreateParameterExpression(),
        ExpressionHelper.CreateLambdaExpression(), ExpressionHelper.CreateLambdaExpression());

      WhereClause whereClause1 =
          repository.CreateMock<WhereClause> (ExpressionHelper.CreateLambdaExpression());

      QueryBody queryBody = new QueryBody (selectClause1, orderByClause1);
      
      queryBody.Add (fromClause1);
      queryBody.Add (whereClause1);

      StringVisitor sv = new StringVisitor ();

      //expectations
      using(repository.Ordered())
      {
        fromClause1.Accept (sv);
        whereClause1.Accept (sv);
        orderByClause1.Accept (sv);
        selectClause1.Accept (sv);
      }

      repository.ReplayAll ();

      sv.VisitQueryBody (queryBody);
      
      repository.VerifyAll();
    }

    [Test]
    public void StringVisitorQueryBodyOnlyWithSelectGroupClause()
    {
      MockRepository repository = new MockRepository ();

      SelectClause selectClause1 =
        repository.CreateMock<SelectClause> (new object[] {ExpressionHelper.CreateLambdaExpression () });

      QueryBody queryBody = new QueryBody (selectClause1);

      StringVisitor sv = new StringVisitor();

      //expectation
      selectClause1.Accept(sv);

      repository.ReplayAll();

      sv.VisitQueryBody(queryBody);

      repository.VerifyAll();

    }

    [Test]
    public void StringVisitorQueryExpression()
    {
      MockRepository repository = new MockRepository();

      MainFromClause fromClause = ExpressionHelper.CreateMainFromClause();
      QueryBody queryBody = new QueryBody (ExpressionHelper.CreateSelectClause(), ExpressionHelper.CreateOrderByClause());


      QueryExpression queryExpression = new QueryExpression (fromClause, queryBody);

      StringVisitor sv = new StringVisitor();

      //expectations
      using(repository.Ordered())
      {
        fromClause.Accept(sv);
        queryBody.Accept(sv);
      }

      repository.ReplayAll();
      sv.VisitQueryExpression (queryExpression);
      repository.VerifyAll();

    }

    [Test]
    public void StringVisitorForAdditionalFromClauses()
    {
      AdditionalFromClause fromClause = ExpressionHelper.CreateAdditionalFromClause ();

      StringVisitor sv = new StringVisitor ();

      sv.VisitAdditionalFromClause (fromClause);

      Assert.That (sv.ToString (), Text.Contains ("from"));
    }



  }
}