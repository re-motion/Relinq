using Rubicon.Utilities;

namespace Rubicon.Data.Linq.DataObjectModel
{
  public struct BinaryCondition : ICriterion
  {
    public enum ConditionKind { Equal, NotEqual, GreaterThan, GreaterThanOrEqual, LessThan, LessThanOrEqual, Like, Contains }

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
  }
}