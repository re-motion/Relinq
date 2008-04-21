using Rubicon.Data.Linq.Clauses;
using Rubicon.Utilities;

namespace Rubicon.Data.Linq.DataObjectModel
{
  public struct OrderingField
  {
    public readonly FieldDescriptor FieldDescriptor;
    public readonly OrderDirection Direction;

    public OrderingField (FieldDescriptor fieldDescriptor, OrderDirection direction)
    {
      fieldDescriptor.GetMandatoryColumn (); // assert that there is a column for ordering

      FieldDescriptor = fieldDescriptor;
      Direction = direction;
    }

    public Column Column
    {
      get { return FieldDescriptor.GetMandatoryColumn(); }
    }

    public override string ToString ()
    {
      return FieldDescriptor.ToString()+ " " + Direction.ToString();
    }
  }
}