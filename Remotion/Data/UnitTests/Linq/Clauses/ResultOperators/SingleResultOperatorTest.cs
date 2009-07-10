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
using Remotion.Data.Linq.Clauses;
using Remotion.Data.Linq.Clauses.ExecutionStrategies;
using Remotion.Data.Linq.Clauses.ResultOperators;

namespace Remotion.Data.UnitTests.Linq.Clauses.ResultOperators
{
  [TestFixture]
  public class SingleResultOperatorTest
  {
    private SingleResultOperator _resultOperatorNoDefault;
    private SingleResultOperator _resultOperatorWithDefault;
    private ClauseMapping _clauseMapping;
    private CloneContext _cloneContext;

    [SetUp]
    public void SetUp ()
    {
      _resultOperatorNoDefault = new SingleResultOperator (false);
      _resultOperatorWithDefault = new SingleResultOperator (true);
      _clauseMapping = new ClauseMapping ();
      _cloneContext = new CloneContext (_clauseMapping);
    }

    [Test]
    public void Clone ()
    {
      var clone = _resultOperatorWithDefault.Clone (_cloneContext);

      Assert.That (clone, Is.InstanceOfType (typeof (SingleResultOperator)));
      Assert.That (((SingleResultOperator) clone).ReturnDefaultWhenEmpty, Is.True);
    }

    [Test]
    public void Clone_ReturnDefaultIfEmpty_False ()
    {
      var clone = _resultOperatorNoDefault.Clone (_cloneContext);

      Assert.That (clone, Is.InstanceOfType (typeof (SingleResultOperator)));
      Assert.That (((SingleResultOperator) clone).ReturnDefaultWhenEmpty, Is.False);
    }

     [Test]
     public void ExecuteInMemory ()
     {
       var items = new[] { 1 };
       var result = _resultOperatorWithDefault.ExecuteInMemory (items);

       Assert.That (result, Is.EqualTo (1));
     }

     [Test]
     public void ExecuteInMemory_Empty_Default ()
     {
       var items = new int[0];
       var result = _resultOperatorWithDefault.ExecuteInMemory (items);

       Assert.That (result, Is.EqualTo (0));
     }

     [Test]
     [ExpectedException (typeof (InvalidOperationException), ExpectedMessage = "Sequence contains no elements")]
     public void ExecuteInMemory_Empty_NoDefault ()
     {
       var items = new int[0];
       _resultOperatorNoDefault.ExecuteInMemory (items);
     }

     [Test]
     [ExpectedException (typeof (InvalidOperationException), ExpectedMessage = "Sequence contains more than one element")]
     public void ExecuteInMemory_TooManyItems ()
     {
       var items = new[] { 1, 2 };
       _resultOperatorWithDefault.ExecuteInMemory (items);
     }

     [Test]
     public void ExecutionStrategy_Default ()
     {
       Assert.That (_resultOperatorWithDefault.ExecutionStrategy, Is.SameAs (SingleExecutionStrategy.InstanceWithDefaultWhenEmpty));
     }

     [Test]
     public void ExecutionStrategy_NoDefault ()
     {
       Assert.That (_resultOperatorNoDefault.ExecutionStrategy, Is.SameAs (SingleExecutionStrategy.InstanceNoDefaultWhenEmpty));
     }
  }
}