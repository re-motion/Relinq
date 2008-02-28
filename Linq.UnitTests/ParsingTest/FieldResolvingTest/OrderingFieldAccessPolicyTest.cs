using System;
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using NUnit.Framework.SyntaxHelpers;
using Rubicon.Collections;
using Rubicon.Data.Linq.Parsing.FieldResolving;

namespace Rubicon.Data.Linq.UnitTests.ParsingTest.FieldResolvingTest
{
  [TestFixture]
  public class OrderingFieldAccessPolicyTest
  {
    [Test]
    [ExpectedException (typeof (NotSupportedException), ExpectedMessage = "Ordering by 'Rubicon.Data.Linq.UnitTests.Student_Detail.Student' "
        + "is not supported because it is a relation member.")]
    public void AdjustMemberInfosForRelation ()
    {
      MemberInfo joinMember = typeof (Student_Detail_Detail).GetProperty ("Student_Detail");
      MemberInfo relationMember = typeof (Student_Detail).GetProperty ("Student");

      OrderingFieldAccessPolicy policy = new OrderingFieldAccessPolicy ();
      policy.AdjustMemberInfosForRelation (relationMember, new[] { joinMember });
    }
  }
}