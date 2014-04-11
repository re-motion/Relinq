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
