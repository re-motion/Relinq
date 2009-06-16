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
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using Remotion.Data.Linq.Clauses;
using Remotion.Data.Linq.Clauses.Expressions;
using Remotion.Utilities;

namespace Remotion.Data.Linq.Parsing.FieldResolving
{
  /// <summary>
  /// identifies the parameter and members used by an expression of a field access
  /// </summary>
  public class ClauseFieldResolverVisitor : ExpressionTreeVisitor
  {
    public struct Result
    {
      public Result (MemberInfo accessedMember, MemberInfo[] joinMembers)
          : this ()
      {
        AccessedMember = accessedMember;
        JoinMembers = joinMembers;
      }

      public MemberInfo AccessedMember { get; private set; }
      public MemberInfo[] JoinMembers { get; private set; }
    }

    private readonly IDatabaseInfo _databaseInfo;

    private IResolveableClause _resolvedClause;
    private MemberInfo _accessedMember;
    private List<MemberInfo> _joinMembers;
    private Expression _expressionTreeRoot;
    private bool _optimizeRelatedKeyAccess;

    public ClauseFieldResolverVisitor (IDatabaseInfo databaseInfo)
    {
      ArgumentUtility.CheckNotNull ("databaseInfo", databaseInfo);
      _databaseInfo = databaseInfo;
    }

    public Result ParseFieldAccess (IResolveableClause resolvedClause, Expression fieldAccessExpression, Expression expressionTreeRoot, bool optimizeRelatedKeyAccess)
    {
      ArgumentUtility.CheckNotNull ("resolvedClause", resolvedClause);
      ArgumentUtility.CheckNotNull ("fieldAccessExpression", fieldAccessExpression);
      ArgumentUtility.CheckNotNull ("expressionTreeRoot", expressionTreeRoot);

      _resolvedClause = resolvedClause;
      _accessedMember = null;
      _joinMembers = new List<MemberInfo> ();
      _expressionTreeRoot = expressionTreeRoot;
      _optimizeRelatedKeyAccess = optimizeRelatedKeyAccess;

      VisitExpression (fieldAccessExpression);
      return new Result (_accessedMember, _joinMembers.ToArray());
    }

    protected override Expression VisitExpression (Expression expression)
    {
      ArgumentUtility.CheckNotNull ("expression", expression);

      if (expression is ParameterExpression || expression is MemberExpression || expression is QuerySourceReferenceExpression)
        return base.VisitExpression (expression);
      else
      {
        string message = string.Format ("Only MemberExpressions, QuerySourceReferenceExpressions, and ParameterExpressions can be resolved, found "
            + "'{0}' in expression '{1}'.", expression, _expressionTreeRoot);
        throw new FieldAccessResolveException (message);
      }
    }

    // TODO 1217: Store QuerySourceReferenceExpression if such an expression is found
    protected override Expression VisitParameterExpression (ParameterExpression expression)
    {
      ArgumentUtility.CheckNotNull ("expression", expression);

      if (expression.Name != _resolvedClause.Identifier.Name)
      {
        string message = string.Format ("This clause can only resolve field accesses for parameters called '{0}', but a parameter "
                                        + "called '{1}' was given.", _resolvedClause.Identifier.Name, expression.Name);
        throw new FieldAccessResolveException (message);
      }

      if (expression.Type != _resolvedClause.Identifier.Type)
      {
        string message = string.Format ("This clause can only resolve field accesses for parameters of type '{0}', but a parameter "
                                        + "of type '{1}' was given.", _resolvedClause.Identifier.Type, expression.Type);
        throw new FieldAccessResolveException (message);
      }

      return base.VisitParameterExpression (expression);
    }

    protected override Expression VisitQuerySourceReferenceExpression (QuerySourceReferenceExpression expression)
    {
      if (expression.ReferencedClause != _resolvedClause)
      {
        string message = string.Format ("This clause can only resolve field accesses for itself ('{0}'), but a reference to a clause "
                                        + "called '{1}' was given.", _resolvedClause.Identifier.Name, expression.ReferencedClause.Identifier.Name);
        throw new FieldAccessResolveException (message);
      }

      return base.VisitQuerySourceReferenceExpression (expression);
    }

    protected override Expression VisitMemberExpression (MemberExpression expression)
    {
      ArgumentUtility.CheckNotNull ("expression", expression);
      bool isFirstMember = _accessedMember == null;
      if (isFirstMember && (!_optimizeRelatedKeyAccess || !IsOptimizableRelatedKeyAccess(expression)))
      {
        // for non-optimized (or non-optimizable) related key access, we leave _accessedMember null, we'll take the next one
        // eg. sd.Student.ID => we'll take sd.Student, not ID as the accessed member
        _accessedMember = expression.Member;
      }

      Expression result = base.VisitMemberExpression (expression);

      if (!isFirstMember)
        _joinMembers.Add (expression.Member);
      return result;
    }

    private bool IsOptimizableRelatedKeyAccess (MemberExpression expression)
    {
      var primaryKeyMember = _databaseInfo.GetPrimaryKeyMember (expression.Expression.Type);
      return expression.Member.Equals (primaryKeyMember);
    }
  }
}
