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
using Remotion.Utilities;

namespace Remotion.Data.Linq.Parsing.Details.WhereConditionParsing
{
  public class ConstantExpressionParser : IWhereConditionParser
  {
    private readonly IDatabaseInfo _databaseInfo;

    public ConstantExpressionParser(IDatabaseInfo databaseInfo)
    {
      ArgumentUtility.CheckNotNull ("databaseInfo", databaseInfo);
      _databaseInfo = databaseInfo;
    }

    public ICriterion Parse (ConstantExpression constantExpression, ParseContext parseContext)
    {
      object newValue = _databaseInfo.ProcessWhereParameter (constantExpression.Value);
      return new Constant (newValue);
    }

    public bool CanParse(Expression expression)
    {
      return expression is ConstantExpression;
    }

    ICriterion IWhereConditionParser.Parse(Expression expression, ParseContext parseContext)
    {
      return Parse ((ConstantExpression) expression, parseContext);
    }
  }
}
