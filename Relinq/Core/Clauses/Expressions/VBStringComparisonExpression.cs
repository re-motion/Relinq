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
using System.Linq.Expressions;
using Remotion.Linq.Clauses.ExpressionTreeVisitors;
using Remotion.Linq.Parsing;
using Remotion.Linq.Utilities;

namespace Remotion.Linq.Clauses.Expressions
{
  /// <summary>
  /// Represents a VB-specific comparison expression. To explicitly support this expression type, implement <see cref="IVBSpecificExpressionVisitor"/>.
  /// To treat this expression as if it were an ordinary <see cref="BinaryExpression"/>, call its <see cref="Reduce"/> method and visit the result.
  /// </summary>
  /// <remarks>
  /// <para>
  /// Subclasses of <see cref="ThrowingExpressionTreeVisitor"/> that do not implement <see cref="IVBSpecificExpressionVisitor"/> will, by default, 
  /// automatically reduce this expression type to <see cref="BinaryExpression"/> in the 
  /// <see cref="ThrowingExpressionTreeVisitor.VisitExtensionExpression"/> method.
  /// </para>
  /// <para>
  /// Subclasses of <see cref="ExpressionTreeVisitor"/> that do not implement <see cref="IVBSpecificExpressionVisitor"/> will, by default, 
  /// ignore this expression and visit its child expressions via the <see cref="ExpressionTreeVisitor.VisitExtensionExpression"/> and 
  /// <see cref="VisitChildren"/> methods.
  /// </para>
  /// </remarks>
  public class VBStringComparisonExpression : ExtensionExpression
  {
    public const ExpressionType ExpressionType = (ExpressionType) 100003;

    private readonly Expression _comparison;
    private readonly bool _textCompare;

    public VBStringComparisonExpression (Expression comparison, bool textCompare)
        : base(comparison.Type, ExpressionType)
    {
      ArgumentUtility.CheckNotNull ("comparison", comparison);

      _comparison = comparison;
      _textCompare = textCompare;
    }

    public Expression Comparison
    {
      get { return _comparison; }
    }

    public bool TextCompare
    {
      get { return _textCompare; }
    }

    public override bool CanReduce
    {
      get { return true; }
    }

    public override Expression Reduce ()
    {
      return _comparison;
    }

    protected internal override Expression VisitChildren (ExpressionTreeVisitor visitor)
    {
      ArgumentUtility.CheckNotNull ("visitor", visitor);

      var newExpression = visitor.VisitExpression (_comparison);
      if (newExpression != _comparison)
        return new VBStringComparisonExpression (newExpression, _textCompare);
      else
        return this;
    }

    public override Expression Accept (ExpressionTreeVisitor visitor)
    {
      ArgumentUtility.CheckNotNull ("visitor", visitor);

      var specificVisitor = visitor as IVBSpecificExpressionVisitor;
      if (specificVisitor != null)
        return specificVisitor.VisitVBStringComparisonExpression (this);
      else
        return base.Accept (visitor);
    }

    public override string ToString ()
    {
      return string.Format ("VBCompareString({0}, {1})", FormattingExpressionTreeVisitor.Format(Comparison), TextCompare);
    }
    
  }
}