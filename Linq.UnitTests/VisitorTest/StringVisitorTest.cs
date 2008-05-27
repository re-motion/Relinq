using System;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using NUnit.Framework;
using Rhino.Mocks;
using Remotion.Data.Linq.Clauses;
using Remotion.Data.Linq.Visitor;

namespace Remotion.Data.Linq.UnitTests.VisitorTest
{
  using Text = NUnit.Framework.SyntaxHelpers.Text;
  using System.Linq;

  [TestFixture]
  public class StringVisitorTest
  {
    [Test]
    public void StringVisitorForMainFromClause ()
    {
      MainFromClause fromClause = ExpressionHelper.CreateMainFromClause();
      StringVisitor sv = new StringVisitor();
      sv.VisitMainFromClause (fromClause);
      Assert.AreEqual ("from Int32 i in value(Remotion.Data.Linq.UnitTests.TestQueryable`1[Remotion.Data.Linq.UnitTests.Student]) ",
          sv.ToString());
    }

    [Test]
    public void StringVisitorForFromClauseWithJoins ()
    {
      MainFromClause fromClause = ExpressionHelper.CreateMainFromClause();

      MockRepository repository = new MockRepository();
      JoinClause joinClause1 =
          repository.CreateMock<JoinClause> (ExpressionHelper.CreateClause(), ExpressionHelper.CreateParameterExpression(),
              ExpressionHelper.CreateExpression(), ExpressionHelper.CreateExpression(), ExpressionHelper.CreateExpression());
      JoinClause joinClause2 =
          repository.CreateMock<JoinClause> (ExpressionHelper.CreateClause(), ExpressionHelper.CreateParameterExpression(),
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
    public void StringVisitorForJoins ()
    {
      JoinClause joinClause = ExpressionHelper.CreateJoinClause();

      StringVisitor sv = new StringVisitor();

      sv.VisitJoinClause (joinClause);

      Assert.That (sv.ToString(), Text.Contains ("join"));
    }


    [Test]
    public void StringVisitorForSelectClause ()
    {
      SelectClause selectClause = ExpressionHelper.CreateSelectClause();
      StringVisitor sv = new StringVisitor();

      sv.VisitSelectClause (selectClause);

      Assert.AreEqual ("select 0", sv.ToString());
    }

    [Test]
    public void StringVisitorForSelectClause_WithNullProjection ()
    {
      SelectClause selectClause = new SelectClause (ExpressionHelper.CreateClause(), null, false);
      StringVisitor sv = new StringVisitor ();

      sv.VisitSelectClause (selectClause);

      Assert.AreEqual ("select <value>", sv.ToString ());
    }

    [Test]
    public void StringVisitorForGroupClause ()
    {
      GroupClause groupClause =
          new GroupClause (ExpressionHelper.CreateClause(), ExpressionHelper.CreateExpression(), ExpressionHelper.CreateExpression());
      StringVisitor sv = new StringVisitor();

      sv.VisitGroupClause (groupClause);

      Assert.That (sv.ToString(), Text.Contains ("group"));
    }

    [Test]
    public void StringVisitorForWhereClause ()
    {
      WhereClause whereClasue = ExpressionHelper.CreateWhereClause();
      StringVisitor sv = new StringVisitor();

      sv.VisitWhereClause (whereClasue);

      Assert.AreEqual("where (1 = 2) ", sv.ToString());
    }

    [Test]
    public void StringVisitorForLetClause ()
    {
      LetClause letClause = ExpressionHelper.CreateLetClause();
      StringVisitor sv = new StringVisitor();

      sv.VisitLetClause (letClause);

      Assert.That (sv.ToString(), Text.Contains ("let"));
    }

    [Test]
    public void StringVisitorForOrderingClauseAsc ()
    {
      OrderingClause orderingClause = ExpressionHelper.CreateOrderingClause();
      StringVisitor sv = new StringVisitor();

      sv.VisitOrderingClause (orderingClause);

      Assert.That (sv.ToString(), Text.Contains ("ascending"));
    }


    [Test]
    public void StringVisitorForOrderingClauseDesc ()
    {
      OrderingClause orderingClause = new OrderingClause (ExpressionHelper.CreateClause(), ExpressionHelper.CreateLambdaExpression(), OrderDirection.Desc);
      StringVisitor sv = new StringVisitor();

      sv.VisitOrderingClause (orderingClause);

      Assert.That (sv.ToString(), Text.Contains ("descending"));
    }


    [Test]
    public void StringVisitorForOrderByClauseOrderDirectionAsc ()
    {
      OrderByClause orderByClause = ExpressionHelper.CreateOrderByClause();

      MockRepository repository = new MockRepository();

      OrderingClause ordering1 = repository.CreateMock<OrderingClause> (ExpressionHelper.CreateClause (), ExpressionHelper.CreateLambdaExpression (), OrderDirection.Asc);
      OrderingClause ordering2 = repository.CreateMock<OrderingClause> (ExpressionHelper.CreateClause (), ExpressionHelper.CreateLambdaExpression (), OrderDirection.Asc);

      orderByClause.Add (ordering1);
      orderByClause.Add (ordering2);

      StringVisitor sv = new StringVisitor();

      //expectations
      using (repository.Ordered())
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
      OrderByClause orderByClause = ExpressionHelper.CreateOrderByClause();

      MockRepository repository = new MockRepository();

      OrderingClause ordering1 =
          repository.CreateMock<OrderingClause> (ExpressionHelper.CreateClause(), ExpressionHelper.CreateLambdaExpression(), OrderDirection.Desc);
      OrderingClause ordering2 =
          repository.CreateMock<OrderingClause> (ExpressionHelper.CreateClause(), ExpressionHelper.CreateLambdaExpression(), OrderDirection.Desc);

      orderByClause.Add (ordering1);
      orderByClause.Add (ordering2);

      StringVisitor sv = new StringVisitor();

      //expectations
      using (repository.Ordered())
      {
        ordering1.Accept (sv);
        ordering2.Accept (sv);
      }

      repository.ReplayAll();

      sv.VisitOrderByClause (orderByClause);

      repository.VerifyAll();
    }

    [Test]
    public void StringVisitorForOrderByClauseOrderDirectionMixedAscDesc ()
    {
      OrderByClause orderByClause = ExpressionHelper.CreateOrderByClause();

      MockRepository repository = new MockRepository();

      OrderingClause ordering1 =
          repository.CreateMock<OrderingClause> (ExpressionHelper.CreateClause(), ExpressionHelper.CreateLambdaExpression(), OrderDirection.Desc);
      OrderingClause ordering2 =
          repository.CreateMock<OrderingClause> (ExpressionHelper.CreateClause(), ExpressionHelper.CreateLambdaExpression(), OrderDirection.Asc);

      orderByClause.Add (ordering1);
      orderByClause.Add (ordering2);

      StringVisitor sv = new StringVisitor();

      //expectations
      using (repository.Ordered())
      {
        ordering1.Accept (sv);
        ordering2.Accept (sv);
      }

      repository.ReplayAll();

      sv.VisitOrderByClause (orderByClause);

      repository.VerifyAll();
    }

    [Test]
    public void StringVisitorQueryExpression_NoBodyClauses ()
    {
      MockRepository repository = new MockRepository();

      MainFromClause fromClause = ExpressionHelper.CreateMainFromClause();
      SelectClause selectClause1 =
          repository.CreateMock<SelectClause> (ExpressionHelper.CreateClause (), ExpressionHelper.CreateLambdaExpression (), false);
      
      QueryModel queryModel = new QueryModel (typeof (IQueryable<string>), fromClause, selectClause1);

      StringVisitor sv = new StringVisitor();

      //expectations
      using (repository.Ordered())
      {
        fromClause.Accept (sv);
        selectClause1.Accept (sv);
      }

      repository.ReplayAll();
      sv.VisitQueryModel (queryModel);
      repository.VerifyAll();
    }

    [Test]
    public void StringVisitorQueryExpression_WithBodyClauses ()
    {
      MockRepository repository = new MockRepository ();

      MainFromClause fromClause = ExpressionHelper.CreateMainFromClause ();
      SelectClause selectClause1 =
          repository.CreateMock<SelectClause> (ExpressionHelper.CreateClause (), ExpressionHelper.CreateLambdaExpression (), false);
      OrderByClause orderByClause1 =
          repository.CreateMock<OrderByClause> (ExpressionHelper.CreateOrderingClause ());
      AdditionalFromClause fromClause1 =
          repository.CreateMock<AdditionalFromClause> (ExpressionHelper.CreateClause (), Expression.Parameter(typeof(Student),"p"),
              ExpressionHelper.CreateLambdaExpression (), ExpressionHelper.CreateLambdaExpression ());
      WhereClause whereClause1 =
          repository.CreateMock<WhereClause> (ExpressionHelper.CreateClause (), ExpressionHelper.CreateLambdaExpression ());

      QueryModel queryModel = new QueryModel (typeof (IQueryable<string>), fromClause, selectClause1);
      queryModel.AddBodyClause (orderByClause1);
      queryModel.AddBodyClause (fromClause1);
      queryModel.AddBodyClause (whereClause1);

      StringVisitor sv = new StringVisitor ();

      //expectations
      using (repository.Ordered ())
      {
        fromClause.Accept (sv);
        orderByClause1.Accept (sv);
        fromClause1.Accept (sv);
        whereClause1.Accept (sv);
        selectClause1.Accept (sv);
      }

      repository.ReplayAll ();
      sv.VisitQueryModel (queryModel);
      repository.VerifyAll ();
    }

    [CompilerGenerated]
    class DisplayClass
    {
      public object source;
    }

    [Test]
    public void StringVisitorForAdditionalFromClauses_WithMemberAccessToDisplayClass ()
    {
      var displayClass = new DisplayClass {source = ExpressionHelper.CreateQuerySource()};

      LambdaExpression fromExpression =
          Expression.Lambda (Expression.MakeMemberAccess (Expression.Constant (displayClass), displayClass.GetType ().GetField ("source")),
          Expression.Parameter (typeof (Student), "s2"));

      AdditionalFromClause fromClause = new AdditionalFromClause (ExpressionHelper.CreateClause (), ExpressionHelper.CreateParameterExpression (), 
          fromExpression, ExpressionHelper.CreateLambdaExpression ());

      StringVisitor sv = new StringVisitor();

      sv.VisitAdditionalFromClause (fromClause);

      Assert.AreEqual ("from Int32 i in source ", sv.ToString());
    }

    [Test]
    public void StringVisitorForAdditionalFromClauses_WithOtherFromExpression ()
    {
      LambdaExpression fromExpression = Expression.Lambda (Expression.Constant (1));

      AdditionalFromClause fromClause = new AdditionalFromClause (ExpressionHelper.CreateClause (), ExpressionHelper.CreateParameterExpression (),
          fromExpression, ExpressionHelper.CreateLambdaExpression ());

      StringVisitor sv = new StringVisitor ();

      sv.VisitAdditionalFromClause (fromClause);

      Assert.AreEqual ("from Int32 i in 1 ", sv.ToString ());
    }

    [Test]
    public void StringVisitorForAdditionalFromClauses_WithConstantNullFromExpression ()
    {
      LambdaExpression fromExpression = Expression.Lambda (Expression.Constant (null));

      AdditionalFromClause fromClause = new AdditionalFromClause (ExpressionHelper.CreateClause (), ExpressionHelper.CreateParameterExpression (),
          fromExpression, ExpressionHelper.CreateLambdaExpression ());

      StringVisitor sv = new StringVisitor ();

      sv.VisitAdditionalFromClause (fromClause);

      Assert.AreEqual ("from Int32 i in null ", sv.ToString ());
    }

    [Test]
    public void StringVisitorForSubQueryFromClause ()
    {
      IClause previousClause = ExpressionHelper.CreateMainFromClause();
      ParameterExpression identifier = Expression.Parameter (typeof (Student), "s");
      ParameterExpression subQueryIdentifier = Expression.Parameter (typeof (Student), "s2");
      Expression querySource = Expression.Constant (null);
      MainFromClause mainFromClause = new MainFromClause (subQueryIdentifier, querySource);
      LambdaExpression subQueryProjection = Expression.Lambda (Expression.Constant (1));
      SelectClause selectClause = new SelectClause (previousClause, subQueryProjection, false);
      QueryModel subQuery = new QueryModel (typeof (string), mainFromClause, selectClause);
      LambdaExpression projectionExpression = ExpressionHelper.CreateLambdaExpression();

      SubQueryFromClause subQueryFromClause = new SubQueryFromClause (previousClause, identifier, subQuery, projectionExpression);
      
      StringVisitor sv = new StringVisitor();
      sv.VisitSubQueryFromClause (subQueryFromClause);

      Assert.AreEqual ("from Student s in (from Student s2 in null select 1) ", sv.ToString());
    }
  }
}