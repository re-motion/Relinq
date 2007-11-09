using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using NUnit.Framework.SyntaxHelpers;

namespace Rubicon.Data.DomainObjects.Linq.UnitTests
{
  [TestFixture]
  public class QueryModelTest
  {
    [Test]
    public void Initialize_WithQuery ()
    {
      Query query = new Query ();
      QueryModel model = new QueryModel (query);
      Assert.AreSame (query, model.From);
    }

    [Test]
    [ExpectedException (typeof (ArgumentNullException))]
    public void Initialization_ThrowsOnNull ()
    {
      QueryModel model = new QueryModel (null);
    }

    [Test]
    public void AddSelectedColumn ()
    {
      QueryModel model = new QueryModel (new Query ());
      model.AddSelectedColumn ("OrderNumber");
      Assert.That (model.SelectedColumns, Is.EqualTo (new SelectedColumn[] { new SelectedColumn ("OrderNumber", null) }));
    }

    [Test]
    public void AddSelectedColumns ()
    {
      QueryModel model = new QueryModel (new Query ());
      model.AddSelectedColumn ("OrderNumber");
      model.AddSelectedColumn ("OrderID");
      Assert.That (model.SelectedColumns,
          Is.EqualTo (new SelectedColumn[] { new SelectedColumn ("OrderNumber", null), new SelectedColumn ("OrderID", null) }));
    }

    [Test]
    public void AddSelectedColumns_Ordering ()
    {
      QueryModel model = new QueryModel (new Query ());
      model.AddSelectedColumn ("OrderNumber");
      model.AddSelectedColumn ("OrderID");
      Assert.That (model.SelectedColumns,
          Is.EqualTo (new SelectedColumn[] { new SelectedColumn ("OrderNumber", null), new SelectedColumn ("OrderID", null) }));

      model = new QueryModel (new Query ());
      model.AddSelectedColumn ("OrderID");
      model.AddSelectedColumn ("OrderNumber");
      Assert.That (model.SelectedColumns,
          Is.EqualTo (new SelectedColumn[] { new SelectedColumn ("OrderID", null), new SelectedColumn ("OrderNumber", null) }));
    }

    [Test]
    public void AddSelectedColumn_WithNullAlias ()
    {
      QueryModel model = new QueryModel (new Query ());
      model.AddSelectedColumn ("OrderNumber", null);
      Assert.That (model.SelectedColumns, Is.EqualTo (new SelectedColumn[] { new SelectedColumn ("OrderNumber", null) }));
    }

    [Test]
    public void AddSelectedColumn_WithAlias ()
    {
      QueryModel model = new QueryModel (new Query ());
      model.AddSelectedColumn ("OrderNumber", "Alias");
      Assert.That (model.SelectedColumns, Is.EqualTo (new SelectedColumn[] { new SelectedColumn ("OrderNumber", "Alias") }));
    }
  }
}
