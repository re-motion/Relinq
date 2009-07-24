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
using Remotion.Utilities;

namespace Remotion.Data.Linq.Clauses.ResultOperators
{
  /// <summary>
  /// Represents the reverse part of a query. This is a result operator, operating on the whole result set of a query.
  /// </summary>
  /// <example>
  /// In C#, the "reverse" clause in the following example corresponds to a <see cref="ReverseResultOperator"/>.
  /// <code>
  /// var query = (from s in Students
  ///              select s).Reverse();
  /// </code>
  /// </example>
  public class ReverseResultOperator : ResultOperatorBase
  {
    public ReverseResultOperator ()
        : base (CollectionExecutionStrategy.Instance)
    {
    }

    public override ResultOperatorBase Clone (CloneContext cloneContext)
    {
      return new ReverseResultOperator ();
    }

    public override IExecuteInMemoryData ExecuteInMemory (IExecuteInMemoryData input)
    {
      ArgumentUtility.CheckNotNull ("input", input);
      return InvokeGenericExecuteMethod<ExecuteInMemorySequenceData, ExecuteInMemorySequenceData> (input, ExecuteInMemory<object>);
    }

    public ExecuteInMemorySequenceData ExecuteInMemory<T> (ExecuteInMemorySequenceData input)
    {
      var sequence = input.GetCurrentSequenceInfo<T> ();
      var result = sequence.Sequence.Reverse();
      return new ExecuteInMemorySequenceData (result, sequence.ItemExpression);
    }

    public override Type GetResultType (Type inputResultType)
    {
      ArgumentUtility.CheckNotNull ("inputResultType", inputResultType);
      ReflectionUtility.GetItemTypeOfIEnumerable (inputResultType, "inputResultType"); // check whether inputResultType implements IEnumerable<T>

      return inputResultType;
    }

    public override string ToString ()
    {
      return "Reverse()";
    }
  }
}