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
using Remotion.Data.Linq.DataObjectModel;
using Remotion.Utilities;

namespace Remotion.Data.Linq.Clauses
{
  public class MemberFromClause : AdditionalFromClause
  {
    private readonly MemberExpression _memberExpression;

    public MemberFromClause (IClause previousClause, ParameterExpression identifier, LambdaExpression fromExpression, LambdaExpression projectionExpression)
        : base(previousClause, identifier, fromExpression, projectionExpression)
    {
      MemberExpression memberExpression = fromExpression.Body as MemberExpression;
      if (memberExpression != null)
        _memberExpression = memberExpression;
      else
        throw new ArgumentException ("From expression must contain a MemberExpression.");
    }


    public MemberExpression MemberExpression
    {
      get { return _memberExpression; }
    }

    public override void Accept (IQueryVisitor visitor)
    {
      ArgumentUtility.CheckNotNull ("visitor", visitor);
      visitor.VisitMemberFromClause (this);
    }

    public override IColumnSource GetFromSource (IDatabaseInfo databaseInfo)
    {
      ArgumentUtility.CheckNotNull ("databaseInfo", databaseInfo);
      var relatedTable = DatabaseInfoUtility.GetRelatedTable (databaseInfo, MemberExpression.Member);
      relatedTable.SetAlias (Identifier.Name);
      return relatedTable;
    }
  }
}
