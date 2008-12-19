// This file is part of the re-motion Core Framework (www.re-motion.org)
// Copyright (C) 2005-2008 rubicon informationstechnologie gmbh, www.rubicon.eu
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
