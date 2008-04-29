using System;
using Rubicon.Utilities;

namespace Rubicon.Data.Linq.DataObjectModel
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