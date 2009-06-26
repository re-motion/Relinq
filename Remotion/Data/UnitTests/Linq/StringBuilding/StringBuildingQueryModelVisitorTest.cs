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
using System.Linq.Expressions;
using NUnit.Framework;
using NUnit.Framework.SyntaxHelpers;
using Remotion.Data.Linq;
using Remotion.Data.Linq.Clauses;
using Remotion.Data.Linq.Clauses.Expressions;
using Remotion.Data.Linq.Clauses.ResultModifications;
using Remotion.Data.Linq.StringBuilding;

namespace Remotion.Data.UnitTests.Linq.StringBuilding
{
  [TestFixture]
  public class StringBuildingQueryModelVisitorTest
  {
    [Test]
    public void StringVisitor_ForExpression_WithQuerySourceReference ()
    {
      var referencedClause = ExpressionHelper.CreateMainFromClause ("i", typeof (int), ExpressionHelper.CreateQuerySource ());
      var predicate = Expression.MakeBinary (ExpressionType.GreaterThan, new QuerySourceReferenceExpression (referencedClause), Expression.Constant (0));
      var whereClause = new WhereClause (predicate);
      var sv = new StringBuildingQueryModelVisitor ();

      sv.VisitWhereClause (whereClause);
      Assert.That (sv.ToString(), Is.EqualTo ("where ([i] > 0) "));
    }

    [Test]
    public void StringVisitorForMainFromClause ()
    {
      MainFromClause fromClause = ExpressionHelper.CreateMainFromClause();
      var sv = new StringBuildingQueryModelVisitor();
      sv.VisitMainFromClause (fromClause);
      Assert.That (sv.ToString (), Is.EqualTo ("from Int32 main in TestQueryable<Student>() "));
    }

    [Test]
    public void StringVisitorForJoins ()
    {
      JoinClause joinClause = ExpressionHelper.CreateJoinClause();

      var sv = new StringBuildingQueryModelVisitor();

      sv.VisitJoinClause (joinClause);

      Assert.That (sv.ToString(), NUnit.Framework.SyntaxHelpers.Text.Contains ("join"));
    }

    [Test]
    public void StringVisitorForSelectClause ()
    {
      SelectClause selectClause = ExpressionHelper.CreateSelectClause();
      var sv = new StringBuildingQueryModelVisitor();

      sv.VisitSelectClause (selectClause);

      Assert.That (sv.ToString (), Is.EqualTo ("select 0"));
    }

    [Test]
    public void StringVisitorForSelectClause_WithResultModifications ()
    {
      SelectClause selectClause = ExpressionHelper.CreateSelectClause ();
      selectClause.ResultModifications.Add (new TakeResultModification (5));
      selectClause.ResultModifications.Add (new CountResultModification ());
      var sv = new StringBuildingQueryModelVisitor ();

      sv.VisitSelectClause (selectClause);

      Assert.That (sv.ToString (), Is.EqualTo ("(select 0).Take(5).Count()"));
    }

    [Test]
    public void StringVisitorForGroupClause ()
    {
      var groupClause = new GroupClause (ExpressionHelper.CreateExpression(), ExpressionHelper.CreateExpression());
      var sv = new StringBuildingQueryModelVisitor();

      sv.VisitGroupClause (groupClause);

      Assert.That (sv.ToString(), NUnit.Framework.SyntaxHelpers.Text.Contains ("group"));
    }

    [Test]
    public void StringVisitorForWhereClause ()
    {
      WhereClause whereClasue = ExpressionHelper.CreateWhereClause();
      var sv = new StringBuildingQueryModelVisitor();

      sv.VisitWhereClause (whereClasue);

      Assert.That (sv.ToString (), Is.EqualTo ("where (1 = 2) "));
    }

    [Test]
    public void StringVisitorForOrderingClauseAsc ()
    {
      Ordering ordering = ExpressionHelper.CreateOrdering();
      var sv = new StringBuildingQueryModelVisitor();

      sv.VisitOrdering (ordering);

      Assert.That (sv.ToString(), NUnit.Framework.SyntaxHelpers.Text.Contains ("ascending"));
    }

    [Test]
    public void StringVisitorForOrderingClauseDesc ()
    {
      var ordering = new Ordering (
          ExpressionHelper.CreateExpression(),
          OrderingDirection.Desc);

      var sv = new StringBuildingQueryModelVisitor();
      sv.VisitOrdering (ordering);

      Assert.That (sv.ToString(), NUnit.Framework.SyntaxHelpers.Text.Contains ("descending"));
    }
    
    [Test]
    public void StringVisitorForAdditionalFromClauses_WithQuerySourceReferenceExpression ()
    {
      var mainFromClause = ExpressionHelper.CreateMainFromClause_Detail();
      var fromExpression = ExpressionHelper.Resolve<Student_Detail, Student> (mainFromClause, sd => sd.Student);

      var fromClause = new AdditionalFromClause ("i",
          typeof (int),
          fromExpression);

      var sv = new StringBuildingQueryModelVisitor();

      sv.VisitAdditionalFromClause (fromClause);

      Assert.That (sv.ToString (), Is.EqualTo ("from Int32 i in [sd].Student "));
    }
    [Test]
    public void StringVisitorForAdditionalFromClauses_WithOtherFromExpression ()
    {
      var fromExpression = Expression.Constant (1);

      var fromClause = new AdditionalFromClause ("i",
          typeof (int),
          fromExpression);

      var sv = new StringBuildingQueryModelVisitor();

      sv.VisitAdditionalFromClause (fromClause);

      Assert.That (sv.ToString (), Is.EqualTo ("from Int32 i in 1 "));
    }

    [Test]
    public void StringVisitorForAdditionalFromClauses_WithConstantNullFromExpression ()
    {
      var fromExpression = Expression.Constant (null);

      var fromClause = new AdditionalFromClause ("i",
          typeof (int),
          fromExpression);

      var sv = new StringBuildingQueryModelVisitor();

      sv.VisitAdditionalFromClause (fromClause);

      Assert.That (sv.ToString (), Is.EqualTo ("from Int32 i in null "));
    }

    [Test]
    public void StringVisitorForSubQueryFromClause ()
    {
      Expression querySource = Expression.Constant (null);
      var mainFromClause = new MainFromClause ("s2", typeof (Student), querySource);
      Expression subQuerySelector = Expression.Constant (1);
      var selectClause = new SelectClause (subQuerySelector);

      var subQuery = new QueryModel (typeof (string), mainFromClause, selectClause);

      var subQueryFromClause = new SubQueryFromClause ("s", typeof (Student), new SubQueryExpression (subQuery));

      var sv = new StringBuildingQueryModelVisitor();
      sv.VisitAdditionalFromClause (subQueryFromClause);

      Assert.That (sv.ToString (), Is.EqualTo ("from Student s in {from Student s2 in null select 1} "));
    }
  }
}