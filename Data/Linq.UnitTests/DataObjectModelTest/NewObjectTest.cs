using System;
using NUnit.Framework;
using Remotion.Data.Linq.DataObjectModel;
using NUnit.Framework.SyntaxHelpers;

namespace Remotion.Data.Linq.UnitTests.DataObjectModelTest
{
  [TestFixture]
  public class NewObjectTest
  {
    [Test]
    public void Equals_True_WithoutArguments ()
    {
      NewObject one = new NewObject (typeof (object).GetConstructor (Type.EmptyTypes), new IEvaluation[0]);
      NewObject two = new NewObject (typeof (object).GetConstructor (Type.EmptyTypes), new IEvaluation[0]);

      Assert.That (one, Is.EqualTo (two));
    }

    [Test]
    public void Equals_True_WithArguments ()
    {
      Table table = new Table();
      Column column1 = new Column (table, "x");
      Column column2 = new Column (table, "y");
      NewObject one = new NewObject (typeof (object).GetConstructor (Type.EmptyTypes), new IEvaluation[] { column1, column2 });
      NewObject two = new NewObject (typeof (object).GetConstructor (Type.EmptyTypes), new IEvaluation[] { column1, column2 });

      Assert.That (one, Is.EqualTo (two));
    }

    [Test]
    public void Equals_False_WithNull ()
    {
      NewObject one = new NewObject (typeof (object).GetConstructor (Type.EmptyTypes), new IEvaluation[0]);

      Assert.That (one, Is.Not.EqualTo (null));
    }

    [Test]
    public void Equals_False_WithOtherObject ()
    {
      NewObject one = new NewObject (typeof (object).GetConstructor (Type.EmptyTypes), new IEvaluation[0]);

      Assert.That (one, Is.Not.EqualTo (5));
    }

    [Test]
    public void Equals_False_WithDifferentConstructor ()
    {
      Table table = new Table ();
      Column column1 = new Column (table, "x");
      Column column2 = new Column (table, "y");
      NewObject one = new NewObject (typeof (object).GetConstructor (Type.EmptyTypes), new IEvaluation[] { column1, column2 });
      NewObject two = new NewObject (typeof (NewObjectTest).GetConstructor (Type.EmptyTypes), new IEvaluation[] { column1, column2 });

      Assert.That (one, Is.Not.EqualTo (two));
    }

    [Test]
    public void Equals_False_WithDifferentNumberOfArguments ()
    {
      Table table = new Table ();
      Column column1 = new Column (table, "x");
      Column column2 = new Column (table, "y");
      NewObject one = new NewObject (typeof (object).GetConstructor (Type.EmptyTypes), new IEvaluation[] { column1, column2 });
      NewObject two = new NewObject (typeof (object).GetConstructor (Type.EmptyTypes), new IEvaluation[] { column1 });

      Assert.That (one, Is.Not.EqualTo (two));
    }

    [Test]
    public void Equals_False_WithDifferentArguments ()
    {
      Table table = new Table ();
      Column column1 = new Column (table, "x");
      Column column2 = new Column (table, "y");
      NewObject one = new NewObject (typeof (object).GetConstructor (Type.EmptyTypes), new IEvaluation[] { column2 });
      NewObject two = new NewObject (typeof (object).GetConstructor (Type.EmptyTypes), new IEvaluation[] { column1 });

      Assert.That (one, Is.Not.EqualTo (two));
    }

    [Test]
    public void GetHashCode_Equal_WithoutArguments ()
    {
      NewObject one = new NewObject (typeof (object).GetConstructor (Type.EmptyTypes), new IEvaluation[0]);
      NewObject two = new NewObject (typeof (object).GetConstructor (Type.EmptyTypes), new IEvaluation[0]);

      Assert.That (one.GetHashCode (), Is.EqualTo (two.GetHashCode ()));
    }

    [Test]
    public void GetHashCode_Equal_WithArguments ()
    {
      Table table = new Table ();
      Column column1 = new Column (table, "x");
      Column column2 = new Column (table, "y");
      NewObject one = new NewObject (typeof (object).GetConstructor (Type.EmptyTypes), new IEvaluation[] { column1, column2 });
      NewObject two = new NewObject (typeof (object).GetConstructor (Type.EmptyTypes), new IEvaluation[] { column1, column2 });

      Assert.That (one.GetHashCode (), Is.EqualTo (two.GetHashCode ()));
    }
  }
}