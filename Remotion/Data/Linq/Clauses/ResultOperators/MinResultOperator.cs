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
  /// Represents the min part of a query. This is a result operator, operating on the whole result set of a query.
  /// </summary>
  /// <example>
  /// In C#, the "min" clause in the following example corresponds to a <see cref="MinResultOperator"/>.
  /// <code>
  /// var query = (from s in Students
  ///              select s.ID).Min();
  /// </code>
  /// </example>
  public class MinResultOperator : ResultOperatorBase
  {
    /// <summary>
    /// Initializes a new instance of the <see cref="MinResultOperator"/>.
    /// </summary>
    public MinResultOperator ()
        : base (ScalarExecutionStrategy.Instance)
    {
    }

    public override ResultOperatorBase Clone (CloneContext cloneContext)
    {
      return new MinResultOperator();
    }

    public override Type GetResultType (Type inputResultType)
    {
      ArgumentUtility.CheckNotNull ("inputResultType", inputResultType);
      return ReflectionUtility.GetItemTypeOfIEnumerable (inputResultType, "inputResultType");
    }

    public override IExecuteInMemoryData ExecuteInMemory (IExecuteInMemoryData input)
    {
      ArgumentUtility.CheckNotNull ("input", input);
      return InvokeGenericExecuteMethod<ExecuteInMemorySequenceData, ExecuteInMemoryValueData> (input, ExecuteInMemory<object>);
    }

    public ExecuteInMemoryValueData ExecuteInMemory<T> (ExecuteInMemorySequenceData input)
    {
      var sequence = input.GetCurrentSequenceInfo<T> ();
      var result = sequence.Sequence.Min ();
      return new ExecuteInMemoryValueData (result);
    }

    public override string ToString ()
    {
      return "Min()";
    }
  }
}