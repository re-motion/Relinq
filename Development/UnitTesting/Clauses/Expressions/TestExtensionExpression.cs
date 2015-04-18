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
using Remotion.Linq.Clauses.ExpressionTreeVisitors;
using Remotion.Linq.Parsing;
using Remotion.Utilities;

namespace Remotion.Linq.Development.UnitTesting.Clauses.Expressions
{
  public class TestExtensionExpression : ExtensionExpression
  {
    private readonly Expression _expression;

    public TestExtensionExpression (Expression expression)
        : base(expression.Type)
    {
      ArgumentUtility.CheckNotNull ("expression", expression);

      _expression = expression;
    }

    public Expression Expression
    {
      get { return _expression; }
    }

    public override bool CanReduce
    {
      get
      {
        return true;
      }
    }

    public override Expression Reduce ()
    {
      return _expression;
    }

    protected override Expression VisitChildren (RelinqExpressionVisitor visitor)
    {
      var result = visitor.Visit (_expression);
      if (result != _expression)
        return new TestExtensionExpression (result);

      return this;
    }

    public override string ToString ()
    {
      return "Test(" + FormattingExpressionVisitor.Format (_expression) + ")";
    }
  }
}