// Copyright (c) rubicon IT GmbH, www.rubicon.eu
//
// See the NOTICE file distributed with this work for additional information
// regarding copyright ownership.  rubicon licenses this file to you under 
// the Apache License, Version 2.0 (the "License"); you may not use this 
// file except in compliance with the License.  You may obtain a copy of the 
// License at
//
//   http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software 
// distributed under the License is distributed on an "AS IS" BASIS, WITHOUT 
// WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.  See the 
// License for the specific language governing permissions and limitations
// under the License.
// 
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using NUnit.Framework;
using Remotion.Linq.Clauses;
using Remotion.Linq.Clauses.Expressions;
using Remotion.Linq.Clauses.ResultOperators;
using Remotion.Linq.Clauses.StreamedData;
using Remotion.Linq.Development.UnitTesting;
using Remotion.Linq.Development.UnitTesting.Clauses.Expressions;
using Remotion.Linq.UnitTests.Parsing.ExpressionVisitors;
using Remotion.Linq.UnitTests.TestDomain;
using Rhino.Mocks;

namespace Remotion.Linq.UnitTests
{
  [TestFixture]
  public class QueryModelTest
  {
    private MainFromClause _mainFromClause;
    private SelectClause _selectClause;
    private QueryModel _queryModel;

    [SetUp]
    public void SetUp ()
    {
      _mainFromClause = ExpressionHelper.CreateMainFromClause_Int();
      _selectClause = ExpressionHelper.CreateSelectClause();
      _queryModel = new QueryModel (_mainFromClause, _selectClause);
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
    public void GetOutputDataInfo_FromSelectClause ()
    {
      var outputDataInfo = _queryModel.GetOutputDataInfo ();
      Assert.That (outputDataInfo, Is.InstanceOf (typeof (StreamedSequenceInfo)));
      Assert.That (outputDataInfo.DataType, Is.SameAs (typeof (IQueryable<int>)));
      Assert.That (((StreamedSequenceInfo) outputDataInfo).ItemExpression, Is.SameAs (_queryModel.SelectClause.Selector));
    }

    [Test]
    public void GetOutputDataInfo_FromResultOperator ()
    {
      _queryModel.ResultOperators.Add (new CountResultOperator ());

      var outputDataInfo = _queryModel.GetOutputDataInfo ();
      Assert.That (outputDataInfo, Is.InstanceOf (typeof (StreamedValueInfo)));
      Assert.That (outputDataInfo.DataType, Is.SameAs (typeof (int)));
    }

    [Test]
    public void GetOutputDataInfo_FromMultipleResultOperators ()
    {
      _queryModel.ResultOperators.Add (new DistinctResultOperator ());
      _queryModel.ResultOperators.Add (new SingleResultOperator (false));

      var outputDataInfo = _queryModel.GetOutputDataInfo ();
      Assert.That (outputDataInfo, Is.InstanceOf (typeof (StreamedValueInfo)));
      Assert.That (outputDataInfo.DataType, Is.SameAs (typeof (int)));
    }

    [Test]
    public void GetOutputDataInfo_WithResultTypeOverride ()
    {
      _queryModel.ResultTypeOverride = typeof (List<>);
      
      var outputDataInfo = _queryModel.GetOutputDataInfo ();
      
      Assert.That (outputDataInfo, Is.InstanceOf (typeof (StreamedSequenceInfo)));
      Assert.That (outputDataInfo.DataType, Is.SameAs (typeof (List<int>)));
      Assert.That (((StreamedSequenceInfo) outputDataInfo).ItemExpression, Is.SameAs (_queryModel.SelectClause.Selector));
    }

    [Test]
    public void GetOutputDataInfo_WithResultTypeOverride_Null ()
    {
      _queryModel.ResultTypeOverride = typeof (IEnumerable<object>);
      _queryModel.ResultTypeOverride = null;

      var outputDataInfo = _queryModel.GetOutputDataInfo ();

      Assert.That (outputDataInfo, Is.InstanceOf (typeof (StreamedSequenceInfo)));
      Assert.That (outputDataInfo.DataType, Is.SameAs (typeof (IQueryable<int>)));
      Assert.That (((StreamedSequenceInfo) outputDataInfo).ItemExpression, Is.SameAs (_queryModel.SelectClause.Selector));
    }

    [Test]
    public void GetOutputDataInfo_WithIncompatibleResultTypeOverride_AndSetsResultTypeOverrideToNull ()
    {
      _queryModel.ResultTypeOverride = typeof (int);

      var invalidOperationException = Assert.Throws<InvalidOperationException> (() => _queryModel.GetOutputDataInfo());

      Assert.That (_queryModel.ResultTypeOverride, Is.Null);

      Assert.That (
          invalidOperationException.Message,
          Is.EqualTo (
              "The query model's result type cannot be changed to 'System.Int32'. "
              + "The result type may only be overridden and set to values compatible with the ResultOperators' current data type ('System.Linq.IQueryable`1[System.Int32]')."));
      Assert.That (invalidOperationException.InnerException, Is.InstanceOf<ArgumentException>());
      Assert.That (
          invalidOperationException.InnerException.Message,
          Is.EqualTo ("'System.Int32' cannot be used as the data type for a sequence with an ItemExpression of type 'System.Int32'.\r\nParameter name: dataType"));
    }

    [Test]
    public new void ToString ()
    {
      var queryModel = new QueryModel (new MainFromClause ("x", typeof (Cook), Expression.Constant (0)), 
          new SelectClause (Expression.Constant (0)));
      Assert.That (queryModel.ToString(), Is.EqualTo ("from Cook x in 0 select 0"));
    }

    [Test]
    public void ToString_WithBodyClauses ()
    {
      var queryModel = new QueryModel (new MainFromClause ("x", typeof (Cook), Expression.Constant (0)),
          new SelectClause (Expression.Constant (0)));
      queryModel.BodyClauses.Add (new WhereClause (Expression.Constant (false)));
      var orderByClause = new OrderByClause ();
      orderByClause.Orderings.Add (new Ordering (Expression.Constant (1), OrderingDirection.Asc));
      queryModel.BodyClauses.Add (orderByClause);
      
      Assert.That (queryModel.ToString (), Is.EqualTo ("from Cook x in 0 where False orderby 1 asc select 0"));
    }

    [Test]
    public void ToString_WithResultOperators ()
    {
      var queryModel = new QueryModel (new MainFromClause ("x", typeof (Cook), Expression.Constant (0)),
          new SelectClause (Expression.Constant (0)));
      queryModel.ResultOperators.Add (new DistinctResultOperator ());
      queryModel.ResultOperators.Add (new CountResultOperator ());

      Assert.That (queryModel.ToString (), Is.EqualTo ("from Cook x in 0 select 0 => Distinct() => Count()"));
    }

    [Test]
    public void ToString_IdentityQuery ()
    {
      var mainFromClause = new MainFromClause ("x", typeof (Cook), Expression.Constant (0));
      var queryModel = new QueryModel (mainFromClause, new SelectClause (new QuerySourceReferenceExpression (mainFromClause)));
      Assert.That (queryModel.ToString (), Is.EqualTo ("0"));
    }

    [Test]
    public void ToString_IdentityQuery_UsesFormatter ()
    {
      var referencedSource = new MainFromClause ("y", typeof (Cook), Expression.Constant (0));
      var mainFromClause = new MainFromClause ("x", typeof (Cook), new QuerySourceReferenceExpression (referencedSource));
      var queryModel = new QueryModel (mainFromClause, new SelectClause (new QuerySourceReferenceExpression (mainFromClause)));
      Assert.That (queryModel.ToString (), Is.EqualTo ("[y]"));
    }

    [Test]
    public void Clone_ReturnsNewQueryModel ()
    {
      var queryModel = ExpressionHelper.CreateQueryModel<Cook>();
      var clone = queryModel.Clone();

      Assert.That (clone, Is.Not.Null);
      Assert.That (clone, Is.Not.SameAs (queryModel));
    }

    [Test]
    public void Clone_Keeps_ResultTypeOverride ()
    {
      var queryModel = ExpressionHelper.CreateQueryModel<Cook> ();
      queryModel.ResultTypeOverride = typeof (List<Cook>);
      var clone = queryModel.Clone ();

      Assert.That (clone.ResultTypeOverride, Is.SameAs (queryModel.ResultTypeOverride));
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
      var querySourceMapping = new QuerySourceMapping();

      var clone = _queryModel.Clone (querySourceMapping);

      Assert.That (
          ((QuerySourceReferenceExpression) querySourceMapping.GetExpression (_queryModel.MainFromClause)).ReferencedQuerySource,
          Is.SameAs (clone.MainFromClause));
    }

    [Test]
    public void Clone_ReplacesQuerySourceReferenceExpression ()
    {
      var originalExpression = new QuerySourceReferenceExpression (ExpressionHelper.CreateMainFromClause_Int());
      _queryModel.BodyClauses.Add (new AdditionalFromClause ("inner", typeof (int), originalExpression));

      var querySourceMapping = new QuerySourceMapping();
      var newExpression = new QuerySourceReferenceExpression (ExpressionHelper.CreateMainFromClause_Int());
      querySourceMapping.AddMapping (originalExpression.ReferencedQuerySource, newExpression);

      var clone = _queryModel.Clone (querySourceMapping);

      Assert.That (
          ((AdditionalFromClause) clone.BodyClauses[0]).FromExpression,
          Is.SameAs (newExpression));
    }

    [Test]
    public void Clone_ReplacesNestedQuerySourceReferenceExpression ()
    {
      var originalExpression = new QuerySourceReferenceExpression (ExpressionHelper.CreateMainFromClause_Int());
      _queryModel.BodyClauses.Add (new AdditionalFromClause ("inner", typeof (int), Expression.Not (originalExpression)));

      var querySourceMapping = new QuerySourceMapping();
      var newExpression = new QuerySourceReferenceExpression (ExpressionHelper.CreateMainFromClause_Int());
      querySourceMapping.AddMapping (originalExpression.ReferencedQuerySource, newExpression);

      var clone = _queryModel.Clone (querySourceMapping);

      Assert.That (
          ((UnaryExpression) ((AdditionalFromClause) clone.BodyClauses[0]).FromExpression).Operand,
          Is.SameAs (newExpression));
    }

    [Test]
    public void Clone_DoesNotHaveMappingForQuerySourceReferenceExpression_IgnoresExpression ()
    {
      var unmappedExpression = new QuerySourceReferenceExpression (ExpressionHelper.CreateMainFromClause_Int());
      _queryModel.BodyClauses.Add (new AdditionalFromClause ("inner", typeof (int), unmappedExpression));

      var clone = _queryModel.Clone (new QuerySourceMapping());

      Assert.That (
          ((AdditionalFromClause) clone.BodyClauses[0]).FromExpression,
          Is.SameAs (unmappedExpression));
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
      var oldReferencedClause = ExpressionHelper.CreateMainFromClause_Int();
      _queryModel.SelectClause.Selector = new QuerySourceReferenceExpression (oldReferencedClause);

      var querySourceMapping = new QuerySourceMapping();
      var newReferenceExpression = new QuerySourceReferenceExpression (ExpressionHelper.CreateMainFromClause_Int());
      querySourceMapping.AddMapping (oldReferencedClause, newReferenceExpression);

      var clone = _queryModel.Clone (querySourceMapping);
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
      var oldReferencedClause = ExpressionHelper.CreateMainFromClause_Int();
      bodyClause.Predicate = new QuerySourceReferenceExpression (oldReferencedClause);
      _queryModel.BodyClauses.Add (bodyClause);

      var querySourceMapping = new QuerySourceMapping();
      var newReferenceExpression = new QuerySourceReferenceExpression (ExpressionHelper.CreateMainFromClause_Int());
      querySourceMapping.AddMapping (oldReferencedClause, newReferenceExpression);

      var clone = _queryModel.Clone (querySourceMapping);
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
      var querySourceMapping = new QuerySourceMapping();

      var resultOperatorMock = MockRepository.GenerateMock<ResultOperatorBase> ();
      _queryModel.ResultOperators.Add (resultOperatorMock);

      resultOperatorMock
          .Expect (mock => mock.Clone (Arg<CloneContext>.Matches (cc => cc.QuerySourceMapping == querySourceMapping)))
          .Return (ExpressionHelper.CreateResultOperator ());
      resultOperatorMock.Replay ();

      _queryModel.Clone (querySourceMapping);

      resultOperatorMock.VerifyAllExpectations ();
    }

    [Test]
    public void Clone_ReplacesSubQueryExpression ()
    {
      var subQueryModel = ExpressionHelper.CreateQueryModel<Cook>();
      var expression = new SubQueryExpression (subQueryModel);
      var queryModel = new QueryModel (_mainFromClause, new SelectClause (expression));
      var clone = queryModel.Clone();

      Assert.That (((SubQueryExpression) clone.SelectClause.Selector).QueryModel, Is.Not.SameAs (subQueryModel));
    }

    [Test]
    public void Clone_ReplacesSubQueryExpression_WithCorrectCloneContext ()
    {
      var subQueryModel = ExpressionHelper.CreateQueryModel<Cook>();
      var referencedClause = ExpressionHelper.CreateMainFromClause_Int();
      subQueryModel.MainFromClause.FromExpression = new QuerySourceReferenceExpression (referencedClause);
      var expression = new SubQueryExpression (subQueryModel);

      var querySourceMapping = new QuerySourceMapping();
      var newReferenceExpression = new QuerySourceReferenceExpression (ExpressionHelper.CreateMainFromClause_Int());
      querySourceMapping.AddMapping (referencedClause, newReferenceExpression);

      var queryModel = new QueryModel (_mainFromClause, new SelectClause (expression));
      var clone = queryModel.Clone (querySourceMapping);

      var clonedSubQueryModel = ((SubQueryExpression) clone.SelectClause.Selector).QueryModel;
      Assert.That (clonedSubQueryModel, Is.Not.SameAs (subQueryModel));
      Assert.That (clonedSubQueryModel.MainFromClause.FromExpression, Is.SameAs (newReferenceExpression));
    }

    [Test]
    public void Clone_VisitsExtensionExpression_AndChildren ()
    {
      var referencedClause = ExpressionHelper.CreateMainFromClause_Int();
      var expression = new ReducibleExtensionExpression (new QuerySourceReferenceExpression (referencedClause));

      var querySourceMapping = new QuerySourceMapping();
      var newReferenceExpression = new QuerySourceReferenceExpression (ExpressionHelper.CreateMainFromClause_Int());
      querySourceMapping.AddMapping (referencedClause, newReferenceExpression);
      
      var queryModel = new QueryModel (_mainFromClause, new SelectClause (expression));
      var clone = queryModel.Clone (querySourceMapping);

      Assert.That (((ReducibleExtensionExpression) clone.SelectClause.Selector), Is.Not.SameAs (expression));
      Assert.That (
          ((QuerySourceReferenceExpression) ((ReducibleExtensionExpression) clone.SelectClause.Selector).Expression),
          Is.SameAs (newReferenceExpression));
    }

    [Test]
    public void Clone_VisitsUnknownExtensionExpression_Ignored ()
    {
      var expression = new UnknownExpression (typeof (object));

      var queryModel = new QueryModel (_mainFromClause, new SelectClause (expression));
      var clone = queryModel.Clone();

      Assert.That (clone.SelectClause.Selector, Is.SameAs (expression));
    }


    [Test]
    public void TransformExpressions ()
    {
      var fromClause = new MainFromClause ("item", typeof (string), ExpressionHelper.CreateExpression());
      var selectClause = new SelectClause (ExpressionHelper.CreateExpression());
      var bodyClauseMock = MockRepository.GenerateMock<IBodyClause>();

      var oldFromExpression = fromClause.FromExpression;
      var newFromExpression = ExpressionHelper.CreateExpression();

      var oldSelectorExpression = selectClause.Selector;
      var newSelectorExpression = ExpressionHelper.CreateExpression();

      Func<Expression, Expression> transformation = ex =>
      {
        if (ex == oldFromExpression)
          return newFromExpression;
        if (ex == oldSelectorExpression)
          return newSelectorExpression;
        else
          return ex;
      };

      var queryModel = new QueryModel (fromClause, selectClause);
      queryModel.BodyClauses.Add (bodyClauseMock);

      queryModel.TransformExpressions (transformation);

      Assert.That (fromClause.FromExpression, Is.SameAs (newFromExpression));
      Assert.That (selectClause.Selector, Is.SameAs (newSelectorExpression));
      bodyClauseMock.AssertWasCalled (mock => mock.TransformExpressions (transformation));
    }

    [Test]
    public void TransformExpressions_PassedToResultOperators ()
    {
      Func<Expression, Expression> transformer = ex => ex;
      var resultOperatorMock = MockRepository.GenerateMock<ResultOperatorBase> ();
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
      var mainFromClause = new MainFromClause ("test0", typeof (Cook), ExpressionHelper.CreateQueryable<Cook>().Expression);
      var queryModel = ExpressionHelper.CreateQueryModel (mainFromClause);
      var identifier = queryModel.GetNewName ("test");
      Assert.That (identifier, Is.EqualTo ("test1"));
    }

    [Test]
    public void GetNewName_AlreadyExists_ChangedMainFromClause ()
    {
      var mainFromClause = new MainFromClause ("test0", typeof (Cook), ExpressionHelper.CreateQueryable<Cook>().Expression);
      var queryModel = ExpressionHelper.CreateQueryModel<Cook>();
      queryModel.MainFromClause = mainFromClause;
      var identifier = queryModel.GetNewName ("test");
      Assert.That (identifier, Is.EqualTo ("test1"));
    }

    [Test]
    public void GetNewName_AlreadyExists_BodyClauses ()
    {
      var additionalFromClause = new AdditionalFromClause (
          "test0",
          typeof (Cook),
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
          typeof (Cook),
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
      Assert.That (_queryModel.BodyClauses, Has.Member (orderByClause));
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
      Assert.That (_queryModel.BodyClauses, Has.Member (clause));
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
    public void Execute ()
    {
      var executorMock = MockRepository.GenerateMock<IQueryExecutor>();
      var mockResult = new[] { 0, 0, 0 };
      executorMock.Expect (mock => mock.ExecuteCollection<int> (_queryModel)).Return (mockResult);

      var result = (StreamedSequence) _queryModel.Execute (executorMock);

      executorMock.VerifyAllExpectations();
      Assert.That (result.GetTypedSequence<int>().ToArray(), Is.EqualTo (mockResult));
    }

    [Test]
    public void IsIdentityQuery_True ()
    {
      var queryModel = new QueryModel (_mainFromClause, new SelectClause (new QuerySourceReferenceExpression (_mainFromClause)));
      Assert.That (queryModel.IsIdentityQuery (), Is.True);
    }

    [Test]
    public void IsIdentityQuery_True_WithResultOperator ()
    {
      var queryModel = new QueryModel (_mainFromClause, new SelectClause (new QuerySourceReferenceExpression (_mainFromClause)));
      queryModel.ResultOperators.Add (new DistinctResultOperator ());
      Assert.That (queryModel.IsIdentityQuery (), Is.True);
    }

    [Test]
    public void IsIdentityQuery_False_BodyClause ()
    {
      var queryModel = new QueryModel (_mainFromClause, new SelectClause (new QuerySourceReferenceExpression (_mainFromClause)));
      queryModel.BodyClauses.Add (new WhereClause (Expression.Constant (false)));
      Assert.That (queryModel.IsIdentityQuery (), Is.False);
    }

    [Test]
    public void IsIdentityQuery_False_Selector_NonReference ()
    {
      var queryModel = new QueryModel (_mainFromClause, new SelectClause (Expression.Constant(0)));
      Assert.That (queryModel.IsIdentityQuery (), Is.False);
    }

    [Test]
    public void IsIdentityQuery_False_Selector_WrongReference ()
    {
      var queryModel = new QueryModel (_mainFromClause, new SelectClause (new QuerySourceReferenceExpression (ExpressionHelper.CreateMainFromClause_Int())));
      Assert.That (queryModel.IsIdentityQuery (), Is.False);
    }

    [Test]
    public void ConvertToSubquery ()
    {
      var queryModel = new QueryModel (_mainFromClause, new SelectClause (new QuerySourceReferenceExpression (_mainFromClause)));

      var result = queryModel.ConvertToSubQuery ("test");

      Assert.That (result.MainFromClause.ItemName, Is.EqualTo ("test"));
      Assert.That (result.MainFromClause.ItemType, Is.SameAs(typeof(int)));
      Assert.That (result.MainFromClause.FromExpression, Is.TypeOf(typeof (SubQueryExpression)));
      Assert.That (((SubQueryExpression) result.MainFromClause.FromExpression).QueryModel, Is.SameAs(queryModel));
      Assert.That (result.SelectClause.Selector, Is.TypeOf(typeof(QuerySourceReferenceExpression)));
      Assert.That (((QuerySourceReferenceExpression) result.SelectClause.Selector).ReferencedQuerySource, Is.SameAs (result.MainFromClause));
    }

    [Test]
    public void ConvertToSubquery_CovariantSubquery ()
    {
      var queryModel = new QueryModel (_mainFromClause, new SelectClause (new QuerySourceReferenceExpression (_mainFromClause)));
      queryModel.ResultTypeOverride = typeof (IEnumerable<object>);

      var result = queryModel.ConvertToSubQuery ("test");

      Assert.That (result.MainFromClause.ItemName, Is.EqualTo ("test"));
      Assert.That (result.MainFromClause.ItemType, Is.SameAs (typeof (object)));
      Assert.That (result.MainFromClause.FromExpression, Is.TypeOf (typeof (SubQueryExpression)));
      Assert.That (((SubQueryExpression) result.MainFromClause.FromExpression).QueryModel, Is.SameAs (queryModel));
      Assert.That (result.SelectClause.Selector.Type, Is.SameAs (typeof (object)));
    }

    [Test]
    [ExpectedException (typeof (InvalidOperationException), ExpectedMessage = 
      "The query must return a sequence of items, but it selects a single object of type 'System.Int32'.")]
    public void ConvertToSubquery_NoStreamedSequenceInfo ()
    {
      var queryModel = new QueryModel (_mainFromClause, new SelectClause (new QuerySourceReferenceExpression (_mainFromClause)));
      queryModel.ResultOperators.Add (new CountResultOperator());

      queryModel.ConvertToSubQuery ("test");
    }
  }
}
