using Rubicon.Utilities;

namespace Rubicon.Data.Linq.DataObjectModel
{
  public struct Column : ICriterion
  {
    public readonly string Name;
    public readonly IFromSource FromSource;

    public Column (IFromSource fromSource, string name)
    {
      ArgumentUtility.CheckNotNull ("name", name);
      ArgumentUtility.CheckNotNull ("fromSource", fromSource);
      Name = name;
      FromSource = fromSource;
    }

    public override string ToString ()
    {
      return (FromSource != null ? FromSource.AliasString : "<null>") + "." + Name;
    }
  }
}