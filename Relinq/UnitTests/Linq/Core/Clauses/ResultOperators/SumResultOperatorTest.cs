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
using System.Linq.Expressions;
using NUnit.Framework;
using Remotion.Linq.Clauses;
using Remotion.Linq.Clauses.ResultOperators;
using Remotion.Linq.Clauses.StreamedData;
using Remotion.Linq.Utilities;

namespace Remotion.Linq.UnitTests.Linq.Core.Clauses.ResultOperators
{
  [TestFixture]
  public class SumResultOperatorTest
  {
    private SumResultOperator _resultOperator;

    [SetUp]
    public void SetUp ()
    {
      _resultOperator = new SumResultOperator ();
    }

    [Test]
    public void Clone ()
    {
      var clonedClauseMapping = new QuerySourceMapping ();
      var cloneContext = new CloneContext (clonedClauseMapping);
      var clone = _resultOperator.Clone (cloneContext);

      Assert.That (clone, Is.InstanceOf (typeof (SumResultOperator)));
    }

    [Test]
    public void ExecuteInMemory ()
    {
      var input = new StreamedSequence (new[] { 1, 2, 3 }, new StreamedSequenceInfo (typeof (int[]), Expression.Constant (0)));
      var result = _resultOperator.ExecuteInMemory<int> (input);

      Assert.That (result.Value, Is.EqualTo (6));
    }

    [Test]
    [ExpectedException (typeof (NotSupportedException), ExpectedMessage = "Cannot calculate the sum of objects of type 'System.String' in memory.")]
    public void ExecuteInMemory_UnsupportedType ()
    {
      var input = new StreamedSequence (new[] { "1", "2", "3" }, new StreamedSequenceInfo (typeof (string[]), Expression.Constant ("0")));
      _resultOperator.ExecuteInMemory<string> (input);
    }

    [Test]
    public void GetOutputDataInfo ()
    {
      var studentExpression = Expression.Constant (0.0f);
      var input = new StreamedSequenceInfo (typeof (float[]), studentExpression);
      var result = _resultOperator.GetOutputDataInfo (input);

      Assert.That (result, Is.InstanceOf (typeof (StreamedValueInfo)));
      Assert.That (result.DataType, Is.SameAs (typeof (float)));
    }

    [Test]
    public void GetOutputDataInfo_CovariantSequence ()
    {
      var studentExpression = Expression.Constant (0.0f);
      var input = new StreamedSequenceInfo (typeof (object[]), studentExpression);
      var result = _resultOperator.GetOutputDataInfo (input);

      Assert.That (result, Is.InstanceOf (typeof (StreamedValueInfo)));
      Assert.That (result.DataType, Is.SameAs (typeof (object)));
    }

    [Test]
    [ExpectedException (typeof (ArgumentTypeException))]
    public void GetOutputDataInfo_InvalidInput ()
    {
      var input = new StreamedScalarValueInfo (typeof (float));
      _resultOperator.GetOutputDataInfo (input);
    }
  }
}
