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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Remotion.Linq.Collections;
using Remotion.Linq.Parsing.ExpressionTreeVisitors.Transformation.PredefinedTransformations;
using Remotion.Linq.Utilities;

namespace Remotion.Linq.Parsing.ExpressionTreeVisitors.Transformation
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
    /// <item><see cref="VBInformationIsNothingExpressionTransformer"/></item>
    /// <item><see cref="InvocationOfLambdaExpressionTransformer"/></item>
    /// <item><see cref="NullableValueTransformer"/></item>
    /// <item><see cref="KeyValuePairNewExpressionTransformer"/></item>
    /// <item><see cref="DictionaryEntryNewExpressionTransformer"/></item>
    /// <item><see cref="TupleNewExpressionTransformer"/></item>
    /// </list>
    /// </remarks>
    public static ExpressionTransformerRegistry CreateDefault ()
    {
      var registry = new ExpressionTransformerRegistry();
      registry.Register (new VBCompareStringExpressionTransformer ());
      registry.Register (new VBInformationIsNothingExpressionTransformer ());
      registry.Register (new InvocationOfLambdaExpressionTransformer ());
      registry.Register (new NullableValueTransformer ());

      registry.Register (new KeyValuePairNewExpressionTransformer());
      registry.Register (new DictionaryEntryNewExpressionTransformer());
      registry.Register (new TupleNewExpressionTransformer());
      
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

    /// <summary>
    /// Registers the specified <see cref="IExpressionTransformer{T}"/> for the transformer's 
    /// <see cref="IExpressionTransformer{T}.SupportedExpressionTypes"/>. If <see cref="IExpressionTransformer{T}.SupportedExpressionTypes"/>
    /// returns <see langword="null" />, the <paramref name="transformer"/> is registered as a generic transformer which will be applied to all
    /// <see cref="Expression"/> nodes.
    /// </summary>
    /// <typeparam name="T">The type of expressions handled by the <paramref name="transformer"/>. This should be a type implemented by all
    /// expressions identified by <see cref="IExpressionTransformer{T}.SupportedExpressionTypes"/>. For generic transformers, <typeparamref name="T"/> 
    /// must be <see cref="Expression"/>.</typeparam>
    /// <param name="transformer">The transformer to register.</param>
    /// <remarks>
    /// <para>
    /// The order in which transformers are registered is the same order on which they will later be applied by 
    /// <see cref="TransformingExpressionTreeVisitor"/>. When more than one transformer is registered for a certain <see cref="ExpressionType"/>,
    /// each of them will get a chance to transform a given <see cref="Expression"/>, until the first one returns a new <see cref="Expression"/>.
    /// At that point, the transformation will start again with the new <see cref="Expression"/> (and, if the expression's type has changed, potentially 
    /// different transformers).
    /// </para>
    /// <para>
    /// When generic transformers are registered, they act as if they had been registered for all <see cref="ExpressionType"/> values (including
    /// custom ones). They will be applied in the order registered, but only after all respective specific transformers have run (without modifying 
    /// the expression, which would restart the transformation process with the new expression as explained above).
    /// </para>
    /// <para>
    /// When an <see cref="IExpressionTransformer{T}"/> is registered for an incompatible <see cref="ExpressionType"/>, this is not detected until 
    /// the transformer is actually applied to an <see cref="Expression"/> of that <see cref="ExpressionType"/>.
    /// </para>
    /// </remarks>
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