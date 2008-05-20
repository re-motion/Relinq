namespace Remotion.Data.Linq.DataObjectModel
{
  public interface IColumnSource
  {
    string Alias { get; }
    string AliasString { get; }
    bool IsTable { get; }
  }
}