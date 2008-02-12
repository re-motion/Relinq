using Rubicon.Utilities;

namespace Rubicon.Data.Linq.DataObjectModel
{
  public struct Join : IFieldSource
  {
    public Table LeftSide { get; private set; }
    public IFieldSource RightSide { get; private set; }
    public Column LeftColumn { get; private set; }
    public Column RightColumn { get; private set; }

    public Join (Table leftSide, IFieldSource rightSide, Column leftColumn, Column rightColumn) : this()
    {
      ArgumentUtility.CheckNotNull ("leftSide", leftSide);
      ArgumentUtility.CheckNotNull ("rightSide", rightSide);
      ArgumentUtility.CheckNotNull ("leftColumn", leftColumn);
      ArgumentUtility.CheckNotNull ("rightColumn", rightColumn);

      LeftSide = leftSide;
      RightSide = rightSide;
      LeftColumn = leftColumn;
      RightColumn = rightColumn;
    }

    public override string ToString ()
    {
      return string.Format ("({0} inner join {1} on {2} = {3})", LeftSide, RightSide, LeftColumn, RightColumn);
    }
  }
}