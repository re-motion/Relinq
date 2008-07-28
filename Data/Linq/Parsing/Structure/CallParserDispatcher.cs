/* Copyright (C) 2005 - 2008 rubicon informationstechnologie gmbh
 *
 * This program is free software: you can redistribute it and/or modify it under 
 * the terms of the re:motion license agreement in license.txt. If you did not 
 * receive it, please visit http://www.re-motion.org/licensing.
 * 
 * Unless otherwise provided, this software is distributed on an "AS IS" basis, 
 * WITHOUT WARRANTY OF ANY KIND, either express or implied. 
 */

using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using Remotion.Text;
using Remotion.Utilities;

namespace Remotion.Data.Linq.Parsing.Structure
{
  public class CallParserDispatcher
  {
    private readonly Dictionary<string, Delegate> _parseMethods = new Dictionary<string, Delegate> ();

    public void RegisterParser (string methodName, Action<ParseResultCollector, MethodCallExpression> parser)
    {
      ArgumentUtility.CheckNotNull ("methodName", methodName);
      ArgumentUtility.CheckNotNull ("parser", parser);
      _parseMethods.Add (methodName, parser);
    }

    public void RegisterParser (string methodName, Action<ParseResultCollector, MethodCallExpression, ParameterExpression> parser)
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

      var parser = GetParser (expression.Method) as Action<ParseResultCollector, MethodCallExpression, ParameterExpression>;
      if (parser != null)
        parser (collector, expression, potentialFromIdentifier);
      else
        ((Action<ParseResultCollector, MethodCallExpression>) GetParser (expression.Method)) (collector, expression);

    }
  }
}
