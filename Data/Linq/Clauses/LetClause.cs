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
using Remotion.Data.Linq.Clauses;
using Remotion.Data.Linq.DataObjectModel;
using Remotion.Data.Linq.Parsing.FieldResolving;
using Remotion.Utilities;

namespace Remotion.Data.Linq.Clauses
{
  public class LetClause : IBodyClause, IResolveableClause
  {
    private readonly ParameterExpression _identifier;
    private readonly Expression _expression;

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

    public IClause PreviousClause { get; private set; }
    public LambdaExpression ProjectionExpression { get; private set; }
    public QueryModel QueryModel { get; private set; }

    public Expression Expression
    {
      get { return _expression; }
    }

    public ParameterExpression Identifier
    {
      get { return _identifier; }
    }

    public FieldDescriptor ResolveField (ClauseFieldResolver resolver, Expression partialFieldExpression, Expression fullFieldExpression, JoinedTableContext joinedTableContext)
    {
      ArgumentUtility.CheckNotNull ("resolver", resolver);
      ArgumentUtility.CheckNotNull ("partialFieldExpression", partialFieldExpression);
      ArgumentUtility.CheckNotNull ("fullFieldExpression", fullFieldExpression);
      ArgumentUtility.CheckNotNull ("joinedTableContext", joinedTableContext);

      return resolver.ResolveField (GetColumnSource (resolver.DatabaseInfo), Identifier, partialFieldExpression, fullFieldExpression, joinedTableContext);
    }

    public virtual LetColumnSource GetColumnSource (IDatabaseInfo databaseInfo)
    { 
      // TODO: IsTable should also be true if the let clause constructs an object, eg: let x = new {o.ID, o.OrderNumber}
      return new LetColumnSource (Identifier.Name, databaseInfo.IsTableType (Identifier.Type));
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
  }
}
