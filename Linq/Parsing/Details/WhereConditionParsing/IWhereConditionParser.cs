using System.Collections.Generic;
using System.Linq.Expressions;
using Remotion.Data.Linq.DataObjectModel;

namespace Remotion.Data.Linq.Parsing.Details.WhereConditionParsing
{
  public interface IWhereConditionParser<TExpression> : IParser<TExpression>, IParser
        where TExpression : Expression
  {
    ICriterion Parse (TExpression expression, List<FieldDescriptor> fieldDescriptors);
  }

  public interface IWhereConditionParser  : IParser
  {
    ICriterion Parse (Expression expression, List<FieldDescriptor> fieldDescriptors);
  }
  

}