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
#if NET_3_5
using System.Diagnostics;
#endif
using System.Linq.Expressions;
using Remotion.Linq.Utilities;
using Remotion.Utilities;

namespace Remotion.Linq.Clauses
{
  /// <summary>
  /// Represents the where part of a query, filtering data items according to some <see cref="Predicate"/>.
  /// </summary>
  /// <example>
  /// In C#, the "where" clause in the following sample corresponds to a <see cref="WhereClause"/>:
  /// <ode>
  /// var query = from s in Students
  ///             where s.First == "Hugo"
  ///             select s;
  /// </ode>
  /// </example>
  public sealed class WhereClause : IBodyClause
  {
    private Expression _predicate;

    /// <summary>
    /// Initializes a new instance of the <see cref="WhereClause"/> class.
    /// </summary>
    /// <param name="predicate">The predicate used to filter data items.</param>
    public WhereClause (Expression predicate)
    {
      ArgumentUtility.CheckNotNull ("predicate", predicate);
      _predicate = predicate;
    }

    /// <summary>
    /// Gets the predicate, the expression representing the where condition by which the data items are filtered
    /// </summary>
#if NET_3_5
    [DebuggerDisplay ("{Remotion.Linq.Clauses.ExpressionVisitors.FormattingExpressionVisitor.Format (Predicate),nq}")]
#endif
    public Expression Predicate
    {
      get { return _predicate; }
      set { _predicate = ArgumentUtility.CheckNotNull ("value", value); }
    }

    /// <summary>
    /// Accepts the specified visitor by calling its <see cref="IQueryModelVisitor.VisitWhereClause"/> method.
    /// </summary>
    /// <param name="visitor">The visitor to accept.</param>
    /// <param name="queryModel">The query model in whose context this clause is visited.</param>
    /// <param name="index">The index of this clause in the <paramref name="queryModel"/>'s <see cref="QueryModel.BodyClauses"/> collection.</param>
    public void Accept (IQueryModelVisitor visitor, QueryModel queryModel, int index)
    {
      ArgumentUtility.CheckNotNull ("visitor", visitor);
      ArgumentUtility.CheckNotNull ("queryModel", queryModel);

      visitor.VisitWhereClause (this, queryModel, index);
    }

    /// <summary>
    /// Transforms all the expressions in this clause and its child objects via the given <paramref name="transformation"/> delegate.
    /// </summary>
    /// <param name="transformation">The transformation object. This delegate is called for each <see cref="Expression"/> within this
    /// clause, and those expressions will be replaced with what the delegate returns.</param>
    public void TransformExpressions (Func<Expression, Expression> transformation)
    {
      ArgumentUtility.CheckNotNull ("transformation", transformation);
      Predicate = transformation (Predicate);
    }

    /// <summary>
    /// Clones this clause.
    /// </summary>
    /// <param name="cloneContext">The clones of all query source clauses are registered with this <see cref="CloneContext"/>.</param>
    /// <returns></returns>
    public WhereClause Clone (CloneContext cloneContext)
    {
      ArgumentUtility.CheckNotNull ("cloneContext", cloneContext);

      var clone = new WhereClause (Predicate);
      return clone;
    }

    IBodyClause IBodyClause.Clone (CloneContext cloneContext)
    {
      return Clone (cloneContext);
    }

    public override string ToString ()
    {
      return "where " + Predicate.BuildString();
    }
  }
}
