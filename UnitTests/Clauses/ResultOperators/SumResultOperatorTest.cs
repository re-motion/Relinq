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

namespace Remotion.Linq.UnitTests.Clauses.ResultOperators
{
  [TestFixture]
  public class SumResultOperatorTest
  {
    private SumResultOperator _resultOperator;

    [SetUp]
    public void SetUp ()
    {
      _resultOperator = new SumResultOperator ();
    }

    [Test]
    public void Clone ()
    {
      var clonedClauseMapping = new QuerySourceMapping ();
      var cloneContext = new CloneContext (clonedClauseMapping);
      var clone = _resultOperator.Clone (cloneContext);

      Assert.That (clone, Is.InstanceOf (typeof (SumResultOperator)));
    }

    [Test]
    public void ExecuteInMemory ()
    {
      var input = new StreamedSequence (new[] { 1, 2, 3 }, new StreamedSequenceInfo (typeof (int[]), Expression.Constant (0)));
      var result = _resultOperator.ExecuteInMemory<int> (input);

      Assert.That (result.Value, Is.EqualTo (6));
    }

    [Test]
    public void ExecuteInMemory_UnsupportedType ()
    {
      var input = new StreamedSequence (new[] { "1", "2", "3" }, new StreamedSequenceInfo (typeof (string[]), Expression.Constant ("0")));
      Assert.That (
          () => _resultOperator.ExecuteInMemory<string> (input),
          Throws.InstanceOf<NotSupportedException>()
              .With.Message.EqualTo (
                  "Cannot calculate the sum of objects of type 'System.String' in memory."));
    }

    [Test]
    public void GetOutputDataInfo ()
    {
      var studentExpression = Expression.Constant (0.0f);
      var input = new StreamedSequenceInfo (typeof (float[]), studentExpression);
      var result = _resultOperator.GetOutputDataInfo (input);

      Assert.That (result, Is.InstanceOf (typeof (StreamedValueInfo)));
      Assert.That (result.DataType, Is.SameAs (typeof (float)));
    }

    [Test]
    public void GetOutputDataInfo_CovariantSequence ()
    {
      var studentExpression = Expression.Constant (0.0f);
      var input = new StreamedSequenceInfo (typeof (object[]), studentExpression);
      var result = _resultOperator.GetOutputDataInfo (input);

      Assert.That (result, Is.InstanceOf (typeof (StreamedValueInfo)));
      Assert.That (result.DataType, Is.SameAs (typeof (object)));
    }

    [Test]
    public void GetOutputDataInfo_InvalidInput ()
    {
      var input = new StreamedScalarValueInfo (typeof (float));
      Assert.That (
          () => _resultOperator.GetOutputDataInfo (input),
          Throws.ArgumentException
              .With.Message.EqualTo (
                  "Parameter 'inputInfo' has type 'Remotion.Linq.Clauses.StreamedData.StreamedScalarValueInfo' "
                  + "when type 'Remotion.Linq.Clauses.StreamedData.StreamedSequenceInfo' was expected."
                  + "\r\nParameter name: inputInfo"));
    }
  }
}
