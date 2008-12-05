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
using Remotion.Data.Linq.Parsing;
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

      _fromSource = new SubQuery (SubQueryModel, ParseMode.SubQueryInFrom, Identifier.Name);
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
