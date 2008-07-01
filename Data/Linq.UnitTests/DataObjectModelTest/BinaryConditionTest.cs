using System;
using NUnit.Framework;
using Remotion.Data.Linq.DataObjectModel;
using Remotion.Utilities;

namespace Remotion.Data.Linq.UnitTests.DataObjectModelTest
{
  [TestFixture]
  public class BinaryConditionTest
  {
    [Test]
    public void ContainsWithSubQuery ()
    {
      SubQuery subQuery = new SubQuery (ExpressionHelper.CreateQueryModel (), null);
      BinaryCondition binaryCondition = new BinaryCondition(subQuery, new Constant(0), BinaryCondition.ConditionKind.Contains);
      Assert.AreSame (subQuery, binaryCondition.Left);
    }

    [Test]
    [ExpectedException (typeof (ArgumentTypeException), ExpectedMessage = "Argument left has type Remotion.Data.Linq.DataObjectModel.Constant when " 
        + "type Remotion.Data.Linq.DataObjectModel.SubQuery was expected.\r\nParameter name: left")]
    public void ContainsWithInvalidLeftSide ()
    {
      new BinaryCondition (new Constant (0), new Constant (0), BinaryCondition.ConditionKind.Contains);
    }
  }
}