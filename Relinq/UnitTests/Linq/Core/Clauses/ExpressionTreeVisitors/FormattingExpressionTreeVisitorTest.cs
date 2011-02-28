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
using Remotion.Linq.UnitTests.Linq.Core.Parsing;
using Remotion.Linq.UnitTests.Linq.Core.Parsing.ExpressionTreeVisitors;
using Remotion.Linq.Clauses.Expressions;
using Remotion.Linq.Clauses.ExpressionTreeVisitors;

namespace Remotion.Linq.UnitTests.Linq.Core.Clauses.ExpressionTreeVisitors
{
  [TestFixture]
  public class FormattingExpressionTreeVisitorTest
  {
    [Test]
    public void OrdinaryExpression ()
    {
      var expression = Expression.MakeBinary (ExpressionType.GreaterThan, Expression.Constant (1), Expression.Constant (2));
      var formattedExpression = FormattingExpressionTreeVisitor.Format (expression);
      Assert.That (formattedExpression, Is.EqualTo ("(1 > 2)"));
    }

    [Test]
    public void QuerySourceReferenceExpression ()
    {
      var referencedClause = ExpressionHelper.CreateMainFromClause_Int ("i", typeof (int), ExpressionHelper.CreateCookQueryable());
      
      var expression = Expression.MakeBinary (ExpressionType.GreaterThan, new QuerySourceReferenceExpression (referencedClause), Expression.Constant (2));
      var formattedExpression = FormattingExpressionTreeVisitor.Format (expression);
      Assert.That (formattedExpression, Is.EqualTo ("([i] > 2)"));
    }

    [Test]
    public void SubQueryExpression ()
    {
      var queryExpression = ExpressionHelper.MakeExpression (() => (from s in ExpressionHelper.CreateCookQueryable () select s).Count());
      var subQueryModel = ExpressionHelper.ParseQuery (queryExpression);

      var expression = Expression.MakeBinary (ExpressionType.GreaterThan, new SubQueryExpression (subQueryModel), Expression.Constant (2));
      var formattedExpression = FormattingExpressionTreeVisitor.Format (expression);
      Assert.That (formattedExpression, Is.EqualTo ("({TestQueryable<Cook>() => Count()} > 2)"));
    }

    [Test]
    public void VisitUnknownNonExtensionExpression ()
    {
      var expression = new UnknownExpression (typeof (object));
      var result = FormattingExpressionTreeVisitor.Format (expression);

      Assert.That (result, Is.EqualTo("[-1]"));
    }

    [Test]
    public void VisitExtensionExpression ()
    {
      var expression = new TestExtensionExpression (Expression.Constant (0));
      var result = FormattingExpressionTreeVisitor.Format (expression);

      Assert.That (result, Is.EqualTo ("Test(0)"));
    }
  }
}
