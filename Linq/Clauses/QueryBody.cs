using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq.Expressions;
using Rubicon.Data.Linq.Clauses;
using Rubicon.Utilities;
using System.Reflection;

namespace Rubicon.Data.Linq.Clauses
{
  public class QueryBody : IQueryElement
  {
    private readonly ISelectGroupClause _selectOrGroupClause;
    private readonly List<IBodyClause> _bodyClauses = new List<IBodyClause> ();
    private readonly Dictionary<string, FromClauseBase> _fromClauses = new Dictionary<string, FromClauseBase>();

    public QueryBody (ISelectGroupClause selectOrGroupClause)
    {
      ArgumentUtility.CheckNotNull ("SelectOrGroupClause", selectOrGroupClause);
      _selectOrGroupClause = selectOrGroupClause;
    }
     

    public ISelectGroupClause SelectOrGroupClause
    {
      get { return _selectOrGroupClause; }
    }
    
    public ReadOnlyCollection<IBodyClause> BodyClauses
    {
      get { return _bodyClauses.AsReadOnly(); }
    }

    public void Add (IBodyClause clause)
    {
      ArgumentUtility.CheckNotNull ("clause", clause);
      _bodyClauses.Add (clause);
      
      FromClauseBase clauseAsFromClause = clause as FromClauseBase;
      if (clauseAsFromClause != null)
      {
        if (_fromClauses.ContainsKey (clauseAsFromClause.Identifier.Name))
        {
          string message = string.Format ("Multiple from clauses with the same name ('{0}') are not supported.",
            clauseAsFromClause.Identifier.Name);
          throw new NotSupportedException (message);
        }
        _fromClauses.Add (clauseAsFromClause.Identifier.Name, clauseAsFromClause);
      }
    }

    public FromClauseBase GetFromClause (string identifierName, Type identifierType)
    {
      ArgumentUtility.CheckNotNull ("identifierName", identifierName);
      ArgumentUtility.CheckNotNull ("identifierType", identifierType);

      FromClauseBase fromClause;
      if (_fromClauses.TryGetValue (identifierName, out fromClause))
      {
        fromClause.CheckResolvedIdentifierType (identifierType);
        return fromClause;
      }
      else
        return null;
    }

    public virtual void Accept (IQueryVisitor visitor)
    {
      ArgumentUtility.CheckNotNull ("visitor", visitor);
      visitor.VisitQueryBody (this);
    }
  }
}