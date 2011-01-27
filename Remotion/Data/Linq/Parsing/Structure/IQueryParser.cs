using System;
using System.Collections.ObjectModel;
using System.Linq.Expressions;

namespace Remotion.Data.Linq.Parsing.Structure
{
  /// <summary>
  /// <see cref="IQueryParser"/> is implemented by classes taking an <see cref="Expression"/> tree and parsing it into a <see cref="QueryModel"/>.
  /// Implementations first apply a number of <see cref="ProcessingSteps"/> to the <see cref="Expression"/> tree, and then analyze the query's
  /// structure using the parsers registered in the <see cref="NodeTypeRegistry"/>.
  /// </summary>
  /// <remarks>
  /// The default implementation of this interface is <see cref="QueryParser"/>. LINQ providers can, however, implement <see cref="IQueryParser"/>
  /// themselves, eg. in order to decorate or replace the functionality of <see cref="QueryParser"/>.
  /// </remarks>
  public interface IQueryParser
  {
    /// <summary>
    /// Gets the node type registry used to parse <see cref="MethodCallExpression"/> instances in <see cref="GetParsedQuery"/>.
    /// </summary>
    /// <value>The node type registry.</value>
    MethodCallExpressionNodeTypeRegistry NodeTypeRegistry { get; }

    /// <summary>
    /// Gets the processing steps used by <see cref="GetParsedQuery"/> to process the <see cref="Expression"/> tree before analyzing its structure.
    /// </summary>
    /// <value>The processing steps.</value>
    ReadOnlyCollection<IExpressionTreeProcessingStep> ProcessingSteps { get; }

    /// <summary>
    /// Gets the <see cref="QueryModel"/> of the given <paramref name="expressionTreeRoot"/>.
    /// </summary>
    /// <param name="expressionTreeRoot">The expression tree to parse.</param>
    /// <returns>A <see cref="QueryModel"/> that represents the query defined in <paramref name="expressionTreeRoot"/>.</returns>
    QueryModel GetParsedQuery (Expression expressionTreeRoot);
  }
}