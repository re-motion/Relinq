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
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Linq;
using Remotion.Linq.Clauses.Expressions;
using Remotion.Linq.Clauses.ExpressionTreeVisitors;
using Remotion.Linq.Clauses.StreamedData;
using Remotion.Linq.Utilities;

namespace Remotion.Linq.Clauses.ResultOperators
{
  /// <summary>
  /// Represents aggregating the items returned by a query into a single value. The first item is used as the seeding value for the aggregating 
  /// function.
  /// This is a result operator, operating on the whole result set of a query.
  /// </summary>
  /// <example>
  /// In C#, the "Aggregate" call in the following example corresponds to an <see cref="AggregateResultOperator"/>.
  /// <code>
  /// var result = (from s in Students
  ///              select s.Name).Aggregate((allNames, name) => allNames + " " + name);
  /// </code>
  /// </example>
  public class AggregateResultOperator : ValueFromSequenceResultOperatorBase
  {
    private LambdaExpression _func;

    /// <summary>
    /// Initializes a new instance of the <see cref="AggregateResultOperator"/> class.
    /// </summary>
    /// <param name="func">The aggregating function. This is a <see cref="LambdaExpression"/> taking a parameter that represents the value accumulated so 
    /// far and returns a new accumulated value. This is a resolved expression, i.e. items streaming in from prior clauses and result operators
    /// are represented as expressions containing <see cref="QuerySourceReferenceExpression"/> nodes.</param>
    public AggregateResultOperator (LambdaExpression func)
    {
      ArgumentUtility.CheckNotNull ("func", func);

      Func = func;
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

        if (!DescribesValidFuncType (value))
        {
          var message = string.Format (
              "The aggregating function must be a LambdaExpression that describes an instantiation of 'Func<T,T>', but it is '{0}'.", 
              value.Type);
          throw new ArgumentTypeException (message, "value", typeof (Func<,>), value.Type);
        }

        _func = value; 
      }
    }

    /// <inheritdoc cref="ResultOperatorBase.ExecuteInMemory" />
    public override StreamedValue ExecuteInMemory<T> (StreamedSequence input)
    {
      ArgumentUtility.CheckNotNull ("input", input);

      var sequence = input.GetTypedSequence<T>();
      var funcLambda = ReverseResolvingExpressionTreeVisitor.ReverseResolveLambda (input.DataInfo.ItemExpression, Func, 1);
      var func = (Func<T, T, T>) funcLambda.Compile ();
      var result = sequence.Aggregate (func);
      return new StreamedValue (result, (StreamedValueInfo) GetOutputDataInfo (input.DataInfo));
    }

    /// <inheritdoc />
    public override ResultOperatorBase Clone (CloneContext cloneContext)
    {
      return new AggregateResultOperator (Func);
    }

    /// <inheritdoc />
    public override IStreamedDataInfo GetOutputDataInfo (IStreamedDataInfo inputInfo)
    {
      var sequenceInfo = ArgumentUtility.CheckNotNullAndType<StreamedSequenceInfo> ("inputInfo", inputInfo);

      var expectedItemType = GetExpectedItemType();
      CheckSequenceItemType (sequenceInfo, expectedItemType);

      return new StreamedScalarValueInfo (Func.Body.Type);
    }

    public override void TransformExpressions (Func<Expression, Expression> transformation)
    {
      ArgumentUtility.CheckNotNull ("transformation", transformation);
      Func = (LambdaExpression) transformation (Func);
    }

    /// <inheritdoc />
    public override string ToString ()
    {
      return "Aggregate(" + FormattingExpressionTreeVisitor.Format (Func) + ")";
    }

    private bool DescribesValidFuncType (LambdaExpression value)
    {
      var funcType = value.Type;
      if (!funcType.IsGenericType || funcType.GetGenericTypeDefinition () != typeof (Func<,>))
        return false;

      var genericArguments = funcType.GetGenericArguments ();
      return genericArguments[0].IsAssignableFrom (genericArguments[1]);
    }

    private Type GetExpectedItemType ()
    {
      return Func.Type.GetGenericArguments ()[0];
    }
  }
}