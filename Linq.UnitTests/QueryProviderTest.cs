using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using NUnit.Framework;
using Rhino.Mocks;
using Rhino.Mocks.Constraints;
using Rubicon.Data.Linq.QueryProviderImplementation;
using Rubicon.Utilities;
using System.Collections;

namespace Rubicon.Data.Linq.UnitTests
{
  [TestFixture]
  public class QueryProviderTest
  {
    private MockRepository _mockRepository;
    private QueryProviderBase _queryProvider;
    private IQueryExecutor _executor;

    [SetUp]
    public void SetUp()
    {
      _mockRepository = new MockRepository();
      _executor = _mockRepository.CreateMock<IQueryExecutor>();
      _queryProvider = new TestQueryProvider (_executor);
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
    public void GenericExecute_Single()
    {
      Expression expression = TestQueryGenerator.CreateSimpleQuery_SelectExpression (ExpressionHelper.CreateQuerySource());
      Expect.Call (_executor.ExecuteSingle (null)).Constraints (
          new PredicateConstraint<QueryExpression> (
              delegate (QueryExpression queryExpression) { return queryExpression.GetExpressionTree() == expression; }
              )).Return (0);

      _mockRepository.ReplayAll();

      _queryProvider.Execute<int> (expression);

      _mockRepository.VerifyAll();
    }

    [Test]
    public void Execute_Single ()
    {
      Expression expression = TestQueryGenerator.CreateSimpleQuery_SelectExpression (ExpressionHelper.CreateQuerySource ());
      Expect.Call (_executor.ExecuteSingle (null)).Constraints (
          new PredicateConstraint<QueryExpression> (
              delegate (QueryExpression queryExpression) { return queryExpression.GetExpressionTree () == expression; }
              )).Return (0);

      _mockRepository.ReplayAll ();

      _queryProvider.Execute (expression);

      _mockRepository.VerifyAll ();
    }

    [Test]
    public void GenericExecute_Collection ()
    {
      IQueryable<Student> query = TestQueryGenerator.CreateSimpleQuery (ExpressionHelper.CreateQuerySource(_executor));
      Student student = new Student();
      
      Expression expression = query.Expression;
      Expect.Call (_executor.ExecuteCollection (null)).Constraints (
          new PredicateConstraint<QueryExpression> (
              delegate (QueryExpression queryExpression) { return queryExpression.GetExpressionTree () == expression; }
              )).Return (new Student[] {student});

      _mockRepository.ReplayAll ();

      List<Student> students = new List<Student> (query);
      Assert.AreEqual (1, students.Count);
      Assert.AreSame (student, students[0]);

      _mockRepository.VerifyAll ();
    }

    [Test]
    public void NonGenericExecute_Collection ()
    {
      IQueryable<Student> query = TestQueryGenerator.CreateSimpleQuery (ExpressionHelper.CreateQuerySource (_executor));
      Student student = new Student ();

      Expression expression = query.Expression;
      Expect.Call (_executor.ExecuteCollection (null)).Constraints (
          new PredicateConstraint<QueryExpression> (
              delegate (QueryExpression queryExpression) { return queryExpression.GetExpressionTree () == expression; }
              )).Return (new Student[] { student });

      _mockRepository.ReplayAll ();

      ArrayList students = new ArrayList();
      IEnumerable nonGenericQuery = query;
      foreach (object o in nonGenericQuery)
        students.Add (o);

      Assert.AreEqual (1, students.Count);
      Assert.AreSame (student, students[0]);

      _mockRepository.VerifyAll ();
    }
  }
}