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

namespace Remotion.Data.Linq.Parsing.Details.WhereConditionParsing
{
  public class ParameterExpressionParser : IWhereConditionParser
  {
    private readonly ClauseFieldResolver _resolver;

    public ParameterExpressionParser (ClauseFieldResolver resolver)
    {
      ArgumentUtility.CheckNotNull ("resolver", resolver);
      _resolver = resolver;
    }

    public ICriterion Parse (ParameterExpression parameterExpression, ParseContext parseContext)
    {
      FieldDescriptor fieldDescriptor = parseContext.QueryModel.ResolveField (_resolver, parameterExpression, parseContext.JoinedTableContext);
      parseContext.FieldDescriptors.Add (fieldDescriptor);
      return fieldDescriptor.GetMandatoryColumn ();
    }

    ICriterion IWhereConditionParser.Parse (Expression expression, ParseContext parseContext)
    {
      return Parse ((ParameterExpression) expression, parseContext);
    }

    public bool CanParse(Expression expression)
    {
      return expression is ParameterExpression;
    }
  }
}
