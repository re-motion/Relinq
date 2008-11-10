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