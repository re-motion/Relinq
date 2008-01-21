using Rubicon.Data.Linq.Clauses;
using Rubicon.Utilities;

namespace Rubicon.Data.Linq.DataObjectModel
{
  public struct OrderingField : ICriterion
  {
    public readonly Column Column;
    public readonly OrderDirection Direction;

    public OrderingField(Column field,OrderDirection direction)
    {
      ArgumentUtility.CheckNotNull ("field", field);
      ArgumentUtility.CheckNotNull ("direction", direction);

      Column = field;
      Direction = direction;

    }

    public override string ToString ()
    {
      return Column.ToString()+ " " + Direction.ToString();
    }
  }
}