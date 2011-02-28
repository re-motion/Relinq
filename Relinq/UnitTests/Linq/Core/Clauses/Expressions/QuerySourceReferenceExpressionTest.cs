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
using System.Linq.Expressions;
using NUnit.Framework;
using NUnit.Framework.SyntaxHelpers;
using Remotion.Data.Linq.Clauses.Expressions;
using Remotion.Data.Linq.UnitTests.Linq.Core.Parsing.Structure.IntermediateModel;

namespace Remotion.Data.Linq.UnitTests.Linq.Core.Clauses.Expressions
{
  [TestFixture]
  public class QuerySourceReferenceExpressionTest : ExpressionNodeTestBase
  {
    [Test]
    public void NodeType ()
    {
      Assert.That (QuerySourceReferenceExpression.ExpressionType, Is.EqualTo ((ExpressionType) 100001));
      ExtensionExpressionTestHelper.CheckUniqueNodeType (typeof (QuerySourceReferenceExpression), QuerySourceReferenceExpression.ExpressionType);
    }

    [Test]
    public void Initialization ()
    {
      var referenceExpression = new QuerySourceReferenceExpression (SourceClause);
      Assert.That (referenceExpression.Type, Is.SameAs (typeof (int)));
      Assert.That (referenceExpression.NodeType, Is.EqualTo (QuerySourceReferenceExpression.ExpressionType));
    }

    [Test]
    public void Equals_True ()
    {
      var referenceExpression1 = new QuerySourceReferenceExpression (SourceClause);
      var referenceExpression2 = new QuerySourceReferenceExpression (SourceClause);
      Assert.That (referenceExpression1, Is.EqualTo (referenceExpression2));
    }

    [Test]
    public void Equals_False ()
    {
      var referenceExpression1 = new QuerySourceReferenceExpression (SourceClause);
      var referenceExpression2 = new QuerySourceReferenceExpression (ExpressionHelper.CreateMainFromClause_Int());
      Assert.That (referenceExpression1, Is.Not.EqualTo (referenceExpression2));
    }

    [Test]
    public void GetHashCode_EqualObjects ()
    {
      var referenceExpression1 = new QuerySourceReferenceExpression (SourceClause);
      var referenceExpression2 = new QuerySourceReferenceExpression (SourceClause);
      Assert.That (referenceExpression1.GetHashCode (), Is.EqualTo (referenceExpression2.GetHashCode ()));
    }
  }
}
