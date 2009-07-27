// This file is part of the re-motion Core Framework (www.re-motion.org)
// Copyright (C) 2005-2009 rubicon informationstechnologie gmbh, www.rubicon.eu
// 
// The re-motion Core Framework is free software; you can redistribute it 
// and/or modify it under the terms of the GNU Lesser General Public License 
// version 3.0 as published by the Free Software Foundation.
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
using Remotion.Data.Linq.Clauses.ExecutionStrategies;
using Remotion.Data.Linq.Clauses.StreamedData;
using Remotion.Utilities;

namespace Remotion.Data.Linq.Clauses.ResultOperators
{
  /// <summary>
  /// Represents the distinct part of a query. This is a result operator, operating on the whole result set of a query.
  /// </summary>
  /// <example>
  /// In C#, the "distinct" clause in the following example corresponds to a <see cref="DistinctResultOperator"/>.
  /// <code>
  /// var query = (from s in Students
  ///              select s).Distinct();
  /// </code>
  /// </example>
  public class DistinctResultOperator : ResultOperatorBase
  {
    /// <summary>
    /// Initializes a new instance of the <see cref="DistinctResultOperator"/>.
    /// </summary>
    public DistinctResultOperator ()
        : base (CollectionExecutionStrategy.Instance)
    {
    }

    public override ResultOperatorBase Clone (CloneContext cloneContext)
    {
      return new DistinctResultOperator();
    }

    public override IStreamedData ExecuteInMemory (IStreamedData input)
    {
      ArgumentUtility.CheckNotNull ("input", input);
      return InvokeGenericExecuteMethod<StreamedSequence, StreamedSequence> (input, ExecuteInMemory<object>);
    }

    public StreamedSequence ExecuteInMemory<T> (StreamedSequence input)
    {
      var sequence = input.GetCurrentSequenceInfo<T> ();
      var result = sequence.Sequence.Distinct();
      return new StreamedSequence (result, sequence.ItemExpression);
    }

    public override Type GetResultType (Type inputResultType)
    {
      ArgumentUtility.CheckNotNull ("inputResultType", inputResultType);
      ReflectionUtility.GetItemTypeOfIEnumerable (inputResultType, "inputResultType"); // check whether inputResultType implements IEnumerable<T>

      return inputResultType;
    }

    public override string ToString ()
    {
      return "Distinct()";
    }
  }
}