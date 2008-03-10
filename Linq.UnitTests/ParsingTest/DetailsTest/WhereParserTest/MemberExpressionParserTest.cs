using System.Collections.Generic;
using System.Linq.Expressions;
using NUnit.Framework;
using Rubicon.Data.Linq.Clauses;
using Rubicon.Data.Linq.DataObjectModel;
using Rubicon.Data.Linq.Parsing.Details.WhereParser;
using Rubicon.Data.Linq.Parsing.FieldResolving;

namespace Rubicon.Data.Linq.UnitTests.ParsingTest.DetailsTest.WhereParserTest
{
  [TestFixture]
  public class MemberExpressionParserTest
  {
    [Test]
    public void Parse()
    {
      ParameterExpression parameter = Expression.Parameter (typeof (Student), "s");
      MainFromClause fromClause = new MainFromClause (parameter, ExpressionHelper.CreateQuerySource ());;
      QueryExpression queryExpression = new QueryExpression (fromClause, ExpressionHelper.CreateQueryBody());
      JoinedTableContext context = new JoinedTableContext ();
      FromClauseFieldResolver resolver = 
        new FromClauseFieldResolver(StubDatabaseInfo.Instance,context,new WhereFieldAccessPolicy(StubDatabaseInfo.Instance));
      MemberExpressionParser parser = new MemberExpressionParser (queryExpression, resolver);

      List<FieldDescriptor> fieldCollection = new List<FieldDescriptor>();
      MemberExpression memberExpression = Expression.MakeMemberAccess (parameter, typeof (Student).GetProperty ("ID"));
      parser.Parse (memberExpression, fieldCollection);
      Assert.IsNotNull (fieldCollection);
    }
  }
}