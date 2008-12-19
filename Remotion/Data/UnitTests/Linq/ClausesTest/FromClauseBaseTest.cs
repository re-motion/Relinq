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
using System.Linq;
using System.Linq.Expressions;
using NUnit.Framework;
using NUnit.Framework.SyntaxHelpers;
using Remotion.Data.Linq;
using Remotion.Data.Linq.Clauses;
using Remotion.Data.Linq.DataObjectModel;
using Remotion.Data.Linq.Parsing.FieldResolving;

namespace Remotion.Data.UnitTests.Linq.ClausesTest
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
      Assert.AreEqual (new Table ("studentTable", "s1"), fromClause.GetFromSource (StubDatabaseInfo.Instance));
    }

    [Test]
    public void GetTable_CachesInstance ()
    {
      ParameterExpression id = Expression.Parameter (typeof (Student), "s1");
      IQueryable querySource = ExpressionHelper.CreateQuerySource ();

      MainFromClause fromClause = ExpressionHelper.CreateMainFromClause(id, querySource);
      IColumnSource t1 = fromClause.GetFromSource (StubDatabaseInfo.Instance);
      IColumnSource t2 = fromClause.GetFromSource (StubDatabaseInfo.Instance);
      Assert.AreSame (t1, t2);
    }

    [Test]
    public void AddJoinClause()
    {
      MainFromClause fromClause = ExpressionHelper.CreateMainFromClause();

      JoinClause joinClause1 = ExpressionHelper.CreateJoinClause();
      JoinClause joinClause2 = ExpressionHelper.CreateJoinClause();

      fromClause.Add (joinClause1);
      fromClause.Add (joinClause2);

      Assert.That (fromClause.JoinClauses, Is.EqualTo (new object[] { joinClause1, joinClause2 }));
      Assert.AreEqual (2, fromClause.JoinClauses.Count);
    }

    [Test]
    public void FromClause_ImplementsIQueryElement()
    {
      Assert.IsTrue (typeof (IQueryElement).IsAssignableFrom (typeof (FromClauseBase)));
    }

    [Test]
    public void Resolve_Succeeds_MainFromClause ()
    {
      ParameterExpression identifier = Expression.Parameter (typeof (Student), "fromIdentifier1");
      MainFromClause fromClause = ExpressionHelper.CreateMainFromClause(identifier, ExpressionHelper.CreateQuerySource ());

      JoinedTableContext context = new JoinedTableContext ();
      SelectFieldAccessPolicy policy = new SelectFieldAccessPolicy ();
      ClauseFieldResolver resolver = new ClauseFieldResolver (StubDatabaseInfo.Instance, policy);
      FieldDescriptor fieldDescriptor = fromClause.ResolveField (resolver, identifier, identifier, context);
      Assert.AreEqual (new Column (new Table ("studentTable", "fromIdentifier1"), "*"), fieldDescriptor.Column);
      //Assert.AreSame (fromClause, fieldDescriptor.FromClause);
    }

    [Test]
    public void Resolve_Succeeds_AdditionalFromClause ()
    {
      ParameterExpression identifier = Expression.Parameter (typeof (Student), "fromIdentifier1");
      AdditionalFromClause fromClause = CreateAdditionalFromClause (identifier);

      JoinedTableContext context = new JoinedTableContext ();
      SelectFieldAccessPolicy policy = new SelectFieldAccessPolicy ();
      ClauseFieldResolver resolver = new ClauseFieldResolver (StubDatabaseInfo.Instance, policy);
      FieldDescriptor fieldDescriptor = fromClause.ResolveField (resolver, identifier, identifier, context);
      Assert.AreEqual (new Column (new Table ("studentTable", "fromIdentifier1"), "*"), fieldDescriptor.Column);
      //Assert.AreSame (fromClause, fieldDescriptor.FromClause);
    }

    private AdditionalFromClause CreateAdditionalFromClause (ParameterExpression additionalFromIdentifier)
    {
      MainFromClause mainFromClause = ExpressionHelper.CreateMainFromClause(ExpressionHelper.CreateParameterExpression (), ExpressionHelper.CreateQuerySource ());
      LambdaExpression fromExpression = Expression.Lambda (Expression.Constant (null, typeof (IQueryable<Student>)));
      LambdaExpression projectionExpression = Expression.Lambda (Expression.Constant (null, typeof (Student)));
      return new AdditionalFromClause (mainFromClause, additionalFromIdentifier, fromExpression, projectionExpression);
    }
  }
}
