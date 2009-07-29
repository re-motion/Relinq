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
using System.Linq.Expressions;
using NUnit.Framework;
using NUnit.Framework.SyntaxHelpers;
using Remotion.Data.Linq.Clauses;
using Remotion.Data.Linq.Clauses.ResultOperators;
using Remotion.Data.Linq.Clauses.StreamedData;
using Remotion.Data.UnitTests.Linq.TestDomain;
using Remotion.Utilities;

namespace Remotion.Data.UnitTests.Linq.Clauses.ResultOperators
{
  [TestFixture]
  public class AverageResultOperatorTest
  {
    private AverageResultOperator _resultOperatorWithDateTimeResult;
    private AverageResultOperator _resultOperatorWithDoubleResult;

    [SetUp]
    public void SetUp ()
    {
      _resultOperatorWithDateTimeResult = new AverageResultOperator (typeof (DateTime));
      _resultOperatorWithDoubleResult = new AverageResultOperator (typeof (double));
    }

    [Test]
    public void Clone ()
    {
      var clonedClauseMapping = new QuerySourceMapping ();
      var cloneContext = new CloneContext (clonedClauseMapping);
      var clone = _resultOperatorWithDateTimeResult.Clone (cloneContext);

      Assert.That (clone, Is.InstanceOfType (typeof (AverageResultOperator)));
      Assert.That (((AverageResultOperator) clone).ResultType, Is.SameAs (typeof (DateTime)));
    }

    [Test]
    public void ExecuteInMemory ()
    {
      var input = new StreamedSequence (new[] { 1, 2, 3 }, new StreamedSequenceInfo (typeof (int[]), Expression.Constant (0)));
      var result = _resultOperatorWithDoubleResult.ExecuteInMemory<int> (input);

      Assert.That (result.Value, Is.EqualTo (2.0));
    }

    [Test]
    [ExpectedException (typeof (NotSupportedException), ExpectedMessage = "Cannot calculate the average of objects of type 'System.String' in memory.")]
    public void ExecuteInMemory_UnsupportedType ()
    {
      var input = new StreamedSequence (new[] { "1", "2", "3" }, new StreamedSequenceInfo (typeof (string[]), Expression.Constant ("0")));
      _resultOperatorWithDoubleResult.ExecuteInMemory<string> (input);
    }

    [Test]
    [ExpectedException (typeof (NotSupportedException), ExpectedMessage = "Cannot calculate the average of items of type 'System.Int32' in memory so "
        + "that a value of type 'System.DateTime' is returned. Instead, a value of type 'System.Double' would be returned. This does not match the "
        + "ResultType of the AverageResultOperator.")]
    public void ExecuteInMemory_EnumerableMethodDoesntMatchResultType ()
    {
      var input = new StreamedSequence (new[] { 1, 2, 3 }, new StreamedSequenceInfo (typeof (int[]), Expression.Constant (0)));
      _resultOperatorWithDateTimeResult.ExecuteInMemory<int> (input);
    }

    [Test]
    public void GetOutputDataInfo ()
    {
      var studentExpression = Expression.Constant (new Student ());
      var input = new StreamedSequenceInfo (typeof (Student[]), studentExpression);
      var result = _resultOperatorWithDateTimeResult.GetOutputDataInfo (input);

      Assert.That (result, Is.InstanceOfType (typeof (StreamedValueInfo)));
      Assert.That (result.DataType, Is.SameAs (typeof (DateTime)));
    }

    [Test]
    [ExpectedException (typeof (ArgumentTypeException))]
    public void GetOutputDataInfo_InvalidInput ()
    {
      var input = new StreamedScalarValueInfo (typeof (Student));
      _resultOperatorWithDateTimeResult.GetOutputDataInfo (input);
    }
  }
}