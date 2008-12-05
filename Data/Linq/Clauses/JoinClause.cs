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
using System.Linq.Expressions;
using Remotion.Utilities;


namespace Remotion.Data.Linq.Clauses
{
  public class JoinClause : IClause
  {
    private readonly ParameterExpression _identifier;
    private readonly Expression _inExpression;
    private readonly Expression _onExpression;
    private readonly Expression _equalityExpression;
    private readonly ParameterExpression _intoIdentifier;

    public JoinClause (IClause previousClause,ParameterExpression identifier, Expression inExpression, Expression onExpression, Expression equalityExpression)
    {
      ArgumentUtility.CheckNotNull ("identifier", identifier);
      ArgumentUtility.CheckNotNull ("inExpression", inExpression);
      ArgumentUtility.CheckNotNull ("onExpression", onExpression);
      ArgumentUtility.CheckNotNull ("equalityExpression", equalityExpression);
      ArgumentUtility.CheckNotNull ("previousClause", previousClause);

      _identifier = identifier;
      _inExpression = inExpression;
      _onExpression = onExpression;
      _equalityExpression = equalityExpression;
      PreviousClause = previousClause;
    }

    
    public JoinClause (IClause previousClause,ParameterExpression identifier, Expression inExpression, Expression onExpression, 
                       Expression equalityExpression,ParameterExpression intoIdentifier)
        : this (previousClause,identifier, inExpression, onExpression, equalityExpression)
    {
      ArgumentUtility.CheckNotNull ("intoIdentifier", intoIdentifier);

      _intoIdentifier = intoIdentifier;
    }

    public IClause PreviousClause { get; private set; }

    public ParameterExpression Identifier
    {
      get { return _identifier; }
    }

    public Expression InExpression
    {
      get { return _inExpression; }
    }

    public Expression OnExpression
    {
      get { return _onExpression; }
    }

    public Expression EqualityExpression
    {
      get { return _equalityExpression; }
    }

    public ParameterExpression IntoIdentifier
    {
      get { return _intoIdentifier; }
    }

    public virtual void Accept (IQueryVisitor visitor)
    {
      ArgumentUtility.CheckNotNull ("visitor", visitor);
      visitor.VisitJoinClause (this);
    }
  }
}
