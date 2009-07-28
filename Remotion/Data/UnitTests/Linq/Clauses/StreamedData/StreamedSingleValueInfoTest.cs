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
using NUnit.Framework;
using NUnit.Framework.SyntaxHelpers;
using Remotion.Data.Linq;
using Remotion.Data.Linq.Clauses.StreamedData;
using Remotion.Data.Linq.EagerFetching;
using Remotion.Data.UnitTests.Linq.TestDomain;
using Rhino.Mocks;

namespace Remotion.Data.UnitTests.Linq.Clauses.StreamedData
{
  [TestFixture]
  public class StreamedSingleValueInfoTest
  {
    //private StreamedSingleValueInfo _streamedScalarValueInfo;

    //[SetUp]
    //public void SetUp ()
    //{
    //  _streamedScalarValueInfo = new StreamedSingleValueInfo (typeof (Student));
    //}

    //[Test]
    //public void ExecuteQueryModel ()
    //{
    //  var queryModel = ExpressionHelper.CreateQueryModel ();
    //  var fetchRequests = new FetchRequestBase[0];

    //  var executorMock = MockRepository.GenerateMock<IQueryExecutor> ();
    //  executorMock.Expect (mock => mock.ExecuteSingle<Student> (queryModel, fetchRequests)).Return (1);

    //  var streamedData = _streamedScalarValueInfo.ExecuteQueryModel (queryModel, fetchRequests, executorMock);
    //  Assert.That (streamedData, Is.InstanceOfType (typeof (StreamedValue)));
    //  Assert.That (streamedData.DataInfo, Is.SameAs (_streamedScalarValueInfo));
    //}
  }
}