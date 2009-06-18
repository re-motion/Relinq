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
using System.Linq.Expressions;
using Remotion.Utilities;

namespace Remotion.Data.Linq.Clauses
{
  /// <summary>
  /// Extends <see cref="FromClauseBase"/>. <see cref="AdditionalFromClause"/> is used for from clauses which is no <see cref="MainFromClause"/>.
  /// example:from a in queryable1 from b in queryable (the additional <see cref="AdditionalFromClause"/> is the second from)
  /// </summary>
  public class AdditionalFromClause : FromClauseBase, IBodyClause
  {
    /// <summary>
    /// Initialize a new instance of <see cref="AdditionalFromClause"/>
    /// </summary>
    /// <param name="previousClause">The previous <see cref="IClause"/> of this from clause.</param>
    /// <param name="identifier">The identifierer of the from expression.</param>
    /// <param name="fromExpression">The expression of the from expression.</param>
    /// <param name="resultSelector">The projection of identifier to from expression.</param>
    public AdditionalFromClause (IClause previousClause, ParameterExpression identifier, LambdaExpression fromExpression,
        LambdaExpression resultSelector)
        : base (previousClause,identifier)
    {
      ArgumentUtility.CheckNotNull ("previousClause", previousClause);
      ArgumentUtility.CheckNotNull ("identifier", identifier);
      ArgumentUtility.CheckNotNull ("fromExpression", fromExpression);
      ArgumentUtility.CheckNotNull ("resultSelector", resultSelector);

      FromExpression = fromExpression;
      ResultSelector = resultSelector;
    }

    /// <summary>
    /// The expression of a from expression.
    /// </summary>
    // TODO 1158: Replace by QuerySourceType
    public LambdaExpression FromExpression { get; private set; }

    /// <summary>
    /// The projection of a from expression.
    /// </summary>
    // TODO 1158: Remove
    public LambdaExpression ResultSelector { get; private set; }

    /// <summary>
    /// The appropriate <see cref="QueryModel"/> of the <see cref="AdditionalFromClause"/>.
    /// </summary>
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

    public virtual AdditionalFromClause Clone (CloneContext cloneContext)
    {
      ArgumentUtility.CheckNotNull ("cloneContext", cloneContext);

      var newPreviousClause = cloneContext.ClonedClauseMapping.GetClause<IClause> (PreviousClause);
      var result = new AdditionalFromClause (newPreviousClause, Identifier, FromExpression, ResultSelector);
      cloneContext.ClonedClauseMapping.AddMapping (this, result);
      result.AddClonedJoinClauses (JoinClauses, cloneContext);
      return result;
    }

    IBodyClause IBodyClause.Clone (CloneContext cloneContext)
    {
      return Clone (cloneContext);
    }
  }
}
