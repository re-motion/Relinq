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
using Remotion.Linq.Clauses.Expressions;
using Remotion.Linq.Clauses.ExpressionTreeVisitors;
using Remotion.Linq.Parsing;
using Remotion.Linq.Utilities;

namespace Remotion.Data.Linq.UnitTests.Linq.Core.Parsing
{
  public class TestExtensionExpression : ExtensionExpression
  {
    private readonly Expression _constantExpression;

    public TestExtensionExpression (Expression constantExpression)
        : base(constantExpression.Type)
    {
      ArgumentUtility.CheckNotNull ("constantExpression", constantExpression);

      _constantExpression = constantExpression;
    }

    // TODO Review: Rename to Expression
    public Expression ConstantExpression
    {
      get { return _constantExpression; }
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
      return _constantExpression;
    }

    protected override Expression VisitChildren (ExpressionTreeVisitor visitor)
    {
      var result = visitor.VisitExpression (_constantExpression);
      if (result != _constantExpression)
        return new TestExtensionExpression (result);

      return this;
    }

    public override string ToString ()
    {
      return "Test(" + FormattingExpressionTreeVisitor.Format (_constantExpression) + ")";
    }
  }
}