using System.Collections.Generic;
using NUnit.Framework;
using Remotion.Data.Linq.DataObjectModel;
using Remotion.Data.Linq.Parsing.Details;
using Remotion.Data.Linq.Parsing.FieldResolving;

namespace Remotion.Data.Linq.UnitTests.ParsingTest.DetailsTest
{
  public class DetailParserTestBase
  {
    protected QueryModel QueryModel;
    protected ParseContext ParseContext;

    [SetUp]
    public virtual void SetUp ()
    {
      QueryModel = ExpressionHelper.CreateQueryModel ();
      ParseContext = new ParseContext(QueryModel, QueryModel.GetExpressionTree(), new List<FieldDescriptor>(), new JoinedTableContext());
    }
  }
}