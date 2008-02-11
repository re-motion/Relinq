using System;
using System.Linq;
using System.Linq.Expressions;
using NUnit.Framework;
using NUnit.Framework.SyntaxHelpers;
using Rhino.Mocks;
using Rubicon.Data.Linq.Clauses;
using Rubicon.Data.Linq.DataObjectModel;

namespace Rubicon.Data.Linq.UnitTests.ClausesTest
{
  [TestFixture]
  public class AdditionalFromClauseTest
  {
    [Test]
    public void Initialize_WithIDAndExpression ()
    {
      ParameterExpression id = ExpressionHelper.CreateParameterExpression ();
      LambdaExpression fromExpression = ExpressionHelper.CreateLambdaExpression ();
      LambdaExpression projectionExpression = ExpressionHelper.CreateLambdaExpression ();

      IClause clause = ExpressionHelper.CreateClause();
      
      AdditionalFromClause fromClause = new AdditionalFromClause (clause,id, fromExpression, projectionExpression);

      Assert.AreSame (id, fromClause.Identifier);
      Assert.AreSame (fromExpression, fromClause.FromExpression);
      Assert.AreSame (projectionExpression, fromClause.ProjectionExpression);
      Assert.AreSame (clause, fromClause.PreviousClause);

      Assert.That (fromClause.JoinClauses, Is.Empty);
      Assert.AreEqual (0, fromClause.JoinClauseCount);
    }

    [Test]
    public void ImplementInterface_IFromLetWhereClause ()
    {
      AdditionalFromClause fromClause = ExpressionHelper.CreateAdditionalFromClause ();
      Assert.IsInstanceOfType (typeof (IBodyClause), fromClause);
    }

    [Test]
    public void GetQuerySource()
    {
      IQueryable<Student> querySource = ExpressionHelper.CreateQuerySource();
      LambdaExpression fromExpression = Expression.Lambda (Expression.Constant (querySource), Expression.Parameter (typeof (Student), "student"));
      AdditionalFromClause fromClause =
          new AdditionalFromClause (ExpressionHelper.CreateClause(), ExpressionHelper.CreateParameterExpression(), 
          fromExpression, ExpressionHelper.CreateLambdaExpression());
      Assert.AreSame (typeof (TestQueryable<Student>), fromClause.GetQuerySourceType());
    }

    
    [Test]
    public void Accept ()
    {
      AdditionalFromClause fromClause = ExpressionHelper.CreateAdditionalFromClause ();

      MockRepository repository = new MockRepository ();
      IQueryVisitor visitorMock = repository.CreateMock<IQueryVisitor> ();

      visitorMock.VisitAdditionalFromClause (fromClause);

      repository.ReplayAll ();

      fromClause.Accept (visitorMock);

      repository.VerifyAll ();

    }

    public class TransparentIdentifierClass<T>
    {
      public T s1;
      public T s2;
      public T fzlbf;
    }

    [Test]
    public void Resolve_FromIdentifier_Directly ()
    {
      ParameterExpression identifier1 = Expression.Parameter (typeof (Student), "s1");
      ParameterExpression identifier2 = Expression.Parameter (typeof (Student), "s2");
      AdditionalFromClause additionalFromClause = CreateAdditionalFromClause(identifier1, identifier2);

      Expression fieldExpression = identifier2;

      FieldDescriptor fieldDescriptor = additionalFromClause.ResolveField (StubDatabaseInfo.Instance, fieldExpression, fieldExpression);
      Assert.AreEqual (new Column (new Table ("sourceTable", "s2"), "*"), fieldDescriptor.Column);
      Assert.AreSame (additionalFromClause, fieldDescriptor.FromClause);
    }

    [Test]
    public void Resolve_FromIdentifier_FromMainClause ()
    {
      ParameterExpression identifier1 = Expression.Parameter (typeof (Student), "s1");
      ParameterExpression identifier2 = Expression.Parameter (typeof (Student), "s2");
      AdditionalFromClause additionalFromClause = CreateAdditionalFromClause (identifier1, identifier2);

      Expression fieldExpression = identifier1;

      FieldDescriptor fieldDescriptor = additionalFromClause.ResolveField (StubDatabaseInfo.Instance, fieldExpression, fieldExpression);
      Assert.AreEqual (new Column (new Table ("sourceTable", "s1"), "*"), fieldDescriptor.Column);
      Assert.AreSame (additionalFromClause.PreviousClause, fieldDescriptor.FromClause);
    }

    [Test]
    public void Resolve_FromIdentifier_Field ()
    {
      ParameterExpression identifier1 = Expression.Parameter (typeof (Student), "s1");
      ParameterExpression identifier2 = Expression.Parameter (typeof (Student), "s2");
      AdditionalFromClause additionalFromClause = CreateAdditionalFromClause (identifier1, identifier2);

      Expression fieldExpression = Expression.MakeMemberAccess (identifier2, typeof (Student).GetProperty ("First"));

      FieldDescriptor fieldDescriptor = additionalFromClause.ResolveField (StubDatabaseInfo.Instance, fieldExpression, fieldExpression);
      Assert.AreEqual (new Column (new Table ("sourceTable", "s2"), "FirstColumn"), fieldDescriptor.Column);
      Assert.AreSame (additionalFromClause, fieldDescriptor.FromClause);
    }

    [Test]
    public void Resolve_FromIdentifier_Field_FromMainClause ()
    {
      ParameterExpression identifier1 = Expression.Parameter (typeof (Student), "s1");
      ParameterExpression identifier2 = Expression.Parameter (typeof (Student), "s2");
      AdditionalFromClause additionalFromClause = CreateAdditionalFromClause (identifier1, identifier2);

      Expression fieldExpression = Expression.MakeMemberAccess (identifier1, typeof (Student).GetProperty ("First"));

      FieldDescriptor fieldDescriptor = additionalFromClause.ResolveField (StubDatabaseInfo.Instance, fieldExpression, fieldExpression);
      Assert.AreEqual (new Column (new Table ("sourceTable", "s1"), "FirstColumn"), fieldDescriptor.Column);
      Assert.AreSame (additionalFromClause.PreviousClause, fieldDescriptor.FromClause);
    }

    [Test]
    public void Resolve_FromIdentifier_Directly_WithTransparentIdentifier ()
    {
      ParameterExpression identifier1 = Expression.Parameter (typeof (Student), "s1");
      ParameterExpression identifier2 = Expression.Parameter (typeof (Student), "s2");
      ParameterExpression transparentIdentifier1 = Expression.Parameter (typeof (TransparentIdentifierClass<Student>), "transparent1");
      ParameterExpression transparentIdentifier2 = Expression.Parameter (typeof (TransparentIdentifierClass<Student>), "transparent2");
      AdditionalFromClause additionalFromClause =
          CreateAdditionalFromClause (identifier1, identifier2, transparentIdentifier1, transparentIdentifier2, identifier2);

      Expression fieldExpression =
          Expression.MakeMemberAccess (transparentIdentifier1, typeof (TransparentIdentifierClass<Student>).GetField ("s1"));

      FieldDescriptor fieldDescriptor = additionalFromClause.ResolveField (StubDatabaseInfo.Instance, fieldExpression, fieldExpression);
      Assert.AreEqual (new Column (new Table ("sourceTable", "s1"), "*"), fieldDescriptor.Column);
      Assert.AreSame (additionalFromClause.PreviousClause, fieldDescriptor.FromClause);
    }

    [Test]
    public void Resolve_FromIdentifier_Field_WithTransparentIdentifier ()
    {
      ParameterExpression identifier1 = Expression.Parameter (typeof (Student), "s1");
      ParameterExpression identifier2 = Expression.Parameter (typeof (Student), "s2");
      ParameterExpression transparentIdentifier1 = Expression.Parameter (typeof (TransparentIdentifierClass<Student>), "transparent1");
      ParameterExpression transparentIdentifier2 = Expression.Parameter (typeof (TransparentIdentifierClass<Student>), "transparent2");
      AdditionalFromClause additionalFromClause =
          CreateAdditionalFromClause (identifier1, identifier2, transparentIdentifier1, transparentIdentifier2, identifier2);

      Expression studentExpression =
          Expression.MakeMemberAccess (transparentIdentifier1, typeof (TransparentIdentifierClass<Student>).GetField ("s1"));
      Expression fieldExpression =
          Expression.MakeMemberAccess (studentExpression, typeof (Student).GetProperty ("First"));

      FieldDescriptor fieldDescriptor = additionalFromClause.ResolveField (StubDatabaseInfo.Instance, fieldExpression, fieldExpression);
      Assert.AreEqual (new Column (new Table ("sourceTable", "s1"), "FirstColumn"), fieldDescriptor.Column);
      Assert.AreSame (additionalFromClause.PreviousClause, fieldDescriptor.FromClause);
    }

    [Test]
    [Ignore ("TODO: Joins")]
    public void Resolve_WithJoin()
    {
    }

    [Test]
    [ExpectedException (typeof (ParserException), ExpectedMessage = "There is no from clause defining identifier 'fzlbf', which is used in "
        + "expression 'fzlbf'.")]
    public void Resolve_InvalidIdentifierName_Directly ()
    {
      ParameterExpression identifier1 = Expression.Parameter (typeof (Student), "s1");
      ParameterExpression identifier2 = Expression.Parameter (typeof (Student), "s2");
      ParameterExpression invalidIdentifier = Expression.Parameter (typeof (Student), "fzlbf");
      AdditionalFromClause additionalFromClause = CreateAdditionalFromClause (identifier1, identifier2);

      Expression fieldExpression = invalidIdentifier;

      additionalFromClause.ResolveField (StubDatabaseInfo.Instance, fieldExpression, fieldExpression);
    }

    [Test]
    [ExpectedException (typeof (ParserException), ExpectedMessage = "There is no from clause defining identifier 'fzlbf', which is used in "
        + "expression 'fzlbf.First'.")]
    public void Resolve_InvalidIdentifierName_Field ()
    {
      ParameterExpression identifier1 = Expression.Parameter (typeof (Student), "s1");
      ParameterExpression identifier2 = Expression.Parameter (typeof (Student), "s2");
      ParameterExpression invalidIdentifier = Expression.Parameter (typeof (Student), "fzlbf");
      AdditionalFromClause additionalFromClause = CreateAdditionalFromClause (identifier1, identifier2);

      Expression fieldExpression = Expression.MakeMemberAccess (invalidIdentifier, typeof (Student).GetProperty ("First"));

      additionalFromClause.ResolveField (StubDatabaseInfo.Instance, fieldExpression, fieldExpression);
    }

    [Test]
    [ExpectedException (typeof (ParserException), ExpectedMessage = "There is no from clause defining identifier 'fzlbf', which is used in "
        + "expression 'transparent1.fzlbf'.")]
    public void Resolve_InvalidIdentifierName_Directly_WithTransparentIdentifier ()
    {
      ParameterExpression identifier1 = Expression.Parameter (typeof (Student), "s1");
      ParameterExpression identifier2 = Expression.Parameter (typeof (Student), "s2");
      ParameterExpression transparentIdentifier1 = Expression.Parameter (typeof (TransparentIdentifierClass<Student>), "transparent1");
      ParameterExpression transparentIdentifier2 = Expression.Parameter (typeof (TransparentIdentifierClass<Student>), "transparent2");
      AdditionalFromClause additionalFromClause =
          CreateAdditionalFromClause (identifier1, identifier2, transparentIdentifier1, transparentIdentifier2, identifier2);

      Expression fieldExpression =
          Expression.MakeMemberAccess (transparentIdentifier1, typeof (TransparentIdentifierClass<Student>).GetField ("fzlbf"));

      additionalFromClause.ResolveField (StubDatabaseInfo.Instance, fieldExpression, fieldExpression);
    }

    [Test]
    [ExpectedException (typeof (ParserException), ExpectedMessage = "There is no from clause defining identifier 'fzlbf', which is used in "
        + "expression 'transparent1.fzlbf.First'.")]
    public void Resolve_InvalidIdentifierName_Field_WithTransparentIdentifier ()
    {
      ParameterExpression identifier1 = Expression.Parameter (typeof (Student), "s1");
      ParameterExpression identifier2 = Expression.Parameter (typeof (Student), "s2");
      ParameterExpression transparentIdentifier1 = Expression.Parameter (typeof (TransparentIdentifierClass<Student>), "transparent1");
      ParameterExpression transparentIdentifier2 = Expression.Parameter (typeof (TransparentIdentifierClass<Student>), "transparent2");
      AdditionalFromClause additionalFromClause =
          CreateAdditionalFromClause (identifier1, identifier2, transparentIdentifier1, transparentIdentifier2, identifier2);

      Expression studentExpression =
          Expression.MakeMemberAccess (transparentIdentifier1, typeof (TransparentIdentifierClass<Student>).GetField ("fzlbf"));
      Expression fieldExpression =
          Expression.MakeMemberAccess (studentExpression, typeof (Student).GetProperty ("First"));

      additionalFromClause.ResolveField (StubDatabaseInfo.Instance, fieldExpression, fieldExpression);
    }

    [Test]
    [ExpectedException (typeof (ParserException), ExpectedMessage = "The identifier 's2' has a different type (System.String) than expected "
        + "(Rubicon.Data.Linq.UnitTests.Student) in expression 's2'.")]
    public void Resolve_InvalidIdentifierType_Directly ()
    {
      ParameterExpression identifier1 = Expression.Parameter (typeof (Student), "s1");
      ParameterExpression identifier2 = Expression.Parameter (typeof (Student), "s2");
      ParameterExpression invalidIdentifier = Expression.Parameter (typeof (string), "s2");
      AdditionalFromClause additionalFromClause = CreateAdditionalFromClause (identifier1, identifier2);

      Expression fieldExpression = invalidIdentifier;

      additionalFromClause.ResolveField (StubDatabaseInfo.Instance, fieldExpression, fieldExpression);
    }

    public class InvalidStudent : Student
    {
    }

    [Test]
    [ExpectedException (typeof (ParserException), ExpectedMessage = "The identifier 's2' has a different type "
        + "(Rubicon.Data.Linq.UnitTests.ClausesTest.AdditionalFromClauseTest+InvalidStudent) than expected (Rubicon.Data.Linq.UnitTests.Student) "
        + "in expression 's2.First'.")]
    public void Resolve_InvalidIdentifierType_Field ()
    {
      ParameterExpression identifier1 = Expression.Parameter (typeof (Student), "s1");
      ParameterExpression identifier2 = Expression.Parameter (typeof (Student), "s2");
      ParameterExpression invalidIdentifier = Expression.Parameter (typeof (InvalidStudent), "s2");
      AdditionalFromClause additionalFromClause = CreateAdditionalFromClause (identifier1, identifier2);

      Expression fieldExpression = Expression.MakeMemberAccess (invalidIdentifier, typeof (InvalidStudent).GetProperty ("First"));

      additionalFromClause.ResolveField (StubDatabaseInfo.Instance, fieldExpression, fieldExpression);
    }

    [Test]
    [ExpectedException (typeof (ParserException), ExpectedMessage = "Expected ParameterExpression or MemberExpression for resolving field access in "
        + "additional from clause, found ConstantExpression (null).")]
    public void Resolve_InvalidExpressionTree ()
    {
      ParameterExpression identifier1 = Expression.Parameter (typeof (Student), "s1");
      ParameterExpression identifier2 = Expression.Parameter (typeof (Student), "s2");
      AdditionalFromClause additionalFromClause = CreateAdditionalFromClause (identifier1, identifier2);

      Expression fieldExpression = Expression.Constant (null, typeof (string));

      additionalFromClause.ResolveField (StubDatabaseInfo.Instance, fieldExpression, fieldExpression);
    }

    private AdditionalFromClause CreateAdditionalFromClause (ParameterExpression mainFromIdentifier, ParameterExpression additionalFromIdentifier,
        params ParameterExpression[] transparentIdentifiers)
    {
      MainFromClause mainFromClause = new MainFromClause (mainFromIdentifier, ExpressionHelper.CreateQuerySource ());
      LambdaExpression fromExpression = Expression.Lambda (Expression.Constant (null, typeof (IQueryable<Student>)));
      LambdaExpression projectionExpression = Expression.Lambda (Expression.Constant (null, typeof (Student)), transparentIdentifiers);
      return new AdditionalFromClause (mainFromClause, additionalFromIdentifier, fromExpression, projectionExpression);
    }
  }
}