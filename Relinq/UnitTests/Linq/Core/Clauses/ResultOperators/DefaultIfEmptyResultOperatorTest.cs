// This file is part of the re-linq project (relinq.codeplex.com)
// Copyright (c) rubicon IT GmbH, www.rubicon.eu
// 
// re-linq is free software; you can redistribute it and/or modify it under 
// the terms of the GNU Lesser General Public License as published by the 
// Free Software Foundation; either version 2.1 of the License, 
// or (at your option) any later version.
// 
// re-linq is distributed in the hope that it will be useful, 
// but WITHOUT ANY WARRANTY; without even the implied warranty of 
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the 
// GNU Lesser General Public License for more details.
// 
// You should have received a copy of the GNU Lesser General Public License
// along with re-linq; if not, see http://www.gnu.org/licenses.
// 
using System;
using System.Collections;
using System.Linq;
using System.Linq.Expressions;
using NUnit.Framework;
using Remotion.Linq.Clauses;
using Remotion.Linq.Clauses.Expressions;
using Remotion.Linq.Clauses.ResultOperators;
using Remotion.Linq.Clauses.StreamedData;

namespace Remotion.Linq.UnitTests.Linq.Core.Clauses.ResultOperators
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
    public void GetConstantOptionalDefaultValue_WithDefaultValue ()
    {
      Assert.That (_resultOperatorWithDefaultValue.GetConstantOptionalDefaultValue (), Is.EqualTo (100));
    }

    [Test]
    [ExpectedException (typeof (InvalidOperationException))]
    public void GetConstantOptionalDefaultValue_WithDefaultValue_NoConstantExpression ()
    {
      var resultOperator = new DefaultIfEmptyResultOperator (new QuerySourceReferenceExpression (ExpressionHelper.CreateMainFromClause_Int ()));
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

      Assert.That (clone, Is.InstanceOf (typeof (DefaultIfEmptyResultOperator)));
      Assert.That (((DefaultIfEmptyResultOperator) clone).OptionalDefaultValue, Is.SameAs (_resultOperatorWithDefaultValue.OptionalDefaultValue));
    }

    [Test]
    public void ExecuteInMemory_WithDefaultValue ()
    {
      IEnumerable items = new int[0];
      var input = new StreamedSequence (items, new StreamedSequenceInfo (typeof (int[]), Expression.Constant (0)));
      var result = _resultOperatorWithDefaultValue.ExecuteInMemory<int> (input);

      Assert.That (result.GetTypedSequence<int>().ToArray(), Is.EqualTo (new[] { 100 }));
    }

    [Test]
    public void ExecuteInMemory_WithoutDefaultValue ()
    {
      IEnumerable items = new int[0];
      var input = new StreamedSequence (items, new StreamedSequenceInfo (typeof (int[]), Expression.Constant (0)));
      var result = _resultOperatorWithoutDefaultValue.ExecuteInMemory<int> (input);

      Assert.That (result.GetTypedSequence<int>().ToArray(), Is.EqualTo (new[] { 0 }));
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
