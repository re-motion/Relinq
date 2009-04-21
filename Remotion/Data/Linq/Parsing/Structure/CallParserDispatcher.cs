// This file is part of the re-motion Core Framework (www.re-motion.org)
// Copyright (C) 2005-2009 rubicon informationstechnologie gmbh, www.rubicon.eu
// 
// The re-motion Core Framework is free software; you can redistribute it 
// and/or modify it under the terms of the GNU Lesser General Public License 
// version 3.0 as published by the Free Software Foundation.
// 
// re-motion is distributed in the hope that it will be useful, 
// but WITHOUT ANY WARRANTY; without even the implied warranty of 
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the 
// GNU Lesser General Public License for more details.
// 
// You should have received a copy of the GNU Lesser General Public License
// along with re-motion; if not, see http://www.gnu.org/licenses.
// 
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

    public void RegisterParser (string methodName, System.Action<ParseResultCollector, MethodCallExpression> parser)
    {
      ArgumentUtility.CheckNotNull ("methodName", methodName);
      ArgumentUtility.CheckNotNull ("parser", parser);
      _parseMethods.Add (methodName, parser);
    }

    public void RegisterParser (string methodName, System.Action<ParseResultCollector, MethodCallExpression, ParameterExpression> parser)
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

      var parser = GetParser (expression.Method) as System.Action<ParseResultCollector, MethodCallExpression, ParameterExpression>;
      if (parser != null)
        parser (collector, expression, potentialFromIdentifier);
      else
        ((System.Action<ParseResultCollector, MethodCallExpression>) GetParser (expression.Method)) (collector, expression);

    }
  }
}
