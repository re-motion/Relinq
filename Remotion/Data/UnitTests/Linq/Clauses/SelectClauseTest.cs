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
using Remotion.Data.Linq.Clauses.ExecutionStrategies;
using Remotion.Data.Linq.Clauses.Expressions;
using Remotion.Data.Linq.Clauses.ResultModifications;
using Rhino.Mocks;
using Remotion.Data.Linq.Clauses;

namespace Remotion.Data.UnitTests.Linq.Clauses
{
  [TestFixture]
  public class SelectClauseTest
  {
    private Expression _selector;
    private SelectClause _selectClause;
    private CloneContext _cloneContext;

    [SetUp]
    public void SetUp ()
    {
      _selector = ExpressionHelper.CreateExpression();
      _selectClause = new SelectClause (_selector);
      _cloneContext = new CloneContext (new ClauseMapping());
    }

    [Test]
    public void InitializeWithExpression ()
    {
      Assert.That (_selectClause.Selector, Is.EqualTo (_selector));
    }

    [Test]
    public void SelectWithMethodCall_ResultModifications ()
    {
      var resultModifierClause = new DistinctResultModification ();
      _selectClause.ResultModifications.Add (resultModifierClause);

      Assert.That (_selectClause.ResultModifications, Is.Not.Empty);
      Assert.That (_selectClause.ResultModifications, Is.EqualTo (new[] { resultModifierClause }));
    }

    [Test]
    [ExpectedException (typeof (ArgumentNullException))]
    public void AddResultModification_Null_ThrowsArgumentNullException ()
    {
      _selectClause.ResultModifications.Add (null);
    }

    [Test]
    [ExpectedException (typeof (ArgumentNullException))]
    public void AddResultModification_WithNull_ThrowsArgumentNullException ()
    {
      var resultModifierClause = new DistinctResultModification ();
      _selectClause.ResultModifications.Add (resultModifierClause);
      _selectClause.ResultModifications[0] = null;
    }

    [Test]
    public void SelectClause_ImplementISelectGroupClause()
    {
      Assert.That (_selectClause, Is.InstanceOfType (typeof (ISelectGroupClause)));
    }
        
    [Test]
    public void Accept()
    {
      var queryModel = ExpressionHelper.CreateQueryModel ();
      var repository = new MockRepository();
      var visitorMock = repository.StrictMock<IQueryModelVisitor>();
      visitorMock.VisitSelectClause (_selectClause, queryModel);
      repository.ReplayAll();
      _selectClause.Accept (visitorMock, queryModel);
      repository.VerifyAll();
    }

    [Test]
    public void Clone ()
    {
      var clone = _selectClause.Clone (_cloneContext);

      Assert.That (clone, Is.Not.Null);
      Assert.That (clone, Is.Not.SameAs (_selectClause));
      Assert.That (clone.Selector, Is.SameAs (_selectClause.Selector));
    }

    [Test]
    public void Clone_ViaInterface_PassesMapping ()
    {
      var fromClause = ExpressionHelper.CreateMainFromClause ();
      _selectClause.Selector = new QuerySourceReferenceExpression (fromClause);

      var newReferenceExpression = new QuerySourceReferenceExpression (ExpressionHelper.CreateMainFromClause ());
      _cloneContext.ClauseMapping.AddMapping (fromClause, newReferenceExpression);

      var clone = ((ISelectGroupClause) _selectClause).Clone (_cloneContext);
      Assert.That (((SelectClause) clone).Selector, Is.SameAs (newReferenceExpression));
    }

    [Test]
    public void Clone_AdjustsExpressions ()
    {
      var oldReferencedExpression = ExpressionHelper.CreateMainFromClause();
      var selector = new QuerySourceReferenceExpression (oldReferencedExpression);
      var selectClause = new SelectClause (selector);

      var newReferencedExpression = ExpressionHelper.CreateMainFromClause ();
      _cloneContext.ClauseMapping.AddMapping (oldReferencedExpression, new QuerySourceReferenceExpression (newReferencedExpression));

      var clone = selectClause.Clone (_cloneContext);

      Assert.That (((QuerySourceReferenceExpression) clone.Selector).ReferencedClause, Is.SameAs (newReferencedExpression));
    }

    [Test]
    public void Clone_ResultModifiers ()
    {
      var resultModifierClause1 = ExpressionHelper.CreateResultModification ();
      _selectClause.ResultModifications.Add (resultModifierClause1);
      var resultModifierClause2 = ExpressionHelper.CreateResultModification ();
      _selectClause.ResultModifications.Add (resultModifierClause2);

      var clone = _selectClause.Clone (_cloneContext);

      Assert.That (clone.ResultModifications.Count, Is.EqualTo (2));
      Assert.That (clone.ResultModifications[0], Is.Not.SameAs (resultModifierClause1));
      Assert.That (clone.ResultModifications[0].GetType(), Is.SameAs (resultModifierClause1.GetType()));
      Assert.That (clone.ResultModifications[1], Is.Not.SameAs (resultModifierClause2));
      Assert.That (clone.ResultModifications[1].GetType (), Is.SameAs (resultModifierClause2.GetType ()));
    }

    [Test]
    public void Clone_ResultModifiers_PassesMapping ()
    {
      var resultModifierClauseMock = MockRepository.GenerateMock<ResultModificationBase> (CollectionExecutionStrategy.Instance);
      _selectClause.ResultModifications.Add (resultModifierClauseMock);

      resultModifierClauseMock
          .Expect (mock => mock.Clone (Arg.Is (_cloneContext)))
          .Return (ExpressionHelper.CreateResultModification());
      resultModifierClauseMock.Replay();

      _selectClause.Clone (_cloneContext);

      resultModifierClauseMock.VerifyAllExpectations();
    }

    [Test]
    public void TransformExpressions ()
    {
      var oldExpression = ExpressionHelper.CreateExpression ();
      var newExpression = ExpressionHelper.CreateExpression ();
      var clause = new SelectClause (oldExpression);

      clause.TransformExpressions (ex =>
      {
        Assert.That (ex, Is.SameAs (oldExpression));
        return newExpression;
      });

      Assert.That (clause.Selector, Is.SameAs (newExpression));
    }

    [Test]
    public void GetExecutionStrategy_WithoutResultModifiers ()
    {
      Assert.That (_selectClause.GetExecutionStrategy (), Is.SameAs (CollectionExecutionStrategy.Instance));
    }

    [Test]
    public void GetExecutionStrategy_WithResultModifiers ()
    {
      var firstModifier = new FirstResultModification (true);
      _selectClause.ResultModifications.Add (firstModifier);

      Assert.That (_selectClause.GetExecutionStrategy (), Is.SameAs (firstModifier.ExecutionStrategy));
    }

    [Test]
    public void GetExecutionStrategy_WithManyResultModifiers ()
    {
      var takeModifier = new TakeResultModification (7);
      var distinctModifier = new DistinctResultModification ();
      var countModifier = new CountResultModification ();
      _selectClause.ResultModifications.Add (takeModifier);
      _selectClause.ResultModifications.Add (distinctModifier);
      _selectClause.ResultModifications.Add (countModifier);

      Assert.That (_selectClause.GetExecutionStrategy (), Is.SameAs (countModifier.ExecutionStrategy));
    }
  }
}
