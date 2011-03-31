// This file is part of the re-motion Core Framework (www.re-motion.org)
// Copyright (C) 2005-2009 rubicon informationstechnologie gmbh, www.rubicon.eu
// 
// The re-motion Core Framework is free software; you can redistribute it 
// and/or modify it under the terms of the GNU Lesser General Public License 
// as published by the Free Software Foundation; either version 2.1 of the 
// License, or (at your option) any later version.
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
using NUnit.Framework;
using Remotion.Linq;
using Remotion.Linq.Clauses.StreamedData;
using Rhino.Mocks;

namespace Remotion.Linq.UnitTests.Linq.Core.Clauses.StreamedData
{
  [TestFixture]
  public class StreamedScalarValueInfoTest
  {
    private StreamedScalarValueInfo _streamedScalarValueInfo;

    [SetUp]
    public void SetUp ()
    {
      _streamedScalarValueInfo = new StreamedScalarValueInfo (typeof (int));
    }

    [Test]
    public void AdjustDataType ()
    {
      var result = _streamedScalarValueInfo.AdjustDataType (typeof (object));

      Assert.That (result, Is.Not.SameAs (_streamedScalarValueInfo));
      Assert.That (result, Is.TypeOf (typeof (StreamedScalarValueInfo)));
      Assert.That (result.DataType, Is.SameAs (typeof (object)));
    }
    
    [Test]
    public void ExecuteQueryModel ()
    {
      var queryModel = ExpressionHelper.CreateQueryModel_Cook ();
      var executorMock = MockRepository.GenerateMock<IQueryExecutor> ();
      executorMock.Expect (mock => mock.ExecuteScalar<int> (queryModel)).Return(1);

      var streamedData = _streamedScalarValueInfo.ExecuteQueryModel (queryModel, executorMock);

      executorMock.VerifyAllExpectations ();

      Assert.That (streamedData, Is.InstanceOf(typeof(StreamedValue)));
      Assert.That (streamedData.DataInfo, Is.SameAs (_streamedScalarValueInfo));
      Assert.That (streamedData.Value, Is.EqualTo (1));
    }

    [Test]
    [ExpectedException (typeof (InvalidOperationException), ExpectedMessage = "Test")]
    public void ExecuteQueryModel_WithException ()
    {
      var queryModel = ExpressionHelper.CreateQueryModel_Cook ();

      var executorMock = MockRepository.GenerateMock<IQueryExecutor> ();
      executorMock.Expect (mock => mock.ExecuteScalar<int> (queryModel)).Throw (new InvalidOperationException ("Test"));

      _streamedScalarValueInfo.ExecuteQueryModel (queryModel, executorMock);
    }

    [Test]
    public void Equals ()
    {
      Assert.That (new StreamedScalarValueInfo (typeof (int)).Equals (null), Is.False);
      Assert.That (new StreamedScalarValueInfo (typeof (int)).Equals (new StreamedScalarValueInfo (typeof (int))), Is.True);
      Assert.That (new StreamedScalarValueInfo (typeof (int)).Equals (new StreamedScalarValueInfo (typeof (bool))), Is.False);
    }

    [Test]
    public void GetHashCodeTest ()
    {
      Assert.That (new StreamedScalarValueInfo (typeof (int)).GetHashCode (), Is.EqualTo (new StreamedScalarValueInfo (typeof (int)).GetHashCode ()));
      Assert.That (new StreamedScalarValueInfo (typeof (int)).GetHashCode (), Is.Not.EqualTo (new StreamedScalarValueInfo (typeof (string)).GetHashCode ()));
    }
  }
}
