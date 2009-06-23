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
using NUnit.Framework;
using NUnit.Framework.SyntaxHelpers;
using Remotion.Data.Linq.Clauses;
using Remotion.Data.Linq.Clauses.Expressions;
using Remotion.Data.Linq.DataObjectModel;
using Remotion.Data.Linq.Parsing.FieldResolving;

namespace Remotion.Data.UnitTests.Linq.Clauses
{
  [TestFixture]
  public class FromClauseBaseTest
  {
    [Test]
    public void GetTable ()
    {
      ParameterExpression id = Expression.Parameter (typeof (Student), "s1");
      IQueryable querySource = ExpressionHelper.CreateQuerySource ();

      MainFromClause fromClause = ExpressionHelper.CreateMainFromClause(id, querySource);
      Assert.AreEqual (new Table ("studentTable", "s1"), fromClause.GetColumnSource (StubDatabaseInfo.Instance));
    }

    [Test]
    public void GetTable_CachesInstance ()
    {
      ParameterExpression id = Expression.Parameter (typeof (Student), "s1");
      IQueryable querySource = ExpressionHelper.CreateQuerySource ();

      MainFromClause fromClause = ExpressionHelper.CreateMainFromClause(id, querySource);
      IColumnSource t1 = fromClause.GetColumnSource (StubDatabaseInfo.Instance);
      IColumnSource t2 = fromClause.GetColumnSource (StubDatabaseInfo.Instance);
      Assert.AreSame (t1, t2);
    }

    [Test]
    public void AddJoinClause()
    {
      MainFromClause fromClause = ExpressionHelper.CreateMainFromClause();

      JoinClause joinClause1 = ExpressionHelper.CreateJoinClause();
      JoinClause joinClause2 = ExpressionHelper.CreateJoinClause();

      fromClause.AddJoinClause (joinClause1);
      fromClause.AddJoinClause (joinClause2);

      Assert.That (fromClause.JoinClauses, Is.EqualTo (new object[] { joinClause1, joinClause2 }));
      Assert.AreEqual (2, fromClause.JoinClauses.Count);
    }

    [Test]
    public void Resolve_Succeeds_MainFromClause ()
    {
      MainFromClause fromClause = ExpressionHelper.CreateMainFromClause (Expression.Parameter (typeof (Student), "fromIdentifier1"), ExpressionHelper.CreateQuerySource ());

      var context = new JoinedTableContext ();
      var policy = new SelectFieldAccessPolicy ();
      var resolver = new ClauseFieldResolver (StubDatabaseInfo.Instance, policy);
      FieldDescriptor fieldDescriptor = fromClause.ResolveField (resolver, new QuerySourceReferenceExpression (fromClause), context);
      Assert.AreEqual (new Column (new Table ("studentTable", "fromIdentifier1"), "*"), fieldDescriptor.Column);
    }

    [Test]
    public void Resolve_Succeeds_AdditionalFromClause ()
    {
      AdditionalFromClause fromClause = CreateAdditionalFromClause (Expression.Parameter (typeof (Student), "fromIdentifier1"));

      var context = new JoinedTableContext ();
      var policy = new SelectFieldAccessPolicy ();
      var resolver = new ClauseFieldResolver (StubDatabaseInfo.Instance, policy);
      FieldDescriptor fieldDescriptor = fromClause.ResolveField (resolver, new QuerySourceReferenceExpression (fromClause), context);
      Assert.AreEqual (new Column (new Table ("studentTable", "fromIdentifier1"), "*"), fieldDescriptor.Column);
    }

    private AdditionalFromClause CreateAdditionalFromClause (ParameterExpression additionalFromIdentifier)
    {
      MainFromClause mainFromClause = ExpressionHelper.CreateMainFromClause(ExpressionHelper.CreateParameterExpression (), ExpressionHelper.CreateQuerySource ());
      var fromExpression = Expression.Constant (null, typeof (IQueryable<Student>));
      return new AdditionalFromClause (mainFromClause, additionalFromIdentifier, fromExpression);
    }
  }
}
