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
using Remotion.Data.Linq.Parsing.Structure;
using Remotion.Data.Linq.Parsing.Structure.IntermediateModel;

namespace Remotion.Data.UnitTests.Linq.Parsing.Structure
{
  [TestFixture]
  public class QuerySourceClauseMappingTest
  {
    private QuerySourceClauseMapping _mapping;
    private ConstantExpressionNode _node;
    private MainFromClause _clause;

    [SetUp]
    public void SetUp ()
    {
      _node = new ConstantExpressionNode ("x", typeof (int[]), new[] { 1, 2, 3 });
      _mapping = new QuerySourceClauseMapping ();
      _clause = ExpressionHelper.CreateMainFromClause ();
    }

    [Test]
    public void AddMapping ()
    {
      _mapping.AddMapping (_node, _clause);

      Assert.That (_mapping.GetFromClause (_node), Is.Not.Null);
    }

    [Test]
    public void AddMapping_IncreasesCount ()
    {
      _mapping.AddMapping (_node, _clause);

      Assert.That (_mapping.Count, Is.EqualTo (1));
    }

    [Test]
    [ExpectedException (typeof (InvalidOperationException), ExpectedMessage = "Node already has an associated clause.")]
    public void AddMappingTwice ()
    {
      _mapping.AddMapping (_node, _clause);
      _mapping.AddMapping (_node, _clause);
    }

    [Test]
    public void GetFromClause ()
    {
      _mapping.AddMapping (_node, _clause);

      Assert.That (_mapping.GetFromClause (_node), Is.SameAs (_clause));
    }

    [Test]
    [ExpectedException (typeof (KeyNotFoundException), ExpectedMessage = "Node has no associated clause.")]
    public void GetFromClause_ThrowsException ()
    {
      _mapping.GetFromClause (_node);
    }
    
  }
}