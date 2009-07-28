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
using Remotion.Data.Linq.Clauses;
using Remotion.Data.Linq.Clauses.ExecutionStrategies;
using Remotion.Data.Linq.Clauses.ResultOperators;
using Remotion.Data.Linq.Clauses.StreamedData;
using Remotion.Data.UnitTests.Linq.TestDomain;
using Remotion.Utilities;

namespace Remotion.Data.UnitTests.Linq.Clauses.ResultOperators
{
  [TestFixture]
  public class CountResultOperatorTest
  {
    private CountResultOperator _resultOperator;

    [SetUp]
    public void SetUp ()
    {
      _resultOperator = new CountResultOperator ();
    }

    [Test]
    public void Clone ()
    {
      var clonedClauseMapping = new QuerySourceMapping ();
      var cloneContext = new CloneContext (clonedClauseMapping);
      var clone = _resultOperator.Clone (cloneContext);

      Assert.That (clone, Is.InstanceOfType (typeof (CountResultOperator)));
    }

    [Test]
    public void ExecuteInMemory ()
    {
      var input = new StreamedSequence (new[] { 1, 2, 3 }, Expression.Constant (0));
      var result = _resultOperator.ExecuteInMemory<int> (input);

      Assert.That (result.Value, Is.EqualTo (3));
    }

    [Test]
    public void ExecutionStrategy ()
    {
      Assert.That (_resultOperator.ExecutionStrategy, Is.SameAs (ScalarExecutionStrategy.Instance));
    }

    [Test]
    public void GetOutputDataInfo ()
    {
      var studentExpression = Expression.Constant (new Student ());
      var input = new StreamedSequenceInfo (typeof (Student[]), studentExpression);
      var result = _resultOperator.GetOutputDataInfo (input);

      Assert.That (result, Is.InstanceOfType (typeof (StreamedValueInfo)));
      Assert.That (result.DataType, Is.SameAs (typeof (int)));
    }

    [Test]
    [ExpectedException (typeof (ArgumentTypeException))]
    public void GetOutputDataInfo_InvalidInput ()
    {
      var input = new StreamedValueInfo (typeof (Student));
      _resultOperator.GetOutputDataInfo (input);
    }
  }
}