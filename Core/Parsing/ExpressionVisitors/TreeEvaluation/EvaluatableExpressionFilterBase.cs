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
using Remotion.Utilities;

namespace Remotion.Linq.Parsing.ExpressionVisitors.TreeEvaluation
{
  /// <summary>
  /// Base class for typical implementations of the <see cref="IEvaluatableExpressionFilter"/>.
  /// </summary>
  /// <seealso cref="IEvaluatableExpressionFilter"/>
  /// <threadsafety static="true" instance="true" />
  public abstract class EvaluatableExpressionFilterBase : IEvaluatableExpressionFilter
  {
    protected EvaluatableExpressionFilterBase ()
    {
    }

    public virtual bool IsEvaluatableBinary (BinaryExpression node)
    {
      ArgumentUtility.CheckNotNull ("node", node);

      return true;
    }

    public virtual bool IsEvaluatableConditional (ConditionalExpression node)
    {
      ArgumentUtility.CheckNotNull ("node", node);

      return true;
    }

    public virtual bool IsEvaluatableConstant (ConstantExpression node)
    {
      ArgumentUtility.CheckNotNull ("node", node);

      return true;
    }

    public virtual bool IsEvaluatableElementInit (ElementInit node)
    {
      ArgumentUtility.CheckNotNull ("node", node);

      return true;
    }

    public virtual bool IsEvaluatableInvocation (InvocationExpression node)
    {
      ArgumentUtility.CheckNotNull ("node", node);

      return true;
    }

    public virtual bool IsEvaluatableLambda (LambdaExpression node)
    {
      ArgumentUtility.CheckNotNull ("node", node);

      return true;
    }

    public virtual bool IsEvaluatableListInit (ListInitExpression node)
    {
      ArgumentUtility.CheckNotNull ("node", node);

      return true;
    }

    public virtual bool IsEvaluatableMember (MemberExpression node)
    {
      ArgumentUtility.CheckNotNull ("node", node);

      return true;
    }

    public virtual bool IsEvaluatableMemberAssignment (MemberAssignment node)
    {
      ArgumentUtility.CheckNotNull ("node", node);

      return true;
    }

    public virtual bool IsEvaluatableMemberInit (MemberInitExpression node)
    {
      ArgumentUtility.CheckNotNull ("node", node);

      return true;
    }

    public virtual bool IsEvaluatableMemberListBinding (MemberListBinding node)
    {
      ArgumentUtility.CheckNotNull ("node", node);

      return true;
    }

    public virtual bool IsEvaluatableMemberMemberBinding (MemberMemberBinding node)
    {
      ArgumentUtility.CheckNotNull ("node", node);

      return true;
    }

    public virtual bool IsEvaluatableMethodCall (MethodCallExpression node)
    {
      ArgumentUtility.CheckNotNull ("node", node);

      return true;
    }

    public virtual bool IsEvaluatableNew (NewExpression node)
    {
      ArgumentUtility.CheckNotNull ("node", node);

      return true;
    }

    public virtual bool IsEvaluatableNewArray (NewArrayExpression node)
    {
      ArgumentUtility.CheckNotNull ("node", node);

      return true;
    }

    public virtual bool IsEvaluatableTypeBinary (TypeBinaryExpression node)
    {
      ArgumentUtility.CheckNotNull ("node", node);

      return true;
    }

    public virtual bool IsEvaluatableUnary (UnaryExpression node)
    {
      ArgumentUtility.CheckNotNull ("node", node);

      return true;
    }

#if !NET_3_5
    public virtual bool IsEvaluatableBlock (BlockExpression node)
    {
      ArgumentUtility.CheckNotNull ("node", node);

      return true;
    }

    public virtual bool IsEvaluatableCatchBlock (CatchBlock node)
    {
      ArgumentUtility.CheckNotNull ("node", node);

      return true;
    }

    public virtual bool IsEvaluatableDebugInfo (DebugInfoExpression node)
    {
      ArgumentUtility.CheckNotNull ("node", node);

      return true;
    }

    public virtual bool IsEvaluatableDefault (DefaultExpression node)
    {
      ArgumentUtility.CheckNotNull ("node", node);

      return true;
    }

    public virtual bool IsEvaluatableGoto (GotoExpression node)
    {
      ArgumentUtility.CheckNotNull ("node", node);

      return true;
    }

    public virtual bool IsEvaluatableIndex (IndexExpression node)
    {
      ArgumentUtility.CheckNotNull ("node", node);

      return true;
    }

    public virtual bool IsEvaluatableLabel (LabelExpression node)
    {
      ArgumentUtility.CheckNotNull ("node", node);

      return true;
    }

    public virtual bool IsEvaluatableLabelTarget (LabelTarget node)
    {
      ArgumentUtility.CheckNotNull ("node", node);

      return true;
    }

    public virtual bool IsEvaluatableLoop (LoopExpression node)
    {
      ArgumentUtility.CheckNotNull ("node", node);

      return true;
    }

    public virtual bool IsEvaluatableSwitch (SwitchExpression node)
    {
      ArgumentUtility.CheckNotNull ("node", node);

      return true;
    }

    public virtual bool IsEvaluatableSwitchCase (SwitchCase node)
    {
      ArgumentUtility.CheckNotNull ("node", node);

      return true;
    }

    public virtual bool IsEvaluatableTry (TryExpression node)
    {
      ArgumentUtility.CheckNotNull ("node", node);

      return true;
    }
#endif
  }
}