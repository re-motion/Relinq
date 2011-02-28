using System.Linq.Expressions;

namespace Remotion.Linq.Parsing.Structure
{
  /// <summary>
  /// <see cref="IQueryParser"/> is implemented by classes taking an <see cref="Expression"/> tree and parsing it into a <see cref="QueryModel"/>.
  /// </summary>
  /// <remarks>
  /// The default implementation of this interface is <see cref="QueryParser"/>. LINQ providers can, however, implement <see cref="IQueryParser"/>
  /// themselves, eg. in order to decorate or replace the functionality of <see cref="QueryParser"/>.
  /// </remarks>
  public interface IQueryParser
  {
    /// <summary>
    /// Gets the <see cref="QueryModel"/> of the given <paramref name="expressionTreeRoot"/>.
    /// </summary>
    /// <param name="expressionTreeRoot">The expression tree to parse.</param>
    /// <returns>A <see cref="QueryModel"/> that represents the query defined in <paramref name="expressionTreeRoot"/>.</returns>
    QueryModel GetParsedQuery (Expression expressionTreeRoot);
  }
}