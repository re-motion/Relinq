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
using Remotion.Linq.Clauses.ResultOperators;
using Remotion.Linq.Clauses.StreamedData;
using Remotion.Linq.Development.UnitTesting;
using Remotion.Linq.UnitTests.Linq.Core.Clauses.ResultOperators;
using Remotion.Linq.UnitTests.Linq.Core.TestDomain;
using Rhino.Mocks;

namespace Remotion.Linq.UnitTests.Linq.Core.Clauses.StreamedData
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
    [ExpectedException (typeof (ArgumentException), ExpectedMessage =
        "Expected a type implementing IEnumerable<T>, but found 'System.Int32'.\r\nParameter name: dataType")]
    public void Initialization_DataTypeIsnotAssignableFromIEnumerable ()
    {
      new StreamedSequenceInfo (typeof (int), _stringExpression);
    }

    [Test]
    [ExpectedException (typeof (ArgumentException), ExpectedMessage = 
        "ItemExpression is of type 'System.String', but should be 'System.Int32' (or derived from it).\r\nParameter name: itemExpression")]
    public void Initialization_CurrentSequence_WrongItemExpression ()
    {
      new StreamedSequenceInfo (typeof (int[]), _stringExpression);
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
    [ExpectedException (typeof (ArgumentException), ExpectedMessage =
        "'System.Collections.Generic.IEnumerable`1[System.Int32]' cannot be used as the data type for a sequence with an ItemExpression of type "
        + "'System.String'.\r\nParameter name: dataType")]
    public void AdjustDataType_IncompatibleType ()
    {
      _infoWithStringSequence.AdjustDataType (typeof (IEnumerable<int>));
    }

    [Test]
    [ExpectedException (typeof (ArgumentException), ExpectedMessage =
        "'System.Int32' cannot be used as the data type for a sequence with an ItemExpression of type 'System.String'.\r\n"
        + "Parameter name: dataType")]
    public void AdjustDataType_NonEnumerableType ()
    {
      _infoWithStringSequence.AdjustDataType (typeof (int));
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
    [ExpectedException (typeof (ArgumentException), ExpectedMessage =
        "The generic type definition 'System.Collections.Generic.IDictionary`2[TKey,TValue]' could not be closed over the type of the ResultItemType "
        + "('System.String'). The number of generic arguments provided doesn't equal the arity of the generic type definition.\r\n" 
        + "Parameter name: instantiation\r\n"
        + "Parameter name: dataType")]
    public void AdjustDataType_GenericTypeDefinition_WrongNumberOfArguments ()
    {
      _infoWithStringSequence.AdjustDataType (typeof (IDictionary<,>));
    }

    [Test]
    public void MakeClosedGenericExecuteMethod ()
    {
      var executeMethod = typeof (CountResultOperator).GetMethod ("ExecuteInMemory", new[] { typeof (StreamedSequence) });
      var result = _infoWithStringSequence.MakeClosedGenericExecuteMethod (executeMethod);

      Assert.That (result.GetGenericArguments(), Is.EqualTo (new[] { typeof (string) }));
    }

    [Test]
    public void MakeClosedGenericExecuteMethod_WithCovariantDataType ()
    {
      var info = new StreamedSequenceInfo (typeof (IEnumerable<object>), _stringExpression);

      var executeMethod = typeof (CountResultOperator).GetMethod ("ExecuteInMemory", new[] { typeof (StreamedSequence) });
      var result = info.MakeClosedGenericExecuteMethod (executeMethod);

      Assert.That (result.GetGenericArguments (), Is.EqualTo (new[] { typeof (object) }));
    }

    [Test]
    [ExpectedException (typeof (ArgumentException),
        ExpectedMessage = "GenericMethodDefinition must be a generic method definition.\r\nParameter name: genericMethodDefinition")]
    public void MakeClosedGenericExecuteMethod_NonGenericMethod ()
    {
      var executeMethod = typeof (CountResultOperator).GetMethod ("ExecuteInMemory", new[] { typeof (IStreamedData) });
      _infoWithStringSequence.MakeClosedGenericExecuteMethod (executeMethod);
    }

    [Test]
    [ExpectedException (typeof (ArgumentException),
        ExpectedMessage = "GenericMethodDefinition must be a generic method definition.\r\nParameter name: genericMethodDefinition")]
    public void MakeClosedGenericExecuteMethod_NonGenericMethodDefinition ()
    {
      var executeMethod = typeof (CountResultOperator)
          .GetMethod ("ExecuteInMemory", new[] { typeof (StreamedSequence) })
          .MakeGenericMethod (typeof (string));
      _infoWithStringSequence.MakeClosedGenericExecuteMethod (executeMethod);
    }

    [Test]
    [ExpectedException (typeof (ArgumentException),
        ExpectedMessage = "GenericMethodDefinition must have exactly one generic parameter.\r\nParameter name: genericMethodDefinition")]
    public void MakeClosedGenericExecuteMethod_WrongNumberOfGenericParameters ()
    {
      var executeMethod = typeof (TestResultOperator).GetMethod ("InvalidExecuteInMemory_TooManyGenericParameters");
      _infoWithStringSequence.MakeClosedGenericExecuteMethod (executeMethod);
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
    [ExpectedException (typeof (InvalidOperationException), ExpectedMessage = "Test")]
    public void ExecuteQueryModel_WithException ()
    {
      var queryModel = ExpressionHelper.CreateQueryModel<Cook>();

      var executorMock = MockRepository.GenerateMock<IQueryExecutor>();
      executorMock.Expect (mock => mock.ExecuteCollection<string> (queryModel)).Throw (new InvalidOperationException ("Test"));

      _infoWithStringSequence.ExecuteQueryModel (queryModel, executorMock);
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