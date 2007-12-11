using Rubicon.Utilities;

namespace Rubicon.Data.Linq.SqlGeneration.ObjectModel
{
  public struct Table
  {
    public readonly string Name;
    public readonly string Alias;

    public Table(string name,string alias)
    {
      ArgumentUtility.CheckNotNull ("name", name);
      ArgumentUtility.CheckNotNull ("alias", alias);
      Name = name;
      Alias = alias;
    }
  }
}