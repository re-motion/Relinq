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
using System.Collections.Generic;
using System.Linq.Expressions;
using Remotion.Data.Linq.Clauses;
using Remotion.Utilities;


namespace Remotion.Data.Linq.Parsing.Structure.Legacy
{
  public class QueryParser
  {
    private readonly SourceExpressionParser _sourceParser = new SourceExpressionParser (true);

    public QueryParser (Expression expressionTreeRoot)
    {
      ArgumentUtility.CheckNotNull ("expressionTreeRoot", expressionTreeRoot);
      SourceExpression = expressionTreeRoot;
    }

    public Expression SourceExpression { get; private set; }

    public QueryModel GetParsedQuery ()
    {
      ParseResultCollector resultCollector = new ParseResultCollector (SourceExpression);
      _sourceParser.Parse (resultCollector, SourceExpression, null, "parsing query");

      List<QueryModel> subQueries = new List<QueryModel> ();
      resultCollector.Simplify (subQueries);

      QueryModelCreator modelCreator = new QueryModelCreator (SourceExpression, resultCollector);
      QueryModel model = modelCreator.CreateQueryModel();

      foreach (QueryModel subQuery in subQueries)
        subQuery.SetParentQuery (model);

      return model;
    }

    

  }
}