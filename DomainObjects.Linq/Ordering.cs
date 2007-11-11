using System;
using System.Linq.Expressions;

namespace Rubicon.Data.DomainObjects.Linq
{
  public class Ordering
  {
    public enum OrderDirection
    {
      Asc,
      Desc
    } ;

    private readonly Expression _expression;
    private readonly OrderDirection _directionAsc;
    
    public Ordering (Expression expression, OrderDirection direction)
    {
      _expression = expression;
      _directionAsc = direction;
    }


    public Expression Expression
    {
      get { return _expression; }
    }


  }
}