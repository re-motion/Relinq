using System.Collections.Generic;
using System.Linq.Expressions;
using NUnit.Framework;
using Rhino.Mocks;
using Rubicon.Data.Linq.Clauses;
using Rubicon.Data.Linq.DataObjectModel;
using Rubicon.Data.Linq.Parsing.Details.WhereConditionParsing;
using Rubicon.Data.Linq.Parsing.FieldResolving;
using NUnit.Framework.SyntaxHelpers;

namespace Rubicon.Data.Linq.UnitTests.ParsingTest.DetailsTest.WhereConditionParsingTest
{
  [TestFixture]
  public class ParameterExpressionParserTest
  {
    [Test]
    public void Parse ()
    {
      ParameterExpression parameter = Expression.Parameter (typeof (Student), "s");
      MainFromClause fromClause = ExpressionHelper.CreateMainFromClause (parameter, ExpressionHelper.CreateQuerySource ());

      QueryModel queryModel = ExpressionHelper.CreateQueryModel (fromClause);
      FromClauseFieldResolver resolver =
          new FromClauseFieldResolver (StubDatabaseInfo.Instance, new JoinedTableContext (), new WhereFieldAccessPolicy (StubDatabaseInfo.Instance));

      List<FieldDescriptor> fieldDescriptorCollection = new List<FieldDescriptor> ();
      var fromSource = fromClause.GetFromSource (StubDatabaseInfo.Instance);
      FieldSourcePath path = new FieldSourcePath (fromSource, new SingleJoin[0]);
      FieldDescriptor expectedFieldDescriptor = new FieldDescriptor (null, fromClause, path, new Column (fromSource, "IDColumn"));
      ICriterion expectedCriterion = expectedFieldDescriptor.Column;

      ParameterExpressionParser parser = new ParameterExpressionParser (queryModel, resolver);

      ICriterion actualCriterion = parser.Parse (parameter, fieldDescriptorCollection);
      Assert.AreEqual (expectedCriterion, actualCriterion);
      Assert.That (fieldDescriptorCollection, Is.EqualTo (new[] { expectedFieldDescriptor }));
    }
  }
}