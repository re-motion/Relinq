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

namespace Remotion.Linq.UnitTests.Clauses.ResultOperators
{
  [TestFixture]
  public class TakeResultOperatorTest
  {
    private TakeResultOperator _resultOperator;

    [SetUp]
    public void SetUp ()
    {
      _resultOperator = new TakeResultOperator (Expression.Constant (2));
    }

    [Test]
    [ExpectedException (typeof (ArgumentException))]
    public void Initialization_NoIntExpression ()
    {
      new TakeResultOperator (Expression.Constant ("12"));
    }

    [Test]
    public void Clone ()
    {
      var clonedClauseMapping = new QuerySourceMapping ();
      var cloneContext = new CloneContext (clonedClauseMapping);
      var clone = _resultOperator.Clone (cloneContext);

      Assert.That (clone, Is.InstanceOf (typeof (TakeResultOperator)));
    }

    [Test]
    public void ExecuteInMemory ()
    {
      IEnumerable items = new[] { 1, 2, 3, 0, 2 };
      var input = new StreamedSequence (items, new StreamedSequenceInfo (typeof (int[]), Expression.Constant (0)));
      var result = _resultOperator.ExecuteInMemory<int> (input);

      Assert.That (result.GetTypedSequence<int>().ToArray (), Is.EqualTo (new[] { 1, 2 }));
    }

    [Test]
    public void GetConstantCount ()
    {
      Assert.That (_resultOperator.GetConstantCount (), Is.EqualTo (2));
    }

    [Test]
    [ExpectedException (typeof (ArgumentException), ExpectedMessage = 
        "The count expression ('[main]') is no ConstantExpression, it is a QuerySourceReferenceExpression.\r\nParameter name: expression")]
    public void GetConstantCount_NoConstantExpression ()
    {
      var resultOperator = new TakeResultOperator (new QuerySourceReferenceExpression (ExpressionHelper.CreateMainFromClause_Int ()));
      resultOperator.GetConstantCount ();
    }

    [Test]
    public void TransformExpressions ()
    {
      var oldExpression = Expression.Constant (0);
      var newExpression = Expression.Constant (1);
      var resultOperator = new TakeResultOperator (oldExpression);

      resultOperator.TransformExpressions (ex =>
      {
        Assert.That (ex, Is.SameAs (oldExpression));
        return newExpression;
      });

      Assert.That (resultOperator.Count, Is.SameAs (newExpression));
    }
  }
}
