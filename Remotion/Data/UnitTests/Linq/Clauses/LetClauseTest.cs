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
using Rhino.Mocks;
using Remotion.Data.Linq.Clauses;
using Remotion.Data.Linq.DataObjectModel;
using Remotion.Data.Linq.Parsing.FieldResolving;

namespace Remotion.Data.UnitTests.Linq.Clauses
{
  [TestFixture]
  public class LetClauseTest
  {
    private LetClause _letClause;
    private ClonedClauseMapping _clonedClauseMapping;

    [SetUp]
    public void SetUp ()
    {
      _letClause = ExpressionHelper.CreateLetClause ();
      _clonedClauseMapping = new ClonedClauseMapping ();
    }

    [Test]
    public void IntitalizeWithIDAndExpression()
    {
      ParameterExpression identifier = ExpressionHelper.CreateParameterExpression();
      Expression expression = ExpressionHelper.CreateExpression();

      IClause clause = ExpressionHelper.CreateClause();

      var letClause = new LetClause(clause,identifier,expression,ExpressionHelper.CreateLambdaExpression());

      Assert.AreSame (clause, letClause.PreviousClause);
      Assert.AreSame (identifier, letClause.Identifier);
      Assert.AreSame (expression, letClause.Expression);
    }

    [Test]
    public void ImplementInterface()
    {
      Assert.IsInstanceOfType (typeof (IBodyClause), _letClause);
    }
        

    [Test]
    public void Accept ()
    {
      var repository = new MockRepository ();
      var visitorMock = repository.StrictMock<IQueryVisitor> ();

      visitorMock.VisitLetClause (_letClause);

      repository.ReplayAll ();

      _letClause.Accept (visitorMock);

      repository.VerifyAll ();
    }

    [Test]
    public void QueryModelAtInitialization ()
    {
      Assert.IsNull (_letClause.QueryModel);
    }

    [Test]
    public void SetQueryModel ()
    {
      QueryModel model = ExpressionHelper.CreateQueryModel ();
      _letClause.SetQueryModel (model);
      Assert.IsNotNull (_letClause.QueryModel);
    }

    [Test]
    [ExpectedException (typeof (ArgumentNullException))]
    public void SetQueryModelWithNull_Exception ()
    {
      _letClause.SetQueryModel (null);
    }

    [Test]
    [ExpectedException (typeof (InvalidOperationException), ExpectedMessage = "QueryModel is already set")]
    public void SetQueryModelTwice_Exception ()
    {
      QueryModel model = ExpressionHelper.CreateQueryModel ();
      _letClause.SetQueryModel (model);
      _letClause.SetQueryModel (model);
    }
    
    [Test]
    public void GetColumnSource_IsTableTrue ()
    {
      var policy = new SelectFieldAccessPolicy ();
      var resolver = new ClauseFieldResolver (StubDatabaseInfo.Instance, policy);
      
      ParameterExpression identifier = Expression.Parameter (typeof (Student), "s");
      var letClause = ExpressionHelper.CreateLetClause (identifier);
      letClause.SetQueryModel (ExpressionHelper.CreateQueryModel ());
      
      var expected = new LetColumnSource ("s", true);
      Assert.AreEqual (expected.Alias, letClause.GetColumnSource(resolver.DatabaseInfo).Alias);
      Assert.AreEqual (expected.IsTable, letClause.GetColumnSource (resolver.DatabaseInfo).IsTable);
    }

    [Test]
    public void GetColumnSource_IsTableFalse ()
    {
      var policy = new SelectFieldAccessPolicy ();
      var resolver = new ClauseFieldResolver (StubDatabaseInfo.Instance, policy);

      ParameterExpression identifier = Expression.Parameter (typeof (int), "i");
      LetClause letClause = ExpressionHelper.CreateLetClause (identifier);
      letClause.SetQueryModel (ExpressionHelper.CreateQueryModel ());

      var expected = new LetColumnSource ("i", false);
      Assert.AreEqual (expected.Alias, letClause.GetColumnSource (resolver.DatabaseInfo).Alias);
      Assert.AreEqual (expected.IsTable, letClause.GetColumnSource (resolver.DatabaseInfo).IsTable);
    }

    [Test]
    public void Clone ()
    {
      var newPreviousClause = ExpressionHelper.CreateMainFromClause ();
      var clone = _letClause.Clone (newPreviousClause, _clonedClauseMapping);

      Assert.That (clone, Is.Not.Null);
      Assert.That (clone, Is.Not.SameAs (_letClause));
      Assert.That (clone.Expression, Is.SameAs (_letClause.Expression));
      Assert.That (clone.Identifier, Is.SameAs (_letClause.Identifier));
      Assert.That (clone.ProjectionExpression, Is.SameAs (_letClause.ProjectionExpression));
      Assert.That (clone.QueryModel, Is.Null);
      Assert.That (clone.PreviousClause, Is.SameAs (newPreviousClause));
    }

    [Test]
    public void Clone_AddsClauseToMapping ()
    {
      var clone = _letClause.Clone (ExpressionHelper.CreateClause (), _clonedClauseMapping);
      Assert.That (_clonedClauseMapping.GetClause (_letClause), Is.SameAs (clone));
    }
  }
}
