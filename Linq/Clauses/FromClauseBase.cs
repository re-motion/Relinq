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

    public abstract void Accept (IQueryVisitor visitor);
    public abstract Type GetQuerySourceType ();
    public abstract FieldDescriptor ResolveField (IDatabaseInfo databaseInfo, Expression partialFieldExpression, Expression fullFieldExpression);

    protected void CheckExpressionNameAndType (string name, Type type, Expression fullFieldExpression)
    {
      if (name != Identifier.Name)
      {
        string message = string.Format ("There is no from clause defining identifier '{0}', which is used in expression '{1}'.",
            name, fullFieldExpression);
        throw new ParserException (message);
      }

      if (type != Identifier.Type)
      {
        string message = string.Format ("The from identifier '{0}' has a different type ({1}) than expected in expression '{2}' ({3}).",
            name, Identifier.Type, fullFieldExpression, type);
        throw new ParserException (message);
      }
    }

    protected FieldDescriptor CreateFieldDescriptor (MemberInfo member, IDatabaseInfo databaseInfo, Expression fullFieldExpression)
    {
      try
      {
        Table table = GetTable (databaseInfo);
        Column? column = DatabaseInfoUtility.GetColumn (databaseInfo, table, member);
        return new FieldDescriptor (member, this, table, column);
      }
      catch (Exception ex)
      {
        string message = string.Format ("Could not retrieve database metadata for field expression '{0}'.", fullFieldExpression);
        throw new ParserException (message, ex);
      }
    }
  }
}