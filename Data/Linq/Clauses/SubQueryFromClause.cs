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
using System.Linq.Expressions;
using Remotion.Utilities;
using Remotion.Data.Linq.DataObjectModel;

namespace Remotion.Data.Linq.Clauses
{
  public class SubQueryFromClause : FromClauseBase, IBodyClause
  {
    private readonly SubQuery _fromSource;

    public SubQueryFromClause (IClause previousClause, ParameterExpression identifier, QueryModel subQuery, LambdaExpression projectionExpression)
        : base (previousClause, identifier)
    {
      ArgumentUtility.CheckNotNull ("previousClause", previousClause);
      ArgumentUtility.CheckNotNull ("identifier", identifier);
      ArgumentUtility.CheckNotNull ("subQuery", subQuery);
      ArgumentUtility.CheckNotNull ("projectionExpression", projectionExpression);

      SubQueryModel = subQuery;
      ProjectionExpression = projectionExpression;

      _fromSource = new SubQuery (SubQueryModel, Identifier.Name);
    }

    public QueryModel SubQueryModel { get; private set; }
    public LambdaExpression ProjectionExpression { get; private set; }

    public override void Accept (IQueryVisitor visitor)
    {
      ArgumentUtility.CheckNotNull ("visitor", visitor);
      visitor.VisitSubQueryFromClause (this);
    }

    public override Type GetQuerySourceType ()
    {
      return null;
    }

    public override IColumnSource GetFromSource (IDatabaseInfo databaseInfo)
    {
      return _fromSource;
    }

    public QueryModel QueryModel { get; private set; }

    public void SetQueryModel (QueryModel model)
    {
      ArgumentUtility.CheckNotNull ("model", model);
      if (QueryModel != null)
        throw new InvalidOperationException ("QueryModel is already set");
      QueryModel = model;

    }
  }
}
