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
  public struct BinaryEvaluation : IEvaluation
  {
    public enum EvaluationKind { Add, Divide, Modulo, Multiply, Subtract }

    public BinaryEvaluation (IEvaluation left, IEvaluation right, EvaluationKind kind) : this()
    {
      ArgumentUtility.CheckNotNull ("left", left);
      ArgumentUtility.CheckNotNull ("right", right);
      ArgumentUtility.CheckNotNull ("kind", kind);

      Left = left;
      Right = right;
      Kind = kind;
    }

    public IEvaluation Left { get; private set; }
    public IEvaluation Right { get; private set; }
    public EvaluationKind Kind { get; private set; }

    public void Accept (IEvaluationVisitor visitor)
    {
      ArgumentUtility.CheckNotNull ("visitor", visitor);
      visitor.VisitBinaryEvaluation (this);
    }
  }
}
