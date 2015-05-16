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
      var formattedExpression = FormattingExpressionVisitor.Format (expression);
      Assert.That (formattedExpression, Is.EqualTo ("(1 > 2)"));
    }

    [Test]
    public void QuerySourceReferenceExpression ()
    {
      var referencedClause = ExpressionHelper.CreateMainFromClause_Int ("i", typeof (int), ExpressionHelper.CreateQueryable<Cook>());
      
      var expression = Expression.MakeBinary (ExpressionType.GreaterThan, new QuerySourceReferenceExpression (referencedClause), Expression.Constant (2));
      var formattedExpression = FormattingExpressionVisitor.Format (expression);
      Assert.That (formattedExpression, Is.EqualTo ("([i] > 2)"));
    }

    [Test]
    public void SubQueryExpression ()
    {
      var queryExpression = ExpressionHelper.MakeExpression (() => (from s in ExpressionHelper.CreateQueryable<Cook> () select s).Count());
      var subQueryModel = ExpressionHelper.ParseQuery (queryExpression);

      var expression = Expression.MakeBinary (ExpressionType.GreaterThan, new SubQueryExpression (subQueryModel), Expression.Constant (2));
      var formattedExpression = FormattingExpressionVisitor.Format (expression);
      Assert.That (formattedExpression, Is.EqualTo ("({TestQueryable<Cook>() => Count()} > 2)"));
    }

    [Test]
    public void VisitUnknownNonExtensionExpression ()
    {
      var expression = new UnknownExpression (typeof (object));
      var result = FormattingExpressionVisitor.Format (expression);

      Assert.That (result, Is.EqualTo("[-1]"));
    }

    [Test]
    public void VisitExtensionExpression ()
    {
      var expression = new TestExtensionExpression (Expression.Constant (0));
      var result = FormattingExpressionVisitor.Format (expression);

      Assert.That (result, Is.EqualTo ("Test(0)"));
    }
  }
}
