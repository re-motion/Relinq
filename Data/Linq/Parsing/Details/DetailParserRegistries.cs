using Remotion.Data.Linq.Parsing.FieldResolving;

namespace Remotion.Data.Linq.Parsing.Details
{
  public class DetailParserRegistries
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

    public DetailParserRegistries (IDatabaseInfo databaseInfo, ParseMode parseMode)
    {
       _whereConditionParserRegistry = new WhereConditionParserRegistry (databaseInfo);
       _selectProjectionParserRegistry = new SelectProjectionParserRegistry (databaseInfo, parseMode);
    }
  }
}