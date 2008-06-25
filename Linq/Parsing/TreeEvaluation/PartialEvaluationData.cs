using System.Collections.Generic;
using System.Linq.Expressions;

namespace Remotion.Data.Linq.Parsing.TreeEvaluation
{
  public class PartialEvaluationData
  {
    public PartialEvaluationData ()
    {
      UsedParameters = new Dictionary<Expression, HashSet<ParameterExpression>> ();
      DeclaredParameters = new Dictionary<Expression, HashSet<ParameterExpression>> ();
      SubQueries = new Dictionary<Expression, HashSet<SubQueryExpression>> ();
    }

    public Dictionary<Expression, HashSet<ParameterExpression>> UsedParameters { get; private set; }
    public Dictionary<Expression, HashSet<ParameterExpression>> DeclaredParameters { get; private set; }
    public Dictionary<Expression, HashSet<SubQueryExpression>> SubQueries { get; private set; }
  }
}