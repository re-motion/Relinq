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
using Remotion.Linq.Clauses.StreamedData;

namespace Remotion.Linq.UnitTests.Clauses.StreamedData
{
  [TestFixture]
  public class StreamedSequenceTest
  {
    private ConstantExpression _stringExpression;
    private string[] _stringSequence;
    private StreamedSequence _dataWithCovariantSequence;

    [SetUp]
    public void SetUp ()
    {
      _stringExpression = Expression.Constant (0);
      _stringSequence = new[] { "a", "b", "c" };

      _dataWithCovariantSequence = new StreamedSequence (_stringSequence, new StreamedSequenceInfo (typeof (object[]), _stringExpression));
    }

    [Test]
    public void Initialization_CurrentSequence_WithoutItemExpression ()
    {
      Assert.That (
          () => new StreamedSequence (_stringSequence, null),
          Throws.ArgumentNullException);
    }

    [Test]
    public void Initialization_CurrentSequence_WrongItemExpression ()
    {
      Assert.That (
          () => new StreamedSequence (new[] { "1", "2", "3" }, new StreamedSequenceInfo (typeof (int[]), _stringExpression)),
          Throws.ArgumentException
              .With.Message.EqualTo (
                  "Parameter 'sequence' has type 'System.String[]' when type 'System.Int32[]' was expected.\r\nParameter name: sequence"));
    }

    [Test]
    public void DataInfo ()
    {
      Assert.That (_dataWithCovariantSequence.DataInfo.DataType, Is.SameAs (typeof (object[])));
      Assert.That (_dataWithCovariantSequence.DataInfo.ResultItemType, Is.SameAs (typeof (object)));
      Assert.That (_dataWithCovariantSequence.DataInfo.ItemExpression, Is.SameAs (_stringExpression));
    }

    [Test]
    public void GetTypedSequence ()
    {
      var sequenceData = _dataWithCovariantSequence.GetTypedSequence<string> ();
      Assert.That (sequenceData, Is.SameAs (_stringSequence));

      var covariantSequenceData = _dataWithCovariantSequence.GetTypedSequence<object> ();
      Assert.That (covariantSequenceData, Is.SameAs (_stringSequence));
    }

    [Test]
    public void GetTypedSequence_InvalidItemType ()
    {
      Assert.That (
          () => _dataWithCovariantSequence.GetTypedSequence<int>(),
          Throws.InvalidOperationException
              .With.Message.EqualTo (
                  "Cannot retrieve the current value as a sequence with item type 'System.Int32' because its items are of type 'System.Object'."));
    }
  }
}
