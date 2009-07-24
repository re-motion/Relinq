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
  /// Represents the first part of a query. This is a result operator, operating on the whole result set of a query.
  /// </summary>
  /// <example>
  /// In C#, the "first" clause in the following example corresponds to a <see cref="FirstResultOperator"/>.
  /// <code>
  /// var query = (from s in Students
  ///              select s).First();
  /// </code>
  /// </example>
  public class FirstResultOperator : ResultOperatorBase
  {
    /// <summary>
    /// Initializes a new instance of the <see cref="DistinctResultOperator"/>.
    /// </summary>
    /// <param name="returnDefaultWhenEmpty">The flag defines if a default expression should be regarded.</param>
    public FirstResultOperator (bool returnDefaultWhenEmpty)
        : base (returnDefaultWhenEmpty ? SingleExecutionStrategy.InstanceWithDefaultWhenEmpty : SingleExecutionStrategy.InstanceNoDefaultWhenEmpty)
    {
      ReturnDefaultWhenEmpty = returnDefaultWhenEmpty;
    }

    public bool ReturnDefaultWhenEmpty { get; set; }

    public override ResultOperatorBase Clone (CloneContext cloneContext)
    {
      return new FirstResultOperator (ReturnDefaultWhenEmpty);
    }

    public override IExecuteInMemoryData ExecuteInMemory (IExecuteInMemoryData input)
    {
      ArgumentUtility.CheckNotNull ("input", input);
      return InvokeGenericExecuteMethod<ExecuteInMemorySequenceData, ExecuteInMemoryValueData> (input, ExecuteInMemory<object>);
    }

    public ExecuteInMemoryValueData ExecuteInMemory<T> (ExecuteInMemorySequenceData input)
    {
      var sequence = input.GetCurrentSequenceInfo<T> ();
      if (ReturnDefaultWhenEmpty)
      {
        var result = sequence.Sequence.FirstOrDefault ();
        return new ExecuteInMemoryValueData (result);
      }
      else
      {
        var result = sequence.Sequence.First();
        return new ExecuteInMemoryValueData (result);
      }
    }

    public override Type GetResultType (Type inputResultType)
    {
      ArgumentUtility.CheckNotNull ("inputResultType", inputResultType);
      return ReflectionUtility.GetItemTypeOfIEnumerable (inputResultType, "inputResultType");
    }
    
    public override string ToString ()
    {
      if (ReturnDefaultWhenEmpty)
        return "FirstOrDefault()";
      else
        return "First()";
    }

  }
}