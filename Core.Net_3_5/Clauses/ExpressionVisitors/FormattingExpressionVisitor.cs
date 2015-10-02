// Copyright (c) rubicon IT GmbH, www.rubicon.eu
//
// See the NOTICE file distributed with this work for additional information
// regarding copyright ownership.  rubicon licenses this file to you under 
// the Apache License, Version 2.0 (the "License"); you may not use this 
// file except in compliance with the License.  You may obtain a copy of the 
// License at
//
//   http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software 
// distributed under the License is distributed on an "AS IS" BASIS, WITHOUT 
// WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.  See the 
// License for the specific language governing permissions and limitations
// under the License.
// 

using System;
using System.Linq.Expressions;
using Remotion.Linq.Clauses.Expressions;
using Remotion.Linq.Parsing;
using Remotion.Utilities;

namespace Remotion.Linq.Clauses.ExpressionVisitors
{
  /// <summary>
  /// Transforms an expression tree into a human-readable string, taking all the custom expression nodes into account.
  /// It does so by replacing all instances of custom expression nodes by parameters that have the desired string as their names. This is done
  /// to circumvent a limitation in the <see cref="Expression"/> class, where overriding <see cref="Expression.ToString"/> in custom expressions
  /// will not work.
  /// </summary>
  public class FormattingExpressionVisitor : RelinqExpressionVisitor
  {
    public static string Format (Expression expression)
    {
      ArgumentUtility.CheckNotNull ("expression", expression);
      var transformedExpression = new FormattingExpressionVisitor().Visit (expression);
      return transformedExpression.ToString();
    }

    private FormattingExpressionVisitor ()
    {
    }

    protected internal override Expression VisitQuerySourceReference (QuerySourceReferenceExpression expression)
    {
      ArgumentUtility.CheckNotNull ("expression", expression);
      return Expression.Parameter (expression.Type, "[" + expression.ReferencedQuerySource.ItemName + "]");
    }

    protected internal override Expression VisitSubQuery (SubQueryExpression expression)
    {
      ArgumentUtility.CheckNotNull ("expression", expression);
      return Expression.Parameter (expression.Type, "{" + expression.QueryModel + "}");
    }

#if !NET_3_5
    protected override Expression VisitExtension (Expression expression)
    {
      ArgumentUtility.CheckNotNull ("expression", expression);

      if (expression.CanReduce)
        return base.VisitExtension (expression);

      return Expression.Parameter (expression.Type, expression.ToString());
    }
#else
    protected internal override Expression VisitExtension (ExtensionExpression expression)
    {
      ArgumentUtility.CheckNotNull ("expression", expression);
      return Expression.Parameter (expression.Type, expression.ToString ());
    }

    protected override Expression VisitRelinqUnknownNonExtension (Expression expression)
    {
      ArgumentUtility.CheckNotNull ("expression", expression);
      return Expression.Parameter (expression.Type, expression.ToString());
    }
#endif
  }
}
