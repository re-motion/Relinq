using System.Linq.Expressions;
using Rubicon.Data.Linq.DataObjectModel;

namespace Rubicon.Data.Linq.Clauses
{
  public interface IClause :IQueryElement
  {
    IClause PreviousClause { get; }
    FieldDescriptor ResolveField (IDatabaseInfo databaseInfo, Expression partialFieldExpression, Expression fullFieldExpression);
  }
}