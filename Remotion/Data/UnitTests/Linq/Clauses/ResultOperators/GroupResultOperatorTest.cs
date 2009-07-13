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
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using NUnit.Framework;
using NUnit.Framework.SyntaxHelpers;
using Remotion.Data.Linq.Clauses.ExecutionStrategies;
using Remotion.Data.Linq.Clauses.Expressions;
using Remotion.Data.Linq.Clauses.ResultOperators;
using Remotion.Data.Linq.Clauses;
using Remotion.Data.UnitTests.Linq.Parsing;

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
      _cloneContext = new CloneContext (new QuerySourceMapping());
    }

    [Test]
    public void ExecutionStrategy()
    {
      Assert.That (_resultOperator.ExecutionStrategy, Is.SameAs (CollectionExecutionStrategy.Instance));
    }

    [Test]
    public void Clone ()
    {
      var clone = (GroupResultOperator) _resultOperator.Clone (_cloneContext);

      Assert.That (clone, Is.Not.Null);
      Assert.That (clone, Is.Not.SameAs (_resultOperator));

      ExpressionTreeComparer.CheckAreEqualTrees (_resultOperator.KeySelector.DependentExpression, clone.KeySelector.DependentExpression);
      ExpressionTreeComparer.CheckAreEqualTrees (_resultOperator.ElementSelector.DependentExpression, clone.ElementSelector.DependentExpression);
      
      Assert.That (clone.KeySelector.ExpectedInput, Is.SameAs (_resultOperator.KeySelector.ExpectedInput));
      Assert.That (clone.ElementSelector.ExpectedInput, Is.SameAs (_resultOperator.ElementSelector.ExpectedInput));
    }

    [Test]
    public void TransformExpressions_ExpectedInput ()
    {
      var oldKeySelector = ExpressionHelper.CreateExpression ();
      var oldElementSelector = ExpressionHelper.CreateExpression ();
      var clause = new GroupResultOperator (
          ExpressionHelper.CreateInputDependentExpression (oldKeySelector), 
          ExpressionHelper.CreateInputDependentExpression (oldElementSelector));

      var newKeySelector = ExpressionHelper.CreateExpression ();
      var newElementSelector = ExpressionHelper.CreateExpression ();
      clause.TransformExpressions (ex =>
      {
        if (ex == oldElementSelector)
          return newElementSelector;
        else if (ex == oldKeySelector)
          return newKeySelector;
        else
          return ex;
      });

      Assert.That (clause.KeySelector.ExpectedInput, Is.SameAs (newKeySelector));
      Assert.That (clause.ElementSelector.ExpectedInput, Is.SameAs (newElementSelector));
    }

    [Test]
    public void TransformExpressions_DependentExpressionBody ()
    {
      var oldKeySelector = ExpressionHelper.CreateExpression ();
      var oldElementSelector = ExpressionHelper.CreateExpression ();

      var expectedInput = ExpressionHelper.CreateExpression ();
      var parameter = Expression.Parameter (expectedInput.Type, "x");

      var inputDependentKeySelector = new InputDependentExpression (Expression.Lambda (oldKeySelector, parameter), expectedInput);
      var inputDependentElementSelector = new InputDependentExpression (Expression.Lambda (oldElementSelector, parameter), expectedInput);

      var clause = new GroupResultOperator (inputDependentKeySelector, inputDependentElementSelector);

      var newKeySelector = ExpressionHelper.CreateExpression ();
      var newElementSelector = ExpressionHelper.CreateExpression ();
      clause.TransformExpressions (ex =>
      {
        if (ex == oldElementSelector)
          return newElementSelector;
        else if (ex == oldKeySelector)
          return newKeySelector;
        else
          return ex;
      });

      Assert.That (clause.KeySelector.DependentExpression.Body, Is.SameAs (newKeySelector));
      Assert.That (clause.ElementSelector.DependentExpression.Body, Is.SameAs (newElementSelector));
    }

    [Test]
    public new void ToString ()
    {
      var resultOperator = new GroupResultOperator (
          ExpressionHelper.CreateInputDependentExpression (Expression.Constant (1)), 
          ExpressionHelper.CreateInputDependentExpression (Expression.Constant (0)));

      Assert.That (resultOperator.ToString (), Is.EqualTo ("GroupBy(1, 0)"));
    }

    [Test]
    public void ExecuteInMemory ()
    {
      var input = new[] { 1, 2, 3, 4, 5 };

      // group i.ToString() by i % 3

      var expectedInput = new QuerySourceReferenceExpression (
          ExpressionHelper.CreateMainFromClause("i", typeof (int), ExpressionHelper.CreateQuerySource()));

      var keySelector = new InputDependentExpression (ExpressionHelper.CreateLambdaExpression<int, int> (i => i % 3), expectedInput);
      var elementSelector = new InputDependentExpression (ExpressionHelper.CreateLambdaExpression<int, string> (i => i.ToString()), expectedInput);
      var resultOperator = new GroupResultOperator (keySelector, elementSelector);

      var result = ((IEnumerable<IGrouping<int, string>>) resultOperator.ExecuteInMemory (input)).ToArray();

      Assert.That (result.Length, Is.EqualTo (3));
      Assert.That (result[0].ToArray (), Is.EqualTo (new[] { "1", "4" }));
      Assert.That (result[1].ToArray (), Is.EqualTo (new[] { "2", "5" }));
      Assert.That (result[2].ToArray (), Is.EqualTo (new[] { "3" }));
    }
  }
}