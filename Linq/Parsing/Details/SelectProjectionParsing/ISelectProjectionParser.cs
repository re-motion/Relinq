using System.Collections.Generic;
using System.Linq.Expressions;
using Remotion.Data.Linq.DataObjectModel;

namespace Remotion.Data.Linq.Parsing.Details.SelectProjectionParsing
{
  public interface ISelectProjectionParser : IParser
  {
    List<IEvaluation> Parse (Expression expression, List<FieldDescriptor> fieldDescriptors);
  }
}