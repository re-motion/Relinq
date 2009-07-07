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
using System.Linq.Expressions;
using Remotion.Data.Linq.Clauses.Expressions;
using Remotion.Data.Linq.Backend.DataObjectModel;
using Remotion.Data.Linq.Parsing.FieldResolving;
using Remotion.Utilities;

namespace Remotion.Data.Linq.Parsing.Details.SelectProjectionParsing
{
  public class QuerySourceReferenceExpressionParser : ISelectProjectionParser
  {
    // query source reference expression parsing is the same for where conditions and select projections, so delegate to that implementation
    private readonly WhereConditionParsing.QuerySourceReferenceExpressionParser _innerParser;

    public QuerySourceReferenceExpressionParser (FieldResolver resolver)
    {
      ArgumentUtility.CheckNotNull ("resolver", resolver);
      _innerParser = new WhereConditionParsing.QuerySourceReferenceExpressionParser (resolver);
    }

    public IEvaluation Parse (QuerySourceReferenceExpression referenceExpression, ParseContext parseContext)
    {
      ArgumentUtility.CheckNotNull ("referenceExpression", referenceExpression);
      ArgumentUtility.CheckNotNull ("parseContext", parseContext);
      return _innerParser.Parse (referenceExpression, parseContext);
    }

    IEvaluation ISelectProjectionParser.Parse (Expression expression, ParseContext parseContext)
    {
      ArgumentUtility.CheckNotNull ("expression", expression);
      ArgumentUtility.CheckNotNull ("parseContext", parseContext);
      return Parse ((QuerySourceReferenceExpression) expression, parseContext);
    }

    public bool CanParse(Expression expression)
    {
      ArgumentUtility.CheckNotNull ("expression", expression);
      return expression is QuerySourceReferenceExpression;
    }
  }
}
