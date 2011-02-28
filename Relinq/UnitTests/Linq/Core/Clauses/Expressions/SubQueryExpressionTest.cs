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
using System.Linq;
using System.Linq.Expressions;
using NUnit.Framework;
using NUnit.Framework.SyntaxHelpers;
using Remotion.Linq.UnitTests.Linq.Core.Parsing.Structure.IntermediateModel;
using Remotion.Linq.UnitTests.Linq.Core.TestDomain;
using Remotion.Linq.Clauses.Expressions;

namespace Remotion.Linq.UnitTests.Linq.Core.Clauses.Expressions
{
  [TestFixture]
  public class SubQueryExpressionTest : ExpressionNodeTestBase
  {
    [Test]
    public void NodeType ()
    {
      Assert.That (SubQueryExpression.ExpressionType, Is.EqualTo ((ExpressionType) 100002));
      ExtensionExpressionTestHelper.CheckUniqueNodeType (typeof (SubQueryExpression), SubQueryExpression.ExpressionType);
    }

    [Test]
    public void Initialization ()
    {
      var queryModel = ExpressionHelper.CreateQueryModel_Cook();
      var expression = new SubQueryExpression (queryModel);
      Assert.That (expression.Type, Is.SameAs (typeof(IQueryable<Cook>)));
      Assert.That (expression.QueryModel, Is.SameAs (queryModel));
      Assert.That (expression.NodeType, Is.EqualTo (SubQueryExpression.ExpressionType));
    }
  }
}