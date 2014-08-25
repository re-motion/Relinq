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
using System.Linq;
using System.Linq.Expressions;
using NUnit.Framework;
using Remotion.Linq.Clauses.StreamedData;
using Remotion.Linq.UnitTests.TestDomain;

namespace Remotion.Linq.UnitTests.Clauses.ResultOperators
{
  [TestFixture]
  public class SequenceTypePreservingResultOperatorBaseTest
  {
    private TestSequenceTypePreservingResultOperator _resultOperator;

    [SetUp]
    public void SetUp ()
    {
      _resultOperator = new TestSequenceTypePreservingResultOperator ();
    }

    [Test]
    public void GetOutputDataInfo ()
    {
      var cookExpression = Expression.Constant (new Cook ());
      var input = new StreamedSequenceInfo (typeof (object[]), cookExpression);
      var result = _resultOperator.GetOutputDataInfo (input);

      Assert.That (result, Is.InstanceOf (typeof (StreamedSequenceInfo)));
      Assert.That (result, Is.Not.SameAs (input));
      Assert.That (result.DataType, Is.SameAs (typeof (IQueryable<object>)));
      Assert.That (((StreamedSequenceInfo) result).ItemExpression, Is.SameAs (cookExpression));
      Assert.That (((StreamedSequenceInfo) result).ResultItemType, Is.SameAs (typeof (object)));
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
  }
}
