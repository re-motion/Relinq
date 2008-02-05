using System;
using System.Linq;
using System.Linq.Expressions;
using NUnit.Framework;
using NUnit.Framework.SyntaxHelpers;
using Rhino.Mocks;
using Rubicon.Data.Linq.Clauses;
using Rubicon.Data.Linq.DataObjectModel;
using Rubicon.Data.Linq.QueryProviderImplementation;

namespace Rubicon.Data.Linq.UnitTests.ClausesTest
{
  [TestFixture]
  [Ignore ("TODO: Reimplement Resolve test cases according to new definition of Resolve behavior")]
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
      public T fromIdentifier1;
      public T fromIdentifier2;
      public T fromIdentifier5;
      public T transparentField;
    }

    [Test]
    public void Resolve_SimpleMemberAccess_Succeeds ()
    {
      ParameterExpression identifier1 = Expression.Parameter (typeof (Student), "fromIdentifier1");
      MainFromClause mainFromClause = new MainFromClause (identifier1, ExpressionHelper.CreateQuerySource ());
      ParameterExpression identifier2 = Expression.Parameter (typeof (Student), "fromIdentifier2");

      LambdaExpression fromExpression = Expression.Lambda (Expression.Constant (null, typeof (IQueryable<Student>)));
      AdditionalFromClause additionalFromClause = new AdditionalFromClause (mainFromClause, identifier2, fromExpression,
          ExpressionHelper.CreateLambdaExpression ());

      Expression studentExpression = Expression.MakeMemberAccess (Expression.Parameter (typeof (TransparentIdentifierClass<Student>), "transparent"),
          typeof (TransparentIdentifierClass<Student>).GetField ("fromIdentifier2"));

      Expression fieldExpression = Expression.MakeMemberAccess (studentExpression, typeof (Student).GetProperty ("First"));

      FieldDescriptor fieldDescriptor = additionalFromClause.ResolveField (StubDatabaseInfo.Instance, fieldExpression, fieldExpression);
      Assert.AreEqual (new Column (new Table ("sourceTable", "fromIdentifier2"), "FirstColumn"), fieldDescriptor.Column);
      Assert.AreSame (additionalFromClause, fieldDescriptor.FromClause);
    }

    [Test]
    public void Resolve_ParameterAccess_Succeeds ()
    {
      ParameterExpression identifier1 = Expression.Parameter (typeof (Student), "fromIdentifier1");
      MainFromClause mainFromClause = new MainFromClause (identifier1, ExpressionHelper.CreateQuerySource ());
      ParameterExpression identifier2 = Expression.Parameter (typeof (Student), "fromIdentifier2");

      LambdaExpression fromExpression = Expression.Lambda (Expression.Constant (null, typeof (IQueryable<Student>)));
      AdditionalFromClause additionalFromClause = new AdditionalFromClause (mainFromClause, identifier2, fromExpression,
          ExpressionHelper.CreateLambdaExpression ());

      Expression studentExpression = Expression.MakeMemberAccess (Expression.Parameter (typeof (TransparentIdentifierClass<Student>), "transparent"),
          typeof (TransparentIdentifierClass<Student>).GetField ("fromIdentifier2"));

      FieldDescriptor fieldDescriptor = additionalFromClause.ResolveField (StubDatabaseInfo.Instance, studentExpression, studentExpression);
      Assert.AreEqual (new Column (new Table ("sourceTable", "fromIdentifier2"), "*"), fieldDescriptor.Column);
      Assert.AreSame (additionalFromClause, fieldDescriptor.FromClause);
    }


    [Test]
    public void Resolve_SimpleMemberAccessSkipAdditionalFromClause_Succeeds ()
    {
      ParameterExpression identifier1 = Expression.Parameter (typeof (Student), "fromIdentifier1");
      MainFromClause mainFromClause = new MainFromClause (identifier1, ExpressionHelper.CreateQuerySource ());
      ParameterExpression identifier2 = Expression.Parameter (typeof (Student), "fromIdentifier2");

      LambdaExpression fromExpression = Expression.Lambda (Expression.Constant (null, typeof (IQueryable<Student>)));
      AdditionalFromClause additionalFromClause = new AdditionalFromClause (mainFromClause, identifier2, fromExpression,
          ExpressionHelper.CreateLambdaExpression ());

      Expression studentExpression = Expression.MakeMemberAccess (Expression.Parameter (typeof (TransparentIdentifierClass<Student>), "transparent"),
          typeof (TransparentIdentifierClass<Student>).GetField ("fromIdentifier1"));

      Expression fieldExpression = Expression.MakeMemberAccess (studentExpression, typeof (Student).GetProperty ("First"));

      FieldDescriptor fieldDescriptor = additionalFromClause.ResolveField (StubDatabaseInfo.Instance, fieldExpression, fieldExpression);
      Assert.AreEqual (new Column (new Table ("sourceTable", "fromIdentifier1"), "FirstColumn"), fieldDescriptor.Column);
      Assert.AreSame (mainFromClause, fieldDescriptor.FromClause);
    }

    [Test]
    public void Resolve_ParameterAccessSkipAdditionalFromClause_Succeeds ()
    {
      ParameterExpression identifier1 = Expression.Parameter (typeof (Student), "fromIdentifier1");
      MainFromClause mainFromClause = new MainFromClause (identifier1, ExpressionHelper.CreateQuerySource ());
      ParameterExpression identifier2 = Expression.Parameter (typeof (Student), "fromIdentifier2");

      LambdaExpression fromExpression = Expression.Lambda (Expression.Constant (null, typeof (IQueryable<Student>)));
      AdditionalFromClause additionalFromClause = new AdditionalFromClause (mainFromClause, identifier2, fromExpression,
          ExpressionHelper.CreateLambdaExpression ());

      Expression studentExpression = Expression.MakeMemberAccess (Expression.Parameter (typeof (TransparentIdentifierClass<Student>), "transparent"),
          typeof (TransparentIdentifierClass<Student>).GetField ("fromIdentifier1"));

      FieldDescriptor fieldDescriptor = additionalFromClause.ResolveField (StubDatabaseInfo.Instance, studentExpression, studentExpression);
      Assert.AreEqual (new Column (new Table ("sourceTable", "fromIdentifier1"), "*"), fieldDescriptor.Column);
      Assert.AreSame (mainFromClause, fieldDescriptor.FromClause);
    }

    [Test]
    public void Resolve_SimpleMemberAccessSkipTwoAdditionalFromClauses_Succeeds ()
    {
      ParameterExpression identifier1 = Expression.Parameter (typeof (Student), "fromIdentifier1");
      MainFromClause mainFromClause = new MainFromClause (identifier1, ExpressionHelper.CreateQuerySource ());
      ParameterExpression identifier2 = Expression.Parameter (typeof (Student), "fromIdentifier2");
      ParameterExpression identifier3 = Expression.Parameter (typeof (Student), "fromIdentifier3");
      LambdaExpression fromExpression = Expression.Lambda (Expression.Constant (null, typeof (IQueryable<Student>)));

      AdditionalFromClause additionalFromClause1 = new AdditionalFromClause (mainFromClause, identifier2, fromExpression,
          ExpressionHelper.CreateLambdaExpression ());
      AdditionalFromClause additionalFromClause2 = new AdditionalFromClause (additionalFromClause1, identifier3, fromExpression,
          ExpressionHelper.CreateLambdaExpression ());

      Expression transparentExpression = Expression.MakeMemberAccess (
          Expression.Parameter (typeof (TransparentIdentifierClass<TransparentIdentifierClass<Student>>), "transparentParam"),
          typeof (TransparentIdentifierClass<TransparentIdentifierClass<Student>>).GetField ("transparentField"));

      Expression studentExpression = Expression.MakeMemberAccess (transparentExpression,
          typeof (TransparentIdentifierClass<Student>).GetField ("fromIdentifier1"));

      Expression fieldExpression = Expression.MakeMemberAccess (studentExpression, typeof (Student).GetProperty ("First"));

      FieldDescriptor fieldDescriptor = additionalFromClause2.ResolveField (StubDatabaseInfo.Instance, fieldExpression, fieldExpression);
      Assert.AreEqual (new Column (new Table ("sourceTable", "fromIdentifier1"), "FirstColumn"), fieldDescriptor.Column);
      Assert.AreSame (mainFromClause, fieldDescriptor.FromClause);
    }

    [Test]
    [Ignore ("TODO: add assertion")]
    public void Resolve_JoinMemberAccess_Succeeds ()
    {
      ParameterExpression identifier1 = Expression.Parameter (typeof (Student), "fromIdentifier1");
      MainFromClause mainFromClause = new MainFromClause (identifier1, ExpressionHelper.CreateQuerySource ());
      ParameterExpression identifier2 = Expression.Parameter (typeof (Student), "fromIdentifier2");

      LambdaExpression fromExpression = Expression.Lambda (Expression.Constant (null, typeof (IQueryable<Student>)));
      AdditionalFromClause additionalFromClause = new AdditionalFromClause (mainFromClause, identifier2, fromExpression,
          ExpressionHelper.CreateLambdaExpression ());

      Expression studentDetailExpression = Expression.MakeMemberAccess (Expression.Parameter (typeof (TransparentIdentifierClass<Student_Detail>), "transparent"),
          typeof (TransparentIdentifierClass<Student_Detail>).GetField ("fromIdentifier1"));

      Expression studentExpression = Expression.MakeMemberAccess (studentDetailExpression,
          typeof (Student_Detail).GetProperty ("Student"));

      Expression fieldExpression = Expression.MakeMemberAccess (studentExpression, typeof (Student).GetProperty ("First"));

      FieldDescriptor fieldDescriptor = additionalFromClause.ResolveField (StubDatabaseInfo.Instance, fieldExpression, fieldExpression);
      
    }

    [Test]
    [ExpectedException (typeof (ParserException), ExpectedMessage = "There is no from clause defining identifier 'fromIdentifier5', which is used "
        + "in expression 'transparent.fromIdentifier5'.")]
    public void Resolve_InvalidParameter ()
    {
      ParameterExpression identifier1 = Expression.Parameter (typeof (Student), "fromIdentifier1");
      MainFromClause mainFromClause = new MainFromClause (identifier1, ExpressionHelper.CreateQuerySource ());
      ParameterExpression identifier2 = Expression.Parameter (typeof (Student), "fromIdentifier2");

      LambdaExpression fromExpression = Expression.Lambda (Expression.Constant (null, typeof (IQueryable<Student>)));
      AdditionalFromClause additionalFromClause = new AdditionalFromClause (mainFromClause, identifier2, fromExpression,
          ExpressionHelper.CreateLambdaExpression ());

      Expression studentExpression = Expression.MakeMemberAccess (Expression.Parameter (typeof (TransparentIdentifierClass<Student>), "transparent"),
          typeof (TransparentIdentifierClass<Student>).GetField ("fromIdentifier5"));

      additionalFromClause.ResolveField (StubDatabaseInfo.Instance, studentExpression, studentExpression);
    }

    [Test]
    [ExpectedException (typeof (ParserException), ExpectedMessage = "There is no from clause defining identifier 'fromIdentifier2', which is used "
        + "in expression 'transparent.fromIdentifier2.First'.")]
    public void Resolve_InvalidIdentifierName ()
    {
      ParameterExpression identifier1 = Expression.Parameter (typeof (Student), "fromIdentifier1");
      MainFromClause mainFromClause = new MainFromClause (identifier1, ExpressionHelper.CreateQuerySource ());
      ParameterExpression identifier2 = Expression.Parameter (typeof (Student), "fzlbf");

      LambdaExpression fromExpression = Expression.Lambda (Expression.Constant (null, typeof (IQueryable<Student>)));
      AdditionalFromClause additionalFromClause = new AdditionalFromClause (mainFromClause, identifier2, fromExpression,
          ExpressionHelper.CreateLambdaExpression ());

      Expression studentExpression = Expression.MakeMemberAccess (Expression.Parameter (typeof (TransparentIdentifierClass<Student>), "transparent"),
          typeof (TransparentIdentifierClass<Student>).GetField ("fromIdentifier2"));

      Expression fieldExpression = Expression.MakeMemberAccess (studentExpression, typeof (Student).GetProperty ("First"));

      additionalFromClause.ResolveField (StubDatabaseInfo.Instance, fieldExpression, fieldExpression);
      
    }

    [Test]
    [ExpectedException (typeof (ParserException), ExpectedMessage = "The from identifier 'fromIdentifier2' has a different type (System.Int32) than "
        + "expected in expression 'transparent.fromIdentifier2.First' (Rubicon.Data.Linq.UnitTests.Student).")]
    public void Resolve_InvalidIdentifierType ()
    {
      ParameterExpression identifier1 = Expression.Parameter (typeof (Student), "fromIdentifier1");
      MainFromClause mainFromClause = new MainFromClause (identifier1, ExpressionHelper.CreateQuerySource ());
      ParameterExpression identifier2 = Expression.Parameter (typeof (int), "fromIdentifier2");

      LambdaExpression fromExpression = Expression.Lambda (Expression.Constant (null, typeof (IQueryable<Student>)));
      AdditionalFromClause additionalFromClause = new AdditionalFromClause (mainFromClause, identifier2, fromExpression,
          ExpressionHelper.CreateLambdaExpression ());

      Expression studentExpression = Expression.MakeMemberAccess (Expression.Parameter (typeof (TransparentIdentifierClass<Student>), "transparent"),
          typeof (TransparentIdentifierClass<Student>).GetField ("fromIdentifier2"));

      Expression fieldExpression = Expression.MakeMemberAccess (studentExpression, typeof (Student).GetProperty ("First"));

      additionalFromClause.ResolveField (StubDatabaseInfo.Instance, fieldExpression, fieldExpression);
      
    }

    [Test]
    [ExpectedException (typeof (ParserException), ExpectedMessage = "Expected MemberExpression for resolving field access, found ConstantExpression "
        + "(null).")]
    public void Resolve_InvalidOuterExpression ()
    {
      ParameterExpression identifier1 = Expression.Parameter (typeof (Student), "fromIdentifier1");
      MainFromClause mainFromClause = new MainFromClause (identifier1, ExpressionHelper.CreateQuerySource ());
      ParameterExpression identifier2 = Expression.Parameter (typeof (Student), "fromIdentifier2");

      LambdaExpression fromExpression = Expression.Lambda (Expression.Constant (null, typeof (IQueryable<Student>)));
      AdditionalFromClause additionalFromClause = new AdditionalFromClause (mainFromClause, identifier2, fromExpression,
          ExpressionHelper.CreateLambdaExpression ());
            
      Expression fieldExpression = Expression.Constant(null,typeof(Student));

      additionalFromClause.ResolveField (StubDatabaseInfo.Instance, fieldExpression, fieldExpression);

    }

    [Test]
    [ExpectedException (typeof (ParserException), ExpectedMessage = "There is no from clause defining identifier 'First', which is used in "
        + "expression 'null.First'.")]
    public void Resolve_InvalidInnerExpression ()
    {
      ParameterExpression identifier1 = Expression.Parameter (typeof (Student), "fromIdentifier1");
      MainFromClause mainFromClause = new MainFromClause (identifier1, ExpressionHelper.CreateQuerySource ());
      ParameterExpression identifier2 = Expression.Parameter (typeof (Student), "fromIdentifier2");

      LambdaExpression fromExpression = Expression.Lambda (Expression.Constant (null, typeof (IQueryable<Student>)));
      AdditionalFromClause additionalFromClause = new AdditionalFromClause (mainFromClause, identifier2, fromExpression,
          ExpressionHelper.CreateLambdaExpression ());

      Expression studentExpression = Expression.Constant (null, typeof (Student));

      Expression fieldExpression = Expression.MakeMemberAccess (studentExpression, typeof (Student).GetProperty ("First"));

      additionalFromClause.ResolveField (StubDatabaseInfo.Instance, fieldExpression, fieldExpression);
    }

    [Test]
    [ExpectedException (typeof (ParserException), ExpectedMessage = "There is no from clause defining identifier 'Student', which is used in "
        + "expression 'null.Student.First'.")]
    public void Resolve_InvalidInnerMostExpression ()
    {
      ParameterExpression identifier1 = Expression.Parameter (typeof (Student), "fromIdentifier1");
      MainFromClause mainFromClause = new MainFromClause (identifier1, ExpressionHelper.CreateQuerySource ());
      ParameterExpression identifier2 = Expression.Parameter (typeof (Student), "fromIdentifier2");

      LambdaExpression fromExpression = Expression.Lambda (Expression.Constant (null, typeof (IQueryable<Student>)));
      AdditionalFromClause additionalFromClause = new AdditionalFromClause (mainFromClause, identifier2, fromExpression,
          ExpressionHelper.CreateLambdaExpression ());

      Expression studentDetailExpression = Expression.Constant (null, typeof (Student_Detail));

      Expression studentExpression = Expression.MakeMemberAccess (studentDetailExpression,
          typeof (Student_Detail).GetProperty ("Student"));

      Expression fieldExpression = Expression.MakeMemberAccess (studentExpression, typeof (Student).GetProperty ("First"));

      additionalFromClause.ResolveField (StubDatabaseInfo.Instance, fieldExpression, fieldExpression);
    }


  }
}