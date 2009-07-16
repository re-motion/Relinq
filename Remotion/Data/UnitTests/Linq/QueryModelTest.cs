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
using Remotion.Data.Linq.Clauses.ExecutionStrategies;
using Remotion.Data.Linq.Clauses.Expressions;
using Remotion.Data.Linq.Clauses.ResultOperators;
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
    private QuerySourceMapping _querySourceMapping;

    [SetUp]
    public void SetUp ()
    {
      _mainFromClause = ExpressionHelper.CreateMainFromClause();
      _selectClause = ExpressionHelper.CreateSelectClause();
      _queryModel = new QueryModel (_mainFromClause, _selectClause);
      _querySourceMapping = new QuerySourceMapping();
    }

    [Test]
    public void Initialize ()
    {
      Assert.That (_queryModel.MainFromClause, Is.SameAs (_mainFromClause));
      Assert.That (_queryModel.SelectClause, Is.SameAs (_selectClause));
    }

    [Test]
    public void Accept ()
    {
      var repository = new MockRepository();
      var visitorMock = repository.StrictMock<IQueryModelVisitor>();

      visitorMock.Expect (mock => mock.VisitQueryModel (_queryModel));

      repository.ReplayAll();

      _queryModel.Accept (visitorMock);

      repository.VerifyAll();
    }

    [Test]
    public void GetResultType_FromSelectClause ()
    {
      Assert.That (_queryModel.GetResultType(), Is.EqualTo (typeof (IQueryable<int>)));
    }

    [Test]
    public void GetResultType_FromResultOperator ()
    {
      _queryModel.ResultOperators.Add (new CountResultOperator ());
      Assert.That (_queryModel.GetResultType(), Is.EqualTo (typeof (int)));
    }

    [Test]
    public void GetResultType_FromMultipleResultOperators ()
    {
      _queryModel.ResultOperators.Add (new DistinctResultOperator());
      _queryModel.ResultOperators.Add (new SingleResultOperator (false));
      Assert.That (_queryModel.GetResultType(), Is.EqualTo (typeof (int)));
    }

    [Test]
    public new void ToString ()
    {
      var queryModel = new QueryModel (new MainFromClause ("x", typeof (Student), Expression.Constant (0)), 
          new SelectClause (Expression.Constant (0)));
      Assert.That (queryModel.ToString(), Is.EqualTo ("from Student x in 0 select 0"));
    }

    [Test]
    public void ToString_WithBodyClauses ()
    {
      var queryModel = new QueryModel (new MainFromClause ("x", typeof (Student), Expression.Constant (0)),
          new SelectClause (Expression.Constant (0)));
      queryModel.BodyClauses.Add (new WhereClause (Expression.Constant (false)));
      var orderByClause = new OrderByClause ();
      orderByClause.Orderings.Add (new Ordering (Expression.Constant (1), OrderingDirection.Asc));
      queryModel.BodyClauses.Add (orderByClause);
      
      Assert.That (queryModel.ToString (), Is.EqualTo ("from Student x in 0 where False orderby 1 asc select 0"));
    }

    [Test]
    public void ToString_WithResultOperators ()
    {
      var queryModel = new QueryModel (new MainFromClause ("x", typeof (Student), Expression.Constant (0)),
          new SelectClause (Expression.Constant (0)));
      queryModel.ResultOperators.Add (new DistinctResultOperator ());
      queryModel.ResultOperators.Add (new CountResultOperator ());

      Assert.That (queryModel.ToString (), Is.EqualTo ("from Student x in 0 select 0 => Distinct() => Count()"));
    }

    [Test]
    public void Clone_ReturnsNewQueryModel ()
    {
      var queryModel = ExpressionHelper.CreateQueryModel();
      var clone = queryModel.Clone();

      Assert.That (clone, Is.Not.Null);
      Assert.That (clone, Is.Not.SameAs (queryModel));
    }

    [Test]
    public void Clone_HasCloneForMainFromClause ()
    {
      var clone = _queryModel.Clone();

      Assert.That (clone.MainFromClause, Is.Not.SameAs (_queryModel.MainFromClause));
      Assert.That (clone.MainFromClause.ItemName, Is.EqualTo (_queryModel.MainFromClause.ItemName));
      Assert.That (clone.MainFromClause.ItemType, Is.SameAs (_queryModel.MainFromClause.ItemType));
    }

    [Test]
    public void Clone_HasCloneForMainFromClause_PassesMapping ()
    {
      var clone = _queryModel.Clone (_querySourceMapping);
      Assert.That (
          ((QuerySourceReferenceExpression) _querySourceMapping.GetExpression (_queryModel.MainFromClause)).ReferencedClause,
          Is.SameAs (clone.MainFromClause));
    }

    [Test]
    public void Clone_HasCloneForSelectClause ()
    {
      var selectClause = _queryModel.SelectClause;
      var clone = _queryModel.Clone();

      Assert.That (clone.SelectClause, Is.Not.SameAs (_queryModel.SelectClause));
      var cloneSelectClause = clone.SelectClause;
      Assert.That (cloneSelectClause.Selector, Is.EqualTo (selectClause.Selector));
    }

    [Test]
    public void Clone_HasCloneForSelectClause_TransformExpressions ()
    {
      var oldReferencedClause = ExpressionHelper.CreateMainFromClause();
      _queryModel.SelectClause.Selector = new QuerySourceReferenceExpression (oldReferencedClause);

      var newReferenceExpression = new QuerySourceReferenceExpression (ExpressionHelper.CreateMainFromClause());
      _querySourceMapping.AddMapping (oldReferencedClause, newReferenceExpression);

      var clone = _queryModel.Clone (_querySourceMapping);
      Assert.That (clone.SelectClause.Selector, Is.SameAs (newReferenceExpression));
    }

    [Test]
    public void Clone_HasClonesForBodyClauses ()
    {
      var additionalFromClause = ExpressionHelper.CreateAdditionalFromClause();
      var whereClause = ExpressionHelper.CreateWhereClause();
      _queryModel.BodyClauses.Add (additionalFromClause);
      _queryModel.BodyClauses.Add (whereClause);

      var clone = _queryModel.Clone();
      var clonedAdditionalFromClause = (AdditionalFromClause) clone.BodyClauses[0];
      var clonedWhereClause = (WhereClause) clone.BodyClauses[1];

      Assert.That (clonedAdditionalFromClause, Is.Not.SameAs (additionalFromClause));
      Assert.That (clonedAdditionalFromClause.ItemName, Is.EqualTo (additionalFromClause.ItemName));
      Assert.That (clonedAdditionalFromClause.ItemType, Is.SameAs (additionalFromClause.ItemType));
      Assert.That (clonedWhereClause, Is.Not.SameAs (whereClause));
      Assert.That (clonedWhereClause.Predicate, Is.EqualTo (whereClause.Predicate));
    }

    [Test]
    public void Clone_HasCloneForBodyClauses_TransformExpressions ()
    {
      var bodyClause = ExpressionHelper.CreateWhereClause();
      var oldReferencedClause = ExpressionHelper.CreateMainFromClause();
      bodyClause.Predicate = new QuerySourceReferenceExpression (oldReferencedClause);
      _queryModel.BodyClauses.Add (bodyClause);

      var newReferenceExpression = new QuerySourceReferenceExpression (ExpressionHelper.CreateMainFromClause());
      _querySourceMapping.AddMapping (oldReferencedClause, newReferenceExpression);

      var clone = _queryModel.Clone (_querySourceMapping);
      Assert.That (((WhereClause) clone.BodyClauses[0]).Predicate, Is.SameAs (newReferenceExpression));
    }

    [Test]
    public void Clone_ResultOperators ()
    {
      var resultOperator1 = ExpressionHelper.CreateResultOperator ();
      _queryModel.ResultOperators.Add (resultOperator1);
      var resultOperator2 = ExpressionHelper.CreateResultOperator ();
      _queryModel.ResultOperators.Add (resultOperator2);

      var clone = _queryModel.Clone ();

      Assert.That (clone.ResultOperators.Count, Is.EqualTo (2));
      Assert.That (clone.ResultOperators[0], Is.Not.SameAs (resultOperator1));
      Assert.That (clone.ResultOperators[0].GetType (), Is.SameAs (resultOperator1.GetType ()));
      Assert.That (clone.ResultOperators[1], Is.Not.SameAs (resultOperator2));
      Assert.That (clone.ResultOperators[1].GetType (), Is.SameAs (resultOperator2.GetType ()));
    }

    [Test]
    public void Clone_ResultOperators_PassesMapping ()
    {
      var resultOperatorMock = MockRepository.GenerateMock<ResultOperatorBase> (CollectionExecutionStrategy.Instance);
      _queryModel.ResultOperators.Add (resultOperatorMock);

      resultOperatorMock
          .Expect (mock => mock.Clone (Arg<CloneContext>.Matches (cc => cc.QuerySourceMapping == _querySourceMapping)))
          .Return (ExpressionHelper.CreateResultOperator ());
      resultOperatorMock.Replay ();

      _queryModel.Clone (_querySourceMapping);

      resultOperatorMock.VerifyAllExpectations ();
    }


    [Test]
    public void TransformExpressions ()
    {
      Func<Expression, Expression> transformation = ex => ex;
      var fromClauseMock = MockRepository.GenerateMock<MainFromClause> ("item", typeof (string), ExpressionHelper.CreateExpression());
      var bodyClauseMock = MockRepository.GenerateMock<IBodyClause>();
      var selectClauseMock = MockRepository.GenerateMock<SelectClause> (ExpressionHelper.CreateExpression());

      var queryModel = new QueryModel (fromClauseMock, selectClauseMock);
      queryModel.BodyClauses.Add (bodyClauseMock);

      queryModel.TransformExpressions (transformation);

      fromClauseMock.AssertWasCalled (mock => mock.TransformExpressions (transformation));
      bodyClauseMock.AssertWasCalled (mock => mock.TransformExpressions (transformation));
      selectClauseMock.AssertWasCalled (mock => mock.TransformExpressions (transformation));
    }

    [Test]
    public void TransformExpressions_PassedToResultOperators ()
    {
      Func<Expression, Expression> transformer = ex => ex;
      var resultOperatorMock = MockRepository.GenerateMock<ResultOperatorBase> (CollectionExecutionStrategy.Instance);
      _queryModel.ResultOperators.Add (resultOperatorMock);
      resultOperatorMock.Expect (mock => mock.TransformExpressions (transformer));

      resultOperatorMock.Replay ();

      _queryModel.TransformExpressions (transformer);

      resultOperatorMock.VerifyAllExpectations ();
    }


    [Test]
    public void SelectOrGroupClause_Set ()
    {
      var newSelectClause = ExpressionHelper.CreateSelectClause();
      _queryModel.SelectClause = newSelectClause;

      Assert.That (_queryModel.SelectClause, Is.SameAs (newSelectClause));
    }

    [Test]
    [ExpectedException (typeof (ArgumentNullException))]
    public void SelectOrGroupClause_Set_Null ()
    {
      _queryModel.SelectClause = null;
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
      var mainFromClause = new MainFromClause ("test0", typeof (Student), ExpressionHelper.CreateQuerySource().Expression);
      var queryModel = ExpressionHelper.CreateQueryModel (mainFromClause);
      var identifier = queryModel.GetNewName ("test");
      Assert.That (identifier, Is.EqualTo ("test1"));
    }

    [Test]
    public void GetNewName_AlreadyExists_ChangedMainFromClause ()
    {
      var mainFromClause = new MainFromClause ("test0", typeof (Student), ExpressionHelper.CreateQuerySource().Expression);
      var queryModel = ExpressionHelper.CreateQueryModel();
      queryModel.MainFromClause = mainFromClause;
      var identifier = queryModel.GetNewName ("test");
      Assert.That (identifier, Is.EqualTo ("test1"));
    }

    [Test]
    public void GetNewName_AlreadyExists_BodyClauses ()
    {
      var additionalFromClause = new AdditionalFromClause (
          "test0",
          typeof (Student),
          ExpressionHelper.CreateExpression());
      _queryModel.BodyClauses.Add (additionalFromClause);

      var identifier = _queryModel.GetNewName ("test");
      Assert.That (identifier, Is.EqualTo ("test1"));
    }

    [Test]
    public void GetNewName_AlreadyExists_ReplacedBodyClauses ()
    {
      _queryModel.BodyClauses.Add (ExpressionHelper.CreateAdditionalFromClause());

      var additionalFromClause = new AdditionalFromClause (
          "test0",
          typeof (Student),
          ExpressionHelper.CreateExpression());
      _queryModel.BodyClauses[0] = additionalFromClause;

      var identifier = _queryModel.GetNewName ("test");
      Assert.That (identifier, Is.EqualTo ("test1"));
    }

    [Test]
    public void InitializeWithISelectOrGroupClauseAndOrderByClause ()
    {
      var orderByClause = new OrderByClause();
      _queryModel.BodyClauses.Add (orderByClause);

      var ordering = new Ordering (ExpressionHelper.CreateExpression(), OrderingDirection.Asc);
      orderByClause.Orderings.Add (ordering);

      Assert.That (_queryModel.SelectClause, Is.SameAs (_selectClause));
      Assert.That (_queryModel.BodyClauses.Count, Is.EqualTo (1));
      Assert.That (_queryModel.BodyClauses, List.Contains (orderByClause));
    }

    [Test]
    public void AddSeveralOrderByClauses ()
    {
      IBodyClause orderByClause1 = ExpressionHelper.CreateOrderByClause();
      IBodyClause orderByClause2 = ExpressionHelper.CreateOrderByClause();

      _queryModel.BodyClauses.Add (orderByClause1);
      _queryModel.BodyClauses.Add (orderByClause2);

      Assert.That (_queryModel.BodyClauses.Count, Is.EqualTo (2));
      Assert.That (_queryModel.BodyClauses, Is.EqualTo (new object[] { orderByClause1, orderByClause2 }));
    }

    [Test]
    public void AddBodyClause ()
    {
      IBodyClause clause = ExpressionHelper.CreateWhereClause();
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
      _queryModel.BodyClauses.Add (ExpressionHelper.CreateWhereClause());
      _queryModel.BodyClauses[0] = null;
    }

    [Test]
    [ExpectedException (typeof (ArgumentNullException))]
    public void AddResultOperator_Null_ThrowsArgumentNullException ()
    {
      _queryModel.ResultOperators.Add (null);
    }

    [Test]
    [ExpectedException (typeof (ArgumentNullException))]
    public void SetResultOperator_Null_ThrowsArgumentNullException ()
    {
      var resultOperator = new DistinctResultOperator ();
      _queryModel.ResultOperators.Add (resultOperator);
      _queryModel.ResultOperators[0] = null;
    }

    [Test]
    public void GetExecutionStrategy ()
    {
      Assert.That (_queryModel.GetExecutionStrategy (), Is.SameAs (CollectionExecutionStrategy.Instance));
    }

    [Test]
    public void GetExecutionStrategy_WithResultOperators ()
    {
      var firstOperator = new FirstResultOperator (true);
      _queryModel.ResultOperators.Add (firstOperator);

      Assert.That (_queryModel.GetExecutionStrategy (), Is.SameAs (firstOperator.ExecutionStrategy));
    }

    [Test]
    public void GetExecutionStrategy_WithManyResultOperators ()
    {
      var takeOperator = new TakeResultOperator (7);
      var distinctOperator = new DistinctResultOperator ();
      var countOperator = new CountResultOperator ();
      _queryModel.ResultOperators.Add (takeOperator);
      _queryModel.ResultOperators.Add (distinctOperator);
      _queryModel.ResultOperators.Add (countOperator);

      Assert.That (_queryModel.GetExecutionStrategy (), Is.SameAs (countOperator.ExecutionStrategy));
    }
  }
}