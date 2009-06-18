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
using Remotion.Data.Linq.Clauses.ResultModifications;

namespace Remotion.Data.UnitTests.Linq.Clauses.ResultModifications
{
  [TestFixture]
  public class MaxResultModificationTest
  {
    private MaxResultModification _resultModification;

    [SetUp]
    public void SetUp ()
    {
      _resultModification = new MaxResultModification (ExpressionHelper.CreateSelectClause ());
    }

    [Test]
    public void Clone ()
    {
      var newSelectClause = ExpressionHelper.CreateSelectClause ();

      var clone = _resultModification.Clone (newSelectClause, new ClonedClauseMapping ());

      Assert.That (clone, Is.InstanceOfType (typeof (MaxResultModification)));
      Assert.That (clone.SelectClause, Is.SameAs (newSelectClause));
    }

    [Test]
    public void ExecuteInMemory ()
    {
      var items = new[] { 1, 2, 3, 0, 2 };
      var resultModification = new MaxResultModification (ExpressionHelper.CreateSelectClause ());

      var result = resultModification.ExecuteInMemory (items);

      Assert.That (result, Is.EqualTo (new[] { 3 }));
    }

    [Test]
    public void ExecutionStrategy ()
    {
      Assert.That (_resultModification.ExecutionStrategy, Is.SameAs (ScalarExecutionStrategy.Instance));
    }
  }
}