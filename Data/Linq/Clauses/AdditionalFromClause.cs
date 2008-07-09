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

namespace Remotion.Data.Linq.Clauses
{
  public class AdditionalFromClause : FromClauseBase,IBodyClause
  {
    public AdditionalFromClause (IClause previousClause, ParameterExpression identifier, LambdaExpression fromExpression,
        LambdaExpression projectionExpression)
        : base (previousClause,identifier)
    {
      ArgumentUtility.CheckNotNull ("previousClause", previousClause);
      ArgumentUtility.CheckNotNull ("identifier", identifier);
      ArgumentUtility.CheckNotNull ("fromExpression", fromExpression);
      ArgumentUtility.CheckNotNull ("projectionExpression", projectionExpression);

      FromExpression = fromExpression;
      ProjectionExpression = projectionExpression;
    }

    public LambdaExpression FromExpression { get; private set; }
    public LambdaExpression ProjectionExpression { get; private set; }
    public QueryModel QueryModel { get; private set; }

    public override void Accept (IQueryVisitor visitor)
    {
      ArgumentUtility.CheckNotNull ("visitor", visitor);
      visitor.VisitAdditionalFromClause (this);
    }

    public override Type GetQuerySourceType ()
    {
      return FromExpression.Body.Type;
    }
    

    public void SetQueryModel (QueryModel model)
    {
      ArgumentUtility.CheckNotNull ("model", model);
      if (QueryModel != null)
        throw new InvalidOperationException ("QueryModel is already set");
      QueryModel = model;
    }
  }
}
