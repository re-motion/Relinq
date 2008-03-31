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
      QueryModel queryModel = ExpressionHelper.CreateQueryModel();
      SubQuery subQuery = new SubQuery (queryModel, "foo");

      Assert.AreSame (queryModel, subQuery.QueryModel);
      Assert.AreEqual ("foo", subQuery.Alias);
    }

    [Test]
    public void AliasString ()
    {
      QueryModel queryModel = ExpressionHelper.CreateQueryModel();
      SubQuery subQuery = new SubQuery (queryModel, "foo");

      Assert.AreEqual ("foo", subQuery.AliasString);
    }
  }
}