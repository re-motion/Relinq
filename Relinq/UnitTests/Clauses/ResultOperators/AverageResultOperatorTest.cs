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
using System.Linq.Expressions;
using NUnit.Framework;
using Remotion.Linq.Clauses;
using Remotion.Linq.Clauses.ResultOperators;
using Remotion.Linq.Clauses.StreamedData;
using Remotion.Linq.UnitTests.TestDomain;

namespace Remotion.Linq.UnitTests.Clauses.ResultOperators
{
  [TestFixture]
  public class AverageResultOperatorTest
  {
    private AverageResultOperator _resultOperator;

    [SetUp]
    public void SetUp ()
    {
      _resultOperator = new AverageResultOperator ();
    }

    [Test]
    public void Clone ()
    {
      var clonedClauseMapping = new QuerySourceMapping ();
      var cloneContext = new CloneContext (clonedClauseMapping);
      var clone = _resultOperator.Clone (cloneContext);

      Assert.That (clone, Is.InstanceOf (typeof (AverageResultOperator)));
    }

    [Test]
    public void ExecuteInMemory ()
    {
      var input = new StreamedSequence (new[] { 1, 2, 3 }, new StreamedSequenceInfo (typeof (int[]), Expression.Constant (0)));
      var result = _resultOperator.ExecuteInMemory<int> (input);

      Assert.That (result.Value, Is.InstanceOf (typeof (double)));
      Assert.That (result.Value, Is.EqualTo (2.0));
    }

    [Test]
    [ExpectedException (typeof (NotSupportedException), ExpectedMessage = "Cannot calculate the average of objects of type 'System.String' in memory.")]
    public void ExecuteInMemory_UnsupportedType ()
    {
      var input = new StreamedSequence (new[] { "1", "2", "3" }, new StreamedSequenceInfo (typeof (string[]), Expression.Constant ("0")));
      _resultOperator.ExecuteInMemory<string> (input);
    }

    [Test]
    public void GetOutputDataInfo_OutputTypeEqualsItemTypeByDefault ()
    {
      var studentExpression = Expression.Constant (new Cook ());
      var input = new StreamedSequenceInfo (typeof (Cook[]), studentExpression);
      var result = _resultOperator.GetOutputDataInfo (input);

      Assert.That (result, Is.InstanceOf (typeof (StreamedValueInfo)));
      Assert.That (result.DataType, Is.SameAs (typeof (Cook)));
    }

    [Test]
    public void GetOutputDataInfo_IntegerTypesGoToDouble ()
    {
      CheckOutputDataInfo (typeof (int), typeof (double));
      CheckOutputDataInfo (typeof (int?), typeof (double?));
      CheckOutputDataInfo (typeof (long), typeof (double));
      CheckOutputDataInfo (typeof (long?), typeof (double?));
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

    private void CheckOutputDataInfo (Type itemType, Type expectedOutputType)
    {
      var itemExpression = Expression.Constant (Activator.CreateInstance (itemType), itemType);
      var inputInfo = new StreamedSequenceInfo (itemType.MakeArrayType(), itemExpression);
      var outputInfo = _resultOperator.GetOutputDataInfo (inputInfo);

      Assert.That (outputInfo, Is.InstanceOf (typeof (StreamedValueInfo)));
      Assert.That (outputInfo.DataType, Is.SameAs (expectedOutputType));
    }
  }
}
