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
  public class JoinClause : IClause
  {
    private readonly IClause _previousClause;
    private readonly FromClauseBase _fromClause;
    private Type _itemType;
    private string _itemName;
    private ParameterExpression _intoIdentifier;
    private Expression _inExpression;
    private Expression _equalityExpression;
    private Expression _onExpression;

    public JoinClause (IClause previousClause, FromClauseBase fromClause, string name, Type itemType, Expression inExpression, Expression onExpression, Expression equalityExpression)
      : this (previousClause, fromClause, name, itemType, inExpression, onExpression, equalityExpression, null)
    {
    }

    public JoinClause (IClause previousClause, FromClauseBase fromClause, string name, Type itemType, Expression inExpression, Expression onExpression, 
                       Expression equalityExpression,ParameterExpression intoIdentifier)
    {
      ArgumentUtility.CheckNotNull ("previousClause", previousClause);
      ArgumentUtility.CheckNotNull ("name", name);
      ArgumentUtility.CheckNotNull ("itemType", itemType);
      ArgumentUtility.CheckNotNull ("inExpression", inExpression);
      ArgumentUtility.CheckNotNull ("onExpression", onExpression);
      ArgumentUtility.CheckNotNull ("equalityExpression", equalityExpression);
      ArgumentUtility.CheckNotNull ("fromClause", fromClause);

      _previousClause = previousClause;
      _fromClause = fromClause;
      _itemName = name;
      _itemType = itemType;
      _inExpression = inExpression;
      _onExpression = onExpression;
      _equalityExpression = equalityExpression;
      _intoIdentifier = intoIdentifier;
    }

    // TODO: Check whether this property is really necessary.
    public IClause PreviousClause
    {
      get { return _previousClause; }
    }

    public FromClauseBase FromClause
    {
      get { return _fromClause; }
    }

    public Type ItemType
    {
      get { return _itemType; }
      set { _itemType = ArgumentUtility.CheckNotNull("value",value); }
    }

    public string ItemName
    {
      get { return _itemName; }
      set { _itemName = ArgumentUtility.CheckNotNullOrEmpty("value",value); }
    }

    [DebuggerDisplay ("{Remotion.Data.Linq.StringBuilding.FormattingExpressionTreeVisitor.Format (InExpression),nq}")]
    public Expression InExpression
    {
      get { return _inExpression; }
      set { _inExpression = ArgumentUtility.CheckNotNull ("value", value); }
    }

    [DebuggerDisplay ("{Remotion.Data.Linq.StringBuilding.FormattingExpressionTreeVisitor.Format (OnExpression),nq}")]
    public Expression OnExpression
    {
      get { return _onExpression; }
      set { _onExpression = ArgumentUtility.CheckNotNull ("value", value); }
    }

    [DebuggerDisplay ("{Remotion.Data.Linq.StringBuilding.FormattingExpressionTreeVisitor.Format (EqualityExpression),nq}")]
    public Expression EqualityExpression
    {
      get { return _equalityExpression; }
      set { _equalityExpression = ArgumentUtility.CheckNotNull ("value", value); }
    }

    public ParameterExpression IntoIdentifier
    {
      get { return _intoIdentifier; }
      set { _intoIdentifier = ArgumentUtility.CheckNotNull ("value", value); }
    }

    public virtual void Accept (IQueryVisitor visitor)
    {
      ArgumentUtility.CheckNotNull ("visitor", visitor);
      visitor.VisitJoinClause (this);
    }

    public JoinClause Clone (CloneContext cloneContext)
    {
      ArgumentUtility.CheckNotNull ("cloneContext", cloneContext);

      var newPreviousClause = cloneContext.ClonedClauseMapping.GetClause<IClause> (PreviousClause);
      var newFromClause = cloneContext.ClonedClauseMapping.GetClause<FromClauseBase> (FromClause);
      var newInExpression = CloneExpressionTreeVisitor.ReplaceClauseReferences (InExpression, cloneContext);
      var newOnExpression = CloneExpressionTreeVisitor.ReplaceClauseReferences (OnExpression, cloneContext);
      var newEqualityExpression = CloneExpressionTreeVisitor.ReplaceClauseReferences (EqualityExpression, cloneContext);
      var result = new JoinClause (newPreviousClause, newFromClause, ItemName, ItemType, newInExpression, newOnExpression, newEqualityExpression, IntoIdentifier);
      cloneContext.ClonedClauseMapping.AddMapping (this, result);
      return result;
    }
  }
}
