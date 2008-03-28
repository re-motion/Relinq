using System.Collections.Generic;
using System.Linq.Expressions;

namespace Rubicon.Data.Linq.Parsing.TreeEvaluation
{
  public class ParameterUsage
  {
    public ParameterUsage ()
    {
      UsedParameters = new Dictionary<Expression, HashSet<ParameterExpression>> ();
      DeclaredParameters = new Dictionary<Expression, HashSet<ParameterExpression>> ();
    }

    public Dictionary<Expression, HashSet<ParameterExpression>> UsedParameters { get; private set; }
    public Dictionary<Expression, HashSet<ParameterExpression>> DeclaredParameters { get; private set; }
  }
}