// This file is part of the re-motion Core Framework (www.re-motion.org)
// Copyright (C) 2005-2009 rubicon informationstechnologie gmbh, www.rubicon.eu
// 
// The re-motion Core Framework is free software; you can redistribute it 
// and/or modify it under the terms of the GNU Lesser General Public License 
// as published by the Free Software Foundation; either version 2.1 of the 
// License, or (at your option) any later version.
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

namespace Remotion.Data.Linq.SqlBackend.SqlGeneration.BooleanSemantics
{
  /// <summary>
  /// Holds the current <see cref="BooleanSemanticsKind"/> value and allows to temporarily switch the value.
  /// </summary>
  public class BooleanSemanticsHolder
  {
    private BooleanSemanticsKind _currentValue;

    public BooleanSemanticsHolder (BooleanSemanticsKind initialValue)
    {
      _currentValue = initialValue;
    }

    public BooleanSemanticsKind CurrentValue
    {
      get { return _currentValue;  }
    }

    public IDisposable SwitchTo (BooleanSemanticsKind newValue)
    {
      var originalValue = _currentValue;
      _currentValue = newValue;
      return new OriginalValueStore(this, originalValue);
    }

    class OriginalValueStore : IDisposable
    {
      private readonly BooleanSemanticsHolder _holder;
      private readonly BooleanSemanticsKind _originalValue;

      public OriginalValueStore (BooleanSemanticsHolder holder, BooleanSemanticsKind originalValue)
      {
        _holder = holder;
        _originalValue = originalValue;
      }

      public void Dispose ()
      {
        _holder._currentValue = _originalValue;
      }
    }

  }
}