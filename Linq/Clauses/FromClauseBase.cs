using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using Rubicon.Data.Linq.DataObjectModel;
using Rubicon.Utilities;

namespace Rubicon.Data.Linq.Clauses
{
  public abstract class FromClauseBase : IClause
  {
    private readonly ParameterExpression _identifier;
    private readonly List<JoinClause> _joinClauses = new List<JoinClause>();

    public FromClauseBase (IClause previousClause, ParameterExpression identifier)
    {     
      ArgumentUtility.CheckNotNull ("identifier", identifier);

      _identifier = identifier;
      PreviousClause = previousClause;
    }

    public IClause PreviousClause { get; private set; }

    public ParameterExpression Identifier
    {
      get { return _identifier; }
    }

    public IEnumerable<JoinClause> JoinClauses
    {
      get { return _joinClauses; }
    }

    public int JoinClauseCount
    {
      get { return _joinClauses.Count; }
    }

    public void Add (JoinClause joinClause)
    {
      ArgumentUtility.CheckNotNull ("joinClause", joinClause);
      _joinClauses.Add (joinClause);
    }

    public Table GetTable (IDatabaseInfo databaseInfo)
    {
      ArgumentUtility.CheckNotNull ("databaseInfo", databaseInfo);
      return DatabaseInfoUtility.GetTableForFromClause (databaseInfo, this);
    }

    public FieldDescriptor ResolveField (IDatabaseInfo databaseInfo, Expression partialFieldExpression, Expression fullFieldExpression)
    {
      ArgumentUtility.CheckNotNull ("databaseInfo", databaseInfo);
      ArgumentUtility.CheckNotNull ("partialFieldExpression", partialFieldExpression);
      ArgumentUtility.CheckNotNull ("fullFieldExpression", fullFieldExpression);

      FromClauseResolveVisitor visitor = new FromClauseResolveVisitor();
      FromClauseResolveVisitor.Result result = visitor.ParseFieldAccess (partialFieldExpression, fullFieldExpression);

      if (result.Parameter.Name != Identifier.Name)
      {
        string message = string.Format ("This from clause can only resolve field accesses for parameters called '{0}', but a parameter "
          + "called '{1}' was given.", Identifier.Name, result.Parameter.Name);
        throw new FieldAccessResolveException (message);
      }

      if (result.Parameter.Type != Identifier.Type)
      {
        string message = string.Format ("This from clause can only resolve field accesses for parameters of type '{0}', but a parameter "
          + "of type '{1}' was given.", Identifier.Type, result.Parameter.Type);
        throw new FieldAccessResolveException (message);
      }

      MemberInfo member = result.Member;
      Table table = GetTable (databaseInfo);
      Column? column = DatabaseInfoUtility.GetColumn (databaseInfo, table, member);
      return new FieldDescriptor (member, this, table, column);
    }

    public abstract void Accept (IQueryVisitor visitor);
    public abstract Type GetQuerySourceType ();

    internal void CheckResolvedIdentifierType (Type expectedType)
    {
      if (Identifier.Type != expectedType)
      {
        string message = string.Format ("The from clause with identifier '{0}' has type '{1}', but '{2}' was requested.", Identifier.Name,
            Identifier.Type, expectedType);
        throw new ClauseLookupException (message);
      }
    }
  }
}