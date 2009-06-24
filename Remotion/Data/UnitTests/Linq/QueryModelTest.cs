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
using System.Linq.Expressions;
using NUnit.Framework;
using Remotion.Data.Linq;
using Remotion.Data.Linq.Clauses.Expressions;
using Remotion.Data.Linq.StringBuilding;
using Rhino.Mocks;
using Remotion.Data.Linq.Clauses;
using NUnit.Framework.SyntaxHelpers;

namespace Remotion.Data.UnitTests.Linq
{
  [TestFixture]
  public class QueryModelTest
  {
    private Expression _expressionTree;
    private MainFromClause _mainFromClause;
    private SelectClause _selectClause;
    private QueryModel _queryModel;
    private ClonedClauseMapping _clonedClauseMapping;

    [SetUp]
    public void SetUp ()
    {
      _expressionTree = ExpressionHelper.CreateExpression ();
      _mainFromClause = ExpressionHelper.CreateMainFromClause ();
      _selectClause = ExpressionHelper.CreateSelectClause (_mainFromClause);
      _queryModel = new QueryModel (typeof (IQueryable<string>), _mainFromClause, _selectClause);
      _queryModel.SetExpressionTree(_expressionTree);
      _clonedClauseMapping = new ClonedClauseMapping ();
    }

    [Test]
    public void Initialize_WithFromClauseAndBody ()
    {
      Assert.AreSame (_mainFromClause, _queryModel.MainFromClause);
      Assert.AreSame (_selectClause, _queryModel.SelectOrGroupClause);
      Assert.IsNotNull (_queryModel.GetExpressionTree ());
      Assert.AreEqual (typeof (IQueryable<string>), _queryModel.ResultType);
    }

    [Test]
    public void Initialize_WithExpressionTree ()
    {
      Assert.AreSame (_mainFromClause, _queryModel.MainFromClause);
      Assert.AreSame (_selectClause, _queryModel.SelectOrGroupClause);
      Assert.AreSame (_expressionTree, _queryModel.GetExpressionTree ());
    }

    [Test]
    public void Accept()
    {
      var repository = new MockRepository ();
      var visitorMock = repository.StrictMock<IQueryVisitor> ();

      visitorMock.Expect (mock => mock.VisitQueryModel (_queryModel));

      repository.ReplayAll ();

      _queryModel.Accept (visitorMock);

      repository.VerifyAll ();
    }

    [Test]
    public void Override_ToString()
    {
      var sv = new StringBuildingQueryVisitor();
      sv.VisitQueryModel (_queryModel);
      Assert.AreEqual (sv.ToString (), _queryModel.ToString ());
    }

    [Test]
    public void GetExpressionTree_ForSuppliedTree ()
    {
      Expression expressionTree = _queryModel.GetExpressionTree ();
      Assert.That (expressionTree, Is.Not.Null);
      Assert.That (expressionTree, Is.SameAs (_expressionTree));
    }

    [Test]
    public void GetExpressionTree_ForConstructedTree ()
    {
      var queryModel = new QueryModel (typeof (IQueryable<Student>), ExpressionHelper.CreateMainFromClause (), ExpressionHelper.CreateSelectClause ());

      Expression expressionTree = queryModel.GetExpressionTree();
      Assert.That (expressionTree, Is.Not.Null);
      Assert.That (expressionTree, Is.InstanceOfType (typeof (ConstructedQueryExpression)));
    }

    [Test]
    public void GetExpressionTree_ForConstructedTree_Twice ()
    {
      var queryModel = new QueryModel (typeof (IQueryable<Student>), ExpressionHelper.CreateMainFromClause (), ExpressionHelper.CreateSelectClause ());

      Expression expressionTree1 = queryModel.GetExpressionTree ();
      Expression expressionTree2 = queryModel.GetExpressionTree ();
      Assert.That (expressionTree1, Is.SameAs (expressionTree2));
    }

    [Test]
    public void SetExpressionTree ()
    {
      var queryModel = new QueryModel (typeof (IQueryable<Student>), ExpressionHelper.CreateMainFromClause (), ExpressionHelper.CreateSelectClause ());
      var expression = ExpressionHelper.CreateExpression ();
      queryModel.SetExpressionTree (expression);

      Assert.That (queryModel.GetExpressionTree (), Is.SameAs (expression));
    }

    [Test]
    [ExpectedException (typeof (ArgumentNullException))]
    public void SetExpressionTree_Null ()
    {
      var queryModel = new QueryModel (typeof (IQueryable<Student>), ExpressionHelper.CreateMainFromClause (), ExpressionHelper.CreateSelectClause ());
      queryModel.SetExpressionTree (null);
    }


    [Test]
    [ExpectedException (typeof (InvalidOperationException), ExpectedMessage = "This QueryModel already has an expression tree. Call InvalidateExpressionTree before setting a new one.")]
    public void SetExpressionTree_AlreadyHasOne ()
    {
      _queryModel.SetExpressionTree (ExpressionHelper.CreateExpression());
    }

    [Test]
    public void SetExpressionTree_Same ()
    {
      var previousTree = _queryModel.GetExpressionTree();
      _queryModel.SetExpressionTree (previousTree);
      Assert.That (_queryModel.GetExpressionTree (), Is.SameAs (previousTree));
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
    public void Clone_HasSameResultTypeAndExpressionTree ()
    {
      var clone = _queryModel.Clone ();

      Assert.That (clone.ResultType, Is.SameAs (_queryModel.ResultType));
      Assert.That (clone.GetExpressionTree(), Is.SameAs (_queryModel.GetExpressionTree()));
    }

    [Test]
    public void Clone_HasCloneForMainFromClause ()
    {
      var clone = _queryModel.Clone ();

      Assert.That (clone.MainFromClause, Is.Not.SameAs (_queryModel.MainFromClause));
      Assert.That (clone.MainFromClause.Identifier, Is.EqualTo (_queryModel.MainFromClause.Identifier));
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
      Assert.That (cloneSelectClause.PreviousClause, Is.SameAs (clone.MainFromClause));
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
      var additionalFromClause = ExpressionHelper.CreateAdditionalFromClause (_mainFromClause);
      var whereClause = ExpressionHelper.CreateWhereClause(additionalFromClause);
      _queryModel.AddBodyClause (additionalFromClause);
      _queryModel.AddBodyClause (whereClause);
      _selectClause.PreviousClause = whereClause;

      var clone = _queryModel.Clone ();
      var clonedAdditionalFromClause = (AdditionalFromClause) clone.BodyClauses[0];
      var clonedWhereClause = (WhereClause) clone.BodyClauses[1];

      Assert.That (clonedAdditionalFromClause, Is.Not.SameAs (additionalFromClause));
      Assert.That (clonedAdditionalFromClause.Identifier, Is.SameAs (additionalFromClause.Identifier));
      Assert.That (clonedAdditionalFromClause.PreviousClause, Is.SameAs (clone.MainFromClause));
      Assert.That (clonedWhereClause, Is.Not.SameAs (whereClause));
      Assert.That (clonedWhereClause.Predicate, Is.EqualTo (whereClause.Predicate));
      Assert.That (clonedWhereClause.PreviousClause, Is.SameAs (clonedAdditionalFromClause));

      Assert.That (clone.SelectOrGroupClause.PreviousClause, Is.SameAs (clonedWhereClause));
    }

    [Test]
    public void Clone_HasCloneForBodyClauses_PassesMapping ()
    {
      var bodyClause = ExpressionHelper.CreateWhereClause(_mainFromClause);
      _queryModel.AddBodyClause (bodyClause);
      _selectClause.PreviousClause = bodyClause;

      var clone = _queryModel.Clone (_clonedClauseMapping);
      Assert.That (_clonedClauseMapping.GetClause (_queryModel.BodyClauses[0]), Is.SameAs (clone.BodyClauses[0]));
    }
    
    [Test]
    public void InvalidateExpressionTree ()
    {
      Assert.That (_queryModel.GetExpressionTree (), Is.SameAs (_expressionTree));
      _queryModel.InvalidateExpressionTree ();
      Assert.That (_queryModel.GetExpressionTree (), Is.Not.SameAs (_expressionTree));
      Assert.That (_queryModel.GetExpressionTree (), Is.InstanceOfType (typeof (ConstructedQueryExpression)));
    }

    [Test]
    public void AddBodyClauses_SetsExpressionTreeToNull ()
    {
      var bodyClause = ExpressionHelper.CreateAdditionalFromClause ();
      _queryModel.AddBodyClause (bodyClause);
      Assert.That (_queryModel.GetExpressionTree(), Is.InstanceOfType (typeof (ConstructedQueryExpression)));
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
    public void SelectOrGroupClause_Set_InvalidatesExpressionTree ()
    {
      var expressionTreeBefore = _queryModel.GetExpressionTree ();
      _queryModel.SelectOrGroupClause = ExpressionHelper.CreateSelectClause ();
      var expressionTreeAfter = _queryModel.GetExpressionTree ();

      Assert.That (expressionTreeAfter, Is.Not.SameAs (expressionTreeBefore));
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
      var mainFromClause = new MainFromClause (Expression.Parameter (typeof (Student), "test0"), ExpressionHelper.CreateQuerySource ().Expression);
      var queryModel = ExpressionHelper.CreateQueryModel (mainFromClause);
      var identifier = queryModel.GetNewName ("test");
      Assert.That (identifier, Is.EqualTo ("test1"));
    }

    [Test]
    public void GetNewName_AlreadyExists_BodyClauses ()
    {
      var additionalFromClause = new AdditionalFromClause (
          _queryModel.MainFromClause, 
          Expression.Parameter (typeof (Student), "test0"), 
          ExpressionHelper.CreateExpression());
      _queryModel.AddBodyClause (additionalFromClause);

      var identifier = _queryModel.GetNewName ("test");
      Assert.That (identifier, Is.EqualTo ("test1"));
    }

    [Test]
    public void InitializeWithISelectOrGroupClauseAndOrderByClause ()
    {
      var orderByClause = new OrderByClause (_queryModel.MainFromClause);
      var ordering = new Ordering (orderByClause, ExpressionHelper.CreateExpression (), OrderingDirection.Asc);
      orderByClause.Orderings.Add (ordering);

      _queryModel.AddBodyClause (orderByClause);

      Assert.That (_queryModel.SelectOrGroupClause, Is.SameAs (_selectClause));
      Assert.That (_queryModel.BodyClauses.Count, Is.EqualTo (1));
      Assert.That (_queryModel.BodyClauses, List.Contains (orderByClause));
    }

    [Test]
    public void AddSeveralOrderByClauses ()
    {

      IBodyClause orderByClause1 = ExpressionHelper.CreateOrderByClause ();
      IBodyClause orderByClause2 = ExpressionHelper.CreateOrderByClause ();

      _queryModel.AddBodyClause (orderByClause1);
      _queryModel.AddBodyClause (orderByClause2);

      Assert.That (_queryModel.BodyClauses.Count, Is.EqualTo (2));
      Assert.That (_queryModel.BodyClauses, Is.EqualTo (new object[] { orderByClause1, orderByClause2 }));
    }

    [Test]
    public void AddBodyClause ()
    {
      IBodyClause clause = ExpressionHelper.CreateWhereClause ();
      _queryModel.AddBodyClause (clause);

      Assert.That (_queryModel.BodyClauses.Count, Is.EqualTo (1));
      Assert.That (_queryModel.BodyClauses, List.Contains (clause));
    }
  }
}
