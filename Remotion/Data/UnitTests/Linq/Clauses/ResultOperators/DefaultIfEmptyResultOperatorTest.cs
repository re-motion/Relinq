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
using Remotion.Data.Linq.Clauses.ExecutionStrategies;
using Remotion.Data.Linq.Clauses.Expressions;
using Remotion.Data.Linq.Clauses.ResultOperators;
using Remotion.Data.UnitTests.Linq.TestDomain;

namespace Remotion.Data.UnitTests.Linq.Clauses.ResultOperators
{
  [TestFixture]
  public class DefaultIfEmptyResultOperatorTest
  {
    private DefaultIfEmptyResultOperator _resultOperatorWithDefaultValue;
    private DefaultIfEmptyResultOperator _resultOperatorWithoutDefaultValue;

    [SetUp]
    public void SetUp ()
    {
      _resultOperatorWithDefaultValue = new DefaultIfEmptyResultOperator (Expression.Constant (100));
      _resultOperatorWithoutDefaultValue = new DefaultIfEmptyResultOperator (null);
    }

    [Test]
    public void ExecutionStrategy ()
    {
      Assert.That (_resultOperatorWithDefaultValue.ExecutionStrategy, Is.SameAs (CollectionExecutionStrategy.Instance));
    }

    [Test]
    public void GetConstantOptionalDefaultValue_WithDefaultValue ()
    {
      Assert.That (_resultOperatorWithDefaultValue.GetConstantOptionalDefaultValue (), Is.EqualTo (100));
    }

    [Test]
    [ExpectedException (typeof (InvalidOperationException))]
    public void GetConstantOptionalDefaultValue_WithDefaultValue_NoConstantExpression ()
    {
      var resultOperator = new DefaultIfEmptyResultOperator (new QuerySourceReferenceExpression (ExpressionHelper.CreateMainFromClause ()));
      resultOperator.GetConstantOptionalDefaultValue ();
    }

    [Test]
    public void GetConstantOptionalDefaultValue_WithoutDefaultValue ()
    {
      Assert.That (_resultOperatorWithoutDefaultValue.GetConstantOptionalDefaultValue (), Is.SameAs (null));
    }

    [Test]
    public void Clone ()
    {
      var clonedClauseMapping = new QuerySourceMapping ();
      var cloneContext = new CloneContext (clonedClauseMapping);
      var clone = _resultOperatorWithDefaultValue.Clone (cloneContext);

      Assert.That (clone, Is.InstanceOfType (typeof (DefaultIfEmptyResultOperator)));
      Assert.That (((DefaultIfEmptyResultOperator) clone).OptionalDefaultValue, Is.SameAs (_resultOperatorWithDefaultValue.OptionalDefaultValue));
    }

    [Test]
    public void ExecuteInMemory_WithDefaultValue ()
    {
      object items = new int[0];
      IExecuteInMemoryData input = new ExecuteInMemorySequenceData (items, Expression.Constant (0));
      var result = _resultOperatorWithDefaultValue.ExecuteInMemory (input);

      Assert.That (result.GetCurrentSequenceInfo<int>().Sequence.ToArray(), Is.EqualTo (new[] { 100 }));
    }

    [Test]
    public void ExecuteInMemory_WithoutDefaultValue ()
    {
      object items = new int[0];
      IExecuteInMemoryData input = new ExecuteInMemorySequenceData (items, Expression.Constant (0));
      var result = _resultOperatorWithoutDefaultValue.ExecuteInMemory (input);

      Assert.That (result.GetCurrentSequenceInfo<int>().Sequence.ToArray(), Is.EqualTo (new[] { 0 }));
    }

    [Test]
    public void GetResultType ()
    {
      Assert.That (_resultOperatorWithDefaultValue.GetResultType (typeof (IQueryable<Student>)), Is.SameAs (typeof (IQueryable<Student>)));
    }

    [Test]
    public void TransformExpressions_WithDefaultValue ()
    {
      var oldExpression = ExpressionHelper.CreateExpression ();
      var newExpression = ExpressionHelper.CreateExpression ();
      var resultOperator = new DefaultIfEmptyResultOperator (oldExpression);

      resultOperator.TransformExpressions (ex =>
      {
        Assert.That (ex, Is.SameAs (oldExpression));
        return newExpression;
      });

      Assert.That (resultOperator.OptionalDefaultValue, Is.SameAs (newExpression));
    }

    [Test]
    public void TransformExpressions_WithoutDefaultValue ()
    {
      var resultOperator = new DefaultIfEmptyResultOperator (null);

      resultOperator.TransformExpressions (ex =>
      {
        Assert.Fail ("Must not be called.");
        throw new NotImplementedException ();
      });
    }


  }
}