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
using System.Diagnostics;
using System.Linq.Expressions;
using Remotion.Data.Linq.Clauses.ExpressionTreeVisitors;
using Remotion.Utilities;

namespace Remotion.Data.Linq.Clauses
{
  public class GroupClause : ISelectGroupClause
  {
    private Expression _groupExpression;
    private Expression _byExpression;

    public GroupClause (Expression groupExpression, Expression byExpression)
    {
      ArgumentUtility.CheckNotNull ("groupExpression", groupExpression);
      ArgumentUtility.CheckNotNull ("byExpression", byExpression);

      _groupExpression = groupExpression;
      _byExpression = byExpression;
    }

    [DebuggerDisplay ("{Remotion.Data.Linq.StringBuilding.FormattingExpressionTreeVisitor.Format (GroupExpression),nq}")]
    public Expression GroupExpression
    {
      get { return _groupExpression; }
      set { _groupExpression = ArgumentUtility.CheckNotNull("value", value); }
    }

    [DebuggerDisplay ("{Remotion.Data.Linq.StringBuilding.FormattingExpressionTreeVisitor.Format (ByExpression),nq}")]
    public Expression ByExpression
    {
      get { return _byExpression; }
      set { _byExpression = ArgumentUtility.CheckNotNull ("value", value); }
    }

    public void Accept (IQueryModelVisitor visitor)
    {
      ArgumentUtility.CheckNotNull ("visitor", visitor);
      visitor.VisitGroupClause (this);
    }

    public GroupClause Clone (CloneContext cloneContext)
    {
      ArgumentUtility.CheckNotNull ("cloneContext", cloneContext);

      var newGroupExpression = CloneExpressionTreeVisitor.ReplaceClauseReferences (GroupExpression, cloneContext);
      var newByExpression = CloneExpressionTreeVisitor.ReplaceClauseReferences (ByExpression, cloneContext);
      var result = new GroupClause (newGroupExpression, newByExpression);
      cloneContext.ClauseMapping.AddMapping (this, result);
      return result;
    }

    public IExecutionStrategy GetExecutionStrategy ()
    {
      throw new NotImplementedException();
    }

    ISelectGroupClause ISelectGroupClause.Clone (CloneContext cloneContext)
    {
      return Clone (cloneContext);
    }
  }
}
