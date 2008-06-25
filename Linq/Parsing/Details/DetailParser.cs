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

    public DetailParser (IDatabaseInfo databaseInfo, ParseMode parseMode)
    {
       _whereConditionParserRegistry = new WhereConditionParserRegistry (databaseInfo);
       _selectProjectionParserRegistry = new SelectProjectionParserRegistry (databaseInfo, parseMode);
    }
  }
}