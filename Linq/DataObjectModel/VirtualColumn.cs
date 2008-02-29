using Rubicon.Utilities;

namespace Rubicon.Data.Linq.DataObjectModel
{
  /// <summary>
  /// Represents the virtual side of a foreign key relationship, ie. the side that does not contain the foreign key.
  /// </summary>
  public struct VirtualColumn : IColumn
  {
    public Column PrimaryKeyColumn { get; private set; }
    public Column OppositeForeignKeyColumn { get; private set; }

    public VirtualColumn (Column primaryKeyColumn, Column oppositeForeignKeyColumn) : this()
    {
      PrimaryKeyColumn = primaryKeyColumn;
      OppositeForeignKeyColumn = oppositeForeignKeyColumn;
    }

    public override string ToString ()
    {
      return string.Format ("{0}->{1}", PrimaryKeyColumn, OppositeForeignKeyColumn);
    }
  }
}