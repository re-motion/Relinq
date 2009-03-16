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
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using NUnit.Framework;
using NUnit.Framework.SyntaxHelpers;
using Remotion.Data.Linq;
using Remotion.Data.Linq.Parsing;
using Remotion.Data.UnitTests.Linq.TestQueryGenerators;
using Rhino.Mocks;
using Remotion.Data.Linq.Clauses;

namespace Remotion.Data.UnitTests.Linq.ClausesTest
{
  [TestFixture]
  public class SelectClauseTest
  {
    [Test]
    public void InitializeWithExpression ()
    {
      LambdaExpression expression = ExpressionHelper.CreateLambdaExpression ();
      IClause clause = ExpressionHelper.CreateClause();

      var selectClause = new SelectClause (clause, expression, null);
      Assert.AreSame (clause, selectClause.PreviousClause);
      Assert.AreEqual (expression, selectClause.ProjectionExpression);
    }

    [Test]
    public void InitializeWithExpression_New ()
    {
      LambdaExpression expression = ExpressionHelper.CreateLambdaExpression ();
      IClause clause = ExpressionHelper.CreateClause ();
      var query = SelectTestQueryGenerator.CreateSimpleQuery (ExpressionHelper.CreateQuerySource());
      var methodInfo = ParserUtility.GetMethod (() => Enumerable.Count (query));
      MethodCallExpression methodCallExpression = Expression.Call (methodInfo, query.Expression);
      var methodCallExpressions = new List<MethodCallExpression>();
      methodCallExpressions.Add (methodCallExpression);
      
      var selectClause = new SelectClause (clause, expression, methodCallExpressions);
      Assert.AreSame (clause, selectClause.PreviousClause);
    }

    [Test]
    public void SelectWithMethodCall_ResultModifierData ()
    {
      LambdaExpression expression = ExpressionHelper.CreateLambdaExpression ();
      IClause clause = ExpressionHelper.CreateClause ();
      var query = SelectTestQueryGenerator.CreateSimpleQuery (ExpressionHelper.CreateQuerySource ());
      var methodInfo = ParserUtility.GetMethod (() => Enumerable.Count (query));
      MethodCallExpression methodCallExpression = Expression.Call (methodInfo, query.Expression);

      var selectClause = new SelectClause (clause, expression);
      var resultModifierClause = new ResultModifierClause (selectClause, selectClause, methodCallExpression);
      selectClause.AddResultModifierData (resultModifierClause);

      Assert.IsNotEmpty (selectClause.ResultModifierData);
      Assert.That (selectClause.ResultModifierData, Is.EqualTo (new[] { resultModifierClause }));
    }

    [Test]
    public void InitializeWithoutExpression ()
    {
      var selectClause = new SelectClause (ExpressionHelper.CreateClause (), null, null);
      Assert.IsNull (selectClause.ProjectionExpression);
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
      Assert.That (clone.ProjectionExpression, Is.SameAs (originalClause.ProjectionExpression));
      Assert.That (clone.PreviousClause, Is.SameAs (newPreviousClause));
    }

    [Test]
    public void Clone_ResultModifiers ()
    {
      var originalClause = ExpressionHelper.CreateSelectClause ();
      var newPreviousClause = ExpressionHelper.CreateMainFromClause ();

      var resultModifierClause1 = ExpressionHelper.CreateResultModifierClause (originalClause, originalClause);
      originalClause.AddResultModifierData (resultModifierClause1);
      var resultModifierClause2 = ExpressionHelper.CreateResultModifierClause (resultModifierClause1, originalClause);
      originalClause.AddResultModifierData (resultModifierClause2);

      var clone = originalClause.Clone (newPreviousClause);

      Assert.That (clone.ResultModifierData.Count, Is.EqualTo (2));
      Assert.That (clone.ResultModifierData[0], Is.Not.SameAs (resultModifierClause1));
      Assert.That (clone.ResultModifierData[0].ResultModifier, Is.SameAs (resultModifierClause1.ResultModifier));
      Assert.That (clone.ResultModifierData[0].SelectClause, Is.SameAs (clone));
      Assert.That (clone.ResultModifierData[0].PreviousClause, Is.SameAs (clone));

      Assert.That (clone.ResultModifierData[1], Is.Not.SameAs (resultModifierClause2));
      Assert.That (clone.ResultModifierData[1].ResultModifier, Is.SameAs (resultModifierClause2.ResultModifier));
      Assert.That (clone.ResultModifierData[1].PreviousClause, Is.SameAs (clone.ResultModifierData[0]));
    }
  }
}
