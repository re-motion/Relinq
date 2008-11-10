/* Copyright (C) 2005 - 2008 rubicon informationstechnologie gmbh
 *
 * This program is free software: you can redistribute it and/or modify it under 
 * the terms of the re:motion license agreement in license.txt. If you did not 
 * receive it, please visit http://www.re-motion.org/licensing.
 * 
 * Unless otherwise provided, this software is distributed on an "AS IS" basis, 
 * WITHOUT WARRANTY OF ANY KIND, either express or implied. 
 */

using System;
using System.Linq.Expressions;
using NUnit.Framework;
using NUnit.Framework.SyntaxHelpers;
using Remotion.Data.Linq.Clauses;

namespace Remotion.Data.UnitTests.Linq.ClausesTest
{
  [TestFixture]
  public class MemberFromClauseTest
  {
    [Test]
    public void FromExpressionContainsMemberExpression ()
    {
      var previousClause = ExpressionHelper.CreateClause();
      var identifier = ExpressionHelper.CreateParameterExpression();
      var bodyExpression = Expression.MakeMemberAccess (Expression.Constant ("test"), typeof (string).GetProperty ("Length"));
      var fromExpression = Expression.Lambda (bodyExpression);
      var projectionExpression = ExpressionHelper.CreateLambdaExpression();
      var fromClause = new MemberFromClause (previousClause, identifier, fromExpression, projectionExpression);
      Assert.That (fromClause.MemberExpression, Is.SameAs (bodyExpression));
    }

    [Test]
    [ExpectedException (typeof (ArgumentException), ExpectedMessage = "From expression must contain a MemberExpression.")]
    public void FromExpressionContainNoMemberExpression ()
    {
      var previousClause = ExpressionHelper.CreateClause ();
      var identifier = ExpressionHelper.CreateParameterExpression ();
      var bodyExpression = Expression.Constant ("test");
      var fromExpression = Expression.Lambda (bodyExpression);
      var projectionExpression = ExpressionHelper.CreateLambdaExpression ();
      new MemberFromClause (previousClause, identifier, fromExpression, projectionExpression);
    }
  }
}