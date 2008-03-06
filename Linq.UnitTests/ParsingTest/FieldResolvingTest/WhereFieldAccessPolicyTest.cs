using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using NUnit.Framework.SyntaxHelpers;
using Rubicon.Collections;
using Rubicon.Data.Linq.Parsing.FieldResolving;
using System.Linq;

namespace Rubicon.Data.Linq.UnitTests.ParsingTest.FieldResolvingTest
{
  [TestFixture]
  public class WhereFieldAccessPolicyTest
  {
    [Test]
    public void AdjustMemberInfosForRelation ()
    {
      MemberInfo joinMember = typeof (Student_Detail_Detail).GetProperty ("Student_Detail");
      MemberInfo relationMember = typeof (Student_Detail).GetProperty ("IndustrialSector");

      WhereFieldAccessPolicy policy = new WhereFieldAccessPolicy (StubDatabaseInfo.Instance);
      Tuple<MemberInfo, IEnumerable<MemberInfo>> result = policy.AdjustMemberInfosForRelation (relationMember, new[] { joinMember });
      var expected = new Tuple<MemberInfo, IEnumerable<MemberInfo>> (relationMember, new[] { joinMember });

      Assert.AreEqual (expected.A, result.A);
      Assert.That (result.B, Is.EqualTo (expected.B));
    }

    [Test]
    public void AdjustMemberInfosForRelation_VirtualSide ()
    {
      MemberInfo joinMember = typeof (Student_Detail_Detail).GetProperty ("IndustrialSector");
      MemberInfo relationMember = typeof (IndustrialSector).GetProperty ("Student_Detail");
      MemberInfo primaryKeyMember = typeof (Student_Detail).GetProperty ("ID");

      WhereFieldAccessPolicy policy = new WhereFieldAccessPolicy (StubDatabaseInfo.Instance);
      Tuple<MemberInfo, IEnumerable<MemberInfo>> result = policy.AdjustMemberInfosForRelation (relationMember, new[] { joinMember });
      
      var expected = new Tuple<MemberInfo, IEnumerable<MemberInfo>> (primaryKeyMember, new[] { joinMember, relationMember });

      Assert.AreEqual (expected.A, result.A);
      Assert.That (result.B.ToArray(), Is.EqualTo (expected.B.ToArray()));
    }

  }
}