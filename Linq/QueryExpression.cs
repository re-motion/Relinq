using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;
using Rubicon.Data.Linq.Clauses;
using Rubicon.Data.Linq.DataObjectModel;
using Rubicon.Data.Linq.Parsing.FieldResolving;
using Rubicon.Data.Linq.Visitor;
using Rubicon.Utilities;

namespace Rubicon.Data.Linq
{
  public class QueryExpression : IQueryElement
  {
    private readonly List<IBodyClause> _bodyClauses = new List<IBodyClause> ();
    private readonly Dictionary<string, FromClauseBase> _fromClausesByIdentifier = new Dictionary<string, FromClauseBase> ();

    private Expression _expressionTree;

    public QueryExpression (Type resultType, MainFromClause mainFromClause, ISelectGroupClause selectOrGroupClause, Expression expressionTree)
    {
      ArgumentUtility.CheckNotNull ("resultType", resultType);
      ArgumentUtility.CheckNotNull ("mainFromClause", mainFromClause);
      ArgumentUtility.CheckNotNull ("SelectOrGroupClause", selectOrGroupClause);

      ResultType = resultType;
      MainFromClause = mainFromClause;
      SelectOrGroupClause = selectOrGroupClause;
      _expressionTree = expressionTree;
    }

    public QueryExpression (Type resultType, MainFromClause fromClause, ISelectGroupClause selectOrGroupClause)
        : this (resultType, fromClause, selectOrGroupClause, null)
    {
    }

    public Type ResultType { get; private set; }

    public MainFromClause MainFromClause { get; private set; }
    public ISelectGroupClause SelectOrGroupClause { get; private set; }
    
    public ReadOnlyCollection<IBodyClause> BodyClauses
    {
      get { return _bodyClauses.AsReadOnly (); }
    }

    public void AddBodyClause (IBodyClause clause)
    {
      ArgumentUtility.CheckNotNull ("clause", clause);
      _bodyClauses.Add (clause);

      var clauseAsFromClause = clause as FromClauseBase;
      if (clauseAsFromClause == null)
        return;

      if (_fromClausesByIdentifier.ContainsKey (clauseAsFromClause.Identifier.Name))
      {
        string message = string.Format ("Multiple from clauses with the same name ('{0}') are not supported.",
            clauseAsFromClause.Identifier.Name);
        throw new NotSupportedException (message);
      }
      _fromClausesByIdentifier.Add (clauseAsFromClause.Identifier.Name, clauseAsFromClause);
    }

    public FromClauseBase GetFromClause (string identifierName, Type identifierType)
    {
      ArgumentUtility.CheckNotNull ("identifierName", identifierName);
      ArgumentUtility.CheckNotNull ("identifierType", identifierType);

      if (identifierName == MainFromClause.Identifier.Name)
      {
        MainFromClause.CheckResolvedIdentifierType (identifierType);
        return MainFromClause;
      }
      else
        return GetAdditionalFromClause (identifierName, identifierType);
    }

    public FromClauseBase GetAdditionalFromClause (string identifierName, Type identifierType)
    {
      ArgumentUtility.CheckNotNull ("identifierName", identifierName);
      ArgumentUtility.CheckNotNull ("identifierType", identifierType);

      FromClauseBase fromClause;
      if (_fromClausesByIdentifier.TryGetValue (identifierName, out fromClause))
      {
        fromClause.CheckResolvedIdentifierType (identifierType);
        return fromClause;
      }
      return null;
    }


    public void Accept (IQueryVisitor visitor)
    {
      ArgumentUtility.CheckNotNull ("visitor", visitor);

      visitor.VisitQueryExpression (this);
    }

    public override string ToString ()
    {
      var sv = new StringVisitor();
      sv.VisitQueryExpression (this);
      return sv.ToString();
    }

    public Expression GetExpressionTree ()
    {
      if (_expressionTree == null)
      {
        var visitor = new ExpressionTreeBuildingVisitor();
        Accept (visitor);
        _expressionTree = visitor.ExpressionTree;
      }
      return _expressionTree;
    }

    public FieldDescriptor ResolveField (FromClauseFieldResolver resolver, Expression fieldAccessExpression)
    {
      ArgumentUtility.CheckNotNull ("resolver", resolver);
      ArgumentUtility.CheckNotNull ("fieldAccessExpression", fieldAccessExpression);

      return new QueryExpressionFieldResolver (this).ResolveField (resolver, fieldAccessExpression);
    }
  }
}