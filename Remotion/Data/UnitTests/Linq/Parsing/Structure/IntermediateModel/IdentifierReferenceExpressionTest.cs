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
using Remotion.Data.Linq.Parsing.Structure;
using Remotion.Data.Linq.Parsing.Structure.IntermediateModel;

namespace Remotion.Data.UnitTests.Linq.Parsing.Structure.IntermediateModel
{
  [TestFixture]
  public class IdentifierReferenceExpressionTest : ExpressionNodeTestBase
  {
    [Test]
    public void Initialization ()
    {
      var node = ExpressionNodeObjectMother.CreateSelectMany (SourceNode);
      //var node = new ConstantExpressionNode ("x", typeof (int[]), new[] { 1, 2, 3 });
      var querySourceClauseMapping = new QuerySourceClauseMapping();
      var fromClause = ExpressionHelper.CreateMainFromClause();
      querySourceClauseMapping.AddMapping (node, fromClause);

      //var identifierReferenceExpression = new QuerySourceReferenceExpression (fromClause);
      var identifierReferenceExpression = new QuerySourceReferenceExpression (node);
      Assert.That (identifierReferenceExpression.Type, Is.EqualTo (node.QuerySourceElementType));
    }
  }
}