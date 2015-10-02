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
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using JetBrains.Annotations;
using Remotion.Linq.Clauses.Expressions;
using Remotion.Linq.Clauses.ExpressionVisitors;
using Remotion.Linq.Clauses.StreamedData;
using Remotion.Linq.Utilities;
using Remotion.Utilities;

namespace Remotion.Linq.Clauses.ResultOperators
{
  /// <summary>
  /// Represents aggregating the items returned by a query into a single value with an initial seeding value.
  /// This is a result operator, operating on the whole result set of a query.
  /// </summary>
  /// <example>
  /// In C#, the "Aggregate" call in the following example corresponds to an <see cref="AggregateFromSeedResultOperator"/>.
  /// <code>
  /// var result = (from s in Students
  ///              select s).Aggregate(0, (totalAge, s) => totalAge + s.Age);
  /// </code>
  /// </example>
  public sealed class AggregateFromSeedResultOperator : ValueFromSequenceResultOperatorBase
  {
    private static readonly MethodInfo s_executeMethod =
        typeof (AggregateFromSeedResultOperator).GetRuntimeMethodChecked ("ExecuteAggregateInMemory", new[] { typeof (StreamedSequence) });

    private Expression _seed;
    private LambdaExpression _func;
    private LambdaExpression _resultSelector;

    /// <summary>
    /// Initializes a new instance of the <see cref="AggregateFromSeedResultOperator"/> class.
    /// </summary>
    /// <param name="seed">The seed expression.</param>
    /// <param name="func">The aggregating function. This is a <see cref="LambdaExpression"/> taking a parameter that represents the value accumulated so 
    /// far and returns a new accumulated value. This is a resolved expression, i.e. items streaming in from prior clauses and result operators
    /// are represented as expressions containing <see cref="QuerySourceReferenceExpression"/> nodes.</param>
    /// <param name="optionalResultSelector">The result selector, can be <see langword="null" />.</param>
    public AggregateFromSeedResultOperator (Expression seed, LambdaExpression func, LambdaExpression optionalResultSelector)
    {
      ArgumentUtility.CheckNotNull ("seed", seed);
      ArgumentUtility.CheckNotNull ("func", func);
      if (func.Type.GetTypeInfo().IsGenericTypeDefinition)
        throw new ArgumentException ("Open generic delegates are not supported with AggregateFromSeedResultOperator", "func");

      Seed = seed;
      Func = func;
      OptionalResultSelector = optionalResultSelector;
    }

    /// <summary>
    /// Gets or sets the aggregating function. This is a <see cref="LambdaExpression"/> taking a parameter that represents the value accumulated so 
    /// far and returns a new accumulated value. This is a resolved expression, i.e. items streaming in from prior clauses and result operators
    /// are represented as expressions containing <see cref="QuerySourceReferenceExpression"/> nodes.
    /// </summary>
    /// <value>The aggregating function.</value>
    public LambdaExpression Func
    {
      get { return _func; }
      set
      {
        ArgumentUtility.CheckNotNull ("value", value);
        if (value.Type.GetTypeInfo().IsGenericTypeDefinition)
          throw new ArgumentException ("Open generic delegates are not supported with AggregateFromSeedResultOperator", "value");

        if (!DescribesValidFuncType (value))
        {
          var message = string.Format (
              "The aggregating function must be a LambdaExpression that describes an instantiation of 'Func<TAccumulate,TAccumulate>', but it is '{0}'.",
              value.Type);
          throw new ArgumentException (message, "value");
        }

        _func = value;
      }
    }

    /// <summary>
    /// Gets or sets the seed of the accumulation. This is an <see cref="Expression"/> denoting the starting value of the aggregation.
    /// </summary>
    /// <value>The seed of the accumulation.</value>
    public Expression Seed
    {
      get { return _seed; }
      set { _seed = ArgumentUtility.CheckNotNull ("value", value); }
    }

    /// <summary>
    /// Gets or sets the result selector. This is a <see cref="LambdaExpression"/> applied after the aggregation to select the final value.
    /// Can be <see langword="null" />.
    /// </summary>
    /// <value>The result selector.</value>
    public LambdaExpression OptionalResultSelector
    {
      get { return _resultSelector; }
      set
      {
        if (value != null && value.Type.GetTypeInfo().IsGenericTypeDefinition)
          throw new ArgumentException ("Open generic delegates are not supported with AggregateFromSeedResultOperator", "value");

        if (!DescribesValidResultSelectorType (value))
        {
          var message = string.Format (
              "The result selector must be a LambdaExpression that describes an instantiation of 'Func<TAccumulate,TResult>', but it is '{0}'.",
              value.Type);
          throw new ArgumentException (message, "value");
        }

        _resultSelector = value;
      }
    }

    /// <summary>
    /// Gets the constant value of the <see cref="Seed"/> property, assuming it is a <see cref="ConstantExpression"/>. If it is
    /// not, an <see cref="InvalidOperationException"/> is thrown.
    /// </summary>
    /// <typeparam name="T">The expected seed type. If the item is not of this type, an <see cref="InvalidOperationException"/> is thrown.</typeparam>
    /// <returns>The constant value of the <see cref="Seed"/> property.</returns>
    public T GetConstantSeed<T> ()
    {
      return GetConstantValueFromExpression<T> ("seed", Seed);
    }

    /// <inheritdoc cref="ResultOperatorBase.ExecuteInMemory" />
    public override StreamedValue ExecuteInMemory<TInput> (StreamedSequence input)
    {
      ArgumentUtility.CheckNotNull ("input", input);

      var closedExecuteMethod = s_executeMethod.MakeGenericMethod (typeof (TInput), Seed.Type, GetResultType());
      return (StreamedValue) InvokeExecuteMethod (closedExecuteMethod, input);
    }

    /// <summary>
    /// Executes the aggregating operation in memory.
    /// </summary>
    /// <typeparam name="TInput">The type of the source items.</typeparam>
    /// <typeparam name="TAggregate">The type of the aggregated items.</typeparam>
    /// <typeparam name="TResult">The type of the result items.</typeparam>
    /// <param name="input">The input sequence.</param>
    /// <returns>A <see cref="StreamedValue"/> object holding the aggregated value.</returns>
    public StreamedValue ExecuteAggregateInMemory<TInput, TAggregate, TResult> (StreamedSequence input)
    {
      ArgumentUtility.CheckNotNull ("input", input);

      var sequence = input.GetTypedSequence<TInput> ();
      var seed = GetConstantSeed<TAggregate> ();
      var funcLambda = ReverseResolvingExpressionVisitor.ReverseResolveLambda (input.DataInfo.ItemExpression, Func, 1);
      var func = (Func<TAggregate, TInput, TAggregate>) funcLambda.Compile ();

      var aggregated = sequence.Aggregate (seed, func);
      var outputDataInfo = GetOutputDataInfo (input.DataInfo);
      if (OptionalResultSelector == null)
      {
        return new StreamedValue (aggregated, outputDataInfo);
      }
      else
      {
        var resultSelector = (Func<TAggregate, TResult>) OptionalResultSelector.Compile ();
        var result = resultSelector (aggregated);
        return new StreamedValue (result, outputDataInfo);
      }
    }

    /// <inheritdoc />
    public override ResultOperatorBase Clone (CloneContext cloneContext)
    {
      return new AggregateFromSeedResultOperator (Seed, Func, OptionalResultSelector);
    }

    /// <inheritdoc />
    public override IStreamedDataInfo GetOutputDataInfo (IStreamedDataInfo inputInfo)
    {
      var sequenceInfo = ArgumentUtility.CheckNotNullAndType<StreamedSequenceInfo> ("inputInfo", inputInfo);

      return GetOutputDataInfo (sequenceInfo);
    }

    // ReSharper disable once UnusedParameter.Local
    private StreamedValueInfo GetOutputDataInfo ([NotNull] StreamedSequenceInfo sequenceInfo)
    {
      Assertion.DebugAssert (Func.Type.GetTypeInfo().IsGenericTypeDefinition == false);
      var aggregatedType = Func.Type.GetTypeInfo().GenericTypeArguments[0];
      if (!aggregatedType.GetTypeInfo().IsAssignableFrom (Seed.Type.GetTypeInfo()))
      {
        var message = string.Format (
            "The seed expression and the aggregating function don't have matching types. The seed is of type '{0}', but the function aggregates '{1}'.",
            Seed.Type,
            aggregatedType);
        throw new InvalidOperationException (message);
      }

      Assertion.DebugAssert (OptionalResultSelector == null || OptionalResultSelector.Type.GetTypeInfo().IsGenericTypeDefinition == false);
      var resultTransformedType = OptionalResultSelector != null ? OptionalResultSelector.Type.GetTypeInfo().GenericTypeArguments[0] : null;
      if (resultTransformedType != null && aggregatedType != resultTransformedType)
      {
        var message = string.Format (
            "The aggregating function and the result selector don't have matching types. The function aggregates type '{0}', "
            + "but the result selector takes '{1}'.",
            aggregatedType,
            resultTransformedType);
        throw new InvalidOperationException (message);
      }

      var resultType = GetResultType ();
      return new StreamedScalarValueInfo (resultType);
    }

    public override void TransformExpressions (Func<Expression, Expression> transformation)
    {
      Seed = transformation (Seed);
      Func = (LambdaExpression) transformation (Func);
      if (OptionalResultSelector != null)
        OptionalResultSelector = (LambdaExpression) transformation (OptionalResultSelector);
    }

    /// <inheritdoc />
    public override string ToString ()
    {
      if (OptionalResultSelector != null)
      {
        return string.Format (
            "Aggregate({0}, {1}, {2})",
            Seed.BuildString(),
            Func.BuildString(),
            OptionalResultSelector.BuildString());
      }
      else
      {
        return string.Format (
            "Aggregate({0}, {1})",
            Seed.BuildString(),
            Func.BuildString());
      }
    }

    private Type GetResultType ()
    {
      return OptionalResultSelector != null ? OptionalResultSelector.Body.Type : Func.Body.Type;
    }

    private bool DescribesValidFuncType (LambdaExpression value)
    {
      var funcType = value.Type;
      if (!funcType.GetTypeInfo().IsGenericType || funcType.GetGenericTypeDefinition () != typeof (Func<,>))
        return false;

      Assertion.DebugAssert (funcType.GetTypeInfo().IsGenericTypeDefinition == false);
      var genericArguments = funcType.GetTypeInfo().GenericTypeArguments;
      return genericArguments[0] == genericArguments[1];
    }

    private bool DescribesValidResultSelectorType (LambdaExpression value)
    {
      return value == null || (value.Type.GetTypeInfo().IsGenericType && value.Type.GetGenericTypeDefinition() == typeof (Func<,>));
    }
  }
}
