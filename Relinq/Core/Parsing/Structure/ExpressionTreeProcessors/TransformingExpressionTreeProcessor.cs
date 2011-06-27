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
using Remotion.Linq.Parsing.ExpressionTreeVisitors;
using Remotion.Linq.Parsing.ExpressionTreeVisitors.Transformation;
using Remotion.Linq.Utilities;

namespace Remotion.Linq.Parsing.Structure.ExpressionTreeProcessors
{
  /// <summary>
  /// Applies a given set of transformations to an <see cref="Expression"/> tree. The transformations are provided by an instance of
  /// <see cref="IExpressionTranformationProvider"/> (eg., <see cref="ExpressionTransformerRegistry"/>).
  /// </summary>
  /// <remarks>
  /// The <see cref="TransformingExpressionTreeProcessor"/> uses the <see cref="TransformingExpressionTreeVisitor"/> to apply the transformations.
  /// It performs a single visiting run over the <see cref="Expression"/> tree.
  /// </remarks>
  public class TransformingExpressionTreeProcessor : IExpressionTreeProcessor
  {
    private readonly IExpressionTranformationProvider _provider;

    /// <summary>
    /// Initializes a new instance of the <see cref="TransformingExpressionTreeProcessor"/> class.
    /// </summary>
    /// <param name="provider">A class providing the transformations to apply to the tree, eg., an instance of 
    /// <see cref="ExpressionTransformerRegistry"/>.</param>
    public TransformingExpressionTreeProcessor (IExpressionTranformationProvider provider)
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