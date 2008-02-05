using System;
using System.Linq.Expressions;
using NUnit.Framework;
using Rhino.Mocks;
using Rubicon.Data.Linq.Clauses;
using Rubicon.Data.Linq.DataObjectModel;
using Rubicon.Data.Linq.UnitTests.ParsingTest;

namespace Rubicon.Data.Linq.UnitTests.ClausesTest
{
  [TestFixture]
  public class WhereClauseTest
  {
    [Test] 
    public void InitializeWithboolExpression()
    {
      LambdaExpression boolExpression = ExpressionHelper.CreateLambdaExpression ();
      IClause clause = ExpressionHelper.CreateClause();
      
      WhereClause whereClause = new WhereClause(clause,boolExpression);
      Assert.AreSame (clause, whereClause.PreviousClause);
      Assert.AreSame (boolExpression, whereClause.BoolExpression);
    }

    [Test]
    public void ImplementInterface()
    {
      WhereClause whereClause = ExpressionHelper.CreateWhereClause();
      Assert.IsInstanceOfType (typeof (IBodyClause), whereClause);
    }
    
    [Test]
    public void WhereClause_ImplementIQueryElement()
    {
      WhereClause whereClause = ExpressionHelper.CreateWhereClause();
      Assert.IsInstanceOfType (typeof (IQueryElement), whereClause);
    }

    [Test]
    public void GetSimplifiedBoolExpression ()
    {
      MethodCallExpression whereMethodCallExpression = TestQueryGenerator.CreateWhereQueryWithEvaluatableSubExpression_WhereExpression (
          ExpressionHelper.CreateQuerySource());
      LambdaExpression boolExpression = (LambdaExpression) ((UnaryExpression) whereMethodCallExpression.Arguments[1]).Operand;
      IClause clause = ExpressionHelper.CreateClause();
      WhereClause whereClause = new WhereClause (clause, boolExpression);
      LambdaExpression simplifiedExpression = whereClause.GetSimplifiedBoolExpression ();
      LambdaExpression simplifiedExpression2 = whereClause.GetSimplifiedBoolExpression ();
      Assert.AreSame (simplifiedExpression2, simplifiedExpression);

      ParameterExpression parameter = Expression.Parameter (typeof (Student), "s");
      Expression expected = Expression.Lambda (Expression.Equal (Expression.MakeMemberAccess (parameter, typeof (Student).GetProperty ("Last")),
          Expression.Constant ("Garcia")), parameter);

      ExpressionTreeComparer.CheckAreEqualTrees (simplifiedExpression, expected);
    }

    [Test]
    public void Accept()
    {
      WhereClause whereClause = ExpressionHelper.CreateWhereClause();

      MockRepository repository = new MockRepository ();
      IQueryVisitor visitorMock = repository.CreateMock<IQueryVisitor> ();

      visitorMock.VisitWhereClause (whereClause);

      repository.ReplayAll();

      whereClause.Accept (visitorMock);

      repository.VerifyAll();
    }

    [Test]
    public void Resolve()
    {
      LambdaExpression expression = ExpressionHelper.CreateLambdaExpression ();
      MockRepository repository = new MockRepository();
      IClause previousClause = repository.CreateMock<IClause>();

      WhereClause clause = new WhereClause (previousClause, expression);

      Expression resolvedFieldExpression = ExpressionHelper.CreateExpression ();
      Table table = new Table ("Foo", "foo");
      FieldDescriptor fieldDescriptor = new FieldDescriptor (null, ExpressionHelper.CreateMainFromClause(), table, new Column (table, "Bar"));
      Expect.Call (previousClause.ResolveField (StubDatabaseInfo.Instance, resolvedFieldExpression, resolvedFieldExpression)).Return (fieldDescriptor);

      repository.ReplayAll();

      FieldDescriptor resolvedFieldDescriptor = clause.ResolveField (StubDatabaseInfo.Instance, resolvedFieldExpression, resolvedFieldExpression);
      Assert.AreEqual (fieldDescriptor, resolvedFieldDescriptor);
      repository.VerifyAll();
    }
  }
}