using System.Linq.Expressions;
using NUnit.Framework;
using System.Collections.Generic;
using Remotion.Data.Linq.Clauses;
using Remotion.Data.Linq.DataObjectModel;
using Remotion.Data.Linq.Parsing.Details;
using Remotion.Data.Linq.Parsing.Details.WhereConditionParsing;
using Remotion.Data.Linq.Parsing.FieldResolving;

namespace Remotion.Data.Linq.UnitTests.ParsingTest.DetailsTest.WhereConditionParsingTest
{
  [TestFixture]
  public class UnaryExpressionParserTest
  {
    [Test]
    public void Parse ()
    {
      QueryModel queryModel = ExpressionHelper.CreateQueryModel();

      WhereClause whereClause = ExpressionHelper.CreateWhereClause ();
      UnaryExpression unaryExpression = Expression.Not (Expression.Constant (5));

      ICriterion criterion = new Constant (5);
      ICriterion expectedCriterion = new NotCriterion (new Constant (5));

      WhereConditionParserRegistry parserRegistry = 
        new WhereConditionParserRegistry (queryModel, StubDatabaseInfo.Instance, new JoinedTableContext());
      parserRegistry.RegisterParser (typeof(ConstantExpression), new ConstantExpressionParser (StubDatabaseInfo.Instance));

      UnaryExpressionParser parser = new UnaryExpressionParser(queryModel.GetExpressionTree(),parserRegistry);

      List<FieldDescriptor> fieldCollection = new List<FieldDescriptor> ();
      ICriterion actualCriterion = parser.Parse (unaryExpression, fieldCollection);
      Assert.AreEqual (expectedCriterion, actualCriterion);
    }
  }
}