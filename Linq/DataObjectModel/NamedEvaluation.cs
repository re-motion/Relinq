using Rubicon.Utilities;

namespace Rubicon.Data.Linq.DataObjectModel
{
  public struct NamedEvaluation : IColumnSource
  {
    public NamedEvaluation (string alias, string aliasString) : this()
    {
      ArgumentUtility.CheckNotNull ("alias", alias);
      ArgumentUtility.CheckNotNull ("aliasString", aliasString);

      Alias = alias;
      AliasString = aliasString;
    }

    public string Alias {get; private set; }

    public string AliasString { get; private set; }
  }
}