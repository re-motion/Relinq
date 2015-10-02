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
using Remotion.Linq.Parsing;
using Remotion.Linq.Parsing.ExpressionVisitors;
using Remotion.Linq.Utilities;
using Remotion.Utilities;

namespace Remotion.Linq.Clauses.Expressions
{
  /// <summary>
  /// Wraps an exception whose partial evaluation caused an exception.
  /// </summary>
  /// <remarks>
  /// <para>
  /// When <see cref="PartialEvaluatingExpressionVisitor"/> encounters an exception while evaluating an independent expression subtree, it
  /// will wrap the subtree within a <see cref="PartialEvaluationExceptionExpression"/>. The wrapper contains both the <see cref="Exception"/> 
  /// instance and the <see cref="EvaluatedExpression"/> that caused the exception.
  /// </para>
  /// <para>
  /// To explicitly support this expression type, implement  <see cref="IPartialEvaluationExceptionExpressionVisitor"/>.
  /// To ignore this wrapper and only handle the inner <see cref="EvaluatedExpression"/>, call the <see cref="Reduce"/> method and visit the result.
  /// </para>
  /// <para>
  /// Subclasses of <see cref="ThrowingExpressionVisitor"/> that do not implement <see cref="IPartialEvaluationExceptionExpressionVisitor"/> will, 
  /// by default, automatically reduce this expression type to the <see cref="EvaluatedExpression"/> in the 
  /// <see cref="ThrowingExpressionVisitor.VisitExtension"/> method.
  /// </para>
  /// <para>
  /// Subclasses of <see cref="RelinqExpressionVisitor"/> that do not implement <see cref="IPartialEvaluationExceptionExpressionVisitor"/> will, 
  /// by default, ignore this expression and visit its child expressions via the <see cref="ExpressionVisitor.VisitExtension"/> and 
  /// <see cref="VisitChildren"/> methods.
  /// </para>
  /// </remarks>
  public sealed class PartialEvaluationExceptionExpression
#if !NET_3_5
    : Expression
#else
    : ExtensionExpression
#endif
  {
#if NET_3_5
    public const ExpressionType ExpressionType = (ExpressionType) 100004;
#endif

    private readonly Exception _exception;
    private readonly Expression _evaluatedExpression;

    public PartialEvaluationExceptionExpression (Exception exception, Expression evaluatedExpression)
#if NET_3_5
      : base (ArgumentUtility.CheckNotNull ("evaluatedExpression", evaluatedExpression).Type, ExpressionType)
#endif
    {
      ArgumentUtility.CheckNotNull ("exception", exception);
      
      _exception = exception;
      _evaluatedExpression = evaluatedExpression;
    }

#if !NET_3_5
    public override Type Type
    {
      get { return _evaluatedExpression.Type; }
    }

    public override ExpressionType NodeType
    {
      get { return ExpressionType.Extension; }
    }
#endif

    public Exception Exception
    {
      get { return _exception; }
    }

    public Expression EvaluatedExpression
    {
      get { return _evaluatedExpression; }
    }

    public override bool CanReduce
    {
      get { return true; }
    }

    public override Expression Reduce ()
    {
      return _evaluatedExpression;
    }

    protected override Expression VisitChildren (ExpressionVisitor visitor)
    {
      ArgumentUtility.CheckNotNull ("visitor", visitor);

      var newEvaluatedExpression = visitor.Visit (_evaluatedExpression);
      if (newEvaluatedExpression != _evaluatedExpression)
        return new PartialEvaluationExceptionExpression (_exception, newEvaluatedExpression);
      else
        return this;
    }

    protected override Expression Accept (ExpressionVisitor visitor)
    {
      ArgumentUtility.CheckNotNull ("visitor", visitor);

      var specificVisitor = visitor as IPartialEvaluationExceptionExpressionVisitor;
      if (specificVisitor != null)
        return specificVisitor.VisitPartialEvaluationException (this);
      else
        return base.Accept (visitor);
    }

    public override string ToString ()
    {
      return string.Format (
          @"PartialEvalException ({0} (""{1}""), {2})",
          _exception.GetType().Name,
          _exception.Message,
          _evaluatedExpression.BuildString());
    }
  }
}