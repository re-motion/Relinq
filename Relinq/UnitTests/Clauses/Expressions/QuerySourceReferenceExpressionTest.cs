// Copyright (c) rubicon IT GmbH, www.rubicon.eu
//
// See the NOTICE file distributed with this work for additional information
// regarding copyright ownership.  rubicon licenses this file to you under 
// the Apache License, Version 2.0 (the "License"); you may not use this 
// file except in compliance with the License.  You may obtain a copy of the 
// License at
//
//   http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software 
// distributed under the License is distributed on an "AS IS" BASIS, WITHOUT 
// WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.  See the 
// License for the specific language governing permissions and limitations
// under the License.
// 
using System;
using System.Linq.Expressions;
using NUnit.Framework;
using Remotion.Linq.Clauses.Expressions;
using Remotion.Linq.Development.UnitTesting;
using Remotion.Linq.UnitTests.Parsing.Structure.IntermediateModel;

namespace Remotion.Linq.UnitTests.Clauses.Expressions
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
