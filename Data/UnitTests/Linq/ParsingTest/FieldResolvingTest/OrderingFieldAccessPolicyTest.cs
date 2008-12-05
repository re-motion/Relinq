// This file is part of the re-motion Core Framework (www.re-motion.org)
// Copyright (C) 2005-2008 rubicon informationstechnologie gmbh, www.rubicon.eu
// 
// The re-motion Core Framework is free software; you can redistribute it 
// and/or modify it under the terms of the GNU Lesser General Public License 
// version 3.0 as published by the Free Software Foundation.
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
