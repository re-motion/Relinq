// This file is part of the re-motion Core Framework (www.re-motion.org)
// Copyright (C) 2005-2008 rubicon informationstechnologie gmbh, www.rubicon.eu
//
// The re-motion Core Framework is free software; you can redistribute it 
// and/or modify it under the terms of the GNU Lesser General Public License 
// version 3.0 as published by the Free Software Foundation.
//
// re-motion is distributed in the hope that it will be useful, 
// but WITHOUT ANY WARRANTY; without even the implied warranty of 
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the 
// GNU Lesser General Public License for more details.
//
// You should have received a copy of the GNU Lesser General Public License
// along with re-motion; if not, see http://www.gnu.org/licenses.
//
using NUnit.Framework;
using NUnit.Framework.SyntaxHelpers;
using Remotion.Data.Linq.DataObjectModel;

namespace Remotion.Data.UnitTests.Linq.DataObjectModelTest
{
  [TestFixture]
  public class FieldSourcePathTest
  {
    [Test]
    public void Initialization ()
    {
      Table source = new Table();
      SingleJoin join1 = new SingleJoin(new Column(source,"s2"), new Column(source,"s1"));
      SingleJoin join2 = new SingleJoin (new Column (source, "s4"), new Column (source, "s3"));
      SingleJoin[] joins = new[] {join1,join2};
      
      FieldSourcePath sourcePath = new FieldSourcePath(source,joins);

      Assert.AreSame (source, sourcePath.FirstSource);
      Assert.That (sourcePath.Joins, Is.EqualTo (joins));
    }

    [Test]
    public void LastTable_NoJoins ()
    {
      Table source = new Table ();
      FieldSourcePath sourcePath = new FieldSourcePath (source, new SingleJoin[0]);
      Assert.AreSame (source, sourcePath.LastSource);
    }

    [Test]
    public void LastTable_OneJoin ()
    {
      Table source = new Table ();
      Table table1 = new Table ();
      SingleJoin join1 = new SingleJoin (new Column (source, "s2"), new Column (table1, "s1"));
      SingleJoin[] joins = new[] { join1 };

      FieldSourcePath sourcePath = new FieldSourcePath (source, joins);
      Assert.AreSame (table1, sourcePath.LastSource);
    }

    [Test]
    public void LastTable_SeveralJoins ()
    {
      Table source = new Table ();
      Table table1 = new Table ();
      Table table2 = new Table ();
      SingleJoin join1 = new SingleJoin (new Column (source, "s2"), new Column (table1, "s1"));
      SingleJoin join2 = new SingleJoin (new Column (table1, "s4"), new Column (table2, "s3"));
      SingleJoin[] joins = new[] { join1, join2 };

      FieldSourcePath sourcePath = new FieldSourcePath (source, joins);
      Assert.AreSame (table2, sourcePath.LastSource);
    }

    [Test]
    public void Equals_True()
    {
      Table source = new Table ("source", "s");
      SingleJoin join1 = new SingleJoin (new Column (source, "s2"), new Column (source, "s1"));
      SingleJoin join2 = new SingleJoin (new Column (source, "s4"), new Column (source, "s3"));
      SingleJoin[] joins = new[] { join1, join2 };

      FieldSourcePath sourcePath1 = new FieldSourcePath (source, joins);
      FieldSourcePath sourcePath2 = new FieldSourcePath (source, joins);

      Assert.AreEqual (sourcePath1, sourcePath2);
    }

    [Test]
    public void Equals_FalseOtherClass ()
    {
      Table source = new Table ("source", "s");
      SingleJoin join1 = new SingleJoin (new Column (source, "s2"), new Column (source, "s1"));
      SingleJoin join2 = new SingleJoin (new Column (source, "s4"), new Column (source, "s3"));
      SingleJoin[] joins = new[] { join1, join2 };

      FieldSourcePath sourcePath = new FieldSourcePath (source, joins);

      Assert.AreNotEqual (new object(), sourcePath);
    }

    [Test]
    public void Equals_FalseTable ()
    {
      Table source1 = new Table ("source", "s");
      Table source2 = new Table ("source", "s");
      SingleJoin join1 = new SingleJoin (new Column (source1, "s2"), new Column (source1, "s1"));
      SingleJoin join2 = new SingleJoin (new Column (source1, "s4"), new Column (source1, "s3"));
      SingleJoin[] joins = new[] { join1, join2 };

      FieldSourcePath sourcePath1 = new FieldSourcePath (source1, joins);
      FieldSourcePath sourcePath2 = new FieldSourcePath (source2, joins);

      Assert.AreNotEqual (sourcePath1, sourcePath2);
    }

    [Test]
    public void Equals_FalseJoins ()
    {
      Table source = new Table ("source", "s");
      SingleJoin join1 = new SingleJoin (new Column (source, "s2"), new Column (source, "s1"));
      SingleJoin join2 = new SingleJoin (new Column (source, "s4"), new Column (source, "s3"));
      SingleJoin join3 = new SingleJoin (new Column (source, "s6"), new Column (source, "s5"));
      SingleJoin[] joins1 = new[] { join1, join2 };
      SingleJoin[] joins2 = new[] { join1, join3 };

      FieldSourcePath sourcePath1 = new FieldSourcePath (source, joins1);
      FieldSourcePath sourcePath2 = new FieldSourcePath (source, joins2);

      Assert.AreNotEqual (sourcePath1, sourcePath2);
    }

    [Test]
    public void GetHashCode_EqualPaths ()
    {
      Table source = new Table ("source", "s");
      SingleJoin join1 = new SingleJoin (new Column (source, "s2"), new Column (source, "s1"));
      SingleJoin join2 = new SingleJoin (new Column (source, "s4"), new Column (source, "s3"));
      SingleJoin[] joins = new[] { join1, join2 };

      FieldSourcePath sourcePath1 = new FieldSourcePath (source, joins);
      FieldSourcePath sourcePath2 = new FieldSourcePath (source, joins);

      Assert.AreEqual (sourcePath1.GetHashCode(), sourcePath2.GetHashCode());
    }
  }
}
