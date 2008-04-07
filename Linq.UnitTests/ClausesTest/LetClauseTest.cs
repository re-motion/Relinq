using System;
using System.Linq.Expressions;
using NUnit.Framework;
using Rhino.Mocks;
using Rubicon.Data.Linq.Clauses;
using Rubicon.Data.Linq.DataObjectModel;
using Rubicon.Data.Linq.Parsing.FieldResolving;

namespace Rubicon.Data.Linq.UnitTests.ClausesTest
{
  [TestFixture]
  public class LetClauseTest
  {
    [Test]
    public void IntitalizeWithIDAndExpression()
    {
      ParameterExpression identifier = ExpressionHelper.CreateParameterExpression();
      Expression expression = ExpressionHelper.CreateExpression();

       IClause clause = ExpressionHelper.CreateClause();

      LetClause letClause = new LetClause(clause,identifier,expression,ExpressionHelper.CreateLambdaExpression());

      Assert.AreSame (clause, letClause.PreviousClause);
      Assert.AreSame (identifier, letClause.Identifier);
      Assert.AreSame (expression, letClause.Expression);
    }

    [Test]
    public void ImplementInterface()
    {
      LetClause letClause = ExpressionHelper.CreateLetClause();

      Assert.IsInstanceOfType (typeof (IBodyClause), letClause);
    }
        

    [Test]
    public void LetClause_ImplementsIQueryElement()
    {
      LetClause letClause = ExpressionHelper.CreateLetClause();
      Assert.IsInstanceOfType (typeof (IQueryElement), letClause);
    }

    [Test]
    public void Accept ()
    {
      LetClause letClause = ExpressionHelper.CreateLetClause ();

      MockRepository repository = new MockRepository ();
      IQueryVisitor visitorMock = repository.CreateMock<IQueryVisitor> ();

      visitorMock.VisitLetClause (letClause);

      repository.ReplayAll ();

      letClause.Accept (visitorMock);

      repository.VerifyAll ();
    }

    [Test]
    public void GetNamedEvaluation ()
    {
      LetClause letClause = ExpressionHelper.CreateLetClause ();
      Assert.AreEqual (new NamedEvaluation ("i", "i").Alias, letClause.GetNamedEvaluation().Alias);
      Assert.AreEqual (letClause.Identifier.Name, letClause.GetNamedEvaluation ().AliasString);
    }

    [Test]
    public void Resolve_Succeeds_NamedEvaluation ()
    {
      ParameterExpression identifier = Expression.Parameter (typeof (Student), "student");
      LetClause letClause = 
        new LetClause (ExpressionHelper.CreateMainFromClause(),identifier,ExpressionHelper.CreateExpression(),ExpressionHelper.CreateLambdaExpression());
      
      JoinedTableContext context = new JoinedTableContext ();
      SelectFieldAccessPolicy policy = new SelectFieldAccessPolicy ();

      ClauseFieldResolver resolver = new ClauseFieldResolver (StubDatabaseInfo.Instance, context, policy);
      FieldDescriptor fieldDescriptor = letClause.ResolveField (resolver, identifier, identifier);

      Assert.AreEqual (new Column(new NamedEvaluation("student","student"),"*"),fieldDescriptor.Column);
    }

  }
}