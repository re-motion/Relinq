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
using Remotion.Data.Linq.Parsing.Structure;
using Remotion.Data.Linq.Parsing.Structure.IntermediateModel;

namespace Remotion.Data.UnitTests.Linq.Parsing.Structure
{
  [TestFixture]
  public class QuerySourceClauseMappingTest
  {
    [Test]
    public void AddMapping ()
    {
      var node = new ConstantExpressionNode ("x", typeof (int[]), new[] { 1, 2, 3 });
      var clause = ExpressionHelper.CreateMainFromClause ();
      var mapping = new QuerySourceClauseMapping ();
      mapping.AddMapping (node, clause);

      Assert.That (mapping.GetFromClause (node), Is.Not.Null);
    }

    [Test]
    public void AddMapping_IncreasesCount ()
    {
      var node = new ConstantExpressionNode ("x", typeof (int[]), new[] { 1, 2, 3 });
      var clause = ExpressionHelper.CreateMainFromClause();
      var mapping = new QuerySourceClauseMapping();
      mapping.AddMapping (node, clause);

      Assert.That (mapping.Count, Is.EqualTo (1));
    }

    [Test]
    [ExpectedException (typeof (InvalidOperationException), ExpectedMessage = "Node already has an associated class.")]
    public void AddMappingTwice ()
    {
      var node = new ConstantExpressionNode ("x", typeof (int[]), new[] { 1, 2, 3 });
      var clause = ExpressionHelper.CreateMainFromClause();
      var mapping = new QuerySourceClauseMapping();
      mapping.AddMapping (node, clause);
      mapping.AddMapping (node, clause);
    }

    [Test]
    public void GetFromClause ()
    {
      var node = new ConstantExpressionNode ("x", typeof (int[]), new[] { 1, 2, 3 });
      var clause = ExpressionHelper.CreateMainFromClause ();
      var mapping = new QuerySourceClauseMapping ();
      mapping.AddMapping (node, clause);

      Assert.That (mapping.GetFromClause (node), Is.SameAs (clause));
    }

    [Test]
    [ExpectedException (typeof (KeyNotFoundException), ExpectedMessage = "Node has no associated clause.")]
    public void GetFromClause_ThrowsException ()
    {
      var node = new ConstantExpressionNode ("x", typeof (int[]), new[] { 1, 2, 3 });
      var clause = ExpressionHelper.CreateMainFromClause ();
      var mapping = new QuerySourceClauseMapping ();

      mapping.GetFromClause (node);
    }
    
  }
}