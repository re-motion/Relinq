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
using System.Collections.Generic;
using System.Linq.Expressions;
using Remotion.Data.Linq.Parsing.FieldResolving;
using System.Reflection;
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
      public Result (MemberInfo accessedMember, MemberInfo[] joinMembers, ParameterExpression parameter)
          : this ()
      {
        ArgumentUtility.CheckNotNull ("parameter", parameter);

        AccessedMember = accessedMember;
        JoinMembers = joinMembers;
        Parameter = parameter;
      }

      public MemberInfo AccessedMember { get; private set; }
      public MemberInfo[] JoinMembers { get; private set; }
      public ParameterExpression Parameter { get; private set; }
    }

    private readonly IDatabaseInfo _databaseInfo;

    private ParameterExpression _parameterExpression;
    private MemberInfo _accessedMember;
    private List<MemberInfo> _joinMembers;
    private Expression _expressionTreeRoot;
    private bool _optimizeRelatedKeyAccess;

    public ClauseFieldResolverVisitor (IDatabaseInfo databaseInfo)
    {
      ArgumentUtility.CheckNotNull ("databaseInfo", databaseInfo);
      _databaseInfo = databaseInfo;
    }

    public Result ParseFieldAccess (Expression fieldAccessExpression, Expression expressionTreeRoot, bool optimizeRelatedKeyAccess)
    {
      ArgumentUtility.CheckNotNull ("fieldAccessExpression", fieldAccessExpression);
      ArgumentUtility.CheckNotNull ("expressionTreeRoot", expressionTreeRoot);

      _parameterExpression = null;
      _accessedMember = null;
      _joinMembers = new List<MemberInfo> ();
      _expressionTreeRoot = expressionTreeRoot;
      _optimizeRelatedKeyAccess = optimizeRelatedKeyAccess;

      VisitExpression (fieldAccessExpression);
      return new Result (_accessedMember, _joinMembers.ToArray(), _parameterExpression);
    }

    protected override Expression VisitExpression (Expression expression)
    {
      ArgumentUtility.CheckNotNull ("expression", expression);

      switch (expression.NodeType)
      {
        case ExpressionType.Parameter:
        case ExpressionType.MemberAccess:
          return base.VisitExpression (expression);
        default:
          string message = string.Format ("Only MemberExpressions and ParameterExpressions can be resolved, found '{0}' in expression '{1}'.",
              expression, _expressionTreeRoot);
          throw new FieldAccessResolveException (message);
      }
    }

    protected override Expression VisitParameterExpression (ParameterExpression expression)
    {
      ArgumentUtility.CheckNotNull ("expression", expression);
      _parameterExpression = expression;
      return base.VisitParameterExpression (expression);
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
      // expression.Expression is MemberExpression &&  ?
      return expression.Member.Equals (primaryKeyMember);
    }
  }
}
