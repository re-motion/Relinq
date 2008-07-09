/* Copyright (C) 2005 - 2008 rubicon informationstechnologie gmbh
 *
 * This program is free software: you can redistribute it and/or modify it under 
 * the terms of the re:motion license agreement in license.txt. If you did not 
 * receive it, please visit http://www.re-motion.org/licensing.
 * 
 * Unless otherwise provided, this software is distributed on an "AS IS" basis, 
 * WITHOUT WARRANTY OF ANY KIND, either express or implied. 
 */

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
