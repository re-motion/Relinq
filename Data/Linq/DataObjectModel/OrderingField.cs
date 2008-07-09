/* Copyright (C) 2005 - 2008 rubicon informationstechnologie gmbh
 *
 * This program is free software: you can redistribute it and/or modify it under 
 * the terms of the re:motion license agreement in license.txt. If you did not 
 * receive it, please visit http://www.re-motion.org/licensing.
 * 
 * Unless otherwise provided, this software is distributed on an "AS IS" basis, 
 * WITHOUT WARRANTY OF ANY KIND, either express or implied. 
 */

using Remotion.Data.Linq.Clauses;

namespace Remotion.Data.Linq.DataObjectModel
{
  public struct OrderingField
  {
    public readonly FieldDescriptor FieldDescriptor;
    public readonly OrderDirection Direction;

    public OrderingField (FieldDescriptor fieldDescriptor, OrderDirection direction)
    {
      fieldDescriptor.GetMandatoryColumn (); // assert that there is a column for ordering

      FieldDescriptor = fieldDescriptor;
      Direction = direction;
    }

    public Column Column
    {
      get { return FieldDescriptor.GetMandatoryColumn(); }
    }

    public override string ToString ()
    {
      return FieldDescriptor.ToString()+ " " + Direction.ToString();
    }
  }
}
