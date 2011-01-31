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
using System.Linq;
using System.Linq.Expressions;
using Remotion.Data.Linq.Utilities;

namespace Remotion.Data.Linq.Parsing.ExpressionTreeVisitors.Transformation.PredefinedTransformations
{
  /// <summary>
  /// Replaces <see cref="ConstantExpression"/> nodes that contain queries implementing <see cref="IQueryable{T}"/> with the expression defining
  /// the respective query.
  /// </summary>
  /// <remarks>
  /// Consider the following example:
  /// <code>
  /// var x = from p in Persons where p.Surname == "Freud" select p;
  /// var y = from p in Persons where x.Contains (p) select p;
  /// </code>
  /// This transformation inlines query x in query y, as if someone had written:
  /// <code>
  /// var y = from p in Persons where (from p1 in Persons where p1.Surname == "Freud" select p1).Contains (p) select p;
  /// </code>
  /// </remarks>
  public class QueryableConstantExpressionTransformer : IExpressionTransformer<ConstantExpression>
  {
    public ExpressionType[] SupportedExpressionTypes
    {
      get { return new[] { ExpressionType.Constant }; }
    }

    public Expression Transform (ConstantExpression expression)
    {
      ArgumentUtility.CheckNotNull ("expression", expression);

      var valueAsQueryable = expression.Value as IQueryable;
      if (valueAsQueryable != null)
        return PartialEvaluatingExpressionTreeVisitor.EvaluateIndependentSubtrees (valueAsQueryable.Expression);
      else
        return expression;
    }
  }
}