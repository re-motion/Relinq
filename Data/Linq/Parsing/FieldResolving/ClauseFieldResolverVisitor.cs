/* Copyright (C) 2005 - 2008 rubicon informationstechnologie gmbh
 *
 * This program is free software: you can redistribute it and/or modify it under 
 * the terms of the re:motion license agreement in license.txt. If you did not 
 * receive it, please visit http://www.re-motion.org/licensing.
 * 
 * Unless otherwise provided, this software is distributed on an "AS IS" basis, 
 * WITHOUT WARRANTY OF ANY KIND, either express or implied. 
 */

using System.Collections.Generic;
using System.Linq.Expressions;
using Remotion.Data.Linq.Parsing.FieldResolving;
using Remotion.Data.Linq.Visitor;
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
      if (isFirstMember)
      {
        // for related key access, we leave _accessedMember null, we'll take the next one
        // eg. sd.Student.ID => we'll take sd.Student, not ID as the accessed member
        if (!_optimizeRelatedKeyAccess || !IsOptimizableRelatedKeyAccess(expression))
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
