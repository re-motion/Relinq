using System;
using System.Linq.Expressions;
using NUnit.Framework;
using Rhino.Mocks;
using Rubicon.Data.Linq.Clauses;
using Rubicon.Data.Linq.DataObjectModel;

namespace Rubicon.Data.Linq.UnitTests.ClausesTest
{
  [TestFixture]
  public class SelectClauseTest
  {
    [Test]
    public void InitializeWithExpression ()
    {
      LambdaExpression expression = ExpressionHelper.CreateLambdaExpression ();
      IClause clause = ExpressionHelper.CreateClause();

      SelectClause selectClause = new SelectClause (clause, expression);
      Assert.AreSame (clause, selectClause.PreviousClause);
      Assert.AreEqual (expression, selectClause.ProjectionExpression);
    }

    [Test]
    public void InitializeWithoutExpression ()
    {
      SelectClause selectClause = new SelectClause (ExpressionHelper.CreateClause(),null);
      Assert.IsNull (selectClause.ProjectionExpression);
    }


    [Test]
    public void SelectClause_ImplementISelectGroupClause()
    {
      SelectClause selectClause = ExpressionHelper.CreateSelectClause();

      Assert.IsInstanceOfType (typeof(ISelectGroupClause),selectClause);
    }
        
    [Test]
    public void SelectClause_ImplementIQueryElement()
    {
      SelectClause selectClause = ExpressionHelper.CreateSelectClause();
      Assert.IsInstanceOfType (typeof (IQueryElement), selectClause);
    }

    [Test]
    public void Accept()
    {
      SelectClause selectClause = ExpressionHelper.CreateSelectClause();

      MockRepository repository = new MockRepository();
      IQueryVisitor visitorMock = repository.CreateMock<IQueryVisitor>();

      visitorMock.VisitSelectClause (selectClause);

      repository.ReplayAll();

      selectClause.Accept (visitorMock);

      repository.VerifyAll();
    }

    [Test]
    public void Resolve()
    {
      Expression resolvedFieldExpression = ExpressionHelper.CreateExpression();
      LambdaExpression expression = ExpressionHelper.CreateLambdaExpression ();
      MockRepository repository = new MockRepository();
      IClause previousClause = repository.CreateMock<IClause>();

      SelectClause clause = new SelectClause (previousClause, expression);
      
      Table table = new Table ("Foo", "foo");
      FieldDescriptor fieldDescriptor = new FieldDescriptor (null, ExpressionHelper.CreateMainFromClause (), table, new Column (table, "Bar"));
      Expect.Call (previousClause.ResolveField (StubDatabaseInfo.Instance, resolvedFieldExpression, resolvedFieldExpression)).Return (fieldDescriptor);

      repository.ReplayAll();

      FieldDescriptor resolvedFieldDescriptor = clause.ResolveField (StubDatabaseInfo.Instance, resolvedFieldExpression, resolvedFieldExpression);
      Assert.AreEqual (fieldDescriptor, resolvedFieldDescriptor);
      repository.VerifyAll();
    }
  }
}