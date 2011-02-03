// This file is part of the re-motion Core Framework (www.re-motion.org)
// Copyright (C) 2005-2009 rubicon informationstechnologie gmbh, www.rubicon.eu
// 
// The re-motion Core Framework is free software; you can redistribute it 
// and/or modify it under the terms of the GNU Lesser General Public License 
// as published by the Free Software Foundation; either version 2.1 of the 
// License, or (at your option) any later version.
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
using Remotion.Data.Linq.Parsing.Structure.NodeTypeProviders;

namespace Remotion.Data.Linq.UnitTests.Linq.Core.Parsing.Structure.IntermediateModel
{
  [TestFixture]
  public class ClauseGenerationContextTest
  {
    private ClauseGenerationContext _context;
    private MainSourceExpressionNode _node;
    private MainFromClause _clause;

    [SetUp]
    public void SetUp ()
    {
      _node = ExpressionNodeObjectMother.CreateMainSource ();
      _context = new ClauseGenerationContext (new MethodInfoBasedNodeTypeRegistry ());
      _clause = ExpressionHelper.CreateMainFromClause_Int ();
    }

    [Test]
    public void AddContextInfo ()
    {
      _context.AddContextInfo (_node, _clause);

      Assert.That (_context.GetContextInfo (_node), Is.Not.Null);
    }

    [Test]
    public void AddContextInfo_IncreasesCount ()
    {
      _context.AddContextInfo (_node, _clause);

      Assert.That (_context.Count, Is.EqualTo (1));
    }

    [Test]
    [ExpectedException (typeof (InvalidOperationException), ExpectedMessage = "Node already has associated context info.")]
    public void AddContextInfoTwice ()
    {
      _context.AddContextInfo (_node, _clause);
      _context.AddContextInfo (_node, _clause);
    }

    [Test]
    public void GetContextInfo ()
    {
      _context.AddContextInfo (_node, _clause);

      Assert.That (_context.GetContextInfo (_node), Is.SameAs (_clause));
    }

    [Test]
    [ExpectedException (typeof (KeyNotFoundException), ExpectedMessage = "Node has no associated context info.")]
    public void GetContextInfo_ThrowsException ()
    {
      _context.GetContextInfo (_node);
    }
    
  }
}
