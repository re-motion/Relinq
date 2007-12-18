namespace Rubicon.Data.Linq.DataObjectModel
{
  public struct Constant : IValue, ICriterion
  {
    public readonly object Value;

    public Constant (object value)
    {
      Value = value;
    }

    public override string ToString ()
    {
      return Value.ToString();
    }
  }
}