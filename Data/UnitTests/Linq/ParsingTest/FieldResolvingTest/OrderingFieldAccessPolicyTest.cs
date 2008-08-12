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
using System.Reflection;
using NUnit.Framework;
using NUnit.Framework.SyntaxHelpers;
using Remotion.Data.Linq.Clauses;
using Remotion.Data.Linq.Parsing.FieldResolving;

namespace Remotion.Data.UnitTests.Linq.ParsingTest.FieldResolvingTest
{
  [TestFixture]
  public class OrderingFieldAccessPolicyTest
  {
    private OrderingFieldAccessPolicy _policy;

    [SetUp]
    public void SetUp ()
    {
      _policy = new OrderingFieldAccessPolicy ();
    }

    [Test]
    public void AdjustMemberInfosForFromIdentifier ()
    {
      MainFromClause fromClause =
          ExpressionHelper.CreateMainFromClause (Expression.Parameter (typeof (Student), "s"), ExpressionHelper.CreateQuerySource ());

      var result = _policy.AdjustMemberInfosForAccessedIdentifier (fromClause.Identifier);
      Assert.That (result.A, Is.Null);
      Assert.That (result.B, Is.Empty);
    }

    [Test]
    [ExpectedException (typeof (NotSupportedException), ExpectedMessage = "Ordering by 'Remotion.Data.UnitTests.Linq.Student_Detail.Student' "
        + "is not supported because it is a relation member.")]
    public void AdjustMemberInfosForRelation ()
    {
      MemberInfo joinMember = typeof (Student_Detail_Detail).GetProperty ("Student_Detail");
      MemberInfo relationMember = typeof (Student_Detail).GetProperty ("Student");

      _policy.AdjustMemberInfosForRelation (relationMember, new[] { joinMember });
    }

    [Test]
    public void OptimizeRelatedKeyAccess_False ()
    {
      Assert.That (_policy.OptimizeRelatedKeyAccess (), Is.False);
    }
  }
}
