using System.Linq.Expressions;
using Remotion.Data.Linq.DataObjectModel;

namespace Remotion.Data.Linq.Parsing.Details.SelectProjectionParsing
{
  public interface ISelectProjectionParser : IParser
  {
    IEvaluation Parse (Expression expression, ParseContext parseContext);
  }
}