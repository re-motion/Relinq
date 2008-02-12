using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq.Expressions;
using System.Linq;
using System.Reflection;
using Rubicon.Collections;
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

    public ReadOnlyCollection<JoinClause> JoinClauses
    {
      get { return _joinClauses.AsReadOnly(); }
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

      CheckParameterNameAndType(result.Parameter);
      return CreateFieldDescriptor(databaseInfo, result);
    }

    private void CheckParameterNameAndType (ParameterExpression parameter)
    {
      if (parameter.Name != Identifier.Name)
      {
        string message = string.Format ("This from clause can only resolve field accesses for parameters called '{0}', but a parameter "
            + "called '{1}' was given.", Identifier.Name, parameter.Name);
        throw new FieldAccessResolveException (message);
      }

      if (parameter.Type != Identifier.Type)
      {
        string message = string.Format ("This from clause can only resolve field accesses for parameters of type '{0}', but a parameter "
            + "of type '{1}' was given.", Identifier.Type, parameter.Type);
        throw new FieldAccessResolveException (message);
      }
    }

    private FieldDescriptor CreateFieldDescriptor (IDatabaseInfo databaseInfo, FromClauseResolveVisitor.Result visitorResult)
    {
      MemberInfo member = visitorResult.Members.FirstOrDefault();

      Table table = GetTable (databaseInfo);
      IFieldSource fieldSource = table;
      for (int i = 1; i < visitorResult.Members.Length; i++)
      {
        try
        {
          Table leftTable = DatabaseInfoUtility.GetRelatedTable (databaseInfo, visitorResult.Members[i]);
          Tuple<string, string> joinColumns = DatabaseInfoUtility.GetJoinColumns (databaseInfo, visitorResult.Members[i]);

          Column leftColumn = new Column (leftTable, joinColumns.B); // student
          Column rightColumn = new Column (table, joinColumns.A); // student_detail

          fieldSource = new Join (leftTable, fieldSource, leftColumn, rightColumn);
          table = leftTable;
        }
        catch (Exception ex)
        {
          throw new FieldAccessResolveException (ex.Message, ex);
        }
      }

      Column? column = DatabaseInfoUtility.GetColumn (databaseInfo, table, member);
      return new FieldDescriptor (member, this, fieldSource, column);
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