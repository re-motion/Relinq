using Rubicon.Utilities;

namespace Rubicon.Data.Linq.SqlGeneration.ObjectModel
{
  public struct Constant : IValue
  {
    public readonly object Value;

    public Constant (object value)
    {
      ArgumentUtility.CheckNotNull ("value", value);

      Value = value;
    }
  }
}