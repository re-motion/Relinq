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
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Remotion.Data.Linq.Collections;
using Remotion.Data.Linq.Parsing.ExpressionTreeVisitors.Transformation.PredefinedTransformations;
using Remotion.Data.Linq.Utilities;

namespace Remotion.Data.Linq.Parsing.ExpressionTreeVisitors.Transformation
{
  /// <summary>
  /// Manages registration and lookup of <see cref="IExpressionTransformer{T}"/> objects, and converts them to 
  /// weakly typed <see cref="ExpressionTransformation"/> instances. Use this class together with <see cref="TransformingExpressionTreeVisitor"/>
  /// in order to apply the registered transformers to an <see cref="Expression"/> tree.
  /// </summary>
  public class ExpressionTransformerRegistry : IExpressionTranformationProvider
  {
    /// <summary>
    /// Creates an <see cref="ExpressionTransformerRegistry"/> with the default transformations provided by this library already registered.
    /// New transformers can be registered by calling <see cref="Register{T}"/>.
    /// </summary>
    /// <returns> A default <see cref="ExpressionTransformerRegistry"/>.</returns>
    /// <remarks>
    /// Currently, the default registry contains:
    /// <list type="bullet">
    /// <item><see cref="VBCompareStringExpressionTransformer"/></item>
    /// </list>
    /// </remarks>
    public static ExpressionTransformerRegistry CreateDefault ()
    {
      var registry = new ExpressionTransformerRegistry();
      registry.Register (new VBCompareStringExpressionTransformer());
      return registry;
    }

    private readonly MultiDictionary<ExpressionType, ExpressionTransformation> _transformations =
        new MultiDictionary<ExpressionType, ExpressionTransformation>();

    private readonly List<ExpressionTransformation> _genericTransformations = new List<ExpressionTransformation>();


    public int RegisteredTransformerCount
    {
      get { return _transformations.CountValues(); }
    }

    public ExpressionTransformation[] GetAllTransformations (ExpressionType expressionType)
    {
      return _transformations[expressionType].ToArray();
    }

    public IEnumerable<ExpressionTransformation> GetTransformations (Expression expression)
    {
      ArgumentUtility.CheckNotNull ("expression", expression);

      IList<ExpressionTransformation> matchingTransformations;
      _transformations.TryGetValue (expression.NodeType, out matchingTransformations);

      if (matchingTransformations != null)
        return matchingTransformations.Concat (_genericTransformations);
      else
        return _genericTransformations;
    }

    public void Register<T> (IExpressionTransformer<T> transformer) where T: Expression
    {
      ArgumentUtility.CheckNotNull ("transformer", transformer);

      ExpressionTransformation transformation = expr => TransformExpression (expr, transformer);

      if (transformer.SupportedExpressionTypes == null)
      {
        if (typeof (T) != typeof (Expression))
        {
          var message = string.Format (
              "Cannot register an IExpressionTransformer<{0}> as a generic transformer. Generic transformers must implement "
              + "IExpressionTransformer<Expression>.",
              typeof (T).Name);
          throw new ArgumentException (message, "transformer");
        }

        _genericTransformations.Add (transformation);
      }
      else
      {
        foreach (var expressionType in transformer.SupportedExpressionTypes)
          _transformations.Add (expressionType, transformation);
      }
    }

    private static Expression TransformExpression<T> (Expression expression, IExpressionTransformer<T> transformer) where T: Expression
    {
      T castExpression;
      try
      {
        castExpression = (T) expression;
      }
      catch (InvalidCastException ex)
      {
        var message =
            string.Format (
                "A '{0}' with node type '{1}' cannot be handled by the IExpressionTransformer<{2}>. The transformer was probably registered for "
                + "a wrong ExpressionType.",
                expression.GetType().Name,
                expression.NodeType,
                typeof (T).Name);
        throw new InvalidOperationException (message, ex);
      }
      return transformer.Transform (castExpression);
    }
  }
}