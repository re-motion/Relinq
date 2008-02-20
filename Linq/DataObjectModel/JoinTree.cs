using System;
using System.Collections.Generic;
using Rubicon.Utilities;

namespace Rubicon.Data.Linq.DataObjectModel
{
  public struct JoinTree : IFieldSourcePath
  {
    public Table LeftSide { get; private set; }
    public IFieldSourcePath RightSide { get; private set; }
    public Column LeftColumn { get; private set; }
    public Column RightColumn { get; private set; }

    public JoinTree (Table leftSide, IFieldSourcePath rightSide, Column leftColumn, Column rightColumn) : this()
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

    public Table GetStartingTable ()
    {
      return RightSide.GetStartingTable();
    }

    public SingleJoin GetSingleJoinForRoot()
    {
      return new SingleJoin (LeftColumn, RightColumn);
    }

    public IEnumerable<SingleJoin> GetAllSingleJoins ()
    {
      yield return GetSingleJoinForRoot();

      IFieldSourcePath currentTreeElement = RightSide;
      while (currentTreeElement is JoinTree)
      {
        JoinTree currentJoin = (JoinTree) currentTreeElement;
        yield return currentJoin.GetSingleJoinForRoot();
        currentTreeElement = currentJoin.RightSide;
      }
    }
  }
}