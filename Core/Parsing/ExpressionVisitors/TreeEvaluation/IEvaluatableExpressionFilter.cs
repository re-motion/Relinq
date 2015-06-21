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
using System.Linq.Expressions;
using JetBrains.Annotations;

namespace Remotion.Linq.Parsing.ExpressionVisitors.TreeEvaluation
{
  /// <summary>
  /// The <see cref="IEvaluatableExpressionFilter"/> interface defines an extension point for disabling partial evaluation on specific <see cref="Expression"/> nodes.
  /// </summary>
  /// <remarks>
  /// <para>
  /// Implement the individual evaluation methods and return <see langword="false" /> to mark a specfic <see cref="Expression"/> node as not partially 
  /// evaluatable. Note that the partial evaluation infrastructure will take care of visiting an <see cref="Expression"/> node's children, 
  /// so the determination can usually be constrained to the attributes of the <see cref="Expression"/> node itself.
  /// </para><para>
  /// Use the <see cref="EvaluatableExpressionFilterBase"/> type as a base class for filter implementations that only require testing a few 
  /// <see cref="Expression"/> node types, e.g. to disable partial evaluation for individual method calls.
  /// </para>
  /// </remarks>
  /// <seealso cref="EvaluatableExpressionFilterBase"/>
  /// <threadsafety static="true" instance="true" />
  public interface IEvaluatableExpressionFilter
  {
    bool IsEvaluatableBinary ([NotNull] BinaryExpression node);
    bool IsEvaluatableConditional ([NotNull] ConditionalExpression node);
    bool IsEvaluatableConstant ([NotNull] ConstantExpression node);
    bool IsEvaluatableElementInit ([NotNull] ElementInit node);
    bool IsEvaluatableInvocation ([NotNull] InvocationExpression node);
    bool IsEvaluatableLambda ([NotNull] LambdaExpression node);
    bool IsEvaluatableListInit ([NotNull] ListInitExpression node);
    bool IsEvaluatableMember ([NotNull] MemberExpression node);
    bool IsEvaluatableMemberAssignment ([NotNull] MemberAssignment node);
    bool IsEvaluatableMemberInit ([NotNull] MemberInitExpression node);
    bool IsEvaluatableMemberListBinding ([NotNull] MemberListBinding node);
    bool IsEvaluatableMemberMemberBinding ([NotNull] MemberMemberBinding node);
    bool IsEvaluatableMethodCall ([NotNull] MethodCallExpression node);
    bool IsEvaluatableNew ([NotNull] NewExpression node);
    bool IsEvaluatableNewArray ([NotNull] NewArrayExpression node);
    bool IsEvaluatableTypeBinary ([NotNull] TypeBinaryExpression node);
    bool IsEvaluatableUnary ([NotNull] UnaryExpression node);
#if !NET_3_5
    bool IsEvaluatableBlock ([NotNull] BlockExpression node);
    bool IsEvaluatableCatchBlock ([NotNull] CatchBlock node);
    bool IsEvaluatableDebugInfo ([NotNull] DebugInfoExpression node);
    bool IsEvaluatableDefault ([NotNull] DefaultExpression node);
    bool IsEvaluatableGoto ([NotNull] GotoExpression node);
    bool IsEvaluatableIndex ([NotNull] IndexExpression node);
    bool IsEvaluatableLabel ([NotNull] LabelExpression node);
    bool IsEvaluatableLabelTarget ([NotNull] LabelTarget node);
    bool IsEvaluatableLoop ([NotNull] LoopExpression node);
    bool IsEvaluatableSwitch ([NotNull] SwitchExpression node);
    bool IsEvaluatableSwitchCase ([NotNull] SwitchCase node);
    bool IsEvaluatableTry ([NotNull] TryExpression node);
#endif
  }
}