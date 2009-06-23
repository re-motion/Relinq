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
using System.Linq;
using System.Linq.Expressions;
using Remotion.Data.Linq.Clauses.Expressions;
using Remotion.Data.Linq.Parsing.Details.WhereConditionParsing;
using Remotion.Data.Linq.Parsing.FieldResolving;

namespace Remotion.Data.Linq.Parsing.Details
{
  public class WhereConditionParserRegistry
  {
    private readonly ParserRegistry _parserRegistry;

    public WhereConditionParserRegistry (IDatabaseInfo databaseInfo)
    {
      _parserRegistry = new ParserRegistry ();
      var resolver = new FieldResolver (databaseInfo, new WhereFieldAccessPolicy (databaseInfo));

      RegisterParser (typeof (BinaryExpression), new BinaryExpressionParser (this));
      RegisterParser (typeof (MemberExpression), new MemberExpressionParser (resolver));
      RegisterParser (typeof (ConstantExpression), new ConstantExpressionParser (databaseInfo));
      RegisterParser (typeof (MethodCallExpression), new MethodCallExpressionParser (this));
      RegisterParser (typeof (MethodCallExpression), new LikeParser (this));
      RegisterParser (typeof (MethodCallExpression), new ContainsParser (this));
      RegisterParser (typeof (MethodCallExpression), new ContainsFullTextParser (this));
      RegisterParser (typeof (SubQueryExpression), new SubQueryExpressionParser ());
      RegisterParser (typeof (UnaryExpression), new UnaryExpressionParser (this));
      RegisterParser (typeof (QuerySourceReferenceExpression), new QuerySourceReferenceExpressionParser (resolver));
    }

    public IEnumerable<IWhereConditionParser> GetParsers (Type expressionType)
    {
      return _parserRegistry.GetParsers (expressionType).Cast<IWhereConditionParser> ();
    }
    
    public virtual IWhereConditionParser GetParser (Expression expression)
    {
      return (IWhereConditionParser) _parserRegistry.GetParser (expression);
    }

    public void RegisterParser (Type expressionType, IWhereConditionParser parser)
    {
      _parserRegistry.RegisterParser (expressionType, parser);
    }
  }
}
