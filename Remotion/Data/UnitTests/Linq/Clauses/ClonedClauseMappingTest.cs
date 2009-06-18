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
using System.Collections.Generic;
using NUnit.Framework;
using NUnit.Framework.SyntaxHelpers;
using Remotion.Data.Linq.Clauses;

namespace Remotion.Data.UnitTests.Linq.Clauses
{
  [TestFixture]
  public class ClonedClauseMappingTest
  {
    private ClonedClauseMapping _mapping;
    private MainFromClause _clause1;
    private MainFromClause _clause2;

    [SetUp]
    public void SetUp ()
    {
      _mapping = new ClonedClauseMapping ();
      _clause1 = ExpressionHelper.CreateMainFromClause ();
      _clause2 = ExpressionHelper.CreateMainFromClause ();
    }

    [Test]
    public void AddMapping ()
    {
      _mapping.AddMapping (_clause1, _clause2);
      Assert.That (_mapping.GetClause (_clause1), Is.SameAs (_clause2));
    }

    [Test]
    [ExpectedException (typeof (InvalidOperationException), ExpectedMessage = "Clause has already been associated with a new clause.")]
    public void AddMapping_Twice ()
    {
      _mapping.AddMapping (_clause1, _clause2);
      _mapping.AddMapping (_clause1, _clause2);
    }

    [Test]
    [ExpectedException (typeof (KeyNotFoundException), ExpectedMessage = "Clause has not been associated with a new clause.")]
    public void GetClause_WithoutAssociatedClause ()
    {
      _mapping.GetClause (_clause1);
    }
  }
}