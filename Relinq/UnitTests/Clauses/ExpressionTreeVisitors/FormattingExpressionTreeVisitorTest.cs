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
using Remotion.Linq.Clauses.ExpressionTreeVisitors;
using Remotion.Linq.Development.UnitTesting;
using Remotion.Linq.Development.UnitTesting.Clauses.Expressions;
using Remotion.Linq.UnitTests.Parsing.ExpressionTreeVisitors;
using Remotion.Linq.UnitTests.TestDomain;

namespace Remotion.Linq.UnitTests.Clauses.ExpressionTreeVisitors
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
      var referencedClause = ExpressionHelper.CreateMainFromClause_Int ("i", typeof (int), ExpressionHelper.CreateQueryable<Cook>());
      
      var expression = Expression.MakeBinary (ExpressionType.GreaterThan, new QuerySourceReferenceExpression (referencedClause), Expression.Constant (2));
      var formattedExpression = FormattingExpressionTreeVisitor.Format (expression);
      Assert.That (formattedExpression, Is.EqualTo ("([i] > 2)"));
    }

    [Test]
    public void SubQueryExpression ()
    {
      var queryExpression = ExpressionHelper.MakeExpression (() => (from s in ExpressionHelper.CreateQueryable<Cook> () select s).Count());
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
