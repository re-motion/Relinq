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
using Remotion.Data.Linq.Clauses.ResultModifications;
using Remotion.Data.Linq.Parsing.Structure.IntermediateModel;
using System.Linq;

namespace Remotion.Data.UnitTests.Linq.Parsing.Structure.IntermediateModel
{
  [TestFixture]
  public class ResultModificationExpressionNodeRegistryTest : ExpressionNodeTestBase
  {
    private ResultModificationExpressionNodeRegistry _registry;
    private CountExpressionNode _node;

    [SetUp]
    public override void SetUp ()
    {
      base.SetUp();
      _registry = new ResultModificationExpressionNodeRegistry ();
      _node = new CountExpressionNode (CreateParseInfo (), null);
    }

    [Test]
    public void AddResultModificationNode ()
    {
      _registry.AddResultModificationNode (_node);
      Assert.That (_registry.ToArray(), Is.EqualTo (new object[] { _node }));
    }

    [Test]
    public void ApplyAllToSelectClause ()
    {
      var node2 = new TakeExpressionNode (CreateParseInfo(), 2);
      _registry.AddResultModificationNode (node2);
      _registry.AddResultModificationNode (_node);

      var queryModel = ExpressionHelper.CreateQueryModel();

      _registry.ApplyAll (queryModel, ClauseGenerationContext);
      
      var selectClause = (SelectClause) queryModel.SelectOrGroupClause;

      Assert.That (selectClause.ResultModifications.Count, Is.EqualTo (2));
      Assert.That (selectClause.ResultModifications[0], Is.InstanceOfType (typeof (TakeResultModification)));
      Assert.That (selectClause.ResultModifications[1], Is.InstanceOfType (typeof (CountResultModification)));
    }
  }
}