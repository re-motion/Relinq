using System;
using NUnit.Framework;
using Rubicon.Data.Linq.DataObjectModel;
using Rubicon.Utilities;

namespace Rubicon.Data.Linq.UnitTests.DataObjectModelTest
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
    [ExpectedException (typeof (ArgumentTypeException), ExpectedMessage = "Argument left has type Rubicon.Data.Linq.DataObjectModel.Constant when " 
        + "type Rubicon.Data.Linq.DataObjectModel.SubQuery was expected.\r\nParameter name: left")]
    public void ContainsWithInvalidLeftSide ()
    {
      new BinaryCondition (new Constant (0), new Constant (0), BinaryCondition.ConditionKind.Contains);
    }
  }
}