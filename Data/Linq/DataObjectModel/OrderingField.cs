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
    private readonly FieldDescriptor _fieldDescriptor;
    private readonly OrderDirection _direction;

    public OrderingField (FieldDescriptor fieldDescriptor, OrderDirection direction)
    {
      fieldDescriptor.GetMandatoryColumn (); // assert that there is a column for ordering

      _fieldDescriptor = fieldDescriptor;
      _direction = direction;
    }

    public Column Column
    {
      get { return _fieldDescriptor.GetMandatoryColumn(); }
    }

    public FieldDescriptor FieldDescriptor
    {
      get { return _fieldDescriptor; }
    }

    public OrderDirection Direction
    {
      get { return _direction; }
    }

    public override string ToString ()
    {
      return _fieldDescriptor.ToString()+ " " + _direction.ToString();
    }
  }
}
