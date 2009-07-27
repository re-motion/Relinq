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
using Remotion.Utilities;

namespace Remotion.Data.UnitTests.Linq.Clauses.ResultOperators
{
  [TestFixture]
  public class AverageResultOperatorTest
  {
    private AverageResultOperator _resultOperator;

    [SetUp]
    public void SetUp ()
    {
      _resultOperator = new AverageResultOperator();
    }

    [Test]
    public void ExecutionStrategy ()
    {
      Assert.That (_resultOperator.ExecutionStrategy, Is.SameAs (ScalarExecutionStrategy.Instance));
    }

    [Test]
    public void Clone ()
    {
      var clonedClauseMapping = new QuerySourceMapping ();
      var cloneContext = new CloneContext (clonedClauseMapping);
      var clone = _resultOperator.Clone (cloneContext);

      Assert.That (clone, Is.InstanceOfType (typeof (AverageResultOperator)));
    }

    [Test]
    public void ExecuteInMemory ()
    {
      IStreamedData input = new StreamedSequence (new[] { 1, 2, 3 }, Expression.Constant (0));
      var result = _resultOperator.ExecuteInMemory (input);

      Assert.That (result.GetCurrentSingleValue<double>(), Is.EqualTo (2.0));
    }

    [Test]
    [ExpectedException (typeof (NotSupportedException), ExpectedMessage = "Cannot calculate the average of objects of type 'System.String' in memory.")]
    public void ExecuteInMemory_UnsupportedType ()
    {
      IStreamedData input = new StreamedSequence (new[] { "1", "2", "3" }, Expression.Constant ("0"));
      _resultOperator.ExecuteInMemory (input);
    }

    [Test]
    public void GetResultType ()
    {
      Assert.That (_resultOperator.GetResultType (typeof (IQueryable<int>)), Is.SameAs (typeof (int)));
    }

    [Test]
    [ExpectedException (typeof (ArgumentTypeException))]
    public void GetResultType_InvalidType ()
    {
      _resultOperator.GetResultType (typeof (int));
    }
  }
}