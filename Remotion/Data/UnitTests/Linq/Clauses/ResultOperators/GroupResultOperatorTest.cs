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
using System.Reflection;
using NUnit.Framework;
using NUnit.Framework.SyntaxHelpers;
using Remotion.Data.Linq.Clauses.ExecutionStrategies;
using Remotion.Data.Linq.Clauses.Expressions;
using Remotion.Data.Linq.Clauses.ResultOperators;
using Remotion.Data.Linq.Clauses;
using Remotion.Data.UnitTests.Linq.Parsing;
using Remotion.Data.UnitTests.Linq.TestDomain;
using Remotion.Utilities;

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
    public void ItemType ()
    {
      var expectedItemType = typeof (IGrouping<,>).MakeGenericType (
          _resultOperator.KeySelector.ResolvedExpression.Type, 
          _resultOperator.ElementSelector.ResolvedExpression.Type);

      Assert.That (_resultOperator.ItemType, Is.SameAs (expectedItemType));
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
          "groupings",
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

      var clause = new GroupResultOperator ("groupings", inputDependentKeySelector, inputDependentElementSelector);

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
          "groupings",
          ExpressionHelper.CreateInputDependentExpression (Expression.Constant (1)), 
          ExpressionHelper.CreateInputDependentExpression (Expression.Constant (0)));

      Assert.That (resultOperator.ToString (), Is.EqualTo ("GroupBy(1, 0)"));
    }

    [Test]
    public void ExecuteInMemory ()
    {
      // group [i].ToString() by [j] % 3
      // expected input: new AnonymousType ( a = [i], b = [j] )

      var fromClause1 = ExpressionHelper.CreateMainFromClause ("i", typeof (int), ExpressionHelper.CreateIntQueryable ());
      var fromClause2 = ExpressionHelper.CreateMainFromClause ("j", typeof (int), ExpressionHelper.CreateIntQueryable ());
      var expectedInput = Expression.New (
        typeof (AnonymousType).GetConstructor (new[] {typeof (int), typeof (int) }), 
        new Expression[] { new QuerySourceReferenceExpression (fromClause1), new QuerySourceReferenceExpression (fromClause2) },
        new MemberInfo[] { typeof (AnonymousType).GetProperty ("a"), typeof (AnonymousType).GetProperty ("b") });

      var items = new[] { 
          new AnonymousType (111, 1), 
          new AnonymousType (222, 2), 
          new AnonymousType (333, 3), 
          new AnonymousType (444, 4), 
          new AnonymousType (555, 5) 
      };

      var input = new ExecuteInMemorySequenceData (items, expectedInput);
      
      var keySelector = new InputDependentExpression (ExpressionHelper.CreateLambdaExpression<AnonymousType, int> (trans => trans.b % 3), expectedInput);
      var elementSelector = new InputDependentExpression (ExpressionHelper.CreateLambdaExpression<AnonymousType, string> (trans => trans.a.ToString ()), expectedInput);
      var resultOperator = new GroupResultOperator ("groupings", keySelector, elementSelector);

      var result = resultOperator.ExecuteInMemory (input).GetCurrentSequence<IGrouping<int, string>> ();

      var resultArray = result.A.ToArray ();
      Assert.That (resultArray.Length, Is.EqualTo (3));
      Assert.That (resultArray[0].ToArray (), Is.EqualTo (new[] { "111", "444" }));
      Assert.That (resultArray[1].ToArray (), Is.EqualTo (new[] { "222", "555" }));
      Assert.That (resultArray[2].ToArray (), Is.EqualTo (new[] { "333" }));

      Assert.That (result.B, Is.InstanceOfType (typeof (QuerySourceReferenceExpression)));
      Assert.That (((QuerySourceReferenceExpression) result.B).ReferencedQuerySource, Is.SameAs (resultOperator));
    }

    [Test]
    public void GetResultType ()
    {
      var expectedInput = new QuerySourceReferenceExpression (
          ExpressionHelper.CreateMainFromClause ("s", typeof (Student), ExpressionHelper.CreateStudentQueryable ()));

      var keySelector = new InputDependentExpression (ExpressionHelper.CreateLambdaExpression<Student, int> (s => s.ID), expectedInput);
      var elementSelector = new InputDependentExpression (ExpressionHelper.CreateLambdaExpression<Student, string> (s => s.ToString ()), expectedInput);
      var resultOperator = new GroupResultOperator ("groupings", keySelector, elementSelector);

      Assert.That (resultOperator.GetResultType (typeof (IQueryable<Student>)), Is.SameAs (typeof (IQueryable<IGrouping<int, string>>)));
    }

    [Test]
    public void GetResultType_DerivedItemType ()
    {
      var expectedInput = new QuerySourceReferenceExpression (
          ExpressionHelper.CreateMainFromClause ("s", typeof (Student), ExpressionHelper.CreateStudentQueryable ()));

      var keySelector = new InputDependentExpression (ExpressionHelper.CreateLambdaExpression<Student, int> (s => s.ID), expectedInput);
      var elementSelector = new InputDependentExpression (ExpressionHelper.CreateLambdaExpression<Student, string> (s => s.ToString ()), expectedInput);
      var resultOperator = new GroupResultOperator ("groupings", keySelector, elementSelector);

      Assert.That (resultOperator.GetResultType (typeof (IQueryable<GoodStudent>)), Is.SameAs (typeof (IQueryable<IGrouping<int, string>>)));
    }

    [Test]
    [ExpectedException (typeof (ArgumentTypeException))]
    public void GetResultType_InvalidType_WrongItemType ()
    {
      _resultOperator.GetResultType (typeof (IQueryable<int>));
    }

    [Test]
    [ExpectedException (typeof (ArgumentTypeException))]
    public void GetResultType_InvalidType_NoIQueryable ()
    {
      _resultOperator.GetResultType (typeof (int));
    }

    [Test]
    [ExpectedException (typeof (InvalidOperationException))]
    public void GetResultType_KeyAndElementSelectorDontMatch ()
    {
      var expectedInput1 = new QuerySourceReferenceExpression (
          ExpressionHelper.CreateMainFromClause ("s", typeof (Student), ExpressionHelper.CreateStudentQueryable ()));
      var expectedInput2 = new QuerySourceReferenceExpression (
          ExpressionHelper.CreateMainFromClause ("i", typeof (int), ExpressionHelper.CreateStudentQueryable ()));

      var keySelector = new InputDependentExpression (ExpressionHelper.CreateLambdaExpression<Student, int> (s => s.ID), expectedInput1);
      var elementSelector = new InputDependentExpression (ExpressionHelper.CreateLambdaExpression<int, string> (i => i.ToString ()), expectedInput2);
      var resultOperator = new GroupResultOperator ("groupings", keySelector, elementSelector);

      resultOperator.GetResultType (typeof (IQueryable<Student>));
    }
  }
}