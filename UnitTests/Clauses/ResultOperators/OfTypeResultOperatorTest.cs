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
using Remotion.Linq.Clauses.ResultOperators;
using Remotion.Linq.Clauses.StreamedData;
using Remotion.Linq.UnitTests.TestDomain;

namespace Remotion.Linq.UnitTests.Clauses.ResultOperators
{
  [TestFixture]
  public class OfTypeResultOperatorTest
  {
    private OfTypeResultOperator _resultOperator;

    [SetUp]
    public void SetUp ()
    {
      _resultOperator = new OfTypeResultOperator (typeof (Chef));
    }

    [Test]
    public void Clone ()
    {
      var clonedClauseMapping = new QuerySourceMapping ();
      var cloneContext = new CloneContext (clonedClauseMapping);
      var clone = _resultOperator.Clone (cloneContext);

      Assert.That (clone, Is.InstanceOf (typeof (OfTypeResultOperator)));
      Assert.That (((OfTypeResultOperator) clone).SearchedItemType, Is.SameAs (_resultOperator.SearchedItemType));
    }

    [Test]
    public void ExecuteInMemory ()
    {
      var student1 = new Chef ();
      var student2 = new Chef ();
      var student3 = new Cook ();
      IEnumerable items = new Cook[] { student1, student2, student3 };
      var itemExpression = Expression.Constant (student3, typeof (Cook));
      var input = new StreamedSequence (items, new StreamedSequenceInfo (typeof (Cook[]), itemExpression));

      var result = _resultOperator.ExecuteInMemory<Cook> (input);

      var sequence = result.GetTypedSequence<Chef> ();
      Assert.That (sequence.ToArray (), Is.EquivalentTo (new[] { student1, student2 }));
      Assert.That (result.DataInfo.ResultItemType, Is.EqualTo (typeof (Chef)));
      Assert.That (((UnaryExpression) result.DataInfo.ItemExpression).Operand, Is.SameAs (itemExpression));
    }

    [Test]
    public void GetOutputDataInfo ()
    {
      var studentExpression = Expression.Constant (new Cook ());
      var input = new StreamedSequenceInfo (typeof (Cook[]), studentExpression);
      var result = _resultOperator.GetOutputDataInfo (input);

      Assert.That (result, Is.InstanceOf (typeof (StreamedSequenceInfo)));
      Assert.That (result.DataType, Is.SameAs (typeof (IQueryable<Chef>)));
      Assert.That (((StreamedSequenceInfo) result).ItemExpression, Is.InstanceOf (typeof (UnaryExpression)));
      Assert.That (((StreamedSequenceInfo) result).ItemExpression.NodeType, Is.EqualTo (ExpressionType.Convert));
      Assert.That (((StreamedSequenceInfo) result).ResultItemType, Is.SameAs (typeof (Chef)));
      Assert.That (((UnaryExpression) ((StreamedSequenceInfo) result).ItemExpression).Operand, Is.SameAs (input.ItemExpression));
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
    public new void ToString ()
    {
      Assert.That (_resultOperator.ToString (), Is.EqualTo ("OfType<Remotion.Linq.UnitTests.TestDomain.Chef>()"));
    }
  }
}
