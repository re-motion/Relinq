// This file is part of the re-linq project (relinq.codeplex.com)
// Copyright (C) 2005-2009 rubicon informationstechnologie gmbh, www.rubicon.eu
// 
// re-linq is free software; you can redistribute it and/or modify it under 
// the terms of the GNU Lesser General Public License as published by the 
// Free Software Foundation; either version 2.1 of the License, 
// or (at your option) any later version.
// 
// re-linq is distributed in the hope that it will be useful, 
// but WITHOUT ANY WARRANTY; without even the implied warranty of 
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the 
// GNU Lesser General Public License for more details.
// 
// You should have received a copy of the GNU Lesser General Public License
// along with re-linq; if not, see http://www.gnu.org/licenses.
// 
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using Remotion.Linq.Parsing;
using Remotion.Linq.Utilities;

namespace Remotion.Linq.Clauses.ExpressionTreeVisitors
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

    private readonly Stack<Expression> _accessorPathStack = new Stack<Expression> ();

    private AccessorFindingExpressionTreeVisitor (Expression searchedExpression, ParameterExpression inputParameter)
    {
      ArgumentUtility.CheckNotNull ("searchedExpression", searchedExpression);
      ArgumentUtility.CheckNotNull ("inputParameter", inputParameter);

      _searchedExpression = searchedExpression;
      _inputParameter = inputParameter;
      _accessorPathStack.Push (_inputParameter);
    }

    public LambdaExpression AccessorPath { get; private set; }

    public override Expression VisitExpression (Expression expression)
    {
      if (Equals (expression, _searchedExpression))
      {
        Expression path = _accessorPathStack.Peek ();
        AccessorPath = Expression.Lambda (path, _inputParameter);

        return expression;
      }
      else if (expression is NewExpression || expression is MemberInitExpression || expression is UnaryExpression)
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

    protected override Expression VisitUnaryExpression (UnaryExpression expression)
    {
      if (expression.NodeType == ExpressionType.Convert || expression.NodeType == ExpressionType.ConvertChecked)
      {
        var reverseConvert = Expression.Convert (_accessorPathStack.Peek (), expression.Operand.Type);
        _accessorPathStack.Push (reverseConvert);
        base.VisitUnaryExpression (expression);
        _accessorPathStack.Pop ();
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
      var memberAccess = GetMemberAccessExpression (_accessorPathStack.Peek (), member);
      _accessorPathStack.Push (memberAccess);
      VisitExpression (expression);
      _accessorPathStack.Pop ();
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
