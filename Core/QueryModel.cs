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
#if !NET_3_5
using System.Collections.ObjectModel;
using System.Collections.Specialized;
#endif
using System.Linq;
using System.Linq.Expressions;
using Remotion.Linq.Clauses;
using Remotion.Linq.Clauses.Expressions;
using Remotion.Linq.Clauses.ResultOperators;
using Remotion.Linq.Clauses.StreamedData;
using Remotion.Linq.Parsing;
#if NET_3_5
using Remotion.Linq.Collections;
#endif
using Remotion.Linq.Parsing.Structure;
using Remotion.Linq.Utilities;
using Remotion.Utilities;

namespace Remotion.Linq
{
  /// <summary>
  /// Provides an abstraction of an expression tree created for a LINQ query. <see cref="QueryModel"/> instances are passed to LINQ providers based
  /// on re-linq via <see cref="IQueryExecutor"/>, but you can also use <see cref="QueryParser"/> to parse an expression tree by hand or construct
  /// a <see cref="QueryModel"/> manually via its constructor.
  /// </summary>
  /// <remarks>
  /// The different parts of the query are mapped to clauses, see <see cref="MainFromClause"/>, <see cref="BodyClauses"/>, and 
  /// <see cref="SelectClause"/>. The simplest way to process all the clauses belonging to a <see cref="QueryModel"/> is by implementing
  /// <see cref="IQueryModelVisitor"/> (or deriving from <see cref="QueryModelVisitorBase"/>) and calling <see cref="Accept"/>.
  /// </remarks>
  public sealed class QueryModel
  {
    private sealed class CloningExpressionVisitor : RelinqExpressionVisitor
    {
      private readonly QuerySourceMapping _querySourceMapping;

      public CloningExpressionVisitor (QuerySourceMapping querySourceMapping)
      {
        _querySourceMapping = querySourceMapping;
      }

      protected internal override Expression VisitQuerySourceReference (QuerySourceReferenceExpression expression)
      {
        if (_querySourceMapping.ContainsMapping (expression.ReferencedQuerySource))
          return _querySourceMapping.GetExpression (expression.ReferencedQuerySource);

        return expression;
      }

      protected internal override Expression VisitSubQuery (SubQueryExpression expression)
      {
        var clonedQueryModel = expression.QueryModel.Clone (_querySourceMapping);
        return new SubQueryExpression (clonedQueryModel);
      }

#if NET_3_5
      protected override Expression VisitRelinqUnknownNonExtension (Expression expression)
      {
        //ignore
        return expression;
      }
#endif
    }

    private readonly UniqueIdentifierGenerator _uniqueIdentifierGenerator;

    private MainFromClause _mainFromClause;
    private SelectClause _selectClause;

    /// <summary>
    /// Initializes a new instance of <see cref="QueryModel"/>
    /// </summary>
    /// <param name="mainFromClause">The <see cref="Clauses.MainFromClause"/> of the query. This is the starting point of the query, generating items 
    /// that are filtered and projected by the query.</param>
    /// <param name="selectClause">The <see cref="SelectClause"/> of the query. This is the end point of
    /// the query, it defines what is actually returned for each of the items coming from the <see cref="MainFromClause"/> and passing the 
    /// <see cref="BodyClauses"/>. After it, only the <see cref="ResultOperators"/> modify the result of the query.</param>
    public QueryModel (MainFromClause mainFromClause, SelectClause selectClause)
    {
      ArgumentUtility.CheckNotNull ("mainFromClause", mainFromClause);
      ArgumentUtility.CheckNotNull ("selectClause", selectClause);

      _uniqueIdentifierGenerator = new UniqueIdentifierGenerator();

      MainFromClause = mainFromClause;
      SelectClause = selectClause;

      BodyClauses = new ObservableCollection<IBodyClause>();
      BodyClauses.CollectionChanged += BodyClauses_CollectionChanged;

      ResultOperators = new ObservableCollection<ResultOperatorBase>();
      ResultOperators.CollectionChanged += ResultOperators_CollectionChanged;
    }

    public Type ResultTypeOverride { get; set; }

    public Type GetResultType ()
    {
      return GetOutputDataInfo ().DataType;
    }

    /// <summary>
    /// Gets an <see cref="IStreamedDataInfo"/> object describing the data streaming out of this <see cref="QueryModel"/>. If a query ends with
    /// the <see cref="SelectClause"/>, this corresponds to <see cref="Clauses.SelectClause.GetOutputDataInfo"/>. If a query has 
    /// <see cref="QueryModel.ResultOperators"/>, the data is further modified by those operators.
    /// </summary>
    /// <returns>Gets a <see cref="IStreamedDataInfo"/> object describing the data streaming out of this <see cref="QueryModel"/>.</returns>
    /// <remarks>
    /// The data streamed from a <see cref="QueryModel"/> is often of type <see cref="IQueryable{T}"/> instantiated
    /// with a specific item type, unless the
    /// query ends with a <see cref="ResultOperatorBase"/>. For example, if the query ends with a <see cref="CountResultOperator"/>, the
    /// result type will be <see cref="int"/>.
    /// </remarks>
    public IStreamedDataInfo GetOutputDataInfo ()
    {
      var outputDataInfo = ResultOperators
          .Aggregate ((IStreamedDataInfo) SelectClause.GetOutputDataInfo (), (current, resultOperator) => resultOperator.GetOutputDataInfo (current));

      if (ResultTypeOverride != null)
        return outputDataInfo.AdjustDataType (ResultTypeOverride);
      else
        return outputDataInfo;
    }

    /// <summary>
    /// Gets or sets the query's <see cref="Clauses.MainFromClause"/>. This is the starting point of the query, generating items that are processed by 
    /// the <see cref="BodyClauses"/> and projected or grouped by the <see cref="SelectClause"/>.
    /// </summary>
    public MainFromClause MainFromClause
    {
      get { return _mainFromClause; }
      set
      {
        ArgumentUtility.CheckNotNull ("value", value);
        _mainFromClause = value;
        _uniqueIdentifierGenerator.AddKnownIdentifier (value.ItemName);
      }
    }

    /// <summary>
    /// Gets or sets the query's select clause. This is the end point of the query, it defines what is actually returned for each of the 
    /// items coming from the <see cref="MainFromClause"/> and passing the <see cref="BodyClauses"/>. After it, only the <see cref="ResultOperators"/>
    /// modify the result of the query.
    /// </summary>
    public SelectClause SelectClause
    {
      get { return _selectClause; }
      set
      {
        ArgumentUtility.CheckNotNull ("value", value);
        _selectClause = value;
      }
    }

    /// <summary>
    /// Gets a collection representing the query's body clauses. Body clauses take the items generated by the <see cref="MainFromClause"/>,
    /// filtering (<see cref="WhereClause"/>), ordering (<see cref="OrderByClause"/>), augmenting (<see cref="AdditionalFromClause"/>), or otherwise
    /// processing them before they are passed to the <see cref="SelectClause"/>.
    /// </summary>
    public ObservableCollection<IBodyClause> BodyClauses { get; private set; }

    /// <summary>
    /// Gets the result operators attached to this <see cref="SelectClause"/>. Result operators modify the query's result set, aggregating,
    /// filtering, or otherwise processing the result before it is returned.
    /// </summary>
    public ObservableCollection<ResultOperatorBase> ResultOperators { get; private set; }

    /// <summary>
    /// Gets the <see cref="UniqueIdentifierGenerator"/> which is used by the <see cref="QueryModel"/>.
    /// </summary>
    /// <returns></returns>
    public UniqueIdentifierGenerator GetUniqueIdentfierGenerator ()
    {
      return _uniqueIdentifierGenerator;
    }

    private void ResultOperators_CollectionChanged (object sender, NotifyCollectionChangedEventArgs e)
    {
      ArgumentUtility.CheckNotNull ("e", e);
      ArgumentUtility.CheckItemsNotNullAndType ("e.NewItems", e.NewItems, typeof (ResultOperatorBase));
    }

    /// <summary>
    /// Accepts an implementation of <see cref="IQueryModelVisitor"/> or <see cref="QueryModelVisitorBase"/>, as defined by the Visitor pattern.
    /// </summary>
    public void Accept (IQueryModelVisitor visitor)
    {
      ArgumentUtility.CheckNotNull ("visitor", visitor);
      visitor.VisitQueryModel (this);
    }

    /// <summary>
    /// Returns a <see cref="System.String"/> representation of this <see cref="QueryModel"/>.
    /// </summary>
    public override string ToString ()
    {
      string mainQueryString;
      if (IsIdentityQuery ())
      {
        mainQueryString = MainFromClause.FromExpression.BuildString();
      }
      else
      {
        mainQueryString = MainFromClause + BodyClauses.Aggregate ("", (s, b) => s + " " + b) + " " + SelectClause;
      }

      return ResultOperators.Aggregate (mainQueryString, (s, r) => s + " => " + r);
    }

    /// <summary>
    /// Clones this <see cref="QueryModel"/>, returning a new <see cref="QueryModel"/> equivalent to this instance, but with its clauses being
    /// clones of this instance's clauses. Any <see cref="QuerySourceReferenceExpression"/> in the cloned clauses that points back to another clause 
    /// in this <see cref="QueryModel"/> (including its subqueries) is adjusted to point to the respective clones in the cloned 
    /// <see cref="QueryModel"/>. Any subquery nested in the <see cref="QueryModel"/> is also cloned.
    /// </summary>
    public QueryModel Clone ()
    {
      return Clone (new QuerySourceMapping());
    }

    /// <summary>
    /// Clones this <see cref="QueryModel"/>, returning a new <see cref="QueryModel"/> equivalent to this instance, but with its clauses being
    /// clones of this instance's clauses. Any <see cref="QuerySourceReferenceExpression"/> in the cloned clauses that points back to another clause 
    /// in  this <see cref="QueryModel"/> (including its subqueries) is adjusted to point to the respective clones in the cloned 
    /// <see cref="QueryModel"/>. Any subquery nested in the <see cref="QueryModel"/> is also cloned.
    /// </summary>
    /// <param name="querySourceMapping">The <see cref="QuerySourceMapping"/> defining how to adjust instances of 
    /// <see cref="QuerySourceReferenceExpression"/> in the cloned <see cref="QueryModel"/>. If there is a <see cref="QuerySourceReferenceExpression"/>
    /// that points out of the <see cref="QueryModel"/> being cloned, specify its replacement via this parameter. At the end of the cloning process,
    /// this object maps all the clauses in this original <see cref="QueryModel"/> to the clones created in the process.
    /// </param>
    public QueryModel Clone (QuerySourceMapping querySourceMapping)
    {
      ArgumentUtility.CheckNotNull ("querySourceMapping", querySourceMapping);

      var cloneContext = new CloneContext (querySourceMapping);
      var queryModelBuilder = new QueryModelBuilder();

      queryModelBuilder.AddClause (MainFromClause.Clone (cloneContext));
      foreach (var bodyClause in BodyClauses)
        queryModelBuilder.AddClause (bodyClause.Clone (cloneContext));
      queryModelBuilder.AddClause (SelectClause.Clone (cloneContext));

      foreach (var resultOperator in ResultOperators)
      {
        var resultOperatorClone = resultOperator.Clone (cloneContext);
        queryModelBuilder.AddResultOperator (resultOperatorClone);
      }

      var clone = queryModelBuilder.Build ();
      var cloningExpressionVisitor = new CloningExpressionVisitor (cloneContext.QuerySourceMapping);
      clone.TransformExpressions (cloningExpressionVisitor.Visit);
      clone.ResultTypeOverride = ResultTypeOverride;
      return clone;
    }

    /// <summary>
    /// Transforms all the expressions in this <see cref="QueryModel"/>'s clauses via the given <paramref name="transformation"/> delegate.
    /// </summary>
    /// <param name="transformation">The transformation object. This delegate is called for each <see cref="Expression"/> within this 
    /// <see cref="QueryModel"/>, and those expressions will be replaced with what the delegate returns.</param>
    public void TransformExpressions (Func<Expression, Expression> transformation)
    {
      ArgumentUtility.CheckNotNull ("transformation", transformation);

      MainFromClause.TransformExpressions (transformation);

      foreach (var bodyClause in BodyClauses)
        bodyClause.TransformExpressions (transformation);

      SelectClause.TransformExpressions (transformation);

      foreach (var resultOperator in ResultOperators)
        resultOperator.TransformExpressions (transformation);
    }

    /// <summary>
    /// Returns a new name with the given prefix. The name is different from that of any <see cref="FromClauseBase"/> added
    /// in the <see cref="QueryModel"/>. Note that clause names that are changed after the clause is added as well as names of other clauses
    /// than from clauses are not considered when determining "unique" names. Use names only for readability and debugging, not
    /// for uniquely identifying clauses.
    /// </summary>
    public string GetNewName (string prefix)
    {
      ArgumentUtility.CheckNotNullOrEmpty ("prefix", prefix);
      return _uniqueIdentifierGenerator.GetUniqueIdentifier (prefix);
    }

    private void BodyClauses_CollectionChanged (object sender, NotifyCollectionChangedEventArgs e)
    {
      ArgumentUtility.CheckNotNull ("e", e);
      ArgumentUtility.CheckItemsNotNullAndType ("e.NewItems", e.NewItems, typeof (IBodyClause));

      if (e.NewItems != null)
      {
        foreach (var fromClause in e.NewItems.OfType<IFromClause>())
          _uniqueIdentifierGenerator.AddKnownIdentifier (fromClause.ItemName);
      }
    }

    /// <summary>
    /// Executes this <see cref="QueryModel"/> via the given <see cref="IQueryExecutor"/>. By default, this indirectly calls 
    /// <see cref="IQueryExecutor.ExecuteCollection{T}"/>, but this can be modified by the <see cref="ResultOperators"/>.
    /// </summary>
    /// <param name="executor">The <see cref="IQueryExecutor"/> to use for executing this query.</param>
    public IStreamedData Execute (IQueryExecutor executor)
    {
      ArgumentUtility.CheckNotNull ("executor", executor);

      var dataInfo = GetOutputDataInfo();
      return dataInfo.ExecuteQueryModel (this, executor);
    }

    /// <summary>
    /// Determines whether this <see cref="QueryModel"/> represents an identity query. An identity query is a query without any body clauses
    /// whose <see cref="SelectClause"/> selects exactly the items produced by its <see cref="MainFromClause"/>. An identity query can have
    /// <see cref="ResultOperators"/>.
    /// </summary>
    /// <returns>
    /// 	<see langword="true" /> if this <see cref="QueryModel"/> represents an identity query; otherwise, <see langword="false" />.
    /// </returns>
    /// <example>
    /// An example for an identity query is the subquery in that is produced for the <see cref="Clauses.SelectClause.Selector"/> in the following 
    /// query:
    /// <code>
    /// from order in ...
    /// select order.OrderItems.Count()
    /// </code>
    /// In this query, the <see cref="Clauses.SelectClause.Selector"/> will become a <see cref="SubQueryExpression"/> because 
    /// <see cref="Enumerable.Count{TSource}(System.Collections.Generic.IEnumerable{TSource})"/> is treated as a query operator. The 
    /// <see cref="QueryModel"/> in that <see cref="SubQueryExpression"/> has no <see cref="BodyClauses"/> and a trivial <see cref="SelectClause"/>,
    /// so its <see cref="IsIdentityQuery"/> method returns <see langword="true" />. The outer <see cref="QueryModel"/>, on the other hand, does not
    /// have a trivial <see cref="SelectClause"/>, so its <see cref="IsIdentityQuery"/> method returns <see langword="false" />.
    /// </example>
    public bool IsIdentityQuery ()
    {
      return BodyClauses.Count == 0
             && SelectClause.Selector is QuerySourceReferenceExpression
             && ((QuerySourceReferenceExpression) SelectClause.Selector).ReferencedQuerySource == MainFromClause;
    }

    /// <summary>
    /// Creates a new <see cref="QueryModel"/> that has this <see cref="QueryModel"/> as a sub-query in its <see cref="MainFromClause"/>.
    /// </summary>
    /// <param name="itemName">The name of the new <see cref="QueryModel"/>'s <see cref="FromClauseBase.ItemName"/>.</param>
    /// <returns>A new <see cref="QueryModel"/> whose <see cref="MainFromClause"/>'s <see cref="FromClauseBase.FromExpression"/> is a 
    /// <see cref="SubQueryExpression"/> that holds this <see cref="QueryModel"/> instance.</returns>
    public QueryModel ConvertToSubQuery (string itemName)
    {
      ArgumentUtility.CheckNotNullOrEmpty ("itemName", itemName);

      var outputDataInfo = GetOutputDataInfo() as StreamedSequenceInfo;
      if (outputDataInfo == null)
      {
        var message = string.Format (
            "The query must return a sequence of items, but it selects a single object of type '{0}'.",
            GetOutputDataInfo ().DataType);
        throw new InvalidOperationException (message);
      }

      // from x in (sourceItemQuery)
      // select x

      var mainFromClause = new MainFromClause (
          itemName, 
          outputDataInfo.ResultItemType, 
          new SubQueryExpression (this));
      var selectClause = new SelectClause (new QuerySourceReferenceExpression (mainFromClause));
      return new QueryModel (mainFromClause, selectClause);
    }
  }
}
