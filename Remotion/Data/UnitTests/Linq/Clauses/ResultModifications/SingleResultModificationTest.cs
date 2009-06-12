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
using Remotion.Data.Linq.Clauses.ResultModifications;

namespace Remotion.Data.UnitTests.Linq.Clauses.ResultModifications
{
  [TestFixture]
  public class SingleResultModificationTest
  {
    [Test]
    public void Clone ()
    {
      var selectClause = ExpressionHelper.CreateSelectClause ();
      var newSelectClause = ExpressionHelper.CreateSelectClause ();

      var resultModification = new SingleResultModification (selectClause, true);
      var clone = resultModification.Clone (newSelectClause);

      Assert.That (clone, Is.InstanceOfType (typeof (SingleResultModification)));
      Assert.That (clone.SelectClause, Is.SameAs (newSelectClause));
      Assert.That (((SingleResultModification) clone).ReturnDefaultWhenEmpty, Is.True);
    }

     [Test]
    public void Clone_ReturnDefaultIfEmpty_False ()
    {
      var selectClause = ExpressionHelper.CreateSelectClause ();
      var newSelectClause = ExpressionHelper.CreateSelectClause ();

      var resultModification = new SingleResultModification (selectClause, false);
      var clone = resultModification.Clone (newSelectClause);

      Assert.That (clone, Is.InstanceOfType (typeof (SingleResultModification)));
      Assert.That (clone.SelectClause, Is.SameAs (newSelectClause));
      Assert.That (((SingleResultModification) clone).ReturnDefaultWhenEmpty, Is.False);
    }
  }
}