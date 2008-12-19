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
