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
using Remotion.Linq.Clauses.Expressions;
using Remotion.Linq.Parsing;

namespace Remotion.Linq.UnitTests.Clauses.Expressions.TestDomain
{
  public class TestableExtensionExpressionWithSpecificVisitor : ExtensionExpression
  {
    public TestableExtensionExpressionWithSpecificVisitor (Type type)
        : base (type)
    {
    }

    public override Expression Accept (ExpressionTreeVisitor visitor)
    {
      var specificVisitor = visitor as ISpecificVisitor;
      if (specificVisitor != null)
        return specificVisitor.VisitTestableExtensionExpression (this);
      else
        return base.Accept (visitor);
    }

    protected override Expression VisitChildren (ExpressionTreeVisitor visitor)
    {
      return this;
    }
  }
}