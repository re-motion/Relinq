// This file is part of the re-linq project (relinq.codeplex.com)
// Copyright (C) 2005-2009 rubicon informationstechnologie gmbh, www.rubicon.eu
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
using Remotion.Linq.Clauses;
using Remotion.Linq.Clauses.ResultOperators;
using Remotion.Linq.Clauses.StreamedData;
using Remotion.Linq.Utilities;

namespace Remotion.Linq.UnitTests.Linq.Core.Clauses.ResultOperators
{
  [TestFixture]
  public class AverageResultOperatorTest
  {
    private AverageResultOperator _resultOperator;

    [SetUp]
    public void SetUp ()
    {
      _resultOperator = new AverageResultOperator ();
    }

    [Test]
    public void Clone ()
    {
      var clonedClauseMapping = new QuerySourceMapping ();
      var cloneContext = new CloneContext (clonedClauseMapping);
      var clone = _resultOperator.Clone (cloneContext);

      Assert.That (clone, Is.InstanceOf (typeof (AverageResultOperator)));
    }

    [Test]
    public void ExecuteInMemory ()
    {
      var input = new StreamedSequence (new[] { 1, 2, 3 }, new StreamedSequenceInfo (typeof (int[]), Expression.Constant (0)));
      var result = _resultOperator.ExecuteInMemory<int> (input);

      Assert.That (result.Value, Is.InstanceOf (typeof (double)));
      Assert.That (result.Value, Is.EqualTo (2.0));
    }

    [Test]
    [ExpectedException (typeof (NotSupportedException), ExpectedMessage = "Cannot calculate the average of objects of type 'System.String' in memory.")]
    public void ExecuteInMemory_UnsupportedType ()
    {
      var input = new StreamedSequence (new[] { "1", "2", "3" }, new StreamedSequenceInfo (typeof (string[]), Expression.Constant ("0")));
      _resultOperator.ExecuteInMemory<string> (input);
    }

    [Test]
    public void GetOutputDataInfo_OutputTypeEqualsItemTypeByDefault ()
    {
      var studentExpression = Expression.Constant (new Cook ());
      var input = new StreamedSequenceInfo (typeof (Cook[]), studentExpression);
      var result = _resultOperator.GetOutputDataInfo (input);

      Assert.That (result, Is.InstanceOf (typeof (StreamedValueInfo)));
      Assert.That (result.DataType, Is.SameAs (typeof (Cook)));
    }

    [Test]
    public void GetOutputDataInfo_IntegerTypesGoToDouble ()
    {
      CheckOutputDataInfo (typeof (int), typeof (double));
      CheckOutputDataInfo (typeof (int?), typeof (double?));
      CheckOutputDataInfo (typeof (long), typeof (double));
      CheckOutputDataInfo (typeof (long?), typeof (double?));
    }

    [Test]
    [ExpectedException (typeof (ArgumentTypeException))]
    public void GetOutputDataInfo_InvalidInput ()
    {
      var input = new StreamedScalarValueInfo (typeof (Cook));
      _resultOperator.GetOutputDataInfo (input);
    }

    private void CheckOutputDataInfo (Type itemType, Type expectedOutputType)
    {
      var itemExpression = Expression.Constant (Activator.CreateInstance (itemType), itemType);
      var inputInfo = new StreamedSequenceInfo (itemType.MakeArrayType(), itemExpression);
      var outputInfo = _resultOperator.GetOutputDataInfo (inputInfo);

      Assert.That (outputInfo, Is.InstanceOf (typeof (StreamedValueInfo)));
      Assert.That (outputInfo.DataType, Is.SameAs (expectedOutputType));
    }
  }
}
