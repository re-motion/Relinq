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
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using NUnit.Framework;
using NUnit.Framework.SyntaxHelpers;
using Remotion.Data.Linq;
using Remotion.Data.Linq.EagerFetching;
using Rhino.Mocks;
using Remotion.Data.UnitTests.Linq.TestQueryGenerators;
using Remotion.Utilities;
using System.Collections;
using Remotion.Data.Linq.ExtensionMethods;

namespace Remotion.Data.UnitTests.Linq
{
  [TestFixture]
  public class QueryProviderBaseTest
  {
    private MockRepository _mockRepository;
    private QueryProviderBase _queryProvider;
    private IQueryExecutor _executorMock;

    [SetUp]
    public void SetUp()
    {
      _mockRepository = new MockRepository();
      _executorMock = _mockRepository.StrictMock<IQueryExecutor>();
      _queryProvider = new TestQueryProvider (_executorMock);
    }

    [Test]
    public void Initialization()
    {
      Assert.IsNotNull (_queryProvider);
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
    [ExpectedException (typeof (ArgumentTypeException))]
    public void CreateQueryUnwrapsException ()
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
      Expression expression = SelectTestQueryGenerator.CreateSimpleQuery_SelectExpression (ExpressionHelper.CreateQuerySource ());
      var queryModel = _queryProvider.GenerateQueryModel (expression);

      Assert.That (queryModel.GetExpressionTree (), Is.SameAs (expression));
    }

    [Test]
    public void GenericExecute_Single()
    {
      Expression expression = SelectTestQueryGenerator.CreateSimpleQuery_SelectExpression (ExpressionHelper.CreateQuerySource());
      Expect
          .Call (
          _executorMock.ExecuteSingle (
              Arg<QueryModel>.Matches (queryModel => queryModel.GetExpressionTree() == expression),
              Arg<IEnumerable<FetchRequestBase>>.List.Equal (new FetchManyRequest[0])))
          .Return (0);

      _mockRepository.ReplayAll();

      _queryProvider.Execute<int> (expression);

      _mockRepository.VerifyAll();
    }

    [Test]
    public void Execute_Single ()
    {
      Expression expression = SelectTestQueryGenerator.CreateSimpleQuery_SelectExpression (ExpressionHelper.CreateQuerySource ());
      Expect
          .Call (
          _executorMock.ExecuteSingle (
              Arg<QueryModel>.Matches (queryModel => queryModel.GetExpressionTree() == expression),
              Arg<IEnumerable<FetchRequestBase>>.List.Equal (new FetchManyRequest[0])))
          .Return (0);

      _mockRepository.ReplayAll ();

      _queryProvider.Execute (expression);

      _mockRepository.VerifyAll ();
    }

    [Test]
    public void GenericExecute_Collection ()
    {
      IQueryable<Student> query = SelectTestQueryGenerator.CreateSimpleQuery (ExpressionHelper.CreateQuerySource(_executorMock));
      var student = new Student();
      
      Expression expression = query.Expression;
      Expect
          .Call (
          _executorMock.ExecuteCollection (
              Arg<QueryModel>.Matches (queryModel => queryModel.GetExpressionTree () == expression),
              Arg<IEnumerable<FetchRequestBase>>.List.Equal (new FetchManyRequest[0])))
          .Return (new[] { student });

      _mockRepository.ReplayAll ();

      var students = new List<Student> (query); // enumerates query -> ExecuteCollection
      Assert.AreEqual (1, students.Count);
      Assert.AreSame (student, students[0]);

      _mockRepository.VerifyAll ();
    }

    [Test]
    public void NonGenericExecute_Collection ()
    {
      IQueryable<Student> query = SelectTestQueryGenerator.CreateSimpleQuery (ExpressionHelper.CreateQuerySource (_executorMock));
      var student = new Student ();

      Expression expression = query.Expression;
      Expect
          .Call (
          _executorMock.ExecuteCollection (
              Arg<QueryModel>.Matches (queryModel => queryModel.GetExpressionTree () == expression),
              Arg<IEnumerable<FetchRequestBase>>.List.Equal (new FetchManyRequest[0])))
          .Return (new[] { student });

      _mockRepository.ReplayAll ();

      var students = new ArrayList();
      IEnumerable nonGenericQuery = query;
      foreach (object o in nonGenericQuery) // enumerates query -> ExecuteCollection
        students.Add (o);

      Assert.AreEqual (1, students.Count);
      Assert.AreSame (student, students[0]);

      _mockRepository.VerifyAll ();
    }

    [Test]
    public void GetFetchRequests_None ()
    {
      var originalExpression = ExpressionHelper.CreateExpression ();
      var expression = originalExpression;

      var requests = _queryProvider.GetFetchRequests (ref expression);

      Assert.That (requests, Is.Empty);
      Assert.That (expression, Is.SameAs (originalExpression));
    }

    [Test]
    public void GetFetchRequests_TopLevel ()
    {
      var relatedObjectSelector1 = ExpressionHelper.CreateLambdaExpression<Student, IEnumerable<Student>> (s => s.Friends);
      var relatedObjectSelector2 = ExpressionHelper.CreateLambdaExpression<Student, IEnumerable<int>> (s => s.Scores);
      var innerExpression = ExpressionHelper.CreateExpression ();
      var originalExpression = new FetchManyExpression (new FetchManyExpression (innerExpression, relatedObjectSelector1), relatedObjectSelector2);
      Expression expression = originalExpression;

      var requests = _queryProvider.GetFetchRequests (ref expression);

      Assert.That (requests.Length, Is.EqualTo (2));
      Assert.That (requests.Select (fr => fr.RelatedObjectSelector).ToArray (), 
          Is.EquivalentTo (new Expression[] { relatedObjectSelector1, relatedObjectSelector2 }));
      Assert.That (expression, Is.Not.SameAs (originalExpression));
      Assert.That (expression, Is.SameAs (innerExpression));
    }

    [Test]
    public void GetFetchRequests_ThenFetch ()
    {
      var relatedObjectSelector1 = ExpressionHelper.CreateLambdaExpression<Student, IEnumerable<Student>> (s => s.Friends);
      var relatedObjectSelector2 = ExpressionHelper.CreateLambdaExpression<Student, IEnumerable<int>> (s => s.Scores);
      var innerExpression = ExpressionHelper.CreateExpression ();
      var originalExpression = new ThenFetchManyExpression (new FetchManyExpression (innerExpression, relatedObjectSelector1), relatedObjectSelector2);
      Expression expression = originalExpression;

      var requests = _queryProvider.GetFetchRequests (ref expression);

      Assert.That (requests.Length, Is.EqualTo (1));
      Assert.That (requests.Single ().RelatedObjectSelector, Is.SameAs (relatedObjectSelector1));
      Assert.That (requests.Single ().InnerFetchRequests.Count (), Is.EqualTo (1));
      Assert.That (requests.Single ().InnerFetchRequests.Single ().RelatedObjectSelector, Is.SameAs (relatedObjectSelector2));
      Assert.That (expression, Is.Not.SameAs (originalExpression));
      Assert.That (expression, Is.SameAs (innerExpression));
    }
    
    [Test]
    public void ExecuteCollection_WithFetchRequests ()
    {
      Expression<Func<Student, IEnumerable<Student>>> relatedObjectSelector = s => s.Friends;
      IQueryable<Student> query =
          SelectTestQueryGenerator.CreateSimpleQuery (ExpressionHelper.CreateQuerySource (_executorMock)).FetchMany (relatedObjectSelector);

      _executorMock.Expect (
          mock =>
          mock.ExecuteCollection (
              Arg<QueryModel>.Is.Anything,
              Arg<IEnumerable<FetchRequestBase>>.Matches (frs => frs.Single ().RelatedObjectSelector == relatedObjectSelector)))
          .Return (new Student[0]);

      _mockRepository.ReplayAll ();

      query.ToArray (); // enumerate query

      _executorMock.VerifyAllExpectations ();
    }

    [Test]
    public void ExecuteSingle_WithFetchRequests ()
    {
      Expression<Func<Student, IEnumerable<Student>>> relatedObjectSelector = s => s.Friends;
      IQueryable<Student> query =
          SelectTestQueryGenerator.CreateSimpleQuery (ExpressionHelper.CreateQuerySource (_executorMock)).FetchMany (relatedObjectSelector);

      _executorMock.Expect (
          mock =>
          mock.ExecuteSingle (
              Arg<QueryModel>.Is.Anything,
              Arg<IEnumerable<FetchRequestBase>>.Matches (frs => frs.Single ().RelatedObjectSelector == relatedObjectSelector)))
          .Return (null);

      _mockRepository.ReplayAll ();

      query.Single(); // enumerate query

      _executorMock.VerifyAllExpectations ();
    }
  }
}
