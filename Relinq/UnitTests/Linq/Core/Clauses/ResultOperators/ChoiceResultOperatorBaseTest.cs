// This file is part of the re-linq project (relinq.codeplex.com)
// Copyright (C) rubicon IT GmbH, www.rubicon.eu
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
using System.Linq.Expressions;
using NUnit.Framework;
using Remotion.Linq.UnitTests.Linq.Core.TestDomain;
using Remotion.Linq.Clauses.StreamedData;
using Remotion.Linq.Utilities;

namespace Remotion.Linq.UnitTests.Linq.Core.Clauses.ResultOperators
{
  [TestFixture]
  public class ChoiceResultOperatorBaseTest
  {
    private TestChoiceResultOperator _resultOperatorWithDefaultWhenEmpty;
    private TestChoiceResultOperator _resultOperatorNoDefaultWhenEmpty;

    [SetUp]
    public void SetUp ()
    {
      _resultOperatorWithDefaultWhenEmpty = new TestChoiceResultOperator (true);
      _resultOperatorNoDefaultWhenEmpty = new TestChoiceResultOperator (false);
    }

    [Test]
    public void GetOutputDataInfo ()
    {
      var studentExpression = Expression.Constant (new Cook ());
      var input = new StreamedSequenceInfo (typeof (Cook[]), studentExpression);
      var result = _resultOperatorNoDefaultWhenEmpty.GetOutputDataInfo (input);

      Assert.That (result, Is.InstanceOf (typeof (StreamedSingleValueInfo)));
      Assert.That (result.DataType, Is.SameAs (typeof (Cook)));
    }

    [Test]
    public void GetOutputDataInfo_DefaultWhenEmpty ()
    {
      var studentExpression = Expression.Constant (new Cook ());
      var input = new StreamedSequenceInfo (typeof (Cook[]), studentExpression);

      Assert.That (((StreamedSingleValueInfo) _resultOperatorWithDefaultWhenEmpty.GetOutputDataInfo (input)).ReturnDefaultWhenEmpty, Is.True);
      Assert.That (((StreamedSingleValueInfo) _resultOperatorNoDefaultWhenEmpty.GetOutputDataInfo (input)).ReturnDefaultWhenEmpty, Is.False);
    }

    [Test]
    [ExpectedException (typeof (ArgumentTypeException))]
    public void GetOutputDataInfo_InvalidInput ()
    {
      var input = new StreamedScalarValueInfo (typeof (Cook));
      _resultOperatorNoDefaultWhenEmpty.GetOutputDataInfo (input);
    }
  }
}
