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
using Remotion.Data.Linq.Parsing;
using Remotion.Data.Linq.UnitTests.Linq.Core.Parsing.ExpressionTreeVisitorTests;
using Rhino.Mocks;

namespace Remotion.Data.Linq.UnitTests.Linq.Core.Parsing
{
  [TestFixture]
  public class ThrowingExpressionTreeVisitorTest
  {
    private TestThrowingExpressionTreeVisitor _visitor;

    [SetUp]
    public void SetUp ()
    {
      _visitor = new TestThrowingExpressionTreeVisitor ();
    }

    [Test]
    public void ExtensionExpression_ReducedExpressionIsVisited ()
    {
      ExpressionTreeVisitor visitor =new TestThrowingConstantExpressionTreeVisitor();
      var constantExpression = Expression.Constant (0);
      var expression = new TestExtensionExpression (constantExpression);

      var result = visitor.VisitExpression (expression);

      Assert.That (result, Is.Not.SameAs (constantExpression));
      Assert.That (((ConstantExpression) result).Value, Is.EqualTo("ConstantExpression was visited"));
    }


    [Test]
    [ExpectedException (typeof (NotSupportedException), ExpectedMessage = "Test of VisitUnknownExpression: [-1]")]
    public void VisitUnknownExpression ()
    {
      VisitExpression (_visitor, (ExpressionType) (-1));
    }

    [Test]
    [ExpectedException (typeof (NotSupportedException), ExpectedMessage = "Test of VisitUnaryExpression", MatchType = MessageMatch.Contains)]
    public void VisitUnaryExpression ()
    {
      VisitExpression (_visitor, ExpressionType.UnaryPlus);
    }

    [Test]
    [ExpectedException (typeof (NotSupportedException), ExpectedMessage = "Test of VisitBinaryExpression: ", MatchType = MessageMatch.Contains)]
    public void VisitBinaryExpression ()
    {
      VisitExpression (_visitor, ExpressionType.And);
    }

    [Test]
    [ExpectedException (typeof (NotSupportedException), ExpectedMessage = "Test of VisitTypeBinaryExpression: ", MatchType = MessageMatch.Contains)]
    public void VisitTypeBinaryExpression ()
    {
      VisitExpression (_visitor, ExpressionType.TypeIs);
    }

    [Test]
    [ExpectedException (typeof (NotSupportedException), ExpectedMessage = "Test of VisitConstantExpression: ", MatchType = MessageMatch.Contains)]
    public void VisitConstantExpression ()
    {
      VisitExpression (_visitor, ExpressionType.Constant);
    }

    [Test]
    [ExpectedException (typeof (NotSupportedException), ExpectedMessage = "Test of VisitConditionalExpression: ", MatchType = MessageMatch.Contains)]
    public void VisitConditionalExpression ()
    {
      VisitExpression (_visitor, ExpressionType.Conditional);
    }

    [Test]
    [ExpectedException (typeof (NotSupportedException), ExpectedMessage = "Test of VisitParameterExpression: ", MatchType = MessageMatch.Contains)]
    public void VisitParameterExpression ()
    {
      VisitExpression (_visitor, ExpressionType.Parameter);
    }

    [Test]
    [ExpectedException (typeof (NotSupportedException), ExpectedMessage = "Test of VisitLambdaExpression: ", MatchType = MessageMatch.Contains)]
    public void VisitLambdaExpression ()
    {
      VisitExpression (_visitor, ExpressionType.Lambda);
    }

    [Test]
    [ExpectedException (typeof (NotSupportedException), ExpectedMessage = "Test of VisitMethodCallExpression: ", MatchType = MessageMatch.Contains)]
    public void VisitMethodCallExpression ()
    {
      VisitExpression (_visitor, ExpressionType.Call);
    }

    [Test]
    [ExpectedException (typeof (NotSupportedException), ExpectedMessage = "Test of VisitInvocationExpression: ", MatchType = MessageMatch.Contains)]
    public void VisitInvocationExpression ()
    {
      VisitExpression (_visitor, ExpressionType.Invoke);
    }

    [Test]
    [ExpectedException (typeof (NotSupportedException), ExpectedMessage = "Test of VisitMemberExpression: ", MatchType = MessageMatch.Contains)]
    public void VisitMemberExpression ()
    {
      VisitExpression (_visitor, ExpressionType.MemberAccess);
    }

    [Test]
    [ExpectedException (typeof (NotSupportedException), ExpectedMessage = "Test of VisitNewExpression: ", MatchType = MessageMatch.Contains)]
    public void VisitNewExpression ()
    {
      VisitExpression (_visitor, ExpressionType.New);
    }

    [Test]
    [ExpectedException (typeof (NotSupportedException), ExpectedMessage = "Test of VisitNewArrayExpression: ", MatchType = MessageMatch.Contains)]
    public void VisitNewArrayExpression ()
    {
      VisitExpression (_visitor, ExpressionType.NewArrayInit);
    }

    [Test]
    [ExpectedException (typeof (NotSupportedException), ExpectedMessage = "Test of VisitMemberInitExpression: ", MatchType = MessageMatch.Contains)]
    public void VisitMemberInitExpression ()
    {
      VisitExpression (_visitor, ExpressionType.MemberInit);
    }

    [Test]
    [ExpectedException (typeof (NotSupportedException), ExpectedMessage = "Test of VisitListInitExpression: ", MatchType = MessageMatch.Contains)]
    public void VisitListInitExpression ()
    {
      VisitExpression (_visitor, ExpressionType.ListInit);
    }

    [Test]
    [ExpectedException (typeof (NotSupportedException), ExpectedMessage = "Test of VisitElementInit: ", MatchType = MessageMatch.Contains)]
    public void VisitElementInit ()
    {
      _visitor.VisitElementInit (ExpressionInstanceCreator.CreateElementInit());
    }

    [Test]
    [ExpectedException (typeof (NotSupportedException), ExpectedMessage = "Test of VisitMemberAssignment: ", MatchType = MessageMatch.Contains)]
    public void VisitMemberAssignment ()
    {
      _visitor.VisitMemberBinding (ExpressionInstanceCreator.CreateMemberAssignment ());
    }

    [Test]
    [ExpectedException (typeof (NotSupportedException), ExpectedMessage = "Test of VisitMemberMemberBinding: ", MatchType = MessageMatch.Contains)]
    public void VisitMemberMemberBinding ()
    {
      _visitor.VisitMemberBinding (ExpressionInstanceCreator.CreateMemberMemberBinding ());
    }

    [Test]
    [ExpectedException (typeof (NotSupportedException), ExpectedMessage = "Test of VisitMemberListBinding: ", MatchType = MessageMatch.Contains)]
    public void VisitMemberListBinding ()
    {
      _visitor.VisitMemberBinding (ExpressionInstanceCreator.CreateMemberListBinding ());
    }

    [Test]
    [ExpectedException (typeof (NotSupportedException), ExpectedMessage = "Test of VisitSubQueryExpression: ", MatchType = MessageMatch.Contains)]
    public void VisitSubQueryExpression ()
    {
      _visitor.VisitExpression (new SubQueryExpression (ExpressionHelper.CreateQueryModel_Cook ()));
    }

    [Test]
    [ExpectedException (typeof (NotSupportedException), ExpectedMessage = "Test of VisitQuerySourceReferenceExpression: ", MatchType = MessageMatch.Contains)]
    public void VisitQuerySourceReferenceExpression ()
    {
      _visitor.VisitExpression (new QuerySourceReferenceExpression (ExpressionHelper.CreateMainFromClause_Int ()));
    }

    private void VisitExpression (TestThrowingExpressionTreeVisitor visitor, ExpressionType nodeType)
    {
      visitor.VisitExpression (ExpressionInstanceCreator.GetExpressionInstance (nodeType));
    }

  }
}
