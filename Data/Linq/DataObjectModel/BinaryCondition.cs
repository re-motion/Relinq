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
  public struct BinaryCondition : ICriterion
  {
    public enum ConditionKind { Equal, NotEqual, GreaterThan, GreaterThanOrEqual, LessThan, LessThanOrEqual, Like, Contains, ContainsFulltext }

    private readonly IValue _left;
    private readonly IValue _right;
    private readonly ConditionKind _kind;
 
    public BinaryCondition (IValue left, IValue right, ConditionKind kind)
    {
      ArgumentUtility.CheckNotNull ("left", left);
      ArgumentUtility.CheckNotNull ("right", right);
      ArgumentUtility.CheckNotNull ("kind", kind);

      _left = left;
      _kind = kind;
      _right = right;
    }

    public IValue Left
    {
      get { return _left; }
    }

    public IValue Right
    {
      get { return _right; }
    }

    public ConditionKind Kind
    {
      get { return _kind; }
    }
    
    public override string ToString ()
    {
      return "(" + _left + " " + _kind + " " + _right + ")";
    }

    public void Accept (IEvaluationVisitor visitor)
    {
      ArgumentUtility.CheckNotNull ("visitor", visitor);
      visitor.VisitBinaryCondition (this);
    }
  }
}
