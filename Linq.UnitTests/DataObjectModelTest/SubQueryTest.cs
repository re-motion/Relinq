using NUnit.Framework;
using Rubicon.Data.Linq.DataObjectModel;

namespace Rubicon.Data.Linq.UnitTests.DataObjectModelTest
{
  [TestFixture]
  public class SubQueryTest
  {
    [Test]
    public void Initialize ()
    {
      QueryExpression queryExpression = ExpressionHelper.CreateQueryExpression();
      SubQuery subQuery = new SubQuery (queryExpression, "foo");

      Assert.AreSame (queryExpression, subQuery.QueryExpression);
      Assert.AreEqual ("foo", subQuery.Alias);
    }

    [Test]
    public void AliasString ()
    {
      QueryExpression queryExpression = ExpressionHelper.CreateQueryExpression();
      SubQuery subQuery = new SubQuery (queryExpression, "foo");

      Assert.AreEqual ("foo", subQuery.AliasString);
    }
  }
}