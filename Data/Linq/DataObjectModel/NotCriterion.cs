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
  public struct NotCriterion : ICriterion
  {
    private readonly ICriterion _negatedCriterion;

    public NotCriterion (ICriterion negatedCriterion)
    {
      ArgumentUtility.CheckNotNull ("negatedCriterion", negatedCriterion);
      _negatedCriterion = negatedCriterion;
    }

    public ICriterion NegatedCriterion
    {
      get { return _negatedCriterion; }
    }

    public override string ToString ()
    {
      return "NOT (" + _negatedCriterion + ")";
    }

    public void Accept (IEvaluationVisitor visitor)
    {
      ArgumentUtility.CheckNotNull ("visitor", visitor);
      visitor.VisitNotCriterion (this);
    }

  }
}
