// This file is part of the re-motion Core Framework (www.re-motion.org)
// Copyright (C) 2005-2009 rubicon informationstechnologie gmbh, www.rubicon.eu
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

namespace Remotion.Data.UnitTests.Linq.Parsing.FieldResolving
{
  [TestFixture]
  public class OrderingFieldAccessPolicyTest : FieldAccessPolicyTestBase
  {
    private OrderingFieldAccessPolicy _policy;

    [SetUp]
    public override void SetUp ()
    {
      base.SetUp ();
      _policy = new OrderingFieldAccessPolicy ();
    }

    [Test]
    public void AdjustMemberInfosForFromIdentifier ()
    {
      var result = _policy.AdjustMemberInfosForDirectAccessOfQuerySource (StudentReference);
      Assert.That (result.A, Is.Null);
      Assert.That (result.B, Is.Empty);
    }

    [Test]
    [ExpectedException (typeof (NotSupportedException), ExpectedMessage = "Ordering by 'Remotion.Data.UnitTests.Linq.Student_Detail.Student' "
        + "is not supported because it is a relation member.")]
    public void AdjustMemberInfosForRelation ()
    {
      _policy.AdjustMemberInfosForRelation (StudentDetail_Student_Member, new[] { StudentDetailDetail_StudentDetail_Member });
    }

    [Test]
    public void OptimizeRelatedKeyAccess_False ()
    {
      Assert.That (_policy.OptimizeRelatedKeyAccess (), Is.False);
    }
  }
}
