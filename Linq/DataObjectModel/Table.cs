using Rubicon.Utilities;

namespace Rubicon.Data.Linq.DataObjectModel
{
  public class Table : IFieldSourcePath
  {
    public readonly string Name;
    public readonly string Alias;

    public Table ()
    {
    }

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

    public override bool Equals (object obj)
    {
      Table other = obj as Table;
      return other != null && other.Name == Name && other.Alias == Alias;
    }

    public override int GetHashCode ()
    {
      return EqualityUtility.GetRotatedHashCode (Name, Alias);
    }
  }
}