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
using System.Reflection;
using Remotion.Data.Linq.Clauses.ExecutionStrategies;
using Remotion.Data.Linq.Parsing;
using Remotion.Utilities;

namespace Remotion.Data.Linq.Clauses.ResultOperators
{
  public class SumResultOperator : ResultOperatorBase
  {
    public SumResultOperator ()
        : base (ScalarExecutionStrategy.Instance)
    {
    }

    public override ResultOperatorBase Clone (CloneContext cloneContext)
    {
      return new SumResultOperator();
    }

    public override object ExecuteInMemory (object input)
    {
      ArgumentUtility.CheckNotNull ("input", input);

      var method = typeof (Enumerable).GetMethod ("Sum", BindingFlags.Public | BindingFlags.Static, null, new[] { input.GetType() }, null);
      if (method == null)
      {
        var message = string.Format ("Cannot calculate the sum of an object of type '{0}' in memory.", input.GetType().FullName);
        throw new NotSupportedException (message);
      }
      return method.Invoke (null, new[] { input });
    }

    public override Type GetResultType (Type inputResultType)
    {
      ArgumentUtility.CheckNotNull ("inputResultType", inputResultType);
      return ReflectionUtility.GetItemTypeOfIEnumerable (inputResultType, "inputResultType");
    }

    public override string ToString ()
    {
      return "Sum()";
    }
  }
}