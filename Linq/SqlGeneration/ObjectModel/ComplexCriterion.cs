using Rubicon.Utilities;

namespace Rubicon.Data.Linq.SqlGeneration.ObjectModel
{
  
  public class ComplexCriterion : Criterion
  {
    public enum JunctionKind { And, Or }
    
    public readonly Criterion Left;
    public readonly Criterion Right;
    public readonly JunctionKind Kind;

    public ComplexCriterion (Criterion left, Criterion right, JunctionKind kind)
    {
      ArgumentUtility.CheckNotNull ("kind", kind);
      ArgumentUtility.CheckNotNull ("left", left);
      ArgumentUtility.CheckNotNull ("right", right);

      
      Left = left;
      Kind = kind;
      Right = right;
    }
  }
}