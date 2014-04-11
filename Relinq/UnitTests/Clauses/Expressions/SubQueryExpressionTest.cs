// This file is part of the re-linq project (relinq.codeplex.com)
// Copyright (c) rubicon IT GmbH, www.rubicon.eu
// 
// re-linq is free software; you can redistribute it and/or modify it under 
// the terms of the GNU Lesser General Public License as published by the 
// Free Software Foundation; either version 2.1 of the License, 
// or (at your option) any later version.
// 
// re-linq is distributed in the hope that it will be useful, 
// but WITHOUT ANY WARRANTY; without even the implied warranty of 
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the 
// GNU Lesser General Public License for more details.
// 
// You should have received a copy of the GNU Lesser General Public License
// along with re-linq; if not, see http://www.gnu.org/licenses.
// 

using System;
using System.Linq;
using System.Linq.Expressions;
using NUnit.Framework;
using Remotion.Linq.Clauses.Expressions;
using Remotion.Linq.Development.UnitTesting;
using Remotion.Linq.UnitTests.Parsing.Structure.IntermediateModel;
using Remotion.Linq.UnitTests.TestDomain;

namespace Remotion.Linq.UnitTests.Clauses.Expressions
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
      var queryModel = ExpressionHelper.CreateQueryModel<Cook>();
      var expression = new SubQueryExpression (queryModel);
      Assert.That (expression.Type, Is.SameAs (typeof(IQueryable<Cook>)));
      Assert.That (expression.QueryModel, Is.SameAs (queryModel));
      Assert.That (expression.NodeType, Is.EqualTo (SubQueryExpression.ExpressionType));
    }
  }
}