using Rubicon.Utilities;

namespace Rubicon.Data.Linq.DataObjectModel
{
  public struct ComplexCriterion : ICriterion
  {
    public enum JunctionKind { And, Or }
    
    public readonly ICriterion Left;
    public readonly ICriterion Right;
    public readonly JunctionKind Kind;

    public ComplexCriterion (ICriterion left, ICriterion right, JunctionKind kind)
    {
      ArgumentUtility.CheckNotNull ("kind", kind);
      ArgumentUtility.CheckNotNull ("left", left);
      ArgumentUtility.CheckNotNull ("right", right);

      Left = left;
      Kind = kind;
      Right = right;
    }

    public override string ToString ()
    {
      return "(" + Left + " " + Kind + " " + Right + ")";
    }
  }
}