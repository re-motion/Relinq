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
