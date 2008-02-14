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
      return CreateFieldDescriptor(databaseInfo, result.Members);
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

    private FieldDescriptor CreateFieldDescriptor (IDatabaseInfo databaseInfo, IEnumerable<MemberInfo> members)
    {
      MemberInfo member = members.FirstOrDefault ();
      Table initialTable = DatabaseInfoUtility.GetTableForFromClause (databaseInfo, this);
      Tuple<IFieldSource, Table> fieldData = CalculateJoinData (databaseInfo, initialTable, members.Skip (1));

      Column? column = DatabaseInfoUtility.GetColumn (databaseInfo, fieldData.B, member);
      return new FieldDescriptor (member, this, fieldData.A, column);
    }

    private Tuple<IFieldSource, Table> CalculateJoinData (IDatabaseInfo databaseInfo, Table initialTable, IEnumerable<MemberInfo> joinMembers)
    {
      // start with the table and create all joins necessary for reaching the field in the database
      Table lastTable = initialTable;
      IFieldSource fieldSource = lastTable;
      foreach (MemberInfo member in joinMembers)
      {
        try
        {
          Table leftTable = DatabaseInfoUtility.GetRelatedTable (databaseInfo, member);
          Tuple<string, string> joinColumns = DatabaseInfoUtility.GetJoinColumns (databaseInfo, member);

          Column leftColumn = new Column (leftTable, joinColumns.B); // student
          Column rightColumn = new Column (lastTable, joinColumns.A); // student_detail

          fieldSource = new Join (leftTable, fieldSource, leftColumn, rightColumn);
          lastTable = leftTable;
        }
        catch (Exception ex)
        {
          throw new FieldAccessResolveException (ex.Message, ex);
        }        
      }

      return Tuple.NewTuple (fieldSource, lastTable);
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