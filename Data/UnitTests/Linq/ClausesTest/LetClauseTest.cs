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
using System.Reflection;
using NUnit.Framework;
using Remotion.Data.Linq;
using Rhino.Mocks;
using Remotion.Data.Linq.Clauses;
using Remotion.Data.Linq.DataObjectModel;
using Remotion.Data.Linq.Parsing.FieldResolving;

namespace Remotion.Data.UnitTests.Linq.ClausesTest
{
  [TestFixture]
  public class LetClauseTest
  {
    [Test]
    public void IntitalizeWithIDAndExpression()
    {
      ParameterExpression identifier = ExpressionHelper.CreateParameterExpression();
      Expression expression = ExpressionHelper.CreateExpression();

       IClause clause = ExpressionHelper.CreateClause();

      LetClause letClause = new LetClause(clause,identifier,expression,ExpressionHelper.CreateLambdaExpression());

      Assert.AreSame (clause, letClause.PreviousClause);
      Assert.AreSame (identifier, letClause.Identifier);
      Assert.AreSame (expression, letClause.Expression);
    }

    [Test]
    public void ImplementInterface()
    {
      LetClause letClause = ExpressionHelper.CreateLetClause();

      Assert.IsInstanceOfType (typeof (IBodyClause), letClause);
    }
        

    [Test]
    public void LetClause_ImplementsIQueryElement()
    {
      LetClause letClause = ExpressionHelper.CreateLetClause();
      Assert.IsInstanceOfType (typeof (IQueryElement), letClause);
    }

    [Test]
    public void Accept ()
    {
      LetClause letClause = ExpressionHelper.CreateLetClause ();

      MockRepository repository = new MockRepository ();
      IQueryVisitor visitorMock = repository.StrictMock<IQueryVisitor> ();

      visitorMock.VisitLetClause (letClause);

      repository.ReplayAll ();

      letClause.Accept (visitorMock);

      repository.VerifyAll ();
    }

    [Test]
    public void QueryModelAtInitialization ()
    {
      LetClause letClause = ExpressionHelper.CreateLetClause ();
      Assert.IsNull (letClause.QueryModel);
    }

    [Test]
    public void SetQueryModel ()
    {
      LetClause letClause = ExpressionHelper.CreateLetClause ();
      QueryModel model = ExpressionHelper.CreateQueryModel ();
      letClause.SetQueryModel (model);
      Assert.IsNotNull (letClause.QueryModel);
    }

    [Test]
    [ExpectedException (typeof (ArgumentNullException))]
    public void SetQueryModelWithNull_Exception ()
    {
      LetClause letClause = ExpressionHelper.CreateLetClause ();
      letClause.SetQueryModel (null);
    }

    [Test]
    [ExpectedException (typeof (InvalidOperationException), ExpectedMessage = "QueryModel is already set")]
    public void SetQueryModelTwice_Exception ()
    {
      LetClause letClause = ExpressionHelper.CreateLetClause ();
      QueryModel model = ExpressionHelper.CreateQueryModel ();
      letClause.SetQueryModel (model);
      letClause.SetQueryModel (model);
    }


    [Test]
    public void GetColumnSource_IsTableTrue ()
    {
      SelectFieldAccessPolicy policy = new SelectFieldAccessPolicy ();
      JoinedTableContext context = new JoinedTableContext ();
      ClauseFieldResolver resolver = new ClauseFieldResolver (StubDatabaseInfo.Instance, policy);
      
      ParameterExpression identifier = Expression.Parameter (typeof (Student), "s");
      LetClause letClause = ExpressionHelper.CreateLetClause (identifier);
      letClause.SetQueryModel (ExpressionHelper.CreateQueryModel ());
      
      LetColumnSource expected = new LetColumnSource ("s", true);
      Assert.AreEqual (expected.Alias, letClause.GetColumnSource(resolver.DatabaseInfo).Alias);
      Assert.AreEqual (expected.IsTable, letClause.GetColumnSource (resolver.DatabaseInfo).IsTable);
    }

    [Test]
    public void GetColumnSource_IsTableFalse ()
    {
      SelectFieldAccessPolicy policy = new SelectFieldAccessPolicy ();
      JoinedTableContext context = new JoinedTableContext ();
      ClauseFieldResolver resolver = new ClauseFieldResolver (StubDatabaseInfo.Instance, policy);

      ParameterExpression identifier = Expression.Parameter (typeof (int), "i");
      LetClause letClause = ExpressionHelper.CreateLetClause (identifier);
      letClause.SetQueryModel (ExpressionHelper.CreateQueryModel ());

      LetColumnSource expected = new LetColumnSource ("i", false);
      Assert.AreEqual (expected.Alias, letClause.GetColumnSource (resolver.DatabaseInfo).Alias);
      Assert.AreEqual (expected.IsTable, letClause.GetColumnSource (resolver.DatabaseInfo).IsTable);
    }
    
  }
}
