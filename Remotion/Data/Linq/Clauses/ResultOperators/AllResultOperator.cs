// This file is part of the re-motion Core Framework (www.re-motion.org)
// Copyright (C) 2005-2009 rubicon informationstechnologie gmbh, www.rubicon.eu
// 
// The re-motion Core Framework is free software; you can redistribute it 
// and/or modify it under the terms of the GNU Lesser General Public License 
// as published by the Free Software Foundation; either version 2.1 of the 
// License, or (at your option) All later version.
// 
// re-motion is distributed in the hope that it will be useful, 
// but WITHOUT All WARRANTY; without even the implied warranty of 
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the 
// GNU Lesser General Public License for more details.
// 
// You should have received a copy of the GNU Lesser General Public License
// along with re-motion; if not, see http://www.gnu.org/licenses.
// 
using System;
using System.Linq;
using System.Linq.Expressions;
using Remotion.Data.Linq.Clauses.ExpressionTreeVisitors;
using Remotion.Data.Linq.Clauses.StreamedData;
using Remotion.Utilities;

namespace Remotion.Data.Linq.Clauses.ResultOperators
{
  /// <summary>
  /// Represents the "all" part of a query. This is a result operator, operating on the whole result set of a query.
  /// </summary>
  /// <example>
  /// In C#, the "All" clause in the following example corresponds to an <see cref="AllResultOperator"/>.
  /// <code>
  /// var result = (from s in Students
  ///              select s).All();
  /// </code>
  /// </example>
  public class AllResultOperator : ValueFromSequenceResultOperatorBase
  {
    private Expression _predicate;

    /// <summary>
    /// Initializes a new instance of the <see cref="AllResultOperator"/> class.
    /// </summary>
    /// <param name="predicate">The predicate to evaluate. This is a resolved version of the body of the <see cref="LambdaExpression"/> that would be 
    /// passed to <see cref="Queryable.All{TSource}"/>.</param>
    public AllResultOperator (Expression predicate)
    {
      ArgumentUtility.CheckNotNull ("predicate", predicate);
      Predicate = predicate;
    }

    /// <summary>
    /// Gets or sets the predicate to evaluate on all items in the sequence.
    /// This is a resolved version of the body of the <see cref="LambdaExpression"/> that would be 
    /// passed to <see cref="Queryable.All{TSource}"/>.
    /// </summary>
    /// <value>The predicate.</value>
    public Expression Predicate
    {
      get { return _predicate; }
      set { _predicate = ArgumentUtility.CheckNotNull ("value", value); }
    }

    public override StreamedValue ExecuteInMemory<T> (StreamedSequence input)
    {
      ArgumentUtility.CheckNotNull ("input", input);

      var sequence = input.GetTypedSequence<T> ();

      var predicateLambda = ReverseResolvingExpressionTreeVisitor.ReverseResolve (input.DataInfo.ItemExpression, Predicate);
      var predicate = (Func<T, bool>) predicateLambda.Compile ();

      var result = sequence.All (predicate);
      return new StreamedValue (result, (StreamedValueInfo) GetOutputDataInfo (input.DataInfo));
    }

    public override ResultOperatorBase Clone (CloneContext cloneContext)
    {
      return new AllResultOperator (Predicate);
    }

    public override void TransformExpressions (Func<Expression, Expression> transformation)
    {
      ArgumentUtility.CheckNotNull ("transformation", transformation);
      Predicate = transformation (Predicate);
    }

    public override IStreamedDataInfo GetOutputDataInfo (IStreamedDataInfo inputInfo)
    {
      ArgumentUtility.CheckNotNullAndType<StreamedSequenceInfo> ("inputInfo", inputInfo);
      return new StreamedScalarValueInfo (typeof (bool));
    }

    public override string ToString ()
    {
      return "All(" + FormattingExpressionTreeVisitor.Format (Predicate) + ")";
    }
  }
}
