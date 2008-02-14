using Rubicon.Utilities;

namespace Rubicon.Data.Linq.DataObjectModel
{
  public struct Table : IFieldSourcePath
  {
    public readonly string Name;
    public readonly string Alias;


    public Table(string name,string alias)
    {
      ArgumentUtility.CheckNotNull ("name", name);
      Name = name;
      Alias = alias;
    }

    public override string ToString ()
    {
      return Name + " (" + Alias + ")";
    }

    public Table GetStartingTable ()
    {
      return this;
    }
  }
}