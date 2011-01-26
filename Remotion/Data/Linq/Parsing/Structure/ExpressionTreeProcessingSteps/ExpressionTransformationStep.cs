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
using Remotion.Data.Linq.Parsing.ExpressionTreeVisitors;
using Remotion.Data.Linq.Parsing.ExpressionTreeVisitors.Transformation;
using Remotion.Data.Linq.Utilities;

namespace Remotion.Data.Linq.Parsing.Structure.ExpressionTreeProcessingSteps
{
  /// <summary>
  /// Applies a given set of transformations to an <see cref="Expression"/> tree. The transformations are provided by an instance of
  /// <see cref="IExpressionTranformationProvider"/> (eg., <see cref="ExpressionTransformerRegistry"/>).
  /// </summary>
  /// <remarks>
  /// The <see cref="ExpressionTransformationStep"/> uses the <see cref="TransformingExpressionTreeVisitor"/> to apply the transformations.
  /// It performs a single visiting run over the <see cref="Expression"/> tree.
  /// </remarks>
  public class ExpressionTransformationStep : IExpressionTreeProcessingStep
  {
    private readonly IExpressionTranformationProvider _provider;

    /// <summary>
    /// Initializes a new instance of the <see cref="ExpressionTransformationStep"/> class.
    /// </summary>
    /// <param name="provider">A class providing the transformations to apply to the tree, eg., an instance of 
    /// <see cref="ExpressionTransformerRegistry"/>.</param>
    public ExpressionTransformationStep (IExpressionTranformationProvider provider)
    {
      ArgumentUtility.CheckNotNull ("provider", provider);

      _provider = provider;
    }

    public IExpressionTranformationProvider Provider
    {
      get { return _provider; }
    }

    public Expression Process (Expression expressionTree)
    {
      ArgumentUtility.CheckNotNull ("expressionTree", expressionTree);

      return TransformingExpressionTreeVisitor.Transform (expressionTree, _provider);
    }
  }
}