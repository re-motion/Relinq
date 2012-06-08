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
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using NUnit.Framework;
using Remotion.Linq.UnitTests.Linq.Core.Clauses.ResultOperators;
using Remotion.Linq.Clauses.ResultOperators;
using Remotion.Linq.Clauses.StreamedData;
using Remotion.Linq.Utilities;
using Rhino.Mocks;

namespace Remotion.Linq.UnitTests.Linq.Core.Clauses.StreamedData
{
  [TestFixture]
  public class StreamedSequenceInfoTest
  {
    private ConstantExpression _intExpression;
    private StreamedSequenceInfo _infoWithIntSequence;

    [SetUp]
    public void SetUp ()
    {
      _intExpression = Expression.Constant (0);
      _infoWithIntSequence = new StreamedSequenceInfo (typeof (int[]), _intExpression);
    }

    [Test]
    [ExpectedException (typeof (ArgumentTypeException), ExpectedMessage =
        "Expected a type implementing IEnumerable<T>, but found 'System.Int32'.\r\nParameter name: dataType")]
    public void Initialization_DataTypeIsnotAssignableFromIEnumerable ()
    {
      new StreamedSequenceInfo (typeof (int), _intExpression);
    }

    [Test]
    [ExpectedException (typeof (ArgumentTypeException))]
    public void Initialization_CurrentSequence_WrongItemExpression ()
    {
      new StreamedSequenceInfo (typeof (string[]), _intExpression);
    }
    
    [Test]
    public void Initialization_CurrentSequence_Assignable ()
    {
        var stringExpression = Expression.Constant ("");
        var sequence = new StreamedSequenceInfo (typeof (object[]), stringExpression);

        Assert.That(sequence.ResultItemType, Is.EqualTo(typeof(object)));
    }

    [Test]
    public void DataType ()
    {
      Assert.That (_infoWithIntSequence.DataType, Is.SameAs (typeof (int[])));
    }

    [Test]
    public void AdjustDataType_CompatibleType ()
    {
      var result = _infoWithIntSequence.AdjustDataType (typeof (IEnumerable<int>));

      Assert.That (result, Is.Not.SameAs (_infoWithIntSequence));
      Assert.That (result, Is.TypeOf (typeof (StreamedSequenceInfo)));
      Assert.That (result.DataType, Is.SameAs (typeof (IEnumerable<int>)));
      Assert.That (((StreamedSequenceInfo) result).ItemExpression, Is.SameAs (_infoWithIntSequence.ItemExpression));
    }

    [Test]
    [ExpectedException (typeof (ArgumentException), ExpectedMessage =
        "'System.Collections.Generic.IEnumerable`1[System.String]' cannot be used as the data type for a sequence with an ItemExpression of type "
        + "'System.Int32'.\r\nParameter name: dataType")]
    public void AdjustDataType_IncompatibleType ()
    {
      _infoWithIntSequence.AdjustDataType (typeof (IEnumerable<string>));
    }

    [Test]
    [ExpectedException (typeof (ArgumentException), ExpectedMessage =
        "'System.Int32' cannot be used as the data type for a sequence with an ItemExpression of type 'System.Int32'.\r\n"
        + "Parameter name: dataType")]
    public void AdjustDataType_NonEnumerableType ()
    {
      _infoWithIntSequence.AdjustDataType (typeof (int));
    }

    [Test]
    public void AdjustDataType_GenericTypeDefinition ()
    {
      var result = _infoWithIntSequence.AdjustDataType (typeof (IEnumerable<>));

      Assert.That (result, Is.Not.SameAs (_infoWithIntSequence));
      Assert.That (result, Is.TypeOf (typeof (StreamedSequenceInfo)));
      Assert.That (result.DataType, Is.SameAs (typeof (IEnumerable<int>)));
      Assert.That (((StreamedSequenceInfo) result).ItemExpression, Is.SameAs (_infoWithIntSequence.ItemExpression));
    }

    [Test]
    [ExpectedException (typeof (ArgumentException), ExpectedMessage =
        "The generic type definition 'System.Collections.Generic.IDictionary`2[TKey,TValue]' could not be closed over the type of the ItemExpression "
        + "('System.Int32'). The type or method has 2 generic parameter(s), but 1 generic argument(s) were provided. A generic argument must be "
        + "provided for each generic parameter.\r\nParameter name: dataType")]
    public void AdjustDataType_GenericTypeDefinition_WrongNumberOfArguments ()
    {
      _infoWithIntSequence.AdjustDataType (typeof (IDictionary<,>));
    }

    [Test]
    public void MakeClosedGenericExecuteMethod ()
    {
      var executeMethod = typeof (CountResultOperator).GetMethod ("ExecuteInMemory", new[] { typeof (StreamedSequence) });
      var result = _infoWithIntSequence.MakeClosedGenericExecuteMethod (executeMethod);

      Assert.That (result.GetGenericArguments(), Is.EqualTo (new[] { typeof (int) }));
    }

    [Test]
    [ExpectedException (typeof (ArgumentException),
        ExpectedMessage = "GenericMethodDefinition must be a generic method definition.\r\nParameter name: genericMethodDefinition")]
    public void MakeClosedGenericExecuteMethod_NonGenericMethod ()
    {
      var executeMethod = typeof (CountResultOperator).GetMethod ("ExecuteInMemory", new[] { typeof (IStreamedData) });
      _infoWithIntSequence.MakeClosedGenericExecuteMethod (executeMethod);
    }

    [Test]
    [ExpectedException (typeof (ArgumentException),
        ExpectedMessage = "GenericMethodDefinition must be a generic method definition.\r\nParameter name: genericMethodDefinition")]
    public void MakeClosedGenericExecuteMethod_NonGenericMethodDefinition ()
    {
      var executeMethod = typeof (CountResultOperator)
          .GetMethod ("ExecuteInMemory", new[] { typeof (StreamedSequence) })
          .MakeGenericMethod (typeof (int));
      _infoWithIntSequence.MakeClosedGenericExecuteMethod (executeMethod);
    }

    [Test]
    [ExpectedException (typeof (ArgumentException),
        ExpectedMessage = "GenericMethodDefinition must have exactly one generic parameter.\r\nParameter name: genericMethodDefinition")]
    public void MakeClosedGenericExecuteMethod_WrongNumberOfGenericParameters ()
    {
      var executeMethod = typeof (TestResultOperator).GetMethod ("InvalidExecuteInMemory_TooManyGenericParameters");
      _infoWithIntSequence.MakeClosedGenericExecuteMethod (executeMethod);
    }

    [Test]
    public void ExecuteQueryModel ()
    {
      var queryModel = ExpressionHelper.CreateQueryModel_Cook();

      var executorMock = MockRepository.GenerateMock<IQueryExecutor>();
      executorMock.Expect (mock => mock.ExecuteCollection<int> (queryModel)).Return (new[] { 1, 2, 3 });

      var streamedData = (StreamedSequence) _infoWithIntSequence.ExecuteQueryModel (queryModel, executorMock);

      executorMock.VerifyAllExpectations();

      Assert.That (streamedData, Is.InstanceOf (typeof (StreamedSequence)));
      Assert.That (streamedData.DataInfo.ItemExpression, Is.SameAs (_infoWithIntSequence.ItemExpression));
      Assert.That (typeof (IQueryable<int>).IsAssignableFrom (streamedData.DataInfo.DataType), Is.True);
      Assert.That (streamedData.GetTypedSequence<int>().ToArray(), Is.EqualTo (new[] { 1, 2, 3 }));
      Assert.That (streamedData.Sequence, Is.InstanceOf (typeof (IQueryable<int>)));
    }

    [Test]
    [ExpectedException (typeof (InvalidOperationException), ExpectedMessage = "Test")]
    public void ExecuteQueryModel_WithException ()
    {
      var queryModel = ExpressionHelper.CreateQueryModel_Cook();

      var executorMock = MockRepository.GenerateMock<IQueryExecutor>();
      executorMock.Expect (mock => mock.ExecuteCollection<int> (queryModel)).Throw (new InvalidOperationException ("Test"));

      _infoWithIntSequence.ExecuteQueryModel (queryModel, executorMock);
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