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
using System.Linq.Expressions;
using Remotion.Utilities;

namespace Remotion.Data.Linq.Parsing.Structure.Legacy
{
  /// <summary>
  /// Delegates to the appropiate parser dependent to the used method expression in an expression chain.
  /// </summary>
  public class SourceExpressionParser
  {
    private readonly bool _isTopLevel;
    private readonly CallParserDispatcher _callDispatcher;
    
    public SourceExpressionParser (bool isTopLevel)
    {
      _isTopLevel = isTopLevel;
      _callDispatcher = new CallParserDispatcher ();

      _callDispatcher.RegisterParser ("Select", ParseSelectSource);
      _callDispatcher.RegisterParser ("SelectMany", ParseSelectManySource);
      _callDispatcher.RegisterParser ("Where", ParseWhereSource);
      _callDispatcher.RegisterParser ("OrderBy", ParseOrderBySource);
      _callDispatcher.RegisterParser ("OrderByDescending", ParseOrderBySource);
      _callDispatcher.RegisterParser ("ThenBy", ParseOrderBySource);
      _callDispatcher.RegisterParser ("ThenByDescending", ParseOrderBySource);
      _callDispatcher.RegisterParser ("Distinct", ParseResultModifierSource);
      _callDispatcher.RegisterParser ("Count", ParseResultModifierSource);
      _callDispatcher.RegisterParser ("First", ParseResultModifierSource);
      _callDispatcher.RegisterParser ("Single", ParseResultModifierSource);
      _callDispatcher.RegisterParser ("Cast", ParseCastSource);
      _callDispatcher.RegisterParser ("Take", ParseResultModifierSource);
      
    }

    public CallParserDispatcher CallDispatcher
    {
      get { return _callDispatcher; }
    }

    public void Parse (ParseResultCollector resultCollector, Expression sourceExpression, ParameterExpression potentialFromIdentifier, string context)
    {
      ArgumentUtility.CheckNotNull ("resultCollector", resultCollector);
      ArgumentUtility.CheckNotNull ("sourceExpression", sourceExpression);
      ArgumentUtility.CheckNotNull ("context", context);

      var methodCallExpression = sourceExpression as MethodCallExpression;
      if (methodCallExpression != null && _callDispatcher.CanParse (methodCallExpression.Method))
        _callDispatcher.Dispatch (resultCollector, methodCallExpression, potentialFromIdentifier);
      else
        ParseSimpleFromSource (resultCollector, sourceExpression, potentialFromIdentifier, context);
    }

    private void ParseSelectSource (ParseResultCollector resultCollector, MethodCallExpression sourceExpression)
    {
      //TODO: extend this to handle subselects in main from clause (possible create a query model or result collector as expected when using let)
      if (_isTopLevel)
        new SelectExpressionParser().Parse (resultCollector, sourceExpression);
      else
        new LetExpressionParser().Parse (resultCollector, sourceExpression);
    }


    private void ParseSelectManySource (ParseResultCollector resultCollector, MethodCallExpression sourceExpression)
    {
      new SelectManyExpressionParser ().Parse (resultCollector, sourceExpression);
    }

    private void ParseWhereSource (ParseResultCollector resultCollector, MethodCallExpression sourceExpression)
    {
      new WhereExpressionParser (_isTopLevel).Parse (resultCollector, sourceExpression);
    }

    private void ParseOrderBySource (ParseResultCollector resultCollector, MethodCallExpression sourceExpression)
    {
      new OrderByExpressionParser (_isTopLevel).Parse (resultCollector, sourceExpression);
    }
    
    //supported method have to be registered from the dispatcher
    private void ParseResultModifierSource (ParseResultCollector resultCollector, MethodCallExpression sourceExpression, ParameterExpression potentialFromIdentifier)
    {
      if (!_isTopLevel)
      {
        string message = string.Format ("Distinct is only allowed at the top level of a query, not in the middle: '{0}'.", resultCollector.ExpressionTreeRoot);
        throw new ParserException (message, sourceExpression, resultCollector.ExpressionTreeRoot, null);
      }

      resultCollector.AddResultModifierExpression (sourceExpression);
      Expression nextExpression = sourceExpression.Arguments[0];
      Parse (resultCollector, nextExpression, potentialFromIdentifier, "first argument of result modifier");
    }

    private void ParseCastSource (ParseResultCollector resultCollector, MethodCallExpression sourceExpression, ParameterExpression potentialFromIdentifier)
    {
      // casts on this level are simply ignored in the query model
      Parse (resultCollector, sourceExpression.Arguments[0], potentialFromIdentifier, "first argument of Cast");
    }

    private void ParseSimpleFromSource (ParseResultCollector resultCollector, Expression sourceExpression, ParameterExpression potentialFromIdentifier, string context)
    {
      new SimpleFromSourceExpressionParser ().Parse (resultCollector, sourceExpression, potentialFromIdentifier, context);
    }
  }
}