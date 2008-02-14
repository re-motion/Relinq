using System;
using System.Reflection;
using Rubicon.Data.Linq.Clauses;
using Rubicon.Data.Linq.DataObjectModel;
using Rubicon.Utilities;

namespace Rubicon.Data.Linq.DataObjectModel
{
  public struct FieldDescriptor
  {
    public FieldDescriptor (MemberInfo member, FromClauseBase fromClause, IFieldSourcePath sourcePath, Column? column)
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
    public Column? Column { get; private set; }
    public IFieldSourcePath SourcePath { get; private set; }
    public FromClauseBase FromClause { get; private set; }

    public Column GetMandatoryColumn()
    {
      if (Column != null)
        return Column.Value;
      else
      {
        string message = string.Format ("The member '{0}.{1}' does not identify a queryable column.",
            Member.DeclaringType.FullName, Member.Name);

        throw new FieldAccessResolveException (message);
      }
    }

    public override string ToString ()
    {
      return string.Format ("{0} ({1}.{2})", Column, FromClause.Identifier.Name, Member != null ? Member.Name : "*");
    }
  }
}