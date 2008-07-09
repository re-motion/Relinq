/* Copyright (C) 2005 - 2008 rubicon informationstechnologie gmbh
 *
 * This program is free software: you can redistribute it and/or modify it under 
 * the terms of the re:motion license agreement in license.txt. If you did not 
 * receive it, please visit http://www.re-motion.org/licensing.
 * 
 * Unless otherwise provided, this software is distributed on an "AS IS" basis, 
 * WITHOUT WARRANTY OF ANY KIND, either express or implied. 
 */

using System.Collections.Generic;
using System.Linq.Expressions;
using Remotion.Data.Linq.DataObjectModel;
using Remotion.Data.Linq.Parsing.FieldResolving;
using Remotion.Utilities;

namespace Remotion.Data.Linq.Parsing.Details.SelectProjectionParsing
{
  public class MemberExpressionParser : ISelectProjectionParser
  {
    // member expression parsing is the same for where conditions and select projections, so delegate to that implementation
    private readonly WhereConditionParsing.MemberExpressionParser _innerParser;

    public MemberExpressionParser (ClauseFieldResolver resolver)
    {
      ArgumentUtility.CheckNotNull ("resolver", resolver);
      _innerParser = new WhereConditionParsing.MemberExpressionParser (resolver);
    }

    public virtual IEvaluation Parse (MemberExpression memberExpression, ParseContext parseContext)
    {
      ArgumentUtility.CheckNotNull ("memberExpression", memberExpression);
      ArgumentUtility.CheckNotNull ("parseContext", parseContext);
      return _innerParser.Parse (memberExpression, parseContext);
    }

    IEvaluation ISelectProjectionParser.Parse (Expression expression, ParseContext parseContext)
    {
      ArgumentUtility.CheckNotNull ("expression", expression);
      ArgumentUtility.CheckNotNull ("parseContext", parseContext);
      return Parse ((MemberExpression) expression, parseContext);
    }

    public bool CanParse(Expression expression)
    {
      ArgumentUtility.CheckNotNull ("expression", expression);
      return expression is MemberExpression;
    }
  }
}
