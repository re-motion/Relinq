using Rubicon.Utilities;

namespace Rubicon.Data.Linq.DataObjectModel
{
  public struct Column : IValue
  {
    public readonly string Name;
    public readonly Table Table;

    public Column (Table table,string name)
    {
      ArgumentUtility.CheckNotNull ("name", name);
      ArgumentUtility.CheckNotNull ("table", table);
      Name = name;
      Table = table;
    }
  }
}