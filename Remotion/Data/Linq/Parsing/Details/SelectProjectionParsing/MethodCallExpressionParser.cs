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
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Remotion.Data.Linq.DataObjectModel;
using Remotion.Utilities;

namespace Remotion.Data.Linq.Parsing.Details.SelectProjectionParsing
{
  public class MethodCallExpressionParser : ISelectProjectionParser
  {
    public SelectProjectionParserRegistry ParserRegistry { get; private set; }

    public MethodCallExpressionParser (SelectProjectionParserRegistry parserRegistry)
    {
      ArgumentUtility.CheckNotNull ("parserRegistry", parserRegistry);
      ParserRegistry = parserRegistry;
    }

    public MethodCall Parse (MethodCallExpression methodCallExpression, ParseContext parseContext)
    {
      ArgumentUtility.CheckNotNull ("methodCallExpression", methodCallExpression);
      ArgumentUtility.CheckNotNull ("parseContext", parseContext);

      return Parse(methodCallExpression, parseContext, methodCallExpression.Arguments);
    }

    public MethodCall Parse (MethodCallExpression methodCallExpression, ParseContext parseContext, IEnumerable<Expression> argumentsExpressions)
    {
      MethodInfo methodInfo = methodCallExpression.Method;
      IEvaluation evaluationObject = ParseEvaluationObject(methodCallExpression, parseContext);

      List<IEvaluation> evaluationArguments = new List<IEvaluation> ();
      foreach (Expression exp in argumentsExpressions)
        evaluationArguments.Add (ParserRegistry.GetParser (exp).Parse (exp, parseContext));

      return new MethodCall (methodInfo, evaluationObject, evaluationArguments);
    }

    private IEvaluation ParseEvaluationObject (MethodCallExpression methodCallExpression, ParseContext parseContext)
    {
      IEvaluation evaluationObject;
      if (methodCallExpression.Object == null)
        evaluationObject = null;
      else
        evaluationObject = ParserRegistry.GetParser (methodCallExpression.Object).Parse (methodCallExpression.Object, parseContext);
      return evaluationObject;
    }

    IEvaluation ISelectProjectionParser.Parse (Expression expression, ParseContext parseContext)
    {
      ArgumentUtility.CheckNotNull ("expression", expression);
      ArgumentUtility.CheckNotNull ("parseContext", parseContext);
      return Parse ((MethodCallExpression) expression, parseContext);
    }

    public virtual bool CanParse(Expression expression)
    {
      ArgumentUtility.CheckNotNull ("expression", expression);
      return expression is MethodCallExpression;
    }
  }
}
