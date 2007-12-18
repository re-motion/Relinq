using Rubicon.Utilities;

namespace Rubicon.Data.Linq.DataObjectModel
{
  public struct BinaryCondition : ICondition
  {
    public enum ConditionKind { Equal, NotEqual, GreaterThan, GreaterThanOrEqual, LessThan, LessThanOrEqual }

    public readonly IValue Left;
    public readonly IValue Right;
    public readonly ConditionKind Kind;

    public BinaryCondition (IValue left, IValue right, ConditionKind kind)
    {
      ArgumentUtility.CheckNotNull ("left", left);
      ArgumentUtility.CheckNotNull ("right", right);
      ArgumentUtility.CheckNotNull ("kind", kind);

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