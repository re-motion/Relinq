using Rubicon.Data.Linq.Clauses;
using Rubicon.Data.Linq.DataObjectModel;
using Rubicon.Utilities;

namespace Rubicon.Data.Linq.DataObjectModel
{
  public class FieldDescriptor
  {
    public FieldDescriptor (Column column, FromClauseBase fromClause)
    {
      ArgumentUtility.CheckNotNull ("fromClause", fromClause);
      Column = column;
      FromClause = fromClause;
    }

    public Column Column { get; private set; }
    public FromClauseBase FromClause { get; private set; }
  }
}