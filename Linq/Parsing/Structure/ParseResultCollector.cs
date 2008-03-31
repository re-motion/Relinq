using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq.Expressions;
using Rubicon.Utilities;

namespace Rubicon.Data.Linq.Parsing.Structure
{
  public class ParseResultCollector
  {
    private readonly List<BodyExpressionDataBase> _bodyExpressions = new List<BodyExpressionDataBase> ();
    private readonly List<LambdaExpression> _projectionExpressions = new List<LambdaExpression> ();

    public ParseResultCollector (Expression expressionTreeRoot)
    {
      ArgumentUtility.CheckNotNull ("expressionTreeRoot", expressionTreeRoot);
      ExpressionTreeRoot = expressionTreeRoot;
    }

    public Expression ExpressionTreeRoot { get; private set; }
    public bool IsDistinct { get; private set; }

    public ReadOnlyCollection<BodyExpressionDataBase> BodyExpressions
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

    public void AddBodyExpression (BodyExpressionDataBase expression)
    {
      ArgumentUtility.CheckNotNull ("expression", expression);
      _bodyExpressions.Add (expression);
    }

    public FromExpressionData ExtractMainFromExpression()
    {
      if (BodyExpressions.Count == 0)
        throw new InvalidOperationException ("There are no body expressions to be extracted.");

      FromExpressionData fromExpressionData = BodyExpressions[0] as FromExpressionData;
      if (fromExpressionData == null)
        throw new InvalidOperationException ("The first body expression is no FromExpressionData.");

      _bodyExpressions.RemoveAt (0);
      return fromExpressionData;
    }

    public void AddProjectionExpression (LambdaExpression expression)
    {
      _projectionExpressions.Add (expression);
    }
  }
}