using System;
using System.Reflection;
using Rubicon.Data.Linq.Clauses;
using Rubicon.Data.Linq.DataObjectModel;
using Rubicon.Data.Linq.Parsing.FieldResolving;
using Rubicon.Utilities;

namespace Rubicon.Data.Linq.DataObjectModel
{
  public struct FieldDescriptor
  {
    public FieldDescriptor (MemberInfo member, FromClauseBase fromClause, FieldSourcePath sourcePath, IColumn column)
        : this()
    {
      ArgumentUtility.CheckNotNull ("fromClause", fromClause);
      ArgumentUtility.CheckNotNull ("sourcePath", sourcePath);

      if (member == null && column == null)
        throw new ArgumentNullException ("member && column", "Either member or column must have a value.");

      Member = member;
      FromClause = fromClause;
      Column = column;
      SourcePath = sourcePath;
    }

    public MemberInfo Member { get; private set; }
    public IColumn Column { get; private set; }
    public FieldSourcePath SourcePath { get; private set; }
    public FromClauseBase FromClause { get; private set; }

    public Column GetMandatoryColumn()
    {
      IColumn column = GetMandatoryIColumn();
      if (column is Column)
        return (Column) column;
      else
      {
        string message = string.Format ("The member '{0}.{1}' is a virtual column, which cannot be used in this context.",
            Member.DeclaringType.FullName, Member.Name);

        throw new FieldAccessResolveException (message);
      }
    }

    public IColumn GetMandatoryIColumn ()
    {
      if (Column != null)
        return Column;
      else
      {
        string message = string.Format ("The member '{0}.{1}' does not identify a queryable column.",
            Member.DeclaringType.FullName, Member.Name);

        throw new FieldAccessResolveException (message);
      }
    }

    public override string ToString ()
    {
      return string.Format ("{0} => {1}", SourcePath, Column);
    }
  }
}