using Rubicon.Utilities;

namespace Rubicon.Data.Linq.SqlGeneration.ObjectModel
{
  public class BinaryCondition : Condition
  {
    public enum ConditionKind { Equal }

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
  }
}