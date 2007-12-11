using Rubicon.Utilities;

namespace Rubicon.Data.Linq.SqlGeneration.ObjectModel
{
  public struct Column : IValue
  {
    public readonly string Name;
    public readonly string TableAlias;

    public Column (string tableAlias,string name)
    {
      ArgumentUtility.CheckNotNull ("name", name);
      ArgumentUtility.CheckNotNull ("tableAlias", tableAlias);
      Name = name;
      TableAlias = tableAlias;
    }

    
  }
}