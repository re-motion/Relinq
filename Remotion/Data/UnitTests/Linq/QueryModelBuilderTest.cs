// This file is part of the re-motion Core Framework (www.re-motion.org)
// Copyright (C) 2005-2009 rubicon informationstechnologie gmbh, www.rubicon.eu
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
using System;
using NUnit.Framework;
using NUnit.Framework.SyntaxHelpers;
using Remotion.Data.Linq;
using Remotion.Data.Linq.Clauses;
using System.Linq;
using Remotion.Utilities;
using Rhino.Mocks;

namespace Remotion.Data.UnitTests.Linq
{
  [TestFixture]
  public class QueryModelBuilderTest
  {
    private QueryModelBuilder _builder;
    private MainFromClause _mainFromClause;
    private WhereClause _whereClause1;
    private WhereClause _whereClause2;
    private SelectClause _selectClause;

    [SetUp]
    public void SetUp ()
    {
      _builder = new QueryModelBuilder();
      _mainFromClause = ExpressionHelper.CreateMainFromClause ();
      _whereClause1 = ExpressionHelper.CreateWhereClause ();
      _whereClause2 = ExpressionHelper.CreateWhereClause ();
      _selectClause = ExpressionHelper.CreateSelectClause ();
    }

    [Test]
    public void AddClause_BodyClause ()
    {
      _builder.AddClause (_whereClause1);

      Assert.That (_builder.BodyClauses, Is.EqualTo (new[] { _whereClause1 }));
    }

    [Test]
    public void AddClause_SelectClause ()
    {
      _builder.AddClause (_selectClause);

      Assert.That (_builder.SelectOrGroupClause, Is.SameAs (_selectClause));
    }

    [Test]
    [ExpectedException (typeof (InvalidOperationException))]
    public void AddClause_SelectClause_Twice ()
    {
      _builder.AddClause (_selectClause);
      _builder.AddClause (_selectClause);
    }

    [Test]
    public void AddClause_MainFromClause ()
    {
      _builder.AddClause (_mainFromClause);

      Assert.That (_builder.MainFromClause, Is.SameAs (_mainFromClause));
    }

    [Test]
    [ExpectedException (typeof (InvalidOperationException))]
    public void AddClause_MainFromClause_Twice ()
    {
      _builder.AddClause (_mainFromClause);
      _builder.AddClause (_mainFromClause);
    }

    [Test]
    [ExpectedException (typeof (ArgumentTypeException))]
    public void AddClause_InvalidClause ()
    {
      _builder.AddClause (MockRepository.GenerateMock<IClause>());
    }

    [Test]
    public void Build ()
    {
      _builder.AddClause (_whereClause1);
      _builder.AddClause (_mainFromClause);
      _builder.AddClause (_whereClause2);
      _builder.AddClause (_selectClause);

      var queryModel = _builder.Build(typeof (IQueryable<int>));

      Assert.That (queryModel.MainFromClause, Is.SameAs (_mainFromClause));
      Assert.That (queryModel.SelectOrGroupClause, Is.SameAs (_selectClause));
      Assert.That (queryModel.BodyClauses, Is.EqualTo(new[] { _whereClause1, _whereClause2 }));
    }

    [Test]
    [ExpectedException (typeof (InvalidOperationException))]
    public void Build_WithoutMainFromClause ()
    {
      _builder.AddClause (_whereClause1);
      _builder.AddClause (_whereClause2);
      _builder.AddClause (_selectClause);

      _builder.Build (typeof (IQueryable<int>));
    }

    [Test]
    [ExpectedException (typeof (InvalidOperationException))]
    public void Build_WithoutSelectOrGroupClause ()
    {
      _builder.AddClause (_whereClause1);
      _builder.AddClause (_whereClause2);
      _builder.AddClause (_mainFromClause);

      _builder.Build (typeof (IQueryable<int>));
    }
  }
}