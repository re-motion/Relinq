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
using Remotion.Linq.Parsing.ExpressionVisitors.Transformation;
using Remotion.Utilities;

namespace Remotion.Linq.Parsing.ExpressionVisitors
{
  /// <summary>
  /// Applies <see cref="ExpressionTransformation"/> delegates obtained from an <see cref="IExpressionTranformationProvider"/> to an expression tree. 
  /// The transformations occur in post-order (transforming child <see cref="Expression"/> nodes before parent nodes). When a transformation changes 
  /// the current <see cref="Expression"/>, its child nodes and itself will be revisited (and may be transformed again).
  /// </summary>
  public sealed class TransformingExpressionVisitor : RelinqExpressionVisitor
  {
    public static Expression Transform (Expression expression, IExpressionTranformationProvider tranformationProvider)
    {
      ArgumentUtility.CheckNotNull ("tranformationProvider", tranformationProvider);
      
      var visitor = new TransformingExpressionVisitor (tranformationProvider);
      return visitor.Visit (expression);
    }

    private readonly IExpressionTranformationProvider _tranformationProvider;

    private TransformingExpressionVisitor (IExpressionTranformationProvider tranformationProvider)
    {
      ArgumentUtility.CheckNotNull ("tranformationProvider", tranformationProvider);

      _tranformationProvider = tranformationProvider;
    }

    public override Expression Visit (Expression expression)
    {
      var newExpression = base.Visit (expression);
      if (newExpression == null)
        return null;

      var transformations = _tranformationProvider.GetTransformations (newExpression);

      foreach (var transformation in transformations)
      {
        var transformedExpression = transformation (newExpression);
        Assertion.IsNotNull (transformedExpression);
        if (transformedExpression != newExpression)
          return Visit (transformedExpression);
      }

      return newExpression;
    }
  }
}