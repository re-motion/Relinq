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
using Remotion.Utilities;

namespace Remotion.Data.Linq.DataObjectModel
{
  public struct ComplexCriterion : ICriterion
  {
    public enum JunctionKind { And, Or }

    public ComplexCriterion (ICriterion left, ICriterion right, JunctionKind kind) : this()
    {
      ArgumentUtility.CheckNotNull ("kind", kind);
      ArgumentUtility.CheckNotNull ("left", left);
      ArgumentUtility.CheckNotNull ("right", right);

      Left = left;
      Kind = kind;
      Right = right;
    }

    public ICriterion Left { get; private set; }
    public ICriterion Right { get; private set; }
    public JunctionKind Kind { get; private set; }

    public override string ToString ()
    {
      return "(" + Left + " " + Kind + " " + Right + ")";
    }

    public void Accept (IEvaluationVisitor visitor)
    {
      ArgumentUtility.CheckNotNull ("visitor", visitor);
      visitor.VisitComplexCriterion (this);
    }
  }
}
