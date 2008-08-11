/* Copyright (C) 2005 - 2008 rubicon informationstechnologie gmbh
 *
 * This program is free software: you can redistribute it and/or modify it under 
 * the terms of the re:motion license agreement in license.txt. If you did not 
 * receive it, please visit http://www.re-motion.org/licensing.
 * 
 * Unless otherwise provided, this software is distributed on an "AS IS" basis, 
 * WITHOUT WARRANTY OF ANY KIND, either express or implied. 
 */

using Remotion.Utilities;

namespace Remotion.Data.Linq.DataObjectModel
{
  public struct Constant : ICriterion
  {
    private readonly object _value;

    public Constant (object value)
        : this()
    {
      _value = value;
    }

    public object Value
    {
      get { return _value; }
    }

    public override string ToString ()
    {
      return _value != null ? _value.ToString() : "<null>";
    }

    public void Accept (IEvaluationVisitor visitor)
    {
      ArgumentUtility.CheckNotNull ("visitor", visitor);
      visitor.VisitConstant (this);
    }
  }
}
