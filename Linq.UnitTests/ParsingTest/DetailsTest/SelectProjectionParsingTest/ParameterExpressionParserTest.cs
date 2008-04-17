using System.Collections.Generic;
using System.Linq.Expressions;
using NUnit.Framework;
using Rubicon.Data.Linq.Clauses;
using Rubicon.Data.Linq.DataObjectModel;
using Rubicon.Data.Linq.Parsing.Details.SelectProjectionParsing;
using Rubicon.Data.Linq.Parsing.FieldResolving;

namespace Rubicon.Data.Linq.UnitTests.ParsingTest.DetailsTest.SelectProjectionParsingTest
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

      ClauseFieldResolver resolver =
          new ClauseFieldResolver (StubDatabaseInfo.Instance, new JoinedTableContext (), new SelectFieldAccessPolicy());

      List<FieldDescriptor> fieldDescriptorCollection = new List<FieldDescriptor> ();
      var fromSource = fromClause.GetFromSource (StubDatabaseInfo.Instance);
      FieldSourcePath path = new FieldSourcePath (fromSource, new SingleJoin[0]);
      FieldDescriptor expectedFieldDescriptor = new FieldDescriptor (null, path, new Column (fromSource, "*"));

      IEvaluation expectedEvaluation = expectedFieldDescriptor.Column;

      ParameterExpressionParser parser = new ParameterExpressionParser (queryModel, resolver);

      IEvaluation actualEvaluation = parser.Parse (parameter, fieldDescriptorCollection);
      Assert.AreEqual (expectedEvaluation, actualEvaluation);
    }
  }
}