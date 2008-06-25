using Remotion.Data.Linq.Parsing.FieldResolving;

namespace Remotion.Data.Linq.Parsing.Details
{
  public class DetailParser
  {
    private readonly WhereConditionParserRegistry _whereConditionParserRegistry;
    private readonly SelectProjectionParserRegistry _selectProjectionParserRegistry;

    public WhereConditionParserRegistry WhereConditionParser
    {
      get { return _whereConditionParserRegistry; }
    }

    public SelectProjectionParserRegistry SelectProjectionParser
    {
      get { return _selectProjectionParserRegistry;  }
    }

    public DetailParser (QueryModel queryModel, IDatabaseInfo databaseInfo, JoinedTableContext context, ParseMode parseMode)
    {
       _whereConditionParserRegistry = new WhereConditionParserRegistry (queryModel, databaseInfo, context);
       _selectProjectionParserRegistry = new SelectProjectionParserRegistry (queryModel, databaseInfo, context, parseMode);
    }

    

  }
}