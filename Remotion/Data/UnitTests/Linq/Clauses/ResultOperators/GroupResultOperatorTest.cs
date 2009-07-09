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
using Remotion.Data.Linq.Clauses.ExecutionStrategies;
using Remotion.Data.Linq.Clauses.Expressions;
using Remotion.Data.Linq.Clauses.ResultOperators;
using Remotion.Data.Linq.Clauses;

namespace Remotion.Data.UnitTests.Linq.Clauses.ResultOperators
{
  [TestFixture]
  public class GroupResultOperatorTest
  {
    private GroupResultOperator _resultOperator;
    private CloneContext _cloneContext;

    [SetUp]
    public void SetUp ()
    {
      _resultOperator = ExpressionHelper.CreateGroupResultOperator ();
      _cloneContext = new CloneContext (new ClauseMapping());
    }

    [Test]
    public void Initialize()
    {
      Expression keySelector = ExpressionHelper.CreateExpression();
      Expression elementSelector = ExpressionHelper.CreateExpression ();

      var groupClause = new GroupResultOperator (keySelector, elementSelector);

      Assert.That (groupClause.KeySelector, Is.SameAs (keySelector));
      Assert.That (groupClause.ElementSelector, Is.SameAs (elementSelector));
      Assert.That (groupClause.ExecutionStrategy, Is.SameAs (CollectionExecutionStrategy.Instance));
    }

    [Test]
    public void Clone ()
    {
      var clone = (GroupResultOperator) _resultOperator.Clone (_cloneContext);

      Assert.That (clone, Is.Not.Null);
      Assert.That (clone, Is.Not.SameAs (_resultOperator));
      Assert.That (clone.KeySelector, Is.SameAs (_resultOperator.KeySelector));
      Assert.That (clone.ElementSelector, Is.SameAs (_resultOperator.ElementSelector));
    }

    [Test]
    public void Clone_AdjustsExpressions ()
    {
      var referencedExpression = ExpressionHelper.CreateMainFromClause();
      var keySelector = new QuerySourceReferenceExpression (referencedExpression);
      var elementSelector = new QuerySourceReferenceExpression (referencedExpression);
      var groupClause = new GroupResultOperator (keySelector, elementSelector);

      var newReferencedExpression = ExpressionHelper.CreateMainFromClause ();
      _cloneContext.ClauseMapping.AddMapping (referencedExpression, new QuerySourceReferenceExpression(newReferencedExpression));

      var clone = (GroupResultOperator) groupClause.Clone (_cloneContext);

      Assert.That (((QuerySourceReferenceExpression) clone.KeySelector).ReferencedClause, Is.SameAs (newReferencedExpression));
      Assert.That (((QuerySourceReferenceExpression) clone.ElementSelector).ReferencedClause, Is.SameAs (newReferencedExpression));
    }

    [Test]
    public void TransformExpressions ()
    {
      var oldKeySelector = ExpressionHelper.CreateExpression ();
      var oldElementSelector = ExpressionHelper.CreateExpression ();
      var clause = new GroupResultOperator (oldKeySelector, oldElementSelector);

      var newKeySelector = ExpressionHelper.CreateExpression ();
      var newElementSelector = ExpressionHelper.CreateExpression ();
      clause.TransformExpressions (ex =>
      {
        if (ex == oldElementSelector)
          return newElementSelector;
        else if (ex == oldKeySelector)
          return newKeySelector;
        else
        {
          Assert.Fail();
          return null;
        }
      });

      Assert.That (clause.KeySelector, Is.SameAs (newKeySelector));
      Assert.That (clause.ElementSelector, Is.SameAs (newElementSelector));
    }

    [Test]
    public new void ToString ()
    {
      var groupClause = new GroupResultOperator (Expression.Constant (1), Expression.Constant (0));

      Assert.That (groupClause.ToString (), Is.EqualTo ("GroupBy(1, 0)"));
    }
  }
}