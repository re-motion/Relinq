using System.Collections.Generic;
using System.Linq.Expressions;
using NUnit.Framework;
using NUnit.Framework.SyntaxHelpers;
using Remotion.Data.Linq.Clauses;
using Remotion.Data.Linq.DataObjectModel;
using Remotion.Data.Linq.Parsing.Details.SelectProjectionParsing;
using Remotion.Data.Linq.Parsing.FieldResolving;

namespace Remotion.Data.Linq.UnitTests.ParsingTest.DetailsTest.SelectProjectionParsingTest
{
  [TestFixture]
  public class MemberExpressionParserTest : DetailParserTestBase
  {
    [Test]
    public void Parse ()
    {
      ParameterExpression parameter = Expression.Parameter (typeof (Student), "s");
      MainFromClause fromClause = ExpressionHelper.CreateMainFromClause (parameter, ExpressionHelper.CreateQuerySource ());
      QueryModel queryModel = ExpressionHelper.CreateQueryModel (fromClause);

      ClauseFieldResolver resolver =
          new ClauseFieldResolver (StubDatabaseInfo.Instance, new SelectFieldAccessPolicy ());

      MemberExpressionParser parser = new MemberExpressionParser (resolver);
      List<FieldDescriptor> fieldDescriptorCollection = new List<FieldDescriptor> ();
      MemberExpression memberExpression = Expression.MakeMemberAccess (parameter, typeof (Student).GetProperty ("ID"));
      var fromSource = fromClause.GetFromSource (StubDatabaseInfo.Instance);
      FieldSourcePath path = new FieldSourcePath (fromSource, new SingleJoin[0]);
      FieldDescriptor expectedFieldDescriptor = new FieldDescriptor (null, path, new Column (fromSource, "IDColumn"));

      IEvaluation actualEvaluation = parser.Parse (memberExpression, ParseContext);
      IEvaluation expectedEvaluation = expectedFieldDescriptor.Column;

      Assert.IsNotNull (fieldDescriptorCollection);
      Assert.AreEqual (expectedEvaluation, actualEvaluation);
    }
  }
}