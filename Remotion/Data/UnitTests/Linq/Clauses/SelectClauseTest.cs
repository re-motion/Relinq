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
using Remotion.Data.Linq;
using Remotion.Data.Linq.Clauses.ExecutionStrategies;
using Remotion.Data.Linq.Clauses.ResultModifications;
using Remotion.Data.Linq.Parsing;
using Remotion.Data.UnitTests.Linq.TestQueryGenerators;
using Rhino.Mocks;
using Remotion.Data.Linq.Clauses;

namespace Remotion.Data.UnitTests.Linq.Clauses
{
  [TestFixture]
  public class SelectClauseTest
  {
    [Test]
    public void InitializeWithExpression ()
    {
      LambdaExpression expression = ExpressionHelper.CreateLambdaExpression ();
      IClause clause = ExpressionHelper.CreateClause();
      var selectClause = new SelectClause (clause, expression);
      Assert.AreSame (clause, selectClause.PreviousClause);
      Assert.AreEqual (expression, selectClause.Selector);
    }

    [Test]
    public void InitializeWithExpression_New ()
    {
      LambdaExpression expression = ExpressionHelper.CreateLambdaExpression ();
      IClause clause = ExpressionHelper.CreateClause ();
      var selectClause = new SelectClause (clause, expression);
      Assert.AreSame (clause, selectClause.PreviousClause);
    }

    [Test]
    public void SelectWithMethodCall_ResultModifications ()
    {
      LambdaExpression expression = ExpressionHelper.CreateLambdaExpression ();
      IClause clause = ExpressionHelper.CreateClause ();

      var selectClause = new SelectClause (clause, expression);
      var resultModifierClause = new DistinctResultModification (selectClause);
      selectClause.AddResultModification (resultModifierClause);

      Assert.IsNotEmpty (selectClause.ResultModifications);
      Assert.That (selectClause.ResultModifications, Is.EqualTo (new[] { resultModifierClause }));
    }

    [Test]
    [ExpectedException (typeof (ArgumentNullException))]
    public void InitializeWithoutExpression ()
    {
      new SelectClause (ExpressionHelper.CreateClause (), null);
    }
    
    [Test]
    public void SelectClause_ImplementISelectGroupClause()
    {
      SelectClause selectClause = ExpressionHelper.CreateSelectClause();
      Assert.IsInstanceOfType (typeof(ISelectGroupClause),selectClause);
    }
        
    [Test]
    public void Accept()
    {
      SelectClause selectClause = ExpressionHelper.CreateSelectClause();
      var repository = new MockRepository();
      var visitorMock = repository.StrictMock<IQueryVisitor>();
      visitorMock.VisitSelectClause (selectClause);
      repository.ReplayAll();
      selectClause.Accept (visitorMock);
      repository.VerifyAll();
    }

    [Test]
    public void Clone ()
    {
      var originalClause = ExpressionHelper.CreateSelectClause ();
      var newPreviousClause = ExpressionHelper.CreateMainFromClause ();
      var clone = originalClause.Clone (newPreviousClause);

      Assert.That (clone, Is.Not.Null);
      Assert.That (clone, Is.Not.SameAs (originalClause));
      Assert.That (clone.Selector, Is.SameAs (originalClause.Selector));
      Assert.That (clone.PreviousClause, Is.SameAs (newPreviousClause));
    }

    [Test]
    public void Clone_ResultModifiers ()
    {
      var originalClause = ExpressionHelper.CreateSelectClause ();
      var newPreviousClause = ExpressionHelper.CreateMainFromClause ();

      var resultModifierClause1 = ExpressionHelper.CreateResultModifierClause (originalClause);
      originalClause.AddResultModification (resultModifierClause1);
      var resultModifierClause2 = ExpressionHelper.CreateResultModifierClause (originalClause);
      originalClause.AddResultModification (resultModifierClause2);

      var clone = originalClause.Clone (newPreviousClause);

      Assert.That (clone.ResultModifications.Count, Is.EqualTo (2));
      Assert.That (clone.ResultModifications[0], Is.Not.SameAs (resultModifierClause1));
      Assert.That (clone.ResultModifications[0].SelectClause, Is.SameAs (clone));

      Assert.That (clone.ResultModifications[1], Is.Not.SameAs (resultModifierClause2));
    }

    [Test]
    public void GetExecutionStrategy_WithoutResultModifiers ()
    {
      var clause = ExpressionHelper.CreateSelectClause ();
      Assert.That (clause.GetExecutionStrategy (), Is.SameAs (CollectionExecutionStrategy.Instance));
    }

    [Test]
    public void GetExecutionStrategy_WithResultModifiers ()
    {
      var clause = ExpressionHelper.CreateSelectClause ();
      var firstModifier = new FirstResultModification (clause, true);
      clause.AddResultModification (firstModifier);

      Assert.That (clause.GetExecutionStrategy (), Is.SameAs (firstModifier.ExecutionStrategy));
    }

    [Test]
    public void GetExecutionStrategy_WithManyResultModifiers ()
    {
      var clause = ExpressionHelper.CreateSelectClause ();
      var takeModifier = new TakeResultModification (clause, 7);
      var distinctModifier = new DistinctResultModification (clause);
      var countModifier = new CountResultModification (clause);
      clause.AddResultModification (takeModifier);
      clause.AddResultModification (distinctModifier);
      clause.AddResultModification (countModifier);

      Assert.That (clause.GetExecutionStrategy (), Is.SameAs (countModifier.ExecutionStrategy));
    }
  }
}
