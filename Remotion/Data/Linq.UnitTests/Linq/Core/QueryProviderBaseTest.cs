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
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using NUnit.Framework;
using NUnit.Framework.SyntaxHelpers;
using Remotion.Data.Linq.Clauses.Expressions;
using Remotion.Data.Linq.Parsing;
using Remotion.Data.Linq.Parsing.ExpressionTreeVisitors.Transformation;
using Remotion.Data.Linq.Parsing.Structure;
using Remotion.Data.Linq.Parsing.Structure.ExpressionTreeProcessingSteps;
using Remotion.Data.Linq.UnitTests.Linq.Core.TestDomain;
using Remotion.Data.Linq.UnitTests.Linq.Core.TestQueryGenerators;
using Remotion.Data.Linq.Utilities;
using Rhino.Mocks;

namespace Remotion.Data.Linq.UnitTests.Linq.Core
{
  [TestFixture]
  public class QueryProviderBaseTest
  {
    private MockRepository _mockRepository;
    private QueryProviderBase _queryProvider;
    private IQueryExecutor _executorMock;
    private TestQueryable<Cook> _queryableWithExecutorMock;
    private MethodCallExpressionNodeTypeRegistry _nodeTypeRegistry;
    private TestQueryProvider _queryProviderWithSpecificRegistry;

    [SetUp]
    public void SetUp()
    {
      _mockRepository = new MockRepository();
      _executorMock = _mockRepository.StrictMock<IQueryExecutor>();
      _queryProvider = new TestQueryProvider (_executorMock);
      _nodeTypeRegistry = new MethodCallExpressionNodeTypeRegistry ();
      _queryProviderWithSpecificRegistry = new TestQueryProvider (_executorMock, _nodeTypeRegistry);
      _queryableWithExecutorMock = new TestQueryable<Cook> (_executorMock);
    }

    [Test]
    public void Initialization()
    {
      Assert.IsNotNull (_queryProvider);
    }

    [Test]
    public void Initialization_WithDefaults ()
    {
      var queryProvider = new TestQueryProvider (_executorMock);
      
      Assert.That (
          queryProvider.ExpressionTreeParser.NodeTypeRegistry.RegisteredMethodInfoCount,
          Is.EqualTo (MethodCallExpressionNodeTypeRegistry.CreateDefault ().RegisteredMethodInfoCount));

      Assert.That (queryProvider.ExpressionTreeParser.ProcessingSteps.Length, Is.EqualTo (2));
      Assert.That (queryProvider.ExpressionTreeParser.ProcessingSteps[0], Is.TypeOf (typeof (PartialEvaluationStep)));
      Assert.That (queryProvider.ExpressionTreeParser.ProcessingSteps[1], Is.TypeOf (typeof (ExpressionTransformationStep)));
      Assert.That (
          ((ExpressionTransformationStep) queryProvider.ExpressionTreeParser.ProcessingSteps[1]).Provider,
          Is.TypeOf (typeof (ExpressionTransformerRegistry)));

      var expressionTransformerRegistry =
          ((ExpressionTransformerRegistry) ((ExpressionTransformationStep) queryProvider.ExpressionTreeParser.ProcessingSteps[1]).Provider);
      Assert.That (
          expressionTransformerRegistry.RegisteredTransformerCount,
          Is.EqualTo (ExpressionTransformerRegistry.CreateDefault ().RegisteredTransformerCount));
    }

    [Test]
    public void CreateQueryReturnsIQueryable()
    {
      Expression expression = ExpressionHelper.CreateExpression();
      IQueryable queryable = _queryProvider.CreateQuery (expression);

      Assert.IsNotNull (queryable);
      Assert.IsInstanceOfType (typeof (IQueryable<int>), queryable);
    }

    [Test]
    [ExpectedException (typeof (ArgumentTypeException), ExpectedMessage = 
        "Expected a type implementing IEnumerable<T>, but found "
        + "'System.Func`1[[System.Int32, mscorlib, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089]]'.\r\n"
        + "Parameter name: expression")]
    public void CreateQuery_NonEnumerableExpression ()
    {
      Expression expression = ExpressionHelper.CreateLambdaExpression ();
      _queryProvider.CreateQuery (expression);
    }

    [Test]
    public void GenericCreateQueryReturnsIQueryable ()
    {
      Expression expression = ExpressionHelper.CreateNewIntArrayExpression ();
      IQueryable<int> queryable = _queryProvider.CreateQuery<int> (expression);

      Assert.IsNotNull (queryable);
    }

    [Test]
    public void GenerateQueryModel ()
    {
      Expression expression = SelectTestQueryGenerator.CreateSimpleQuery_SelectExpression (ExpressionHelper.CreateCookQueryable ());
      var queryModel = _queryProvider.GenerateQueryModel (expression);

      Assert.That (((QuerySourceReferenceExpression) queryModel.SelectClause.Selector).ReferencedQuerySource,
          Is.SameAs (queryModel.MainFromClause));
    }

    [Test]
    public void Execute_Generic ()
    {
      var expectedResult = new Cook[0];
      Expression expression = (from s in _queryableWithExecutorMock select s).Expression;
      _executorMock.Expect (mock => mock.ExecuteCollection<Cook> (
          Arg<QueryModel>.Is.Anything)).Return (expectedResult);

      _executorMock.Replay ();
      var result = _queryProvider.Execute<IEnumerable<Cook>> (expression);
      _executorMock.VerifyAllExpectations ();

      Assert.That (result.ToArray(), Is.EqualTo (expectedResult));
    }

    [Test]
    public void Execute_NonGeneric ()
    {
      var expectedResult = new Cook[0];
      Expression expression = (from s in _queryableWithExecutorMock select s).Expression;
      _executorMock.Expect (mock => mock.ExecuteCollection<Cook> (
          Arg<QueryModel>.Is.Anything)).Return (new Cook[0]);

      _executorMock.Replay ();
      var result = ((IQueryProvider)_queryProvider).Execute (expression);
      _executorMock.VerifyAllExpectations ();

      Assert.That (((IEnumerable<Cook>)result).ToArray (), Is.EqualTo (expectedResult));
    }

    [Test]
    public void Execute_IntegrationWithGenericEnumerable ()
    {
      var queryable = from s in _queryableWithExecutorMock select s;

      var expectedResult = new[] { new Cook () };
      _executorMock
          .Expect (mock => mock.ExecuteCollection<Cook> (Arg<QueryModel>.Is.Anything))
          .Return (expectedResult);

      _executorMock.Replay ();
      var result = queryable.ToArray ();
      _executorMock.VerifyAllExpectations ();

      Assert.That (result, Is.EqualTo (expectedResult));
    }

    [Test]
    public void Execute_IntegrationWithNonGenericEnumerable ()
    {
      var queryable = from s in _queryableWithExecutorMock select s;

      var expectedResult = new[] { new Cook () };
      _executorMock
          .Expect (mock => mock.ExecuteCollection<Cook> (Arg<QueryModel>.Is.Anything))
          .Return (expectedResult);

      _executorMock.Replay ();

      var result = new List<object> ();
      foreach (var s in (IEnumerable) queryable)
        result.Add (s);

      _executorMock.VerifyAllExpectations ();

      Assert.That (result, Is.EqualTo (expectedResult));
    }

    [Test]
    public void Execute_IntegrationWithSingle ()
    {
      var expectedResult = new Cook ();
      _executorMock
                .Expect (mock => mock.ExecuteSingle<Cook> (Arg<QueryModel>.Is.Anything, Arg<bool>.Is.Equal (false)))
                .Return (expectedResult);

      _executorMock.Replay ();
      var result = (from s in _queryableWithExecutorMock select s).Single();
      _executorMock.VerifyAllExpectations ();

      Assert.That (result, Is.SameAs(expectedResult));
    }

    [Test]
    public void Execute_IntegrationWithScalar ()
    {
      _executorMock
                .Expect (mock => mock.ExecuteScalar<int> (Arg<QueryModel>.Is.Anything))
                .Return (7);

      _executorMock.Replay ();
      var result = (from s in _queryableWithExecutorMock select s).Count ();
      _executorMock.VerifyAllExpectations();

      Assert.That (result, Is.EqualTo (7));
    }

    [Test]
    public void Execute_WithDefaultRegistry ()
    {
      Expression expression = ExpressionHelper.MakeExpression (() => from s in ExpressionHelper.CreateCookQueryable () select s);
      _executorMock
          .Expect (mock => mock.ExecuteCollection<Cook> (Arg<QueryModel>.Is.Anything))
          .Return (new Cook[0]);
      _executorMock.Replay ();
      _queryProvider.Execute<IEnumerable<Cook>> (expression);
      _executorMock.VerifyAllExpectations ();
    }

    [Test]
    [ExpectedException (typeof (ParserException), ExpectedMessage = "Could not parse expression 'TestQueryable<Cook>().Select(s => s)': This "
        + "overload of the method 'System.Linq.Queryable.Select' is currently not supported.")]
    public void Execute_WithSpecificRegistry ()
    {
      Expression expression = ExpressionHelper.MakeExpression (() => from s in ExpressionHelper.CreateCookQueryable () select s);
      _executorMock
          .Expect (mock => mock.ExecuteCollection<Cook> (Arg<QueryModel>.Is.Anything))
          .Return (new Cook[0]);
      _executorMock.Replay ();
      _queryProviderWithSpecificRegistry.Execute<IEnumerable<Cook>> (expression);
      _executorMock.VerifyAllExpectations ();
    }
  }
}
