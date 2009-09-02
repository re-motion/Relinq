// This file is part of the re-motion Core Framework (www.re-motion.org)
// Copyright (C) 2005-2009 rubicon informationstechnologie gmbh, www.rubicon.eu
// 
// The re-motion Core Framework is free software; you can redistribute it 
// and/or modify it under the terms of the GNU Lesser General Public License 
// version 3.0 as published by the Free Software Foundation.
// 
// re-motion is distributed in the hope that it will be useful, 
// but WITHOUT ANY WARRANTY; without even the implied warranty of 
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the 
// GNU Lesser General Public License for more details.
// 
// You should have received a copy of the GNU Lesser General Public License
// along with re-motion; if not, see http://www.gnu.org/licenses.
// 
using System;
using System.Linq;
using System.Linq.Expressions;
using NUnit.Framework;
using NUnit.Framework.SyntaxHelpers;
using Remotion.Data.Linq;
using Remotion.Data.Linq.Clauses.ResultOperators;
using Remotion.Data.Linq.Clauses.StreamedData;
using Remotion.Data.UnitTests.Linq.Clauses.ResultOperators;
using Remotion.Utilities;
using Rhino.Mocks;

namespace Remotion.Data.UnitTests.Linq.Clauses.StreamedData
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
      _infoWithIntSequence = new StreamedSequenceInfo(typeof (int[]), _intExpression);
    }

    [Test]
    [ExpectedException (typeof (ArgumentTypeException))]
    public void Initialization_CurrentSequence_WrongItemExpression ()
    {
      new StreamedSequenceInfo (typeof (string[]), _intExpression);
    }

    [Test]
    public void DataType ()
    {
      Assert.That (_infoWithIntSequence.DataType, Is.SameAs (typeof (int[])));
    }

    [Test]
    public void MakeClosedGenericExecuteMethod ()
    {
      var executeMethod = typeof (CountResultOperator).GetMethod ("ExecuteInMemory", new[] { typeof (StreamedSequence) });
      var result = _infoWithIntSequence.MakeClosedGenericExecuteMethod (executeMethod);

      Assert.That (result.GetGenericArguments (), Is.EqualTo (new[] { typeof (int) }));
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
      var queryModel = ExpressionHelper.CreateQueryModel_Student ();

      var executorMock = MockRepository.GenerateMock<IQueryExecutor> ();
      executorMock.Expect (mock => mock.ExecuteCollection<int> (queryModel)).Return (new[] { 1, 2, 3 });

      var streamedData = (StreamedSequence) _infoWithIntSequence.ExecuteQueryModel (queryModel, executorMock);

      executorMock.VerifyAllExpectations ();

      Assert.That (streamedData, Is.InstanceOfType (typeof (StreamedSequence)));
      Assert.That (streamedData.DataInfo.ItemExpression, Is.SameAs (_infoWithIntSequence.ItemExpression));
      Assert.That (typeof (IQueryable<int>).IsAssignableFrom (streamedData.DataInfo.DataType), Is.True);
      Assert.That (streamedData.GetTypedSequence<int>().ToArray(), Is.EqualTo (new[] { 1, 2, 3 }));
      Assert.That (streamedData.Sequence, Is.InstanceOfType (typeof (IQueryable<int>)));
    }

    [Test]
    [ExpectedException (typeof (InvalidOperationException), ExpectedMessage = "Test")]
    public void ExecuteQueryModel_WithException ()
    {
      var queryModel = ExpressionHelper.CreateQueryModel_Student ();

      var executorMock = MockRepository.GenerateMock<IQueryExecutor> ();
      executorMock.Expect (mock => mock.ExecuteCollection<int> (queryModel)).Throw (new InvalidOperationException ("Test"));

      _infoWithIntSequence.ExecuteQueryModel (queryModel, executorMock);
    }
  }
}