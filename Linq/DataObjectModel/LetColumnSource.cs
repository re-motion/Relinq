using Rubicon.Utilities;

namespace Rubicon.Data.Linq.DataObjectModel
{
  // LetColumnSource
  public struct LetColumnSource : IColumnSource
  {
    public LetColumnSource (string alias, bool isTable) : this()
    {
      ArgumentUtility.CheckNotNull ("alias", alias);
      ArgumentUtility.CheckNotNull ("isTable", isTable);
      Alias = alias;
      IsTable = isTable;
    }

    public bool IsTable { get; private set; }

    public string Alias {get; private set; }
    public string AliasString { get { return Alias; }
    }
  }
}