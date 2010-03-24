// This file is part of the re-motion Core Framework (www.re-motion.org)
// Copyright (C) 2005-2009 rubicon informationstechnologie gmbh, www.rubicon.eu
// 
// The re-motion Core Framework is free software; you can redistribute it 
// and/or modify it under the terms of the GNU Lesser General Public License 
// as published by the Free Software Foundation; either version 2.1 of the 
// License, or (at your option) any later version.
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
using System.Linq.Expressions;
using Remotion.Data.Linq.Clauses.Expressions;
using Remotion.Data.Linq.Parsing;
using Remotion.Data.Linq.Utilities;

namespace Remotion.Data.Linq.SqlBackend.SqlStatementModel.SqlSpecificExpressions
{
  public class SqlLiteralExpression : ExtensionExpression
  {
    private readonly object _value;

    public SqlLiteralExpression (object value)
        : base (ArgumentUtility.CheckNotNull ("value", value).GetType())
    {
      if (Type != typeof (int))
      {
        var message = string.Format ("SqlLiteralExpression does not support values of type '{0}'.", Type);
        throw new ArgumentTypeException (message, "value", Type, typeof (int));
      }

      _value = value;
    }

    public object Value
    {
      get { return _value; }
    }

    protected override Expression VisitChildren (ExpressionTreeVisitor visitor)
    {
      return this;
    }

    public override Expression Accept (ExpressionTreeVisitor visitor)
    {
      ArgumentUtility.CheckNotNull ("visitor", visitor);

      var specificVisitor = visitor as ISqlSpecificExpressionVisitor;
      if (specificVisitor != null)
        return specificVisitor.VisitSqlLiteralExpression (this);
      else
        return base.Accept (visitor);
    }

    public override string ToString ()
    {
      return Value.ToString();
    }
  }
}