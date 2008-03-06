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
    [Test]
    public void ResolveField ()
    {
      QueryExpression queryExpression = CreateQueryExpressionForResolve ();

      Expression fieldAccessExpression = Expression.Parameter (typeof (String), "s1");
      JoinedTableContext context = new JoinedTableContext ();
      WhereFieldAccessPolicy policy = new WhereFieldAccessPolicy (StubDatabaseInfo.Instance);
      FromClauseFieldResolver resolver = new FromClauseFieldResolver (StubDatabaseInfo.Instance, context, policy);
      FieldDescriptor descriptor = new QueryExpressionFieldResolver(queryExpression).ResolveField (resolver, fieldAccessExpression);

      Table expectedTable = queryExpression.MainFromClause.GetTable (StubDatabaseInfo.Instance);
      FieldSourcePath expectedPath = new FieldSourcePath(expectedTable, new SingleJoin[0]);

      Assert.AreSame (queryExpression.MainFromClause, descriptor.FromClause);
      Assert.AreEqual (new Column (expectedTable, "*"), descriptor.Column);
      Assert.IsNull (descriptor.Member);
      Assert.AreEqual (expectedPath, descriptor.SourcePath);
    }

    private QueryExpression CreateQueryExpressionForResolve ()
    {
      ParameterExpression s1 = Expression.Parameter (typeof (String), "s1");
      ParameterExpression s2 = Expression.Parameter (typeof (String), "s2");
      MainFromClause mainFromClause = new MainFromClause (s1, ExpressionHelper.CreateQuerySource ());
      AdditionalFromClause additionalFromClause =
          new AdditionalFromClause (mainFromClause, s2, ExpressionHelper.CreateLambdaExpression (), ExpressionHelper.CreateLambdaExpression ());

      QueryBody queryBody = new QueryBody (ExpressionHelper.CreateSelectClause ());
      queryBody.Add (additionalFromClause);

      return new QueryExpression (mainFromClause, queryBody);
    }
  }
}