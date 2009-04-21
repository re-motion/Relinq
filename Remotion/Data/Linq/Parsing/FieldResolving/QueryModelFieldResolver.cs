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
using System.Linq.Expressions;
using Remotion.Data.Linq.DataObjectModel;
using Remotion.Utilities;

namespace Remotion.Data.Linq.Parsing.FieldResolving
{
  public class QueryModelFieldResolver
  {
    private readonly QueryModel _queryModel;

    public QueryModelFieldResolver (QueryModel queryModel)
    {
      ArgumentUtility.CheckNotNull ("queryExpression", queryModel);
      _queryModel = queryModel;
    }

    public FieldDescriptor ResolveField (ClauseFieldResolver resolver, Expression fieldAccessExpression, JoinedTableContext joinedTableContext)
    {
      ArgumentUtility.CheckNotNull ("resolver", resolver);
      ArgumentUtility.CheckNotNull ("fieldAccessExpression", fieldAccessExpression);
      ArgumentUtility.CheckNotNull ("joinedTableContext", joinedTableContext);

      QueryModelFieldResolverVisitor visitor = new QueryModelFieldResolverVisitor (_queryModel);
      QueryModelFieldResolverVisitor.Result visitorResult = visitor.ParseAndReduce (fieldAccessExpression);

      if (visitorResult != null)
        return visitorResult.ResolveableClause.ResolveField (resolver, visitorResult.ReducedExpression, fieldAccessExpression, joinedTableContext);
      else if (_queryModel.ParentQuery != null)
        return _queryModel.ParentQuery.ResolveField (resolver, fieldAccessExpression, joinedTableContext);
      else
      {
        string message = string.Format ("The field access expression '{0}' does not contain a from clause identifier.", fieldAccessExpression);
        throw new FieldAccessResolveException (message);
      }

    }
  }
}
