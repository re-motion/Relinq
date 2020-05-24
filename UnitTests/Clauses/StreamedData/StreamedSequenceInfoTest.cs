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
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using NUnit.Framework;
using Remotion.Linq.Clauses.StreamedData;
using Remotion.Linq.Development.UnitTesting;
using Remotion.Linq.Development.UnitTesting.Clauses.StreamedData;
using Remotion.Linq.UnitTests.TestDomain;
using Rhino.Mocks;

namespace Remotion.Linq.UnitTests.Clauses.StreamedData
{
  [TestFixture]
  public class StreamedSequenceInfoTest
  {
    private ConstantExpression _stringExpression;
    private StreamedSequenceInfo _infoWithStringSequence;

    [SetUp]
    public void SetUp ()
    {
      _stringExpression = Expression.Constant ("dummy");
      _infoWithStringSequence = new StreamedSequenceInfo (typeof (string[]), _stringExpression);
    }

    [Test]
    public void Initialization ()
    {
      Assert.That (_infoWithStringSequence.DataType, Is.SameAs (typeof (string[])));
      Assert.That (_infoWithStringSequence.ResultItemType, Is.SameAs (typeof (string)));
      Assert.That (_infoWithStringSequence.ItemExpression, Is.SameAs (_stringExpression));
    }

    [Test]
    public void Initialization_DataTypeIsnotAssignableFromIEnumerable ()
    {
      Assert.That (
          () => new StreamedSequenceInfo (typeof (int), _stringExpression),
          Throws.ArgumentException.With.Message.EqualTo (
              "Expected a closed generic type implementing IEnumerable<T>, but found 'System.Int32'.\r\nParameter name: dataType"));
    }

    [Test]
    public void Initialization_CurrentSequence_WrongItemExpression ()
    {
      Assert.That (
          () => new StreamedSequenceInfo (typeof (int[]), _stringExpression),
          Throws.ArgumentException.With.Message.EqualTo (
              "ItemExpression is of type 'System.String', but should be 'System.Int32' (or derived from it).\r\nParameter name: itemExpression"));
    }

    [Test]
    public void Initialization_CurrentSequence_Assignable ()
    {
        var sequence = new StreamedSequenceInfo (typeof (object[]), _stringExpression);

        Assert.That (sequence.ResultItemType, Is.EqualTo (typeof (object)));
        Assert.That (sequence.ItemExpression.Type, Is.EqualTo (typeof (string)));
    }

    [Test]
    public void AdjustDataType_SameType ()
    {
      var result = _infoWithStringSequence.AdjustDataType (typeof (IEnumerable<string>));

      Assert.That (result, Is.Not.SameAs (_infoWithStringSequence));
      Assert.That (result, Is.TypeOf (typeof (StreamedSequenceInfo)));
      Assert.That (result.DataType, Is.SameAs (typeof (IEnumerable<string>)));
      Assert.That (((StreamedSequenceInfo) result).ItemExpression, Is.SameAs (_infoWithStringSequence.ItemExpression));
      Assert.That (((StreamedSequenceInfo) result).ResultItemType, Is.SameAs (typeof (string)));
    }

    [Test]
    public void AdjustDataType_MoreGenericType ()
    {
      var result = _infoWithStringSequence.AdjustDataType (typeof (IEnumerable<object>));

      Assert.That (result, Is.Not.SameAs (_infoWithStringSequence));
      Assert.That (result, Is.TypeOf (typeof (StreamedSequenceInfo)));
      Assert.That (result.DataType, Is.SameAs (typeof (IEnumerable<object>)));
      Assert.That (((StreamedSequenceInfo) result).ItemExpression, Is.SameAs (_infoWithStringSequence.ItemExpression));
      Assert.That (((StreamedSequenceInfo) result).ResultItemType, Is.SameAs (typeof (object)));
    }

    [Test]
    public void AdjustDataType_IncompatibleType ()
    {
      Assert.That (
          () => _infoWithStringSequence.AdjustDataType (typeof (IEnumerable<int>)),
          Throws.ArgumentException.With.Message.EqualTo (
              "'System.Collections.Generic.IEnumerable`1[System.Int32]' cannot be used as the data type for a sequence with an ItemExpression of type 'System.String'.\r\n"
              + "Parameter name: dataType"));
    }

    [Test]
    public void AdjustDataType_NonEnumerableType ()
    {
      Assert.That (
          () => _infoWithStringSequence.AdjustDataType (typeof (int)),
          Throws.ArgumentException.With.Message.EqualTo (
              "'System.Int32' cannot be used as the data type for a sequence with an ItemExpression of type 'System.String'.\r\n"
              + "Parameter name: dataType"));
    }

    [Test]
    public void AdjustDataType_GenericTypeDefinition ()
    {
      var result = _infoWithStringSequence.AdjustDataType (typeof (IQueryable<>));

      Assert.That (result, Is.Not.SameAs (_infoWithStringSequence));
      Assert.That (result, Is.TypeOf (typeof (StreamedSequenceInfo)));
      Assert.That (result.DataType, Is.SameAs (typeof (IQueryable<string>)));
    }

    [Test]
    public void AdjustDataType_GenericTypeDefinition_FromCovariantDataType ()
    {
      var info = new StreamedSequenceInfo (typeof (IEnumerable<object>), _stringExpression);
      var result = info.AdjustDataType (typeof (IQueryable<>));

      Assert.That (result, Is.Not.SameAs (info));
      Assert.That (result, Is.TypeOf (typeof (StreamedSequenceInfo)));
      Assert.That (result.DataType, Is.SameAs (typeof (IQueryable<object>)));
      Assert.That (((StreamedSequenceInfo) result).ResultItemType, Is.SameAs (typeof (object)));
      Assert.That (((StreamedSequenceInfo) result).ItemExpression, Is.SameAs (_stringExpression));
    }

    [Test]
    public void AdjustDataType_GenericTypeDefinition_WrongNumberOfArguments ()
    {
      Assert.That (
          () => _infoWithStringSequence.AdjustDataType (typeof (IDictionary<,>)),
          Throws.ArgumentException.With.Message.EqualTo (
              "The generic type definition 'System.Collections.Generic.IDictionary`2[TKey,TValue]' could not be closed over the type of the ResultItemType ('System.String'). "
#if !NET_3_5
              + "The number of generic arguments provided doesn't equal the arity of the generic type definition.\r\n"
              + "Parameter name: instantiation\r\n"
#else
              + "The number of generic arguments provided doesn't equal the arity of the generic type definition.\r\n"
              + "Parameter name: instantiation\r\n"
#endif
              + "Parameter name: dataType"));
    }

    [Test]
    public void ExecuteQueryModel ()
    {
      var queryModel = ExpressionHelper.CreateQueryModel<Cook>();

      var executorMock = MockRepository.GenerateMock<IQueryExecutor>();
      executorMock.Expect (mock => mock.ExecuteCollection<string> (queryModel)).Return (new[] { "a", "b", "c" });

      var streamedData = (StreamedSequence) _infoWithStringSequence.ExecuteQueryModel (queryModel, executorMock);

      executorMock.VerifyAllExpectations();

      Assert.That (streamedData, Is.InstanceOf (typeof (StreamedSequence)));
      Assert.That (streamedData.DataInfo.ItemExpression, Is.SameAs (_infoWithStringSequence.ItemExpression));
      Assert.That (typeof (IQueryable<string>).IsAssignableFrom (streamedData.DataInfo.DataType), Is.True);
      Assert.That (streamedData.GetTypedSequence<string> ().ToArray (), Is.EqualTo (new[] { "a", "b", "c" }));
      Assert.That (streamedData.Sequence, Is.InstanceOf (typeof (IQueryable<string>)));
    }

    [Test]
    public void ExecuteQueryModel_WithException ()
    {
      var queryModel = ExpressionHelper.CreateQueryModel<Cook>();

      var executorMock = MockRepository.GenerateMock<IQueryExecutor>();
      executorMock.Expect (mock => mock.ExecuteCollection<string> (queryModel)).Throw (new InvalidOperationException ("Test"));
      Assert.That (
          () => _infoWithStringSequence.ExecuteQueryModel (queryModel, executorMock),
          Throws.InvalidOperationException
              .With.Message.EqualTo ("Test"));
    }

    [Test]
    public void Equals ()
    {
      // ReSharper disable EqualExpressionComparison
      var constantExpression = Expression.Constant ("test");
      Assert.That (new StreamedSequenceInfo (typeof (string[]), constantExpression).Equals ((object) null), Is.False);
      Assert.That (
          new StreamedSequenceInfo (typeof (string[]), constantExpression).Equals ((object) new StreamedSequenceInfo (typeof (string[]), constantExpression)),
          Is.True);

      // Object type
      Assert.That (
          new StreamedSequenceInfo (typeof (string[]), constantExpression).Equals ((object) new TestStreamedValueInfo (typeof (string[]))),
          Is.False);

      // Expression
      Assert.That (
          new StreamedSequenceInfo (typeof (string[]), constantExpression).Equals (
              (object) new StreamedSequenceInfo (typeof (string[]), Expression.Constant ("test"))),
          Is.False);

      // Data type and expression (data type alone can't be tested because the ctor would throw)
      Assert.That (
          new StreamedSequenceInfo (typeof (string[]), constantExpression).Equals (
              (object) new StreamedSequenceInfo (typeof (char[]), Expression.Constant ('t'))),
          Is.False);
      // ReSharper restore EqualExpressionComparison
    }

    [Test]
    public void GetHashCodeTest ()
    {
      var constantExpression = Expression.Constant ("test");
      Assert.That (
          new StreamedSequenceInfo (typeof (string[]), constantExpression).GetHashCode(),
          Is.EqualTo (new StreamedSequenceInfo (typeof (string[]), constantExpression).GetHashCode()));
    }
  }
}