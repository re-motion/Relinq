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
using Remotion.Data.Linq;
using Remotion.Data.Linq.DataObjectModel;
using Remotion.Data.Linq.Parsing;

namespace Remotion.Data.UnitTests.Linq.DataObjectModelTest
{
  [TestFixture]
  public class SubQueryTest
  {
    [Test]
    public void Initialize ()
    {
      QueryModel queryModel = ExpressionHelper.CreateQueryModel();
      SubQuery subQuery = new SubQuery (queryModel, ParseMode.SubQueryInSelect, "foo");

      Assert.AreSame (queryModel, subQuery.QueryModel);
      Assert.AreEqual ("foo", subQuery.Alias);
    }

    [Test]
    public void AliasString ()
    {
      QueryModel queryModel = ExpressionHelper.CreateQueryModel();
      SubQuery subQuery = new SubQuery (queryModel, ParseMode.SubQueryInSelect, "foo");

      Assert.AreEqual ("foo", subQuery.AliasString);
    }

    [Test]
    public void Equals_True ()
    {
      QueryModel queryModel = ExpressionHelper.CreateQueryModel ();
      SubQuery subQuery1 = new SubQuery (queryModel, ParseMode.SubQueryInSelect, "foo");
      SubQuery subQuery2 = new SubQuery (queryModel, ParseMode.SubQueryInSelect, "foo");

      Assert.AreEqual (subQuery1, subQuery2);
    }

    [Test]
    public void Equals_False ()
    {
      QueryModel queryModel = ExpressionHelper.CreateQueryModel ();
      QueryModel queryModel2 = ExpressionHelper.CreateQueryModel ();

      SubQuery subQuery1 = new SubQuery (queryModel, ParseMode.SubQueryInSelect, "foo");
      SubQuery subQuery2 = new SubQuery (queryModel, ParseMode.SubQueryInSelect, "foo1");
      SubQuery subQuery3 = new SubQuery (queryModel2, ParseMode.SubQueryInSelect, "foo");

      Assert.AreNotEqual (subQuery1, subQuery2);
      Assert.AreNotEqual (subQuery1, subQuery3);
      Assert.AreNotEqual (subQuery2, subQuery3);
    }

    [Test]
    public void GetHashCode_EqualObjects ()
    {
      QueryModel queryModel = ExpressionHelper.CreateQueryModel ();
      SubQuery subQuery1 = new SubQuery (queryModel, ParseMode.SubQueryInSelect, "foo");
      SubQuery subQuery2 = new SubQuery (queryModel, ParseMode.SubQueryInSelect, "foo");

      Assert.AreEqual (subQuery1.GetHashCode(), subQuery2.GetHashCode());
    }

    [Test]
    public void GetHashCode_NullAlias ()
    {
      QueryModel queryModel = ExpressionHelper.CreateQueryModel ();
      SubQuery subQuery1 = new SubQuery (queryModel, ParseMode.SubQueryInSelect, null);
      SubQuery subQuery2 = new SubQuery (queryModel, ParseMode.SubQueryInSelect, null);

      Assert.AreEqual (subQuery1.GetHashCode (), subQuery2.GetHashCode ());
    }
  }
}
