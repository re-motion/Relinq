// This file is part of the re-motion Core Framework (www.re-motion.org)
// Copyright (C) 2005-2009 rubicon informationstechnologie gmbh, www.rubicon.eu
// 
// The re-motion Core Framework is free software; you can redistribute it 
// and/or modify it under the terms of the GNU Lesser General Public License 
// as published by the Free Software Foundation; either version 2.1 of the 
// License, or (at your option) any later version.
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
using System.Linq.Expressions;
using Remotion.Data.Linq.Parsing;
using Remotion.Data.Linq.Utilities;

namespace Remotion.Data.Linq.Clauses.Expressions
{
  /// <summary>
  /// Acts as a base class for custom extension expressions, providing advanced visitor support. Also allows extension expressions to be reduced to 
  /// a tree of standard expressions with equivalent semantics.
  /// </summary>
  public abstract class ExtensionExpression : Expression
  {
    public const ExpressionType ExtensionExpressionNodeType = (ExpressionType) int.MaxValue;

    protected ExtensionExpression (Type type)
        : base (ExtensionExpressionNodeType, ArgumentUtility.CheckNotNull ("", type))
    {
    }

    /// <summary>
    /// Accepts the specified visitor, by default dispatching to <see cref="ExpressionTreeVisitor.VisitUnknownExpression"/>. 
    /// Inheritors of the <see cref="ExtensionExpression"/> class can override this method in order to dispatch to a specific Visit method.
    /// </summary>
    /// <param name="visitor">The visitor whose Visit method should be invoked.</param>
    /// <returns>The <see cref="Expression"/> returned by the visitor.</returns>
    /// <remarks>
    /// Overriders can test the <paramref name="visitor"/> for a specific interface. If the visitor supports the interface, the extension expression 
    /// can dispatch to the respective strongly-typed Visit method declared in the interface. If it does not, the extension expression should call 
    /// the base implementation of <see cref="Accept"/>, which will dispatch to <see cref="ExpressionTreeVisitor.VisitUnknownExpression"/>.
    /// </remarks>
    public virtual Expression Accept (ExpressionTreeVisitor visitor)
    {
      ArgumentUtility.CheckNotNull ("visitor", visitor);

      return visitor.VisitUnknownExpression (this);
    }
  }
}