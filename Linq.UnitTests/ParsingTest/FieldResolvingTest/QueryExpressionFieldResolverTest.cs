using System;
using System.Linq.Expressions;
using NUnit.Framework;
using Rubicon.Data.Linq.Clauses;
using Rubicon.Data.Linq.DataObjectModel;
using Rubicon.Data.Linq.Parsing.FieldResolving;

namespace Rubicon.Data.Linq.UnitTests.ParsingTest.FieldResolvingTest
{
  [TestFixture]
  public class QueryExpressionFieldResolverTest
  {
    private FromClauseFieldResolver _resolver;
    private WhereFieldAccessPolicy _policy;
    private JoinedTableContext _context;

    [SetUp]
    public void SetUp ()
    {
      _policy = new WhereFieldAccessPolicy (StubDatabaseInfo.Instance);
      _context = new JoinedTableContext();
      _resolver = new FromClauseFieldResolver (StubDatabaseInfo.Instance, _context, _policy);
    }

    [Test]
    public void ResolveField ()
    {
      QueryExpression queryExpression = CreateQueryExpressionForResolve ();

      Expression fieldAccessExpression = Expression.Parameter (typeof (String), "s1");
      FieldDescriptor descriptor = new QueryExpressionFieldResolver(queryExpression).ResolveField (_resolver, fieldAccessExpression);

      IFromSource expectedTable = queryExpression.MainFromClause.GetFromSource (StubDatabaseInfo.Instance);
      FieldSourcePath expectedPath = new FieldSourcePath(expectedTable, new SingleJoin[0]);

      Assert.AreSame (queryExpression.MainFromClause, descriptor.FromClause);
      Assert.AreEqual (new Column (expectedTable, "*"), descriptor.Column);
      Assert.IsNull (descriptor.Member);
      Assert.AreEqual (expectedPath, descriptor.SourcePath);
    }

    [Test]
    [ExpectedException (typeof (FieldAccessResolveException), ExpectedMessage = "The field access expression 'fzlbf' does "
        + "not contain a from clause identifier.")]
    public void NoFromIdentifierFound ()
    {
      QueryExpression queryExpression = CreateQueryExpressionForResolve ();
      Expression sourceExpression = Expression.Parameter (typeof (Student), "fzlbf");

      new QueryExpressionFieldResolver (queryExpression).ResolveField (_resolver, sourceExpression);
    }

    [Test]
    public void ResolveInParentQuery ()
    {
      QueryExpression parentQueryExpression = CreateQueryExpressionForResolve ();
      QueryExpression subQueryExpression =
          ExpressionHelper.CreateQueryExpression (new MainFromClause (Expression.Parameter (typeof (Student), "a"), Expression.Constant (null)));
      subQueryExpression.SetParentQuery (parentQueryExpression);
      Expression sourceExpression = Expression.Parameter (typeof (string), "s1");

      QueryExpressionFieldResolver fieldResolver = new QueryExpressionFieldResolver (subQueryExpression);

      FieldDescriptor fieldDescriptor = fieldResolver.ResolveField (_resolver, sourceExpression);
      Assert.AreSame (parentQueryExpression.MainFromClause, fieldDescriptor.FromClause);
    }

    private QueryExpression CreateQueryExpressionForResolve ()
    {
      ParameterExpression s1 = Expression.Parameter (typeof (String), "s1");
      ParameterExpression s2 = Expression.Parameter (typeof (String), "s2");
      MainFromClause mainFromClause = ExpressionHelper.CreateMainFromClause(s1, ExpressionHelper.CreateQuerySource ());
      AdditionalFromClause additionalFromClause =
          new AdditionalFromClause (mainFromClause, s2, ExpressionHelper.CreateLambdaExpression (), ExpressionHelper.CreateLambdaExpression ());

      var expression = ExpressionHelper.CreateQueryExpression (mainFromClause);
      
      expression.AddBodyClause (additionalFromClause);

      return expression;
    }
  }
}