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

    [Test]
    public void Equals_True ()
    {
      QueryModel queryModel = ExpressionHelper.CreateQueryModel ();
      SubQuery subQuery1 = new SubQuery (queryModel, "foo");
      SubQuery subQuery2 = new SubQuery (queryModel, "foo");

      Assert.AreEqual (subQuery1, subQuery2);
    }

    [Test]
    public void Equals_False ()
    {
      QueryModel queryModel = ExpressionHelper.CreateQueryModel ();
      QueryModel queryModel2 = ExpressionHelper.CreateQueryModel ();

      SubQuery subQuery1 = new SubQuery (queryModel, "foo");
      SubQuery subQuery2 = new SubQuery (queryModel, "foo1");
      SubQuery subQuery3 = new SubQuery (queryModel2, "foo");

      Assert.AreNotEqual (subQuery1, subQuery2);
      Assert.AreNotEqual (subQuery1, subQuery3);
      Assert.AreNotEqual (subQuery2, subQuery3);
    }

    [Test]
    public void GetHashCode_EqualObjects ()
    {
      QueryModel queryModel = ExpressionHelper.CreateQueryModel ();
      SubQuery subQuery1 = new SubQuery (queryModel, "foo");
      SubQuery subQuery2 = new SubQuery (queryModel, "foo");

      Assert.AreEqual (subQuery1.GetHashCode(), subQuery2.GetHashCode());
    }

    [Test]
    public void GetHashCode_NullAlias ()
    {
      QueryModel queryModel = ExpressionHelper.CreateQueryModel ();
      SubQuery subQuery1 = new SubQuery (queryModel, null);
      SubQuery subQuery2 = new SubQuery (queryModel, null);

      Assert.AreEqual (subQuery1.GetHashCode (), subQuery2.GetHashCode ());
    }
  }
}