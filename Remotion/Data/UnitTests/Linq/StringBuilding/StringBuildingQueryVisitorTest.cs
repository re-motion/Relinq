// This file is part of the re-motion Core Framework (www.re-motion.org)
// Copyright (C) 2005-2009 rubicon informationstechnologie gmbh, www.rubicon.eu
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
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using NUnit.Framework;
using Remotion.Data.Linq;
using Remotion.Data.Linq.Clauses;
using Remotion.Data.Linq.StringBuilding;
using Rhino.Mocks;

namespace Remotion.Data.UnitTests.Linq.StringBuilding
{
  [TestFixture]
  public class StringBuildingQueryVisitorTest
  {
    [Test]
    public void StringVisitorForMainFromClause ()
    {
      MainFromClause fromClause = ExpressionHelper.CreateMainFromClause();
      var sv = new StringBuildingQueryVisitor();
      sv.VisitMainFromClause (fromClause);
      Assert.AreEqual (
          "from Int32 i in TestQueryable<Student>() ",
          sv.ToString());
    }

    [Test]
    public void StringVisitorForFromClauseWithJoins ()
    {
      MainFromClause fromClause = ExpressionHelper.CreateMainFromClause();

      var repository = new MockRepository();
      var joinClause1 =
          repository.StrictMock<JoinClause> (
              ExpressionHelper.CreateClause(),
              fromClause,
              ExpressionHelper.CreateParameterExpression(),
              ExpressionHelper.CreateExpression(),
              ExpressionHelper.CreateExpression(),
              ExpressionHelper.CreateExpression());
      var joinClause2 =
          repository.StrictMock<JoinClause> (
              ExpressionHelper.CreateClause(),
              fromClause,
              ExpressionHelper.CreateParameterExpression(),
              ExpressionHelper.CreateExpression(),
              ExpressionHelper.CreateExpression(),
              ExpressionHelper.CreateExpression());

      fromClause.AddJoinClause (joinClause1);
      fromClause.AddJoinClause (joinClause2);

      var sv = new StringBuildingQueryVisitor();

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

      var sv = new StringBuildingQueryVisitor();

      sv.VisitJoinClause (joinClause);

      Assert.That (sv.ToString(), NUnit.Framework.SyntaxHelpers.Text.Contains ("join"));
    }


    [Test]
    public void StringVisitorForSelectClause ()
    {
      SelectClause selectClause = ExpressionHelper.CreateSelectClause();
      var sv = new StringBuildingQueryVisitor();

      sv.VisitSelectClause (selectClause);

      Assert.AreEqual ("select 0", sv.ToString());
    }

    [Test]
    public void StringVisitorForGroupClause ()
    {
      var groupClause =
          new GroupClause (ExpressionHelper.CreateClause(), ExpressionHelper.CreateExpression(), ExpressionHelper.CreateExpression());
      var sv = new StringBuildingQueryVisitor();

      sv.VisitGroupClause (groupClause);

      Assert.That (sv.ToString(), NUnit.Framework.SyntaxHelpers.Text.Contains ("group"));
    }

    [Test]
    public void StringVisitorForWhereClause ()
    {
      WhereClause whereClasue = ExpressionHelper.CreateWhereClause();
      var sv = new StringBuildingQueryVisitor();

      sv.VisitWhereClause (whereClasue);

      Assert.AreEqual ("where (1 = 2) ", sv.ToString());
    }

    [Test]
    public void StringVisitorForLetClause ()
    {
      LetClause letClause = ExpressionHelper.CreateLetClause();
      var sv = new StringBuildingQueryVisitor();

      sv.VisitLetClause (letClause);

      Assert.That (sv.ToString(), NUnit.Framework.SyntaxHelpers.Text.Contains ("let"));
    }

    [Test]
    public void StringVisitorForOrderingClauseAsc ()
    {
      Ordering ordering = ExpressionHelper.CreateOrdering();
      var sv = new StringBuildingQueryVisitor();

      sv.VisitOrdering (ordering);

      Assert.That (sv.ToString(), NUnit.Framework.SyntaxHelpers.Text.Contains ("ascending"));
    }


    [Test]
    public void StringVisitorForOrderingClauseDesc ()
    {
      var ordering = new Ordering (
          ExpressionHelper.CreateOrderByClause(),
          ExpressionHelper.CreateExpression(),
          OrderingDirection.Desc);

      var sv = new StringBuildingQueryVisitor();
      sv.VisitOrdering (ordering);

      Assert.That (sv.ToString(), NUnit.Framework.SyntaxHelpers.Text.Contains ("descending"));
    }
    
    [Test]
    public void StringVisitorForOrderByClauseOrderDirectionAsc ()
    {
      OrderByClause orderByClause = ExpressionHelper.CreateOrderByClause();

      var repository = new MockRepository();

      var ordering1 = repository.StrictMock<Ordering> (
          ExpressionHelper.CreateOrderByClause (), ExpressionHelper.CreateExpression (), OrderingDirection.Asc);
      var ordering2 = repository.StrictMock<Ordering> (
          ExpressionHelper.CreateOrderByClause (), ExpressionHelper.CreateExpression (), OrderingDirection.Asc);

      orderByClause.AddOrdering (ordering1);
      orderByClause.AddOrdering (ordering2);

      var sv = new StringBuildingQueryVisitor();

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

      var repository = new MockRepository();

      var ordering1 =
          repository.StrictMock<Ordering> (ExpressionHelper.CreateOrderByClause (), ExpressionHelper.CreateExpression (), OrderingDirection.Desc);
      var ordering2 =
          repository.StrictMock<Ordering> (ExpressionHelper.CreateOrderByClause (), ExpressionHelper.CreateExpression (), OrderingDirection.Desc);

      orderByClause.AddOrdering (ordering1);
      orderByClause.AddOrdering (ordering2);

      var sv = new StringBuildingQueryVisitor();

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

      var repository = new MockRepository();

      var ordering1 =
          repository.StrictMock<Ordering> (ExpressionHelper.CreateOrderByClause(), ExpressionHelper.CreateExpression(), OrderingDirection.Desc);
      var ordering2 =
          repository.StrictMock<Ordering> (ExpressionHelper.CreateOrderByClause (), ExpressionHelper.CreateExpression (), OrderingDirection.Asc);

      orderByClause.AddOrdering (ordering1);
      orderByClause.AddOrdering (ordering2);

      var sv = new StringBuildingQueryVisitor();

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
      var repository = new MockRepository();

      MainFromClause fromClause = ExpressionHelper.CreateMainFromClause();
      var selectClause1 =
          repository.StrictMock<SelectClause> (ExpressionHelper.CreateClause(), ExpressionHelper.CreateExpression());

      var queryModel = new QueryModel (typeof (IQueryable<string>), fromClause, selectClause1);

      var sv = new StringBuildingQueryVisitor();

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
      var repository = new MockRepository();

      MainFromClause fromClause = ExpressionHelper.CreateMainFromClause();
      var selectClause1 =
          repository.StrictMock<SelectClause> (ExpressionHelper.CreateClause(), ExpressionHelper.CreateExpression());
      var orderByClause1 =
          repository.StrictMock<OrderByClause> (ExpressionHelper.CreateClause());
      var fromClause1 =
          repository.StrictMock<AdditionalFromClause> (
              ExpressionHelper.CreateClause(),
              Expression.Parameter (typeof (Student), "p"),
              ExpressionHelper.CreateLambdaExpression(),
              ExpressionHelper.CreateLambdaExpression());
      var whereClause1 =
          repository.StrictMock<WhereClause> (ExpressionHelper.CreateClause(), ExpressionHelper.CreateExpression());

      var queryModel = new QueryModel (typeof (IQueryable<string>), fromClause, selectClause1);
      queryModel.AddBodyClause (orderByClause1);
      queryModel.AddBodyClause (fromClause1);
      queryModel.AddBodyClause (whereClause1);

      var sv = new StringBuildingQueryVisitor();

      //expectations
      using (repository.Ordered())
      {
        fromClause.Accept (sv);
        orderByClause1.Accept (sv);
        fromClause1.Accept (sv);
        whereClause1.Accept (sv);
        selectClause1.Accept (sv);
      }

      repository.ReplayAll();
      sv.VisitQueryModel (queryModel);
      repository.VerifyAll();
    }

    [CompilerGenerated]
    public class DisplayClass
    {
      public object source;
    }

    // TODO 1223: Remove
    [Test]
    public void StringVisitorForAdditionalFromClauses_WithMemberAccessToDisplayClass ()
    {
      var displayClass = new DisplayClass { source = ExpressionHelper.CreateQuerySource() };

      LambdaExpression fromExpression =
          Expression.Lambda (
              Expression.MakeMemberAccess (Expression.Constant (displayClass), displayClass.GetType().GetField ("source")),
              Expression.Parameter (typeof (Student), "s2"));

      var fromClause = new AdditionalFromClause (
          ExpressionHelper.CreateClause(),
          ExpressionHelper.CreateParameterExpression(),
          fromExpression,
          ExpressionHelper.CreateLambdaExpression());

      var sv = new StringBuildingQueryVisitor();

      sv.VisitAdditionalFromClause (fromClause);

      Assert.AreEqual ("from Int32 i in source ", sv.ToString());
    }

    [Test]
    public void StringVisitorForAdditionalFromClauses_WithOtherFromExpression ()
    {
      LambdaExpression fromExpression = Expression.Lambda (Expression.Constant (1));

      var fromClause = new AdditionalFromClause (
          ExpressionHelper.CreateClause(),
          ExpressionHelper.CreateParameterExpression(),
          fromExpression,
          ExpressionHelper.CreateLambdaExpression());

      var sv = new StringBuildingQueryVisitor();

      sv.VisitAdditionalFromClause (fromClause);

      Assert.AreEqual ("from Int32 i in 1 ", sv.ToString());
    }

    [Test]
    public void StringVisitorForAdditionalFromClauses_WithConstantNullFromExpression ()
    {
      LambdaExpression fromExpression = Expression.Lambda (Expression.Constant (null));

      var fromClause = new AdditionalFromClause (
          ExpressionHelper.CreateClause(),
          ExpressionHelper.CreateParameterExpression(),
          fromExpression,
          ExpressionHelper.CreateLambdaExpression());

      var sv = new StringBuildingQueryVisitor();

      sv.VisitAdditionalFromClause (fromClause);

      Assert.AreEqual ("from Int32 i in null ", sv.ToString());
    }

    [Test]
    public void StringVisitorForSubQueryFromClause ()
    {
      IClause previousClause = ExpressionHelper.CreateMainFromClause();
      ParameterExpression identifier = Expression.Parameter (typeof (Student), "s");
      ParameterExpression subQueryIdentifier = Expression.Parameter (typeof (Student), "s2");
      Expression querySource = Expression.Constant (null);
      var mainFromClause = new MainFromClause (subQueryIdentifier, querySource);
      Expression subQuerySelector = Expression.Constant (1);
      var selectClause = new SelectClause (previousClause, subQuerySelector);

      var subQuery = new QueryModel (typeof (string), mainFromClause, selectClause);

      var subQueryFromClause = new SubQueryFromClause (previousClause, identifier, subQuery);

      var sv = new StringBuildingQueryVisitor();
      sv.VisitSubQueryFromClause (subQueryFromClause);

      Assert.AreEqual ("from Student s in (from Student s2 in null select 1) ", sv.ToString());
    }

    [Test]
    public void StringVisitorForMemberFromClause ()
    {
      IClause previousClause = ExpressionHelper.CreateMainFromClause();
      ParameterExpression identifier = Expression.Parameter (typeof (Student), "s");
      LambdaExpression projectionExpression = ExpressionHelper.CreateLambdaExpression();

      MemberExpression bodyExpression = Expression.MakeMemberAccess (Expression.Constant ("test"), typeof (string).GetProperty ("Length"));
      LambdaExpression fromExpression = Expression.Lambda (bodyExpression);

      var memberFromClause = new MemberFromClause (previousClause, identifier, fromExpression, projectionExpression);

      var sv = new StringBuildingQueryVisitor();
      sv.VisitMemberFromClause (memberFromClause);

      Assert.AreEqual ("from Student s in \"test\".Length ", sv.ToString());
    }
  }
}