using System.Collections.Generic;
using System.Linq.Expressions;
using NUnit.Framework;
using Rubicon.Data.Linq.Clauses;
using Rubicon.Data.Linq.DataObjectModel;
using Rubicon.Data.Linq.Parsing.Details.WhereConditionParsing;
using Rubicon.Data.Linq.Parsing.FieldResolving;

namespace Rubicon.Data.Linq.UnitTests.ParsingTest.DetailsTest.WhereConditionParsingTest
{
  [TestFixture]
  public class MemberExpressionParserTest
  {
    [Test]
    public void Parse()
    {
      ParameterExpression parameter = Expression.Parameter (typeof (Student), "s");
      MainFromClause fromClause = ExpressionHelper.CreateMainFromClause(parameter, ExpressionHelper.CreateQuerySource ());;
      QueryModel queryModel = ExpressionHelper.CreateQueryModel (fromClause);
      JoinedTableContext context = new JoinedTableContext ();
      ClauseFieldResolver resolver = 
        new ClauseFieldResolver(StubDatabaseInfo.Instance,context,new WhereFieldAccessPolicy(StubDatabaseInfo.Instance));
      MemberExpressionParser parser = new MemberExpressionParser (queryModel, resolver);

      List<FieldDescriptor> fieldCollection = new List<FieldDescriptor>();
      MemberExpression memberExpression = Expression.MakeMemberAccess (parameter, typeof (Student).GetProperty ("ID"));
      parser.Parse (memberExpression, fieldCollection);
      Assert.IsNotNull (fieldCollection);
    }
  }
}