// Copyright (c) rubicon IT GmbH, www.rubicon.eu
//
// See the NOTICE file distributed with this work for additional information
// regarding copyright ownership.  rubicon licenses this file to you under 
// the Apache License, Version 2.0 (the "License"); you may not use this 
// file except in compliance with the License.  You may obtain a copy of the 
// License at
//
//   http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software 
// distributed under the License is distributed on an "AS IS" BASIS, WITHOUT 
// WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.  See the 
// License for the specific language governing permissions and limitations
// under the License.
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
using Remotion.Linq.Development.UnitTesting;
using Remotion.Linq.Development.UnitTesting.Clauses.Expressions;

namespace Remotion.Linq.UnitTests.Clauses.ResultOperators
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
    public void GetConstantOptionalDefaultValue_WithDefaultValue_IsConstantExpression ()
    {
      Assert.That (_resultOperatorWithDefaultValue.GetConstantOptionalDefaultValue (), Is.EqualTo (100));
    }

    [Test]
    public void GetConstantOptionalDefaultValue_WithDefaultValue_IsReducibleToConstantExpression ()
    {
      var resultOperator = new DefaultIfEmptyResultOperator (
          new ReducibleExtensionExpression (new ReducibleExtensionExpression (Expression.Constant (100))));
      Assert.That (resultOperator.GetConstantOptionalDefaultValue (), Is.EqualTo (100));
    }

    [Test]
    [ExpectedException (typeof (ArgumentException), ExpectedMessage = 
        "The default value expression ('[main]') is no ConstantExpression, it is a QuerySourceReferenceExpression.\r\nParameter name: expression")]
    public void GetConstantOptionalDefaultValue_WithDefaultValue_NoConstantExpression ()
    {
      var resultOperator = new DefaultIfEmptyResultOperator (new QuerySourceReferenceExpression (ExpressionHelper.CreateMainFromClause_Int ()));
      resultOperator.GetConstantOptionalDefaultValue ();
    }

    [Test]
    public void GetConstantOptionalDefaultValue_ChecksForInfiniteRecursion ()
    {
      var resultOperator = new DefaultIfEmptyResultOperator (new RecursiveReducibleExtensionExpression (typeof (int)));
      Assert.That (
          () => resultOperator.GetConstantOptionalDefaultValue(),
#if !NET_3_5
          Throws.ArgumentException.With.Message.EqualTo ("node cannot reduce to itself or null")
#else
          Throws.InvalidOperationException.With.Message.EqualTo ("Reduce cannot return the original expression.")
#endif
          );
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
