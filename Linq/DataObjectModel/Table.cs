using Rubicon.Utilities;

namespace Rubicon.Data.Linq.DataObjectModel
{
  public struct Table : IFieldSource
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

    public override string ToString ()
    {
      return Name + " (" + Alias + ")";
    }
  }
}