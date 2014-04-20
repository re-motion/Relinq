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
  public class ContainsResultOperatorTest
  {
    private ContainsResultOperator _resultOperator;

    [SetUp]
    public void SetUp ()
    {
      _resultOperator = new ContainsResultOperator (Expression.Constant (2));
    }

    [Test]
    public void Clone ()
    {
      var clonedClauseMapping = new QuerySourceMapping ();
      var cloneContext = new CloneContext (clonedClauseMapping);
      var clone = _resultOperator.Clone (cloneContext);

      Assert.That (clone, Is.InstanceOf (typeof (ContainsResultOperator)));
      Assert.That (((ContainsResultOperator) clone).Item, Is.SameAs (_resultOperator.Item));
    }

    [Test]
    public void ExecuteInMemory ()
    {
      IEnumerable items = new[] { 1, 2, 3, 4 };
      var input = new StreamedSequence (items, new StreamedSequenceInfo (typeof (int[]), Expression.Constant (0)));
      var result = _resultOperator.ExecuteInMemory<int> (input);

      Assert.That (result.Value, Is.True);
    }

    [Test]
    public void GetOutputDataInfo ()
    {
      var itemExpression = Expression.Constant (Expression.Constant (new Cook ()));
      var input = new StreamedSequenceInfo (typeof (object[]), itemExpression);
      
      // Works because int is assignable to object - the ItemExpression of type Cook isn't checked here
      var result = _resultOperator.GetOutputDataInfo (input);

      Assert.That (result, Is.InstanceOf (typeof (StreamedScalarValueInfo)));
      Assert.That (result.DataType, Is.SameAs (typeof (bool)));
    }

    [Test]
    [ExpectedException (typeof (ArgumentException), ExpectedMessage = 
        "Parameter 'inputInfo' has type 'Remotion.Linq.Clauses.StreamedData.StreamedScalarValueInfo' "
        + "when type 'Remotion.Linq.Clauses.StreamedData.StreamedSequenceInfo' was expected."
        + "\r\nParameter name: inputInfo")]
    public void GetOutputDataInfo_InvalidInput ()
    {
      var input = new StreamedScalarValueInfo (typeof (Cook));
      _resultOperator.GetOutputDataInfo (input);
    }

    [Test]
    [ExpectedException (typeof (ArgumentException), ExpectedMessage = 
        "The items of the input sequence of type 'Remotion.Linq.UnitTests.TestDomain.Cook' are not compatible with the item expression "
        + "of type 'System.Int32'.\r\nParameter name: inputInfo")]
    public void GetOutputDataInfo_InvalidInput_DoesntMatchItem ()
    {
      var input = new StreamedSequenceInfo (typeof (Cook[]), Expression.Constant (new Cook()));
      _resultOperator.GetOutputDataInfo (input);
    }

    [Test]
    public void GetConstantItem ()
    {
      Assert.That (_resultOperator.GetConstantItem<int> (), Is.EqualTo (2));
    }

    [Test]
    [ExpectedException (typeof (ArgumentException), ExpectedMessage =
        "The item expression ('[main]') is no ConstantExpression, it is a QuerySourceReferenceExpression.\r\nParameter name: expression")]
    public void GetConstantItem_NoConstantExpression ()
    {
      var resultOperator = new ContainsResultOperator (new QuerySourceReferenceExpression (ExpressionHelper.CreateMainFromClause_Int ()));
      resultOperator.GetConstantItem<object> ();
    }

    [Test]
    [ExpectedException (typeof (ArgumentException), ExpectedMessage =
        "The value stored by the item expression ('2') is not of type 'System.DateTime', it is of type 'System.Int32'.\r\nParameter name: expression")]
    public void GetConstantItem_NotExpectedType ()
    {
      _resultOperator.GetConstantItem<DateTime> ();
    }

    [Test]
    public void TransformExpressions ()
    {
      var oldExpression = ExpressionHelper.CreateExpression ();
      var newExpression = ExpressionHelper.CreateExpression ();
      var resultOperator = new ContainsResultOperator (oldExpression);

      resultOperator.TransformExpressions (ex =>
      {
        Assert.That (ex, Is.SameAs (oldExpression));
        return newExpression;
      });

      Assert.That (resultOperator.Item, Is.SameAs (newExpression));
    }
  }
}
