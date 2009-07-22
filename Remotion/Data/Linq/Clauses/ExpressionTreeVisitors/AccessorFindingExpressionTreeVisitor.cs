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
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Remotion.Data.Linq.Parsing;
using Remotion.Utilities;

namespace Remotion.Data.Linq.Clauses.ExpressionTreeVisitors
{
  /// <summary>
  /// Constructs a <see cref="LambdaExpression"/> that is able to extract a specific simple expression from a complex <see cref="NewExpression"/>
  /// or <see cref="MemberInitExpression"/>.
  /// </summary>
  /// <example>
  /// <para>
  /// For example, consider the task of determining the value of a specific query source [s] from an input value corresponding to a complex 
  /// expression. This <see cref="AccessorFindingExpressionTreeVisitor"/> will return a <see cref="LambdaExpression"/> able to perform this task.
  /// </para>
  /// <para>
  /// <list type="bullet">
  /// <item>If the complex expression is [s], it will simply return input => input.</item>
  /// <item>If the complex expression is new { a = [s], b = "..." }, it will return input => input.a.</item>
  /// <item>If the complex expression is new { a = new { b = [s], c = "..." }, d = "..." }, it will return input => input.a.b.</item>
  /// </list>
  /// </para>
  /// </example>
  public class AccessorFindingExpressionTreeVisitor : ExpressionTreeVisitor
  {
    /// <summary>
    /// Constructs a <see cref="LambdaExpression"/> that is able to extract a specific simple <paramref name="searchedExpression"/> from a 
    /// complex <paramref name="fullExpression"/>.
    /// </summary>
    /// <param name="searchedExpression">The expression an accessor to which should be created.</param>
    /// <param name="fullExpression">The full expression containing the <paramref name="searchedExpression"/>.</param>
    /// <param name="inputParameter">The input parameter to be used by the resulting lambda. Its type must match the type of <paramref name="fullExpression"/>.</param>
    /// <remarks>The <see cref="AccessorFindingExpressionTreeVisitor"/> compares the <paramref name="searchedExpression"/> via reference equality,
    /// which means that exactly the same expression reference must be contained by <paramref name="fullExpression"/> for the visitor to return the
    /// expected result. In addition, the visitor can only provide accessors for expressions nested in <see cref="NewExpression"/> or 
    /// <see cref="MemberInitExpression"/>.</remarks>
    /// <returns>A <see cref="LambdaExpression"/> acting as an accessor for the <paramref name="searchedExpression"/> when an input matching 
    /// <paramref name="fullExpression"/> is given.
    /// </returns>
    public static LambdaExpression FindAccessorLambda (Expression searchedExpression, Expression fullExpression, ParameterExpression inputParameter)
    {
      ArgumentUtility.CheckNotNull ("searchedExpression", searchedExpression);
      ArgumentUtility.CheckNotNull ("fullExpression", fullExpression);
      ArgumentUtility.CheckNotNull ("inputParameter", inputParameter);

      if (inputParameter.Type != fullExpression.Type)
      {
        throw new ArgumentTypeException (
            "The inputParameter's type must match the fullExpression's type.",
            "inputParameter",
            fullExpression.Type,
            inputParameter.Type);
      }

      var visitor = new AccessorFindingExpressionTreeVisitor (searchedExpression, inputParameter);
      visitor.VisitExpression (fullExpression);

      if (visitor.AccessorPath != null)
        return visitor.AccessorPath;
      else
      {
        var message = string.Format (
            "The given expression '{0}' does not contain the searched expression '{1}' in a nested NewExpression with member assignments or a "
                + "MemberBindingExpression.",
            FormattingExpressionTreeVisitor.Format (fullExpression),
            FormattingExpressionTreeVisitor.Format (searchedExpression));
        throw new ArgumentException (message, "fullExpression");
      }
    }

    private readonly Expression _searchedExpression;
    private readonly ParameterExpression _inputParameter;

    private readonly Stack<MemberInfo> _members = new Stack<MemberInfo> ();

    private AccessorFindingExpressionTreeVisitor (Expression searchedExpression, ParameterExpression inputParameter)
    {
      ArgumentUtility.CheckNotNull ("searchedExpression", searchedExpression);
      ArgumentUtility.CheckNotNull ("inputParameter", inputParameter);

      _searchedExpression = searchedExpression;
      _inputParameter = inputParameter;
    }

    public LambdaExpression AccessorPath { get; private set; }

    protected override Expression VisitExpression (Expression expression)
    {
      if (expression == _searchedExpression)
      {
        AccessorPath = MakeAccessorPath ();
        return expression;
      }
      else if (expression is NewExpression || expression is MemberInitExpression)
      {
        return base.VisitExpression (expression);
      }
      else
      {
        return expression;
      }
    }

    protected override Expression VisitNewExpression (NewExpression expression)
    {
      if (expression.Members != null && expression.Members.Count > 0)
      {
        for (int i = 0; i < expression.Members.Count; i++)
          CheckAndVisitMemberAssignment (expression.Members[i], expression.Arguments[i]);
      }

      return expression;
    }

    protected override MemberBinding VisitMemberBinding (MemberBinding memberBinding)
    {
      if (memberBinding is MemberAssignment)
        return base.VisitMemberBinding (memberBinding);
      else
        return memberBinding;
    }

    protected override MemberBinding VisitMemberAssignment (MemberAssignment memberAssigment)
    {
      CheckAndVisitMemberAssignment (memberAssigment.Member, memberAssigment.Expression);
      return memberAssigment;
    }

    private void CheckAndVisitMemberAssignment (MemberInfo member, Expression expression)
    {
      _members.Push (member);
      VisitExpression (expression);
      _members.Pop();
    }

    private LambdaExpression MakeAccessorPath ()
    {
      Expression path = _inputParameter;
      foreach (var member in _members.Reverse())
        path = GetMemberAccessExpression (path, member);

      return Expression.Lambda (path, _inputParameter);
    }

    private Expression GetMemberAccessExpression (Expression input, MemberInfo member)
    {
      var methodInfo = member as MethodInfo;
      if (methodInfo != null)
        return Expression.Call (input, methodInfo);
      else
        return Expression.MakeMemberAccess (input, member);
    }
  }
}