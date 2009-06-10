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
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Remotion.Data.Linq.Expressions;
using Remotion.Data.Linq.Parsing.Details.SelectProjectionParsing;
using Remotion.Data.Linq.Parsing.FieldResolving;
using System.Linq;

namespace Remotion.Data.Linq.Parsing.Details
{
  public class SelectProjectionParserRegistry
  {
    private readonly ParserRegistry _parserRegistry;

    public SelectProjectionParserRegistry (IDatabaseInfo databaseInfo, ParseMode parseMode)
    {
      _parserRegistry = new ParserRegistry ();
      
      IResolveFieldAccessPolicy policy;
      if (parseMode == ParseMode.SubQueryInWhere)
        policy = new WhereFieldAccessPolicy (databaseInfo);
      else
        policy = new SelectFieldAccessPolicy();

      ClauseFieldResolver resolver = new ClauseFieldResolver (databaseInfo, policy);

      RegisterParser (typeof (BinaryExpression), new BinaryExpressionParser (this));
      RegisterParser (typeof (ConstantExpression), new ConstantExpressionParser (databaseInfo));
      RegisterParser (typeof (MemberExpression), new MemberExpressionParser (resolver));
      RegisterParser (typeof (MethodCallExpression), new MethodCallExpressionParser (this));
      RegisterParser (typeof (NewExpression), new NewExpressionParser (this));
      RegisterParser (typeof (ParameterExpression), new ParameterExpressionParser (resolver));
      RegisterParser (typeof (SubQueryExpression), new SubQueryExpressionParser ());
    }

    public IEnumerable<ISelectProjectionParser> GetParsers (Type expressionType)
    {
      return _parserRegistry.GetParsers (expressionType).Cast<ISelectProjectionParser>();
    }

    public ISelectProjectionParser GetParser (Expression expression)
    {
      return (ISelectProjectionParser) _parserRegistry.GetParser (expression);
    }

    public void RegisterParser (Type expressionType, ISelectProjectionParser parser)
    {
      _parserRegistry.RegisterParser (expressionType, parser);
    }
  }
}
