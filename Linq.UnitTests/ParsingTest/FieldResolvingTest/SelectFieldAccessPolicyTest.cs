using System.Collections.Generic;
using NUnit.Framework;
using System.Reflection;
using Rubicon.Collections;
using Rubicon.Data.Linq.Parsing.FieldResolving;
using NUnit.Framework.SyntaxHelpers;

namespace Rubicon.Data.Linq.UnitTests.ParsingTest.FieldResolvingTest
{
  [TestFixture]
  public class SelectFieldAccessPolicyTest
  {
    [Test]
    public void AdjustMemberInfosForRelation()
    {
      MemberInfo joinMember = typeof (Student_Detail_Detail).GetProperty ("Student_Detail");
      MemberInfo relationMember = typeof (Student_Detail).GetProperty ("Student");

      SelectFieldAccessPolicy policy = new SelectFieldAccessPolicy();
      Tuple<MemberInfo, IEnumerable<MemberInfo>> result = policy.AdjustMemberInfosForRelation (relationMember, new[] { joinMember });
      var expected = new Tuple<MemberInfo, IEnumerable<MemberInfo>> (null, new[] {joinMember, relationMember});

      Assert.AreEqual (expected.A, result.A);
      Assert.That (result.B, Is.EqualTo (expected.B));
    }
  }
}