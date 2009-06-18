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
using Remotion.Data.Linq.DataObjectModel;
using Remotion.Data.Linq.Parsing.FieldResolving;
using Remotion.Utilities;

namespace Remotion.Data.Linq.Clauses
{
  /// <summary>
  /// Represents the let part of a linq query.
  /// example: let a = b
  /// </summary>
  public class LetClause : IBodyClause, IResolveableClause
  {
    private readonly ParameterExpression _identifier;
    private readonly Expression _expression;

    /// <summary>
    /// Initialize a new instance of <see cref="LetClause"/>
    /// </summary>
    /// <param name="previousClause">The previous clause of type <see cref="IClause"/> in the <see cref="QueryModel"/>.</param>
    /// <param name="identifier">The identifier of a let expression.</param>
    /// <param name="expression">The expression in a let expression.</param>
    /// <param name="projectionExpression">The projection within the let part of the linq query.</param>
    public LetClause (IClause previousClause, ParameterExpression identifier, Expression expression, 
      LambdaExpression projectionExpression)
    {
      ArgumentUtility.CheckNotNull ("previousClause", previousClause);
      ArgumentUtility.CheckNotNull ("identifier", identifier);
      ArgumentUtility.CheckNotNull ("expression", expression);
      ArgumentUtility.CheckNotNull ("projectionExpression", projectionExpression);
      
      
      _identifier = identifier;
      _expression = expression;
      PreviousClause = previousClause;
      ProjectionExpression = projectionExpression;
    }

    /// <summary>
    /// The previous clause of type <see cref="IClause"/> in the <see cref="QueryModel"/>.
    /// </summary>
    public IClause PreviousClause { get; private set; }

    /// <summary>
    /// The projection within the let part of the linq query.
    /// </summary>
    public LambdaExpression ProjectionExpression { get; private set; }
    
    /// <summary>
    /// The <see cref="QueryModel"/> of the <see cref="LetClause"/>
    /// </summary>
    public QueryModel QueryModel { get; private set; }

    /// <summary>
    /// The expression in a let expression.
    /// </summary>
    // TODO 1158: Replace with IEvaluation
    public Expression Expression
    {
      get { return _expression; }
    }

    /// <summary>
    /// The identifier of the let expression.
    /// </summary>
    public ParameterExpression Identifier
    {
      get { return _identifier; }
    }

    public FieldDescriptor ResolveField (ClauseFieldResolver resolver, Expression fieldAccessExpression, JoinedTableContext joinedTableContext)
    {
      ArgumentUtility.CheckNotNull ("resolver", resolver);
      ArgumentUtility.CheckNotNull ("fieldAccessExpression", fieldAccessExpression);
      ArgumentUtility.CheckNotNull ("joinedTableContext", joinedTableContext);

      return resolver.ResolveField (this, fieldAccessExpression, joinedTableContext);
    }

    public virtual LetColumnSource GetColumnSource (IDatabaseInfo databaseInfo)
    { 
      ArgumentUtility.CheckNotNull ("databaseInfo", databaseInfo);

      // TODO 637: IsTable should also be true if the let clause constructs an object, eg: let x = new {o.ID, o.OrderNumber}
      return new LetColumnSource (Identifier.Name, databaseInfo.IsTableType (Identifier.Type));
    }

    IColumnSource IResolveableClause.GetColumnSource (IDatabaseInfo databaseInfo)
    {
      return GetColumnSource (databaseInfo);
    }

    public virtual void Accept (IQueryVisitor visitor)
    {
      ArgumentUtility.CheckNotNull ("visitor", visitor);
      visitor.VisitLetClause (this);
    }

    public void SetQueryModel (QueryModel model)
    {
      ArgumentUtility.CheckNotNull ("model", model);
      if (QueryModel != null)
        throw new InvalidOperationException("QueryModel is already set");
      
      QueryModel = model;
    }

    public LetClause Clone (CloneContext cloneContext)
    {
      ArgumentUtility.CheckNotNull ("cloneContext", cloneContext);

      var newPreviousClause = cloneContext.ClonedClauseMapping.GetClause<IClause> (PreviousClause);
      var result = new LetClause (newPreviousClause, Identifier, Expression, ProjectionExpression);
      cloneContext.ClonedClauseMapping.AddMapping (this, result);
      return result;
    }

    IBodyClause IBodyClause.Clone (CloneContext cloneContext)
    {
      return Clone (cloneContext);
    }
  }
}
