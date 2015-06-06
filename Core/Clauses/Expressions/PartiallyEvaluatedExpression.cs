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
using Remotion.Linq.Utilities;
using Remotion.Utilities;

namespace Remotion.Linq.Clauses.Expressions
{
  /// <summary>
  /// Represents a partially evaluated expression.
  /// </summary>
  /// <remarks>
  /// <para>
  /// To explicitly support this expression type, implement <see cref="IPartialEvaluationExpressionVisitor"/>.
  /// To treat this expression as if it were the evaluated <see cref="ConstantExpression"/>, call its <see cref="Reduce"/> method and visit the result.
  /// </para>
  /// <para>
  /// Subclasses of <see cref="ThrowingExpressionVisitor"/> that do not implement <see cref="IPartialEvaluationExpressionVisitor"/> will, by default, 
  /// automatically reduce this expression type to <see cref="ConstantExpression"/> in the <see cref="ThrowingExpressionVisitor.VisitExtension"/> method.
  /// </para>
  /// <para>
  /// Subclasses of <see cref="RelinqExpressionVisitor"/> that do not implement <see cref="IPartialEvaluationExpressionVisitor"/> will, by default, 
  /// ignore this expression and visit its child expressions via the <see cref="ExpressionVisitor.VisitExtension"/> and <see cref="VisitChildren"/> methods.
  /// </para>
  /// </remarks>
  public sealed class PartiallyEvaluatedExpression 
#if !NET_3_5
    : Expression
#else
    : ExtensionExpression
#endif
  {
#if NET_3_5
    public const ExpressionType ExpressionType = (ExpressionType) 100005;
#endif

    private readonly Expression _originalExpression;
    private readonly ConstantExpression _evaluatedExpression;

    public PartiallyEvaluatedExpression (Expression originalExpression, ConstantExpression evaluatedExpression)
#if NET_3_5
      : base (ArgumentUtility.CheckNotNull ("evaluatedExpression", evaluatedExpression).Type, ExpressionType)
#endif
    {
      ArgumentUtility.CheckNotNull ("originalExpression", originalExpression);
      ArgumentUtility.CheckNotNull ("evaluatedExpression", evaluatedExpression);
      if (originalExpression.Type != evaluatedExpression.Type)
      {
        throw new ArgumentException (
            string.Format (
                "Type '{0}' of parameter 'originalExpression' does not match Type '{1}' of parameter 'evaluatedExpression'.",
                originalExpression.Type,
                evaluatedExpression.Type));
      }

      _originalExpression = originalExpression;
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

    public Expression OriginalExpression
    {
      get { return _originalExpression; }
    }

    public ConstantExpression EvaluatedExpression
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

      var newOriginalExpression = visitor.Visit (_originalExpression);
      var newEvaluatedExpression = visitor.VisitAndConvert (_evaluatedExpression, "VisitChildren");
      if (newOriginalExpression != _originalExpression || newEvaluatedExpression != _evaluatedExpression)
        return new PartiallyEvaluatedExpression (newOriginalExpression, newEvaluatedExpression);
      else
        return this;
    }

    protected override Expression Accept (ExpressionVisitor visitor)
    {
      ArgumentUtility.CheckNotNull ("visitor", visitor);

      var specificVisitor = visitor as IPartialEvaluationExpressionVisitor;
      if (specificVisitor != null)
        return specificVisitor.VisitPartiallyEvaluated (this);
      else
        return base.Accept (visitor);
    }

    public override string ToString ()
    {
      return string.Format ("PartiallyEvaluated({0}, {1})", _originalExpression.BuildString(), _evaluatedExpression.BuildString());
    }
  }
}