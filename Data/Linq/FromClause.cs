/* Copyright (C) 2005 - 2008 rubicon informationstechnologie gmbh
 *
 * This program is free software: you can redistribute it and/or modify it under 
 * the terms of the re:motion license agreement in license.txt. If you did not 
 * receive it, please visit http://www.re-motion.org/licensing.
 * 
 * Unless otherwise provided, this software is distributed on an "AS IS" basis, 
 * WITHOUT WARRANTY OF ANY KIND, either express or implied. 
 */

using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Rubicon.Utilities;

namespace Rubicon.Data.DomainObjects.Linq
{
  public class FromClause
  {
    private readonly ParameterExpression _identifier;
    private readonly Expression _expression;
    private readonly List<JoinClause> _joinClauses = new List<JoinClause>();

    public FromClause (ParameterExpression id, Expression expression)
    {
      ArgumentUtility.CheckNotNull ("id", id);
      ArgumentUtility.CheckNotNull ("expression", expression);

      _identifier = id;
      _expression = expression;
    }

    public ParameterExpression Identifier
    {
      get { return _identifier; }
    }

    public Expression Expression
    {
      get { return _expression; }
    }

    public IEnumerable<JoinClause> JoinClauses
    {
      get { return _joinClauses; }
    }

    public int JoinClauseCount
    {
      get { return _joinClauses.Count; }
    }

    public void Add (JoinClause joinClause)
    {
      ArgumentUtility.CheckNotNull ("joinClause", joinClause);
      _joinClauses.Add (joinClause);
    }
  }
}
