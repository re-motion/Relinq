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
using System.Linq.Expressions;
using Remotion.Data.Linq.Clauses.ExecutionStrategies;
using Remotion.Utilities;
using System.Linq;

namespace Remotion.Data.Linq.Clauses.ResultOperators
{
  /// <summary>
  /// Represents the contains part of a query. This is a result operator, operating on the whole result set of a query.
  /// </summary>
  /// <example>
  /// In C#, the "contains" clause in the following example corresponds to a <see cref="ContainsResultOperator"/>.
  /// <code>
  /// var query = (from s in Students
  ///              select s).Contains(student);
  /// </code>
  /// </example>
  public class ContainsResultOperator : ResultOperatorBase
  {
    private object _item;

    public ContainsResultOperator (object item)
        : base (ScalarExecutionStrategy.Instance)
    {
      Item = item;
    }

    public object Item
    {
      get { return _item; }
      set
      {
        ArgumentUtility.CheckNotNull ("value", value);
        _item = value;
      }
    }

    public override ResultOperatorBase Clone (CloneContext cloneContext)
    {
      return new ContainsResultOperator (Item);
    }

    public override object ExecuteInMemory (object input)
    {
      ArgumentUtility.CheckNotNull ("input", input);
      return InvokeGenericOnEnumerable<bool> (input, ExecuteInMemory);
    }

    public bool ExecuteInMemory (IEnumerable<object> input)
    {
      ArgumentUtility.CheckNotNull ("input", input);
      return input.Contains (Item);
    }

    public override Type GetResultType (Type inputResultType)
    {
      ArgumentUtility.CheckNotNull ("inputResultType", inputResultType);
      return typeof (bool);
    }

    public override string ToString ()
    {
      return "Contains(" + Item + ")";
    }

     
  }
}