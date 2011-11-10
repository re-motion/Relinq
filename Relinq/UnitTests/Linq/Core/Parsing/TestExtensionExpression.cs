// This file is part of the re-linq project (relinq.codeplex.com)
// Copyright (C) rubicon IT GmbH, www.rubicon.eu
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
using System.Linq.Expressions;
using Remotion.Linq.Clauses.Expressions;
using Remotion.Linq.Clauses.ExpressionTreeVisitors;
using Remotion.Linq.Parsing;
using Remotion.Linq.Utilities;

namespace Remotion.Linq.UnitTests.Linq.Core.Parsing
{
  public class TestExtensionExpression : ExtensionExpression
  {
    private readonly Expression _expression;

    public TestExtensionExpression (Expression expression)
        : base(expression.Type)
    {
      ArgumentUtility.CheckNotNull ("constantExpression", expression);

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

    protected override Expression VisitChildren (ExpressionTreeVisitor visitor)
    {
      var result = visitor.VisitExpression (_expression);
      if (result != _expression)
        return new TestExtensionExpression (result);

      return this;
    }

    public override string ToString ()
    {
      return "Test(" + FormattingExpressionTreeVisitor.Format (_expression) + ")";
    }
  }
}