// This file is part of the re-linq project (relinq.codeplex.com)
// Copyright (c) rubicon IT GmbH, www.rubicon.eu
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
using System.Linq.Expressions;
using Remotion.Linq.Clauses.ExpressionTreeVisitors;
using Remotion.Linq.Parsing;
using Remotion.Linq.Parsing.ExpressionTreeVisitors;
using Remotion.Linq.Utilities;

namespace Remotion.Linq.Clauses.Expressions
{
  /// <summary>
  /// Wraps an exception whose partial evaluation caused an exception.
  /// </summary>
  /// <remarks>
  /// <para>
  /// When <see cref="PartialEvaluatingExpressionTreeVisitor"/> encounters an exception while evaluating an independent expression subtree, it
  /// will wrap the subtree within a <see cref="PartialEvaluationExceptionExpression"/>. The wrapper contains both the <see cref="Exception"/> 
  /// instance and the <see cref="EvaluatedExpression"/> that caused the exception.
  /// </para>
  /// <para>
  /// To explicitly support this expression type, implement  <see cref="IPartialEvaluationExceptionExpressionVisitor"/>.
  /// To ignore this wrapper and only handle the inner <see cref="EvaluatedExpression"/>, call the <see cref="Reduce"/> method and visit the result.
  /// </para>
  /// <para>
  /// Subclasses of <see cref="ThrowingExpressionTreeVisitor"/> that do not implement <see cref="IPartialEvaluationExceptionExpressionVisitor"/> will, 
  /// by default, automatically reduce this expression type to the <see cref="EvaluatedExpression"/> in the 
  /// <see cref="ThrowingExpressionTreeVisitor.VisitExtensionExpression"/> method.
  /// </para>
  /// <para>
  /// Subclasses of <see cref="ExpressionTreeVisitor"/> that do not implement <see cref="IPartialEvaluationExceptionExpressionVisitor"/> will, 
  /// by default, ignore this expression and visit its child expressions via the <see cref="ExpressionTreeVisitor.VisitExtensionExpression"/> and 
  /// <see cref="VisitChildren"/> methods.
  /// </para>
  /// </remarks>
  public class PartialEvaluationExceptionExpression : ExtensionExpression
  {
    public const ExpressionType ExpressionType = (ExpressionType) 100004;

    private readonly Exception _exception;
    private readonly Expression _evaluatedExpression;

    public PartialEvaluationExceptionExpression (Exception exception, Expression evaluatedExpression)
      : base (ArgumentUtility.CheckNotNull ("evaluatedExpression", evaluatedExpression).Type, ExpressionType)
    {
      ArgumentUtility.CheckNotNull ("exception", exception);
      
      _exception = exception;
      _evaluatedExpression = evaluatedExpression;
    }

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

    protected internal override Expression VisitChildren (ExpressionTreeVisitor visitor)
    {
      ArgumentUtility.CheckNotNull ("visitor", visitor);

      var newEvaluatedExpression = visitor.VisitExpression (_evaluatedExpression);
      if (newEvaluatedExpression != _evaluatedExpression)
        return new PartialEvaluationExceptionExpression (_exception, newEvaluatedExpression);
      else
        return this;
    }

    public override Expression Accept (ExpressionTreeVisitor visitor)
    {
      ArgumentUtility.CheckNotNull ("visitor", visitor);

      var specificVisitor = visitor as IPartialEvaluationExceptionExpressionVisitor;
      if (specificVisitor != null)
        return specificVisitor.VisitPartialEvaluationExceptionExpression (this);
      else
        return base.Accept (visitor);
    }

    public override string ToString ()
    {
      return string.Format (
          @"PartialEvalException ({0} (""{1}""), {2})",
          _exception.GetType().Name,
          _exception.Message,
          FormattingExpressionTreeVisitor.Format (_evaluatedExpression));
    }
  }
}