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
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Remotion.Data.Linq.Clauses.ExecutionStrategies;
using Remotion.Utilities;

namespace Remotion.Data.Linq.Clauses.ResultOperators
{
  public class OfTypeResultOperator: ResultOperatorBase
  {
    private Type _ofTypeItem;

    public OfTypeResultOperator (Type ofTypeItemType)
        : base (CollectionExecutionStrategy.Instance)
    {
      ArgumentUtility.CheckNotNull ("ofTypeItemType", ofTypeItemType);
      OfTypeItem = ofTypeItemType;
    }

    public Type OfTypeItem
    {
      get { return _ofTypeItem; }
      set
      {
        ArgumentUtility.CheckNotNull ("value", value);
        _ofTypeItem = value;
      }
    }

    public override ResultOperatorBase Clone (CloneContext cloneContext)
    {
      return new OfTypeResultOperator (OfTypeItem);
    }

    public override object ExecuteInMemory (object input)
    {
      ArgumentUtility.CheckNotNull ("input", input);

      var method = (from m in typeof (OfTypeResultOperator).GetMethods ()
                    where m.Name == "ExecuteInMemory" && m.IsGenericMethod
                    select m.MakeGenericMethod (OfTypeItem)).Single ();
      return InvokeExecuteMethod (input, method);
    }

    public IEnumerable<TResult> ExecuteInMemory<TResult> (IEnumerable input)
    {
      return input.OfType<TResult> ();
    }

    public override Type GetResultType (Type inputResultType)
    {
      ArgumentUtility.CheckNotNull ("inputResultType", inputResultType);
      ReflectionUtility.GetItemTypeOfIEnumerable (inputResultType, "inputResultType"); // check whether inputResultType implements IEnumerable<T>

      return typeof (IQueryable<>).MakeGenericType (OfTypeItem);
    }

    public override string ToString ()
    {
      return "OfType()";
    }
    
  }
}