// This file is part of the re-motion Core Framework (www.re-motion.org)
// Copyright (C) 2005-2009 rubicon informationstechnologie gmbh, www.rubicon.eu
// 
// The re-motion Core Framework is free software; you can redistribute it 
// and/or modify it under the terms of the GNU Lesser General Public License 
// as published by the Free Software Foundation; either version 2.1 of the 
// License, or (at your option) any later version.
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
using Remotion.Data.Linq.Clauses;
using Remotion.Data.Linq.Utilities;
using Rhino.Mocks;

namespace Remotion.Data.Linq.UnitTests.Linq.Core
{
  [TestFixture]
  public class QueryModelBuilderTest
  {
    private QueryModelBuilder _builder;
    private MainFromClause _mainFromClause;
    private WhereClause _whereClause1;
    private WhereClause _whereClause2;
    private SelectClause _selectClause;
    private ResultOperatorBase _resultOperator1;
    private ResultOperatorBase _resultOperator2;

    [SetUp]
    public void SetUp ()
    {
      _builder = new QueryModelBuilder();
      _mainFromClause = ExpressionHelper.CreateMainFromClause_Int ();
      _whereClause1 = ExpressionHelper.CreateWhereClause ();
      _whereClause2 = ExpressionHelper.CreateWhereClause ();
      _selectClause = ExpressionHelper.CreateSelectClause ();
      _resultOperator1 = ExpressionHelper.CreateResultOperator ();
      _resultOperator2 = ExpressionHelper.CreateResultOperator ();
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

      Assert.That (_builder.SelectClause, Is.SameAs (_selectClause));
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
    public void AddResultOperator ()
    {
      _builder.AddResultOperator (_resultOperator1);
      Assert.That (_builder.ResultOperators, List.Contains (_resultOperator1));
    }

    [Test]
    public void AddResultOperator_OrderIsRetained ()
    {
      _builder.AddResultOperator (_resultOperator1);
      _builder.AddResultOperator (_resultOperator2);
      Assert.That (_builder.ResultOperators, Is.EqualTo (new[] { _resultOperator1, _resultOperator2 }));
    }

    [Test]
    public void Build ()
    {
      _builder.AddResultOperator (_resultOperator1);
      _builder.AddClause (_whereClause1);
      _builder.AddClause (_mainFromClause);
      _builder.AddClause (_whereClause2);
      _builder.AddClause (_selectClause);
      _builder.AddResultOperator (_resultOperator2);

      var queryModel = _builder.Build ();

      Assert.That (queryModel.MainFromClause, Is.SameAs (_mainFromClause));
      Assert.That (queryModel.SelectClause, Is.SameAs (_selectClause));
      Assert.That (queryModel.BodyClauses, Is.EqualTo (new[] { _whereClause1, _whereClause2 }));
      Assert.That (queryModel.ResultOperators, Is.EqualTo (new[] { _resultOperator1, _resultOperator2 }));
    }

    [Test]
    [ExpectedException (typeof (InvalidOperationException))]
    public void Build_WithoutMainFromClause ()
    {
      _builder.AddClause (_whereClause1);
      _builder.AddClause (_whereClause2);
      _builder.AddClause (_selectClause);

      _builder.Build ();
    }

    [Test]
    [ExpectedException (typeof (InvalidOperationException))]
    public void Build_WithoutSelectOrGroupClause ()
    {
      _builder.AddClause (_whereClause1);
      _builder.AddClause (_whereClause2);
      _builder.AddClause (_mainFromClause);

      _builder.Build ();
    }
  }
}
