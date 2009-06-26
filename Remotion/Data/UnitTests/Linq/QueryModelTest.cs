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
using System.Linq;
using NUnit.Framework;
using Remotion.Data.Linq;
using Remotion.Data.Linq.StringBuilding;
using Rhino.Mocks;
using Remotion.Data.Linq.Clauses;
using NUnit.Framework.SyntaxHelpers;

namespace Remotion.Data.UnitTests.Linq
{
  [TestFixture]
  public class QueryModelTest
  {
    private MainFromClause _mainFromClause;
    private SelectClause _selectClause;
    private QueryModel _queryModel;
    private ClonedClauseMapping _clonedClauseMapping;

    [SetUp]
    public void SetUp ()
    {
      _mainFromClause = ExpressionHelper.CreateMainFromClause ();
      _selectClause = ExpressionHelper.CreateSelectClause ();
      _queryModel = new QueryModel (typeof (IQueryable<string>), _mainFromClause, _selectClause);
      _clonedClauseMapping = new ClonedClauseMapping ();
    }

    [Test]
    public void Initialize ()
    {
      Assert.That (_queryModel.MainFromClause, Is.SameAs (_mainFromClause));
      Assert.That (_queryModel.SelectOrGroupClause, Is.SameAs (_selectClause));
      Assert.That (_queryModel.ResultType, Is.EqualTo (typeof (IQueryable<string>)));
    }

    [Test]
    public void Accept()
    {
      var repository = new MockRepository ();
      var visitorMock = repository.StrictMock<IQueryModelVisitor> ();

      visitorMock.Expect (mock => mock.VisitQueryModel (_queryModel));

      repository.ReplayAll ();

      _queryModel.Accept (visitorMock);

      repository.VerifyAll ();
    }

    [Test]
    public void Override_ToString()
    {
      var sv = new StringBuildingQueryModelVisitor();
      sv.VisitQueryModel (_queryModel);
      Assert.That (_queryModel.ToString(), Is.EqualTo (sv.ToString()));
    }

    [Test]
    public void Clone_ReturnsNewQueryModel ()
    {
      var queryModel = ExpressionHelper.CreateQueryModel ();
      var clone = queryModel.Clone ();

      Assert.That (clone, Is.Not.Null);
      Assert.That (clone, Is.Not.SameAs (queryModel));
    }

    [Test]
    public void Clone_HasSameResultType ()
    {
      var clone = _queryModel.Clone ();

      Assert.That (clone.ResultType, Is.SameAs (_queryModel.ResultType));
    }

    [Test]
    public void Clone_HasCloneForMainFromClause ()
    {
      var clone = _queryModel.Clone ();

      Assert.That (clone.MainFromClause, Is.Not.SameAs (_queryModel.MainFromClause));
      Assert.That (clone.MainFromClause.ItemName, Is.EqualTo (_queryModel.MainFromClause.ItemName));
      Assert.That (clone.MainFromClause.ItemType, Is.SameAs (_queryModel.MainFromClause.ItemType));
    }

    [Test]
    public void Clone_HasCloneForMainFromClause_PassesMapping ()
    {
      var clone = _queryModel.Clone (_clonedClauseMapping);
      Assert.That (_clonedClauseMapping.GetClause (_queryModel.MainFromClause), Is.SameAs (clone.MainFromClause));
    }

    [Test]
    public void Clone_HasCloneForSelectClause ()
    {
      var selectClause = (SelectClause) _queryModel.SelectOrGroupClause;
      var clone = _queryModel.Clone ();

      Assert.That (clone.SelectOrGroupClause, Is.Not.SameAs (_queryModel.SelectOrGroupClause));
      var cloneSelectClause = ((SelectClause) clone.SelectOrGroupClause);
      Assert.That (cloneSelectClause.Selector, Is.EqualTo (selectClause.Selector));
    }

    [Test]
    public void Clone_HasCloneForSelectClause_PassesMapping ()
    {
      var clone = _queryModel.Clone (_clonedClauseMapping);
      Assert.That (_clonedClauseMapping.GetClause (_queryModel.SelectOrGroupClause), Is.SameAs (clone.SelectOrGroupClause));
    }

    [Test]
    public void Clone_HasClonesForBodyClauses ()
    {
      var additionalFromClause = ExpressionHelper.CreateAdditionalFromClause ();
      var whereClause = ExpressionHelper.CreateWhereClause();
      _queryModel.BodyClauses.Add (additionalFromClause);
      _queryModel.BodyClauses.Add (whereClause);

      var clone = _queryModel.Clone ();
      var clonedAdditionalFromClause = (AdditionalFromClause) clone.BodyClauses[0];
      var clonedWhereClause = (WhereClause) clone.BodyClauses[1];

      Assert.That (clonedAdditionalFromClause, Is.Not.SameAs (additionalFromClause));
      Assert.That (clonedAdditionalFromClause.ItemName, Is.EqualTo (additionalFromClause.ItemName));
      Assert.That (clonedAdditionalFromClause.ItemType, Is.SameAs (additionalFromClause.ItemType));
      Assert.That (clonedWhereClause, Is.Not.SameAs (whereClause));
      Assert.That (clonedWhereClause.Predicate, Is.EqualTo (whereClause.Predicate));
    }

    [Test]
    public void Clone_HasCloneForBodyClauses_PassesMapping ()
    {
      var bodyClause = ExpressionHelper.CreateWhereClause();
      _queryModel.BodyClauses.Add (bodyClause);

      var clone = _queryModel.Clone (_clonedClauseMapping);
      Assert.That (_clonedClauseMapping.GetClause (_queryModel.BodyClauses[0]), Is.SameAs (clone.BodyClauses[0]));
    }
    
    [Test]
    public void SelectOrGroupClause_Set ()
    {
      var newSelectClause = ExpressionHelper.CreateSelectClause ();
      _queryModel.SelectOrGroupClause = newSelectClause;

      Assert.That (_queryModel.SelectOrGroupClause, Is.SameAs (newSelectClause));
    }

    [Test]
    [ExpectedException (typeof (ArgumentNullException))]
    public void SelectOrGroupClause_Set_Null ()
    {
      _queryModel.SelectOrGroupClause = null;
    }

    [Test]
    public void GetNewName ()
    {
      var identifier = _queryModel.GetNewName ("test");
      Assert.That (identifier, Is.EqualTo ("test0"));
    }

    [Test]
    public void GetNewName_MoreThanOnce ()
    {
      var identifier1 = _queryModel.GetNewName ("test");
      var identifier2 = _queryModel.GetNewName ("test");
      var identifier3 = _queryModel.GetNewName ("test");
      Assert.That (identifier1, Is.Not.EqualTo (identifier2));
      Assert.That (identifier2, Is.Not.EqualTo (identifier3));
      Assert.That (identifier1, Is.Not.EqualTo (identifier3));
    }

    [Test]
    public void GetNewName_AlreadyExists_MainFromClause ()
    {
      var mainFromClause = new MainFromClause ("test0", typeof (Student), ExpressionHelper.CreateQuerySource ().Expression);
      var queryModel = ExpressionHelper.CreateQueryModel (mainFromClause);
      var identifier = queryModel.GetNewName ("test");
      Assert.That (identifier, Is.EqualTo ("test1"));
    }

    [Test]
    public void GetNewName_AlreadyExists_ChangedMainFromClause ()
    {
      var mainFromClause = new MainFromClause ("test0", typeof (Student), ExpressionHelper.CreateQuerySource ().Expression);
      var queryModel = ExpressionHelper.CreateQueryModel ();
      queryModel.MainFromClause = mainFromClause;
      var identifier = queryModel.GetNewName ("test");
      Assert.That (identifier, Is.EqualTo ("test1"));
    }

    [Test]
    public void GetNewName_AlreadyExists_BodyClauses ()
    {
      var additionalFromClause = new AdditionalFromClause ("test0",
          typeof (Student),
          ExpressionHelper.CreateExpression());
      _queryModel.BodyClauses.Add (additionalFromClause);

      var identifier = _queryModel.GetNewName ("test");
      Assert.That (identifier, Is.EqualTo ("test1"));
    }

    [Test]
    public void GetNewName_AlreadyExists_ReplacedBodyClauses ()
    {
      _queryModel.BodyClauses.Add (ExpressionHelper.CreateAdditionalFromClause ());

      var additionalFromClause = new AdditionalFromClause ("test0",
          typeof (Student),
          ExpressionHelper.CreateExpression ());
      _queryModel.BodyClauses[0] = additionalFromClause;

      var identifier = _queryModel.GetNewName ("test");
      Assert.That (identifier, Is.EqualTo ("test1"));
    }

    [Test]
    public void InitializeWithISelectOrGroupClauseAndOrderByClause ()
    {
      var orderByClause = new OrderByClause ();
      _queryModel.BodyClauses.Add (orderByClause);

      var ordering = new Ordering (ExpressionHelper.CreateExpression (), OrderingDirection.Asc);
      orderByClause.Orderings.Add (ordering);

      Assert.That (_queryModel.SelectOrGroupClause, Is.SameAs (_selectClause));
      Assert.That (_queryModel.BodyClauses.Count, Is.EqualTo (1));
      Assert.That (_queryModel.BodyClauses, List.Contains (orderByClause));
    }

    [Test]
    public void AddSeveralOrderByClauses ()
    {

      IBodyClause orderByClause1 = ExpressionHelper.CreateOrderByClause ();
      IBodyClause orderByClause2 = ExpressionHelper.CreateOrderByClause ();

      _queryModel.BodyClauses.Add (orderByClause1);
      _queryModel.BodyClauses.Add (orderByClause2);

      Assert.That (_queryModel.BodyClauses.Count, Is.EqualTo (2));
      Assert.That (_queryModel.BodyClauses, Is.EqualTo (new object[] { orderByClause1, orderByClause2 }));
    }

    [Test]
    public void AddBodyClause ()
    {
      IBodyClause clause = ExpressionHelper.CreateWhereClause ();
      _queryModel.BodyClauses.Add (clause);

      Assert.That (_queryModel.BodyClauses.Count, Is.EqualTo (1));
      Assert.That (_queryModel.BodyClauses, List.Contains (clause));
    }

    [Test]
    [ExpectedException (typeof (ArgumentNullException))]
    public void AddBodyClause_Null ()
    {
      _queryModel.BodyClauses.Add (null);
    }

    [Test]
    [ExpectedException (typeof (ArgumentNullException))]
    public void SetBodyClause_Null ()
    {
      _queryModel.BodyClauses.Add (ExpressionHelper.CreateWhereClause ());
      _queryModel.BodyClauses[0] = null;
    }
  }
}
