using System.Collections.Generic;
using System.Linq.Expressions;
using Remotion.Data.Linq.DataObjectModel;

namespace Remotion.Data.Linq.Parsing.Details
{
  public interface IParser<TExpression> where TExpression : Expression
  {
    bool CanParse (TExpression expression);
  }

  public interface IParser
  {
  }
}