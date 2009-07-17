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
using System.Collections.Generic;
using System.Linq;
using Remotion.Data.Linq.Clauses.ExecutionStrategies;
using Remotion.Utilities;

namespace Remotion.Data.Linq.Clauses.ResultOperators
{
  /// <summary>
  /// Represents the take part of a query. This is a result operator, operating on the whole result set of a query.
  /// </summary>
  /// <example>
  /// In C#, the "take" clause in the following example corresponds to a <see cref="TakeResultOperator"/>.
  /// <code>
  /// var query = (from s in Students
  ///              select s).Take(3);
  /// </code>
  /// </example>
  public class TakeResultOperator : ResultOperatorBase
  {
    private string _itemName;
    
    /// <summary>
    /// Initializes a new instance of the <see cref="TakeResultOperator"/>.
    /// </summary>
    /// <param name="count">The number of elements which should be returned.</param>
    public TakeResultOperator (int count)
        : base (CollectionExecutionStrategy.Instance)
    {
      Count = count;
    }

    public int Count { get; set; }
    
    public string ItemName
    {
      get { return _itemName; }
      set { _itemName = ArgumentUtility.CheckNotNullOrEmpty ("value", value); }
    }

    public override ResultOperatorBase Clone (CloneContext cloneContext)
    {
      return new TakeResultOperator (Count);
    }

    public override object ExecuteInMemory (object input)
    {
      ArgumentUtility.CheckNotNull ("input", input);
      return InvokeGenericOnEnumerable<IEnumerable<object>> (input, ExecuteInMemory);
    }

    public IEnumerable<T> ExecuteInMemory<T> (IEnumerable<T> input)
    {
      ArgumentUtility.CheckNotNull ("input", input);
      return input.Take (Count);
    }

    public override Type GetResultType (Type inputResultType)
    {
      ArgumentUtility.CheckNotNull ("inputResultType", inputResultType);
      ReflectionUtility.GetItemTypeOfIEnumerable (inputResultType, "inputResultType");

      return inputResultType;
    }

    public override string ToString ()
    {
      return "Take(" + Count + ")";
    }
  }
}