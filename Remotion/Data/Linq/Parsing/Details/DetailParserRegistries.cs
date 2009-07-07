// This file is part of the re-motion Core Framework (www.re-motion.org)
// Copyright (C) 2005-2009 rubicon informationstechnologie gmbh, www.rubicon.eu
// 
// The re-motion Core Framework is free software; you can redistribute it 
// and/or modify it under the terms of the GNU Lesser General Public License 
// version 3.0 as published by the Free Software Foundation.
// 
// re-motion is distributed in the hope that it will be useful, 
// but WITHOUT ANY WARRANTY; without even the implied warranty of 
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the 
// GNU Lesser General Public License for more details.
// 
// You should have received a copy of the GNU Lesser General Public License
// along with re-motion; if not, see http://www.gnu.org/licenses.
// 
using Remotion.Data.Linq.Backend;
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