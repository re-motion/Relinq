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
  public struct BinaryCondition : ICriterion
  {
    public enum ConditionKind { Equal, NotEqual, GreaterThan, GreaterThanOrEqual, LessThan, LessThanOrEqual, Like, Contains, ContainsFulltext }

    public readonly IValue Left;
    public readonly IValue Right;
    public readonly ConditionKind Kind;
 
    public BinaryCondition (IValue left, IValue right, ConditionKind kind)
    {
      ArgumentUtility.CheckNotNull ("left", left);
      ArgumentUtility.CheckNotNull ("right", right);
      ArgumentUtility.CheckNotNull ("kind", kind);

      if (kind == ConditionKind.Contains)
        ArgumentUtility.CheckType<SubQuery> ("left", left);

      Left = left;
      Kind = kind;
      Right = right;
    }


    public override string ToString ()
    {
      return "(" + Left + " " + Kind + " " + Right + ")";
    }

    public void Accept (IEvaluationVisitor visitor)
    {
      ArgumentUtility.CheckNotNull ("visitor", visitor);
      visitor.VisitBinaryCondition (this);
    }
  }
}
