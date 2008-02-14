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
      return CreateFieldDescriptor(databaseInfo, result.AccessedMember, result.JoinMembers);
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

    private FieldDescriptor CreateFieldDescriptor (IDatabaseInfo databaseInfo, MemberInfo accessedMember, IEnumerable<MemberInfo> joinMembers)
    {
      // Documentation example: sdd.Student_Detail.Student.First
      // joinMembers == "Student_Detail", "Student"
      
      Table initialTable = DatabaseInfoUtility.GetTableForFromClause (databaseInfo, this); // Table for sdd
      Tuple<IFieldSourcePath, Table> fieldData = CalculateJoinData (databaseInfo, initialTable, joinMembers);

      Column? column = DatabaseInfoUtility.GetColumn (databaseInfo, fieldData.B, accessedMember);
      return new FieldDescriptor (accessedMember, this, fieldData.A, column);
    }

    private Tuple<IFieldSourcePath, Table> CalculateJoinData (IDatabaseInfo databaseInfo, Table initialTable, IEnumerable<MemberInfo> joinMembers)
    {
      // Documentation example: sdd.Student_Detail.Student.First
      // First create a join for sdd and Student_Detail (identified by the "Student_Detail" member) and use initial table (sdd) as the right side
      // Then create the join for Student_Detail and Student (identified by the "Student" member) and use the first join as the right side
      //      second join
      //    /            \
      // Student       first join
      //              /          \
      //      Student_Detail    sdd

      Table lastTable = initialTable;
      IFieldSourcePath fieldSourcePath = lastTable;
      foreach (MemberInfo member in joinMembers)
      {
        try
        {
          Table leftTable = DatabaseInfoUtility.GetRelatedTable (databaseInfo, member);
          Tuple<string, string> joinColumns = DatabaseInfoUtility.GetJoinColumns (databaseInfo, member);

          // joinColumns holds the columns in the order defined by the member: for "sdd.Student_Detail" it holds sdd.PK/Student_Detail.FK
          // we build the trees in opposite order, so we use the first tuple value as the right column, the second value as the left column
          Column leftColumn = new Column (leftTable, joinColumns.B);
          Column rightColumn = new Column (lastTable, joinColumns.A);

          fieldSourcePath = new Join (leftTable, fieldSourcePath, leftColumn, rightColumn);
          lastTable = leftTable;
        }
        catch (Exception ex)
        {
          throw new FieldAccessResolveException (ex.Message, ex);
        }        
      }

      return Tuple.NewTuple (fieldSourcePath, lastTable);
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