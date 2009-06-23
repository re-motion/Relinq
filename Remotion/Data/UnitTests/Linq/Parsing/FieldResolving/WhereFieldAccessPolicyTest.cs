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
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using NUnit.Framework.SyntaxHelpers;
using Remotion.Collections;
using Remotion.Data.Linq.Parsing.FieldResolving;
using System.Linq;

namespace Remotion.Data.UnitTests.Linq.Parsing.FieldResolving
{
  [TestFixture]
  public class WhereFieldAccessPolicyTest : FieldAccessPolicyTestBase
  {
    private WhereFieldAccessPolicy _policy;

    [SetUp]
    public override void SetUp ()
    {
      base.SetUp();
      _policy = new WhereFieldAccessPolicy (StubDatabaseInfo.Instance);
    }

    [Test]
    public void AdjustMemberInfosForFromIdentifier ()
    {
      var result = _policy.AdjustMemberInfosForDirectAccessOfQuerySource (StudentReference);
      Assert.That (result.A, Is.EqualTo (typeof (Student).GetProperty ("ID")));
      Assert.That (result.B, Is.Empty);
    }

    [Test]
    public void AdjustMemberInfosForRelation ()
    {
      Tuple<MemberInfo, IEnumerable<MemberInfo>> result = _policy.AdjustMemberInfosForRelation (
          StudentDetail_IndustrialSector_Member, new[] { StudentDetailDetail_StudentDetail_Member });

      var expected = new Tuple<MemberInfo, IEnumerable<MemberInfo>> (StudentDetail_IndustrialSector_Member, new[] { StudentDetailDetail_StudentDetail_Member });
      Assert.That (result.A, Is.EqualTo (expected.A));
      Assert.That (result.B, Is.EqualTo (expected.B));
    }

    [Test]
    public void AdjustMemberInfosForRelation_VirtualSide ()
    {
      Tuple<MemberInfo, IEnumerable<MemberInfo>> result = _policy.AdjustMemberInfosForRelation (
          IndustrialSector_StudentDetail_Member, new[] { StudentDetailDetail_IndustrialSector_Member });

      MemberInfo primaryKeyMember = typeof (Student_Detail).GetProperty ("ID");
      var expected = new Tuple<MemberInfo, IEnumerable<MemberInfo>> (
          primaryKeyMember, new[] { StudentDetailDetail_IndustrialSector_Member, IndustrialSector_StudentDetail_Member });

      Assert.That (result.A, Is.EqualTo (expected.A));
      Assert.That (result.B.ToArray(), Is.EqualTo (expected.B.ToArray()));
    }

    [Test]
    public void OptimizeRelatedKeyAccess_True ()
    {
      Assert.That (_policy.OptimizeRelatedKeyAccess(), Is.True);
    }
  }
}