using System.Linq.Expressions;
using Rubicon.Data.Linq.Clauses;
using Rubicon.Data.Linq.DataObjectModel;
using Rubicon.Data.Linq.Parsing.FieldResolving;

namespace Rubicon.Data.Linq.Clauses
{
  public interface IResolveableClause : IClause
  {
    ParameterExpression Identifier {get;  }
    FieldDescriptor ResolveField (ClauseFieldResolver resolver, Expression partialFieldExpression, Expression fullFieldExpression);
  }
}