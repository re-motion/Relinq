using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using Rubicon.Text;
using Rubicon.Utilities;

namespace Rubicon.Data.Linq.Parsing.Structure
{
  public class CallParserDispatcher
  {
    private readonly Dictionary<string, Delegate> _parseMethods = new Dictionary<string, Delegate> ();

    public void RegisterParser (string methodName, Action<ParseResultCollector, Expression> parser)
    {
      ArgumentUtility.CheckNotNull ("methodName", methodName);
      ArgumentUtility.CheckNotNull ("parser", parser);
      _parseMethods.Add (methodName, parser);
    }

    public void RegisterParser (string methodName, Action<ParseResultCollector, Expression, ParameterExpression> parser)
    {
      ArgumentUtility.CheckNotNull ("methodName", methodName);
      ArgumentUtility.CheckNotNull ("parser", parser);
      _parseMethods.Add (methodName, parser);
    }

    public bool CanParse (MethodInfo method)
    {
      ArgumentUtility.CheckNotNull ("method", method);
      return _parseMethods.ContainsKey (method.Name);
    }

    public Delegate GetParser (MethodInfo method)
    {
      ArgumentUtility.CheckNotNull ("method", method);
      if (!CanParse (method))
        throw ParserUtility.CreateParserException (GetExpectedString (), method.Name, "dispatching a method call to a parser", null);
      return _parseMethods[method.Name];
    }

    private string GetExpectedString ()
    {
      return SeparatedStringBuilder.Build (", ", _parseMethods.Keys);
    }

    public void Dispatch (ParseResultCollector collector, MethodCallExpression expression, ParameterExpression potentialFromIdentifier)
    {
      ArgumentUtility.CheckNotNull ("expression", expression);

      Action<ParseResultCollector, Expression, ParameterExpression> parser =
          GetParser (expression.Method) as Action<ParseResultCollector, Expression, ParameterExpression>;
      if (parser != null)
        parser (collector, expression, potentialFromIdentifier);
      else
        ((Action<ParseResultCollector, Expression>) GetParser (expression.Method)) (collector, expression);

    }
  }
}