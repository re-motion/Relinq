using System.Linq.Expressions;
using NUnit.Framework;
using Rubicon.Data.Linq.Clauses;
using Rubicon.Data.Linq.DataObjectModel;
using Rubicon.Data.Linq.Parsing.Details.SelectProjectionParsing;
using Rubicon.Data.Linq.Parsing.FieldResolving;

namespace Rubicon.Data.Linq.UnitTests.ParsingTest.DetailsTest.SelectProjectionParsingTest
{
  [TestFixture]
  public class ConstantExpressionParserTest
  {
    [Test]
    public void Parse ()
    {
      ConstantExpression constantExpression = Expression.Constant (5);

      ParameterExpression parameter = Expression.Parameter (typeof (Student), "s");
      MainFromClause fromClause = ExpressionHelper.CreateMainFromClause (parameter, ExpressionHelper.CreateQuerySource ());
      QueryModel queryModel = ExpressionHelper.CreateQueryModel (fromClause);
      ClauseFieldResolver resolver =
          new ClauseFieldResolver (StubDatabaseInfo.Instance, new JoinedTableContext (), new SelectFieldAccessPolicy ());
      
      ConstantExpressionParser parser = new ConstantExpressionParser(StubDatabaseInfo.Instance);

      IEvaluation result = parser.Parse (constantExpression);

      //expected
      IEvaluation expected = new Constant (5);

      Assert.AreEqual (expected, result);
    }
  }
}