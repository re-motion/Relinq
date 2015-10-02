// Copyright (c) rubicon IT GmbH, www.rubicon.eu
//
// See the NOTICE file distributed with this work for additional information
// regarding copyright ownership.  rubicon licenses this file to you under 
// the Apache License, Version 2.0 (the "License"); you may not use this 
// file except in compliance with the License.  You may obtain a copy of the 
// License at
//
//   http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software 
// distributed under the License is distributed on an "AS IS" BASIS, WITHOUT 
// WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.  See the 
// License for the specific language governing permissions and limitations
// under the License.
// 

using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using Remotion.Linq.Parsing;
using Remotion.Linq.Utilities;
using Remotion.Utilities;

namespace Remotion.Linq.Clauses.ExpressionVisitors
{
  /// <summary>
  /// Constructs a <see cref="LambdaExpression"/> that is able to extract a specific simple expression from a complex <see cref="NewExpression"/>
  /// or <see cref="MemberInitExpression"/>.
  /// </summary>
  /// <example>
  /// <para>
  /// For example, consider the task of determining the value of a specific query source [s] from an input value corresponding to a complex 
  /// expression. This <see cref="AccessorFindingExpressionVisitor"/> will return a <see cref="LambdaExpression"/> able to perform this task.
  /// </para>
  /// <para>
  /// <list type="bullet">
  /// <item>If the complex expression is [s], it will simply return input => input.</item>
  /// <item>If the complex expression is new { a = [s], b = "..." }, it will return input => input.a.</item>
  /// <item>If the complex expression is new { a = new { b = [s], c = "..." }, d = "..." }, it will return input => input.a.b.</item>
  /// </list>
  /// </para>
  /// </example>
  public sealed class AccessorFindingExpressionVisitor : RelinqExpressionVisitor
  {
    /// <summary>
    /// Constructs a <see cref="LambdaExpression"/> that is able to extract a specific simple <paramref name="searchedExpression"/> from a 
    /// complex <paramref name="fullExpression"/>.
    /// </summary>
    /// <param name="searchedExpression">The expression an accessor to which should be created.</param>
    /// <param name="fullExpression">The full expression containing the <paramref name="searchedExpression"/>.</param>
    /// <param name="inputParameter">The input parameter to be used by the resulting lambda. Its type must match the type of <paramref name="fullExpression"/>.</param>
    /// <remarks>The <see cref="AccessorFindingExpressionVisitor"/> compares the <paramref name="searchedExpression"/> via reference equality,
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
        throw new ArgumentException (
            string.Format ("The inputParameter's type '{0}' must match the fullExpression's type '{1}'.", inputParameter.Type, fullExpression.Type),
            "inputParameter");
      }

      var visitor = new AccessorFindingExpressionVisitor (searchedExpression, inputParameter);
      visitor.Visit (fullExpression);

      if (visitor.AccessorPath != null)
        return visitor.AccessorPath;
      else
      {
        var message = string.Format (
            "The given expression '{0}' does not contain the searched expression '{1}' in a nested NewExpression with member assignments or a "
                + "MemberBindingExpression.",
            fullExpression.BuildString(),
            searchedExpression.BuildString());
        throw new ArgumentException (message, "fullExpression");
      }
    }

    private readonly Expression _searchedExpression;
    private readonly ParameterExpression _inputParameter;

    private readonly Stack<Expression> _accessorPathStack = new Stack<Expression> ();

    private AccessorFindingExpressionVisitor (Expression searchedExpression, ParameterExpression inputParameter)
    {
      ArgumentUtility.CheckNotNull ("searchedExpression", searchedExpression);
      ArgumentUtility.CheckNotNull ("inputParameter", inputParameter);

      _searchedExpression = searchedExpression;
      _inputParameter = inputParameter;
      _accessorPathStack.Push (_inputParameter);
    }

    private LambdaExpression AccessorPath { get; set; }

    public override Expression Visit (Expression expression)
    {
      if (Equals (expression, _searchedExpression))
      {
        Expression path = _accessorPathStack.Peek ();
        AccessorPath = Expression.Lambda (path, _inputParameter);

        return expression;
      }
      else if (expression is NewExpression || expression is MemberInitExpression || expression is UnaryExpression)
      {
        return base.Visit (expression);
      }
      else
      {
        return expression;
      }
    }

    protected override Expression VisitNew (NewExpression expression)
    {
      ArgumentUtility.CheckNotNull ("expression", expression);

      if (expression.Members != null && expression.Members.Count > 0)
      {
        for (int i = 0; i < expression.Members.Count; i++)
          CheckAndVisitMemberAssignment (expression.Members[i], expression.Arguments[i]);
      }

      return expression;
    }

    protected override Expression VisitUnary (UnaryExpression expression)
    {
      ArgumentUtility.CheckNotNull ("expression", expression);

      if (expression.NodeType == ExpressionType.Convert || expression.NodeType == ExpressionType.ConvertChecked)
      {
        var reverseConvert = Expression.Convert (_accessorPathStack.Peek (), expression.Operand.Type);
        _accessorPathStack.Push (reverseConvert);
        base.VisitUnary (expression);
        _accessorPathStack.Pop ();
      }

      return expression;
    }

    protected override MemberBinding VisitMemberBinding (MemberBinding memberBinding)
    {
      ArgumentUtility.CheckNotNull ("memberBinding", memberBinding);

      if (memberBinding is MemberAssignment)
        return base.VisitMemberBinding (memberBinding);
      else
        return memberBinding;
    }

    protected override MemberAssignment VisitMemberAssignment (MemberAssignment memberAssigment)
    {
      ArgumentUtility.CheckNotNull ("memberAssigment", memberAssigment);

      CheckAndVisitMemberAssignment (memberAssigment.Member, memberAssigment.Expression);
      return memberAssigment;
    }

    private void CheckAndVisitMemberAssignment (MemberInfo member, Expression expression)
    {
      var memberAccess = GetMemberAccessExpression (_accessorPathStack.Peek (), member);
      _accessorPathStack.Push (memberAccess);
      Visit (expression);
      _accessorPathStack.Pop ();
    }

    private Expression GetMemberAccessExpression (Expression input, MemberInfo member)
    {
      var normalizedInput = EnsureMemberIsAccessibleFromInput (input, member);

      var methodInfo = member as MethodInfo;
      if (methodInfo != null)
        return Expression.Call (normalizedInput, methodInfo);
      else
        return Expression.MakeMemberAccess (normalizedInput, member);
    }

    private Expression EnsureMemberIsAccessibleFromInput (Expression input, MemberInfo member)
    {
      var memberDeclaringType = member.DeclaringType.GetTypeInfo();
      var inputType = input.Type.GetTypeInfo();

      var isMemberDeclaredOnInputType = memberDeclaringType.IsAssignableFrom (inputType);
      if (isMemberDeclaredOnInputType)
        return input;

      Assertion.IsTrue (
          inputType.IsAssignableFrom (memberDeclaringType),
          "Input expression of type '{0}' cannot be converted to declaring type '{1}' of member '{2}'.",
          input.Type.FullName,
          member.DeclaringType.FullName,
          member.Name);

      return Expression.Convert (input, member.DeclaringType);
    }
  }
}
