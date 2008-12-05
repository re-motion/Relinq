// This file is part of the re-motion Core Framework (www.re-motion.org)
// Copyright (C) 2005-2008 rubicon informationstechnologie gmbh, www.rubicon.eu
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
