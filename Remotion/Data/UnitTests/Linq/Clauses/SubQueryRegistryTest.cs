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

namespace Remotion.Data.UnitTests.Linq.Clauses
{
  [TestFixture]
  public class SubQueryRegistryTest
  {
    [Test]
    public void UpdateAllParentQueries ()
    {
      var subQuery = ExpressionHelper.CreateQueryModel ();
      var parentQuery = ExpressionHelper.CreateQueryModel ();
      
      var subQueryRegistry = new SubQueryRegistry();
      subQueryRegistry.Add (subQuery);

      subQueryRegistry.UpdateAllParentQueries (parentQuery);

      Assert.That (subQuery.ParentQuery, Is.SameAs (parentQuery));
    }

    [Test]
    public void Contains_True ()
    {
      var subQuery = ExpressionHelper.CreateQueryModel ();

      var subQueryRegistry = new SubQueryRegistry ();
      subQueryRegistry.Add (subQuery);

      Assert.That (subQueryRegistry.Contains (subQuery), Is.True);
    }

    [Test]
    public void Contains_False ()
    {
      var subQuery = ExpressionHelper.CreateQueryModel ();

      var subQueryRegistry = new SubQueryRegistry ();
      Assert.That (subQueryRegistry.Contains (subQuery), Is.False);
    }
  }
}