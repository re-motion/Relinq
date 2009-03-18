// This file is part of the re-motion Core Framework (www.re-motion.org)
// Copyright (C) 2005-2008 rubicon informationstechnologie gmbh, www.rubicon.eu
// 
// The re-motion Core Framework is free software; you can redistribute it 
// and/or modify it under the terms of the GNU Lesser General Public License 
// version 3.0 as published by the Free Software Foundation.
// 
// re-motion is distributed in the hope that it will be useful, 
// but WITHOUT ANY WARRANTY; without even the implied warranty of 
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the 
// GNU Lesser General Public License for more details.
// 
// You should have received a copy of the GNU Lesser General Public License
// along with re-motion; if not, see http://www.gnu.org/licenses.
// 
using System;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using NUnit.Framework;
using Remotion.Data.Linq;
using Rhino.Mocks;
using Remotion.Data.Linq.Clauses;
using Remotion.Data.Linq.Visitor;

namespace Remotion.Data.UnitTests.Linq.VisitorTest
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
      Assert.AreEqual ("from Int32 i in TestQueryable<Student>() ",
          sv.ToString());
    }

    [Test]
    public void StringVisitorForFromClauseWithJoins ()
    {
      MainFromClause fromClause = ExpressionHelper.CreateMainFromClause();

      MockRepository repository = new MockRepository();
      JoinClause joinClause1 =
          repository.StrictMock<JoinClause> (ExpressionHelper.CreateClause(), fromClause, ExpressionHelper.CreateParameterExpression(),
              ExpressionHelper.CreateExpression(), ExpressionHelper.CreateExpression(), ExpressionHelper.CreateExpression());
      JoinClause joinClause2 =
          repository.StrictMock<JoinClause> (ExpressionHelper.CreateClause (), fromClause, ExpressionHelper.CreateParameterExpression (),
              ExpressionHelper.CreateExpression(), ExpressionHelper.CreateExpression(), ExpressionHelper.CreateExpression());

      fromClause.AddJoinClause (joinClause1);
      fromClause.AddJoinClause (joinClause2);

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
      SelectClause selectClause = new SelectClause (ExpressionHelper.CreateClause (), null, null);
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
      Ordering ordering = ExpressionHelper.CreateOrdering();
      StringVisitor sv = new StringVisitor();

      sv.VisitOrdering (ordering);

      Assert.That (sv.ToString(), Text.Contains ("ascending"));
    }


    [Test]
    public void StringVisitorForOrderingClauseDesc ()
    {
      Ordering ordering = new Ordering (ExpressionHelper.CreateOrderByClause(), ExpressionHelper.CreateLambdaExpression(), OrderingDirection.Desc);
      StringVisitor sv = new StringVisitor();

      sv.VisitOrdering (ordering);

      Assert.That (sv.ToString(), Text.Contains ("descending"));
    }


    [Test]
    public void StringVisitorForOrderByClauseOrderDirectionAsc ()
    {
      OrderByClause orderByClause = ExpressionHelper.CreateOrderByClause();

      MockRepository repository = new MockRepository();

      Ordering ordering1 = repository.StrictMock<Ordering> (ExpressionHelper.CreateOrderByClause (), ExpressionHelper.CreateLambdaExpression (), OrderingDirection.Asc);
      Ordering ordering2 = repository.StrictMock<Ordering> (ExpressionHelper.CreateOrderByClause (), ExpressionHelper.CreateLambdaExpression (), OrderingDirection.Asc);

      orderByClause.AddOrdering (ordering1);
      orderByClause.AddOrdering (ordering2);

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

      Ordering ordering1 =
          repository.StrictMock<Ordering> (ExpressionHelper.CreateOrderByClause(), ExpressionHelper.CreateLambdaExpression(), OrderingDirection.Desc);
      Ordering ordering2 =
          repository.StrictMock<Ordering> (ExpressionHelper.CreateOrderByClause (), ExpressionHelper.CreateLambdaExpression (), OrderingDirection.Desc);

      orderByClause.AddOrdering (ordering1);
      orderByClause.AddOrdering (ordering2);

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

      Ordering ordering1 =
          repository.StrictMock<Ordering> (ExpressionHelper.CreateOrderByClause (), ExpressionHelper.CreateLambdaExpression (), OrderingDirection.Desc);
      Ordering ordering2 =
          repository.StrictMock<Ordering> (ExpressionHelper.CreateOrderByClause (), ExpressionHelper.CreateLambdaExpression (), OrderingDirection.Asc);

      orderByClause.AddOrdering (ordering1);
      orderByClause.AddOrdering (ordering2);

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
          repository.StrictMock<SelectClause> (ExpressionHelper.CreateClause (), ExpressionHelper.CreateLambdaExpression (), null);
      
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
          repository.StrictMock<SelectClause> (ExpressionHelper.CreateClause (), ExpressionHelper.CreateLambdaExpression (), null);
      OrderByClause orderByClause1 =
          repository.StrictMock<OrderByClause> (ExpressionHelper.CreateClause ());
      AdditionalFromClause fromClause1 =
          repository.StrictMock<AdditionalFromClause> (ExpressionHelper.CreateClause (), Expression.Parameter(typeof(Student),"p"),
              ExpressionHelper.CreateLambdaExpression (), ExpressionHelper.CreateLambdaExpression ());
      WhereClause whereClause1 =
          repository.StrictMock<WhereClause> (ExpressionHelper.CreateClause (), ExpressionHelper.CreateLambdaExpression ());

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
      //SelectClause selectClause = new SelectClause (previousClause, subQueryProjection, false);
      SelectClause selectClause = new SelectClause (previousClause, subQueryProjection, null);
      QueryModel subQuery = new QueryModel (typeof (string), mainFromClause, selectClause);
      LambdaExpression projectionExpression = ExpressionHelper.CreateLambdaExpression();

      SubQueryFromClause subQueryFromClause = new SubQueryFromClause (previousClause, identifier, subQuery, projectionExpression);
      
      StringVisitor sv = new StringVisitor();
      sv.VisitSubQueryFromClause (subQueryFromClause);

      Assert.AreEqual ("from Student s in (from Student s2 in null select 1) ", sv.ToString());
    }

    [Test]
    public void StringVisitorForMemberFromClause ()
    {
      IClause previousClause = ExpressionHelper.CreateMainFromClause ();
      ParameterExpression identifier = Expression.Parameter (typeof (Student), "s");
      LambdaExpression projectionExpression = ExpressionHelper.CreateLambdaExpression ();

      var bodyExpression = Expression.MakeMemberAccess (Expression.Constant ("test"), typeof (string).GetProperty ("Length"));
      var fromExpression = Expression.Lambda (bodyExpression);
      
      MemberFromClause memberFromClause = new MemberFromClause (previousClause, identifier, fromExpression, projectionExpression);

      StringVisitor sv = new StringVisitor ();
      sv.VisitMemberFromClause (memberFromClause);

      Assert.AreEqual ("from Student s in \"test\".Length ", sv.ToString ());
    }
  }
}
