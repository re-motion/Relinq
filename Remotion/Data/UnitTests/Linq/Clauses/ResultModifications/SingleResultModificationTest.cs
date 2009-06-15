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
using Remotion.Data.Linq.Clauses.ExecutionStrategies;
using Remotion.Data.Linq.Clauses.ResultModifications;

namespace Remotion.Data.UnitTests.Linq.Clauses.ResultModifications
{
  [TestFixture]
  public class SingleResultModificationTest
  {
    private SingleResultModification _resultModificationNoDefault;
    private SingleResultModification _resultModificationWithDefault;

    [SetUp]
    public void SetUp ()
    {
      _resultModificationNoDefault = new SingleResultModification (ExpressionHelper.CreateSelectClause (), false);
      _resultModificationWithDefault = new SingleResultModification (ExpressionHelper.CreateSelectClause (), true);
    }

    [Test]
    public void Clone ()
    {
      var newSelectClause = ExpressionHelper.CreateSelectClause ();

      var clone = _resultModificationWithDefault.Clone (newSelectClause);

      Assert.That (clone, Is.InstanceOfType (typeof (SingleResultModification)));
      Assert.That (clone.SelectClause, Is.SameAs (newSelectClause));
      Assert.That (((SingleResultModification) clone).ReturnDefaultWhenEmpty, Is.True);
    }

    [Test]
    public void Clone_ReturnDefaultIfEmpty_False ()
    {
      var newSelectClause = ExpressionHelper.CreateSelectClause ();
      var clone = _resultModificationNoDefault.Clone (newSelectClause);

      Assert.That (clone, Is.InstanceOfType (typeof (SingleResultModification)));
      Assert.That (clone.SelectClause, Is.SameAs (newSelectClause));
      Assert.That (((SingleResultModification) clone).ReturnDefaultWhenEmpty, Is.False);
    }

     [Test]
     public void ExecuteInMemory ()
     {
       var items = new[] { 1 };
       var result = _resultModificationWithDefault.ExecuteInMemory (items);

       Assert.That (result, Is.EqualTo (new[] { 1 }));
     }

     [Test]
     public void ExecuteInMemory_Empty_Default ()
     {
       var items = new int[0];
       var result = _resultModificationWithDefault.ExecuteInMemory (items);

       Assert.That (result, Is.EqualTo (new[] { 0 }));
     }

     [Test]
     [ExpectedException (typeof (InvalidOperationException), ExpectedMessage = "Sequence contains no elements")]
     public void ExecuteInMemory_Empty_NoDefault ()
     {
       var items = new int[0];
       _resultModificationNoDefault.ExecuteInMemory (items);
     }

     [Test]
     [ExpectedException (typeof (InvalidOperationException), ExpectedMessage = "Sequence contains more than one element")]
     public void ExecuteInMemory_TooManyItems ()
     {
       var items = new[] { 1, 2 };
       _resultModificationWithDefault.ExecuteInMemory (items);
     }

     [Test]
     public void ExecutionStrategy_Default ()
     {
       Assert.That (_resultModificationWithDefault.ExecutionStrategy, Is.SameAs (SingleExecutionStrategy.InstanceWithDefaultWhenEmpty));
     }

     [Test]
     public void ExecutionStrategy_NoDefault ()
     {
       Assert.That (_resultModificationNoDefault.ExecutionStrategy, Is.SameAs (SingleExecutionStrategy.InstanceNoDefaultWhenEmpty));
     }
  }
}