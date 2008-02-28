using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq.Expressions;
using Rubicon.Utilities;

namespace Rubicon.Data.Linq.Parsing.Structure
{
  public class ParseResultCollector
  {
    private readonly List<BodyExpressionBase> _bodyExpressions = new List<BodyExpressionBase> ();
    private readonly List<LambdaExpression> _projectionExpressions = new List<LambdaExpression> ();

    public ParseResultCollector (Expression expressionTreeRoot)
    {
      ArgumentUtility.CheckNotNull ("expressionTreeRoot", expressionTreeRoot);
      ExpressionTreeRoot = expressionTreeRoot;
    }

    public Expression ExpressionTreeRoot { get; private set; }
    public bool IsDistinct { get; private set; }

    public ReadOnlyCollection<BodyExpressionBase> BodyExpressions
    {
      get { return _bodyExpressions.AsReadOnly(); }
    }

    public ReadOnlyCollection<LambdaExpression> ProjectionExpressions
    {
      get { return _projectionExpressions.AsReadOnly (); }
    }

    public void SetDistinct ()
    {
      IsDistinct = true;
    }

    public void AddBodyExpression (BodyExpressionBase expression)
    {
      ArgumentUtility.CheckNotNull ("expression", expression);
      _bodyExpressions.Add (expression);
    }

    public void AddProjectionExpression (LambdaExpression expression)
    {
      _projectionExpressions.Add (expression);
    }
  }
}