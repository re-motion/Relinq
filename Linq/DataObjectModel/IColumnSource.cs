namespace Rubicon.Data.Linq.DataObjectModel
{
  public interface IColumnSource
  {
    string Alias { get; }
    string AliasString { get; }
  }
}