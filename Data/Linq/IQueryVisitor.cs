/* Copyright (C) 2005 - 2008 rubicon informationstechnologie gmbh
 *
 * This program is free software: you can redistribute it and/or modify it under 
 * the terms of the re:motion license agreement in license.txt. If you did not 
 * receive it, please visit http://www.re-motion.org/licensing.
 * 
 * Unless otherwise provided, this software is distributed on an "AS IS" basis, 
 * WITHOUT WARRANTY OF ANY KIND, either express or implied. 
 */

using Remotion.Data.Linq.Clauses;

namespace Remotion.Data.Linq
{
  public interface IQueryVisitor
  {
    void VisitQueryModel (QueryModel queryModel);
    void VisitMainFromClause (MainFromClause fromClause);
    void VisitAdditionalFromClause (AdditionalFromClause fromClause);
    void VisitMemberFromClause (MemberFromClause fromClause);
    void VisitSubQueryFromClause (SubQueryFromClause clause);
    void VisitJoinClause (JoinClause joinClause);
    void VisitLetClause (LetClause letClause);
    void VisitWhereClause (WhereClause whereClause);
    void VisitOrderByClause (OrderByClause orderByClause);
    void VisitOrderingClause (OrderingClause orderingClause);
    void VisitSelectClause (SelectClause selectClause);
    void VisitGroupClause (GroupClause groupClause);
  }
}
