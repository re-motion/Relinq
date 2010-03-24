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
using Remotion.Data.Linq.Clauses.ExpressionTreeVisitors;
using Remotion.Data.Linq.Parsing;
using Remotion.Data.Linq.Utilities;

namespace Remotion.Data.Linq.SqlBackend.SqlStatementModel.SqlSpecificExpressions
{
  /// <summary>
  /// Corresponds to a SQL CASE expression.
  /// </summary>
  public class SqlCaseExpression : ExtensionExpression
  {
    private readonly Expression _testPredicate;
    private readonly Expression _thenValue;
    private readonly Expression _elseValue;

    public SqlCaseExpression (Expression testPredicate, Expression thenValue, Expression elseValue)
        : base (ArgumentUtility.CheckNotNull ("thenValue", thenValue).Type)
    {
      ArgumentUtility.CheckNotNull ("testPredicate", testPredicate);
      ArgumentUtility.CheckNotNull ("elseValue", elseValue);

      if (testPredicate.Type != typeof (bool))
        throw new ArgumentException ("The test predicate must have boolean type.", "testPredicate");
      if (thenValue.Type != elseValue.Type)
        throw new ArgumentException ("'Then' value and 'Else' value must have the same type.", "thenValue");

      _testPredicate = testPredicate;
      _thenValue = thenValue;
      _elseValue = elseValue;
    }

    public Expression TestPredicate
    {
      get { return _testPredicate; }
    }

    public Expression ThenValue
    {
      get { return _thenValue; }
    }

    public Expression ElseValue
    {
      get { return _elseValue; }
    }

    protected override Expression VisitChildren (ExpressionTreeVisitor visitor)
    {
      ArgumentUtility.CheckNotNull ("visitor", visitor);

      var testPredicate = visitor.VisitExpression (TestPredicate);
      var thenValue = visitor.VisitExpression (ThenValue);
      var elseValue = visitor.VisitExpression (ElseValue);

      if (testPredicate != TestPredicate || thenValue != ThenValue || elseValue != ElseValue)
        return new SqlCaseExpression (testPredicate, thenValue, elseValue);
      else
        return this;
    }

    public override Expression Accept (ExpressionTreeVisitor visitor)
    {
      ArgumentUtility.CheckNotNull ("visitor", visitor);

      var specificVisitor = visitor as ISqlSpecificExpressionVisitor;
      if (specificVisitor != null)
        return specificVisitor.VisitSqlCaseExpression (this);
      else
        return base.Accept (visitor);
    }

    public override string ToString ()
    {
      return 
          "CASE WHEN " 
          + FormattingExpressionTreeVisitor.Format (TestPredicate) 
          + " THEN " 
          + FormattingExpressionTreeVisitor.Format (ThenValue) 
          + " ELSE " 
          + FormattingExpressionTreeVisitor.Format (ElseValue) 
          + " END";
    }
  }
}