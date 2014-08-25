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
using System.Linq.Expressions;
using NUnit.Framework;
using Remotion.Linq.Clauses;
using Remotion.Linq.Clauses.Expressions;
using Remotion.Linq.Clauses.ResultOperators;
using Remotion.Linq.Clauses.StreamedData;
using Remotion.Linq.Development.UnitTesting;
using Remotion.Linq.UnitTests.TestDomain;

namespace Remotion.Linq.UnitTests.Clauses.ResultOperators
{
  [TestFixture]
  public class AllResultOperatorTest
  {
    private MainFromClause _fromClause;
    private Expression _predicate;

    private AllResultOperator _resultOperator;

    [SetUp]
    public void SetUp ()
    {
      _fromClause = ExpressionHelper.CreateMainFromClause_Int ("i", typeof (int), ExpressionHelper.CreateIntQueryable ());
      _predicate = ExpressionHelper.Resolve<int, bool> (_fromClause, j => j > 2);

      _resultOperator = new AllResultOperator (_predicate);
    }

    [Test]
    public void Clone ()
    {
      var clonedClauseMapping = new QuerySourceMapping ();
      var cloneContext = new CloneContext (clonedClauseMapping);
      var clone = _resultOperator.Clone (cloneContext);

      Assert.That (clone, Is.InstanceOf (typeof (AllResultOperator)));
      Assert.That (((AllResultOperator) clone).Predicate, Is.SameAs (_resultOperator.Predicate));
    }

    [Test]
    public void ExecuteInMemory_True ()
    {
      IEnumerable items = new[] { 3, 4 };
      var input = new StreamedSequence (items, new StreamedSequenceInfo (typeof (int[]), new QuerySourceReferenceExpression (_fromClause)));
      var result = _resultOperator.ExecuteInMemory<int> (input);

      Assert.That (result.Value, Is.True);
    }

    [Test]
    public void ExecuteInMemory_False ()
    {
      IEnumerable items = new[] { 1, 2, 3, 4 };
      var input = new StreamedSequence (items, new StreamedSequenceInfo (typeof (int[]), new QuerySourceReferenceExpression (_fromClause)));
      var result = _resultOperator.ExecuteInMemory<int> (input);

      Assert.That (result.Value, Is.False);
    }

    [Test]
    public void GetOutputDataInfo ()
    {
      var itemExpression = Expression.Constant (0);
      var input = new StreamedSequenceInfo (typeof (int[]), itemExpression);
      var result = _resultOperator.GetOutputDataInfo (input);

      Assert.That (result, Is.InstanceOf (typeof (StreamedValueInfo)));
      Assert.That (result.DataType, Is.SameAs (typeof (bool)));
    }

    [Test]
    [ExpectedException (typeof (ArgumentException), ExpectedMessage =
        "Parameter 'inputInfo' has type 'Remotion.Linq.Clauses.StreamedData.StreamedScalarValueInfo'"
        + " when type 'Remotion.Linq.Clauses.StreamedData.StreamedSequenceInfo' was expected."
        + "\r\nParameter name: inputInfo")]
    public void GetOutputDataInfo_InvalidInput ()
    {
      var input = new StreamedScalarValueInfo (typeof (Cook));
      _resultOperator.GetOutputDataInfo (input);
    }

    [Test]
    public void TransformExpressions ()
    {
      var oldExpression = ExpressionHelper.CreateExpression ();
      var newExpression = ExpressionHelper.CreateExpression ();
      var resultOperator = new AllResultOperator (oldExpression);

      resultOperator.TransformExpressions (ex =>
      {
        Assert.That (ex, Is.SameAs (oldExpression));
        return newExpression;
      });

      Assert.That (resultOperator.Predicate, Is.SameAs (newExpression));
    }

    [Test]
    public new void ToString ()
    {
      Assert.That (_resultOperator.ToString (), Is.EqualTo ("All(([i] > 2))"));
    }
  }
}