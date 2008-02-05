using System;
using System.Reflection;
using Rubicon.Data.Linq.Clauses;
using Rubicon.Data.Linq.DataObjectModel;
using Rubicon.Utilities;

namespace Rubicon.Data.Linq.DataObjectModel
{
  public struct FieldDescriptor
  {
    public FieldDescriptor (MemberInfo member, FromClauseBase fromClause, Table table, Column? column)
        : this()
    {
      ArgumentUtility.CheckNotNull ("fromClause", fromClause);

      if (member == null && column == null)
        throw new ArgumentNullException ("member && column", "Either member or column must have a value.");

      Member = member;
      FromClause = fromClause;
      Column = column;
      Table = table;
    }

    public MemberInfo Member { get; private set; }
    public Column? Column { get; private set; }
    public Table Table { get; private set; }
    public FromClauseBase FromClause { get; private set; }

    public Column GetMandatoryColumn()
    {
      if (Column != null)
        return Column.Value;
      else
      {
        string message = string.Format ("The member {0}.{1} does not identify a queryable column in table {2}.",
            Member.DeclaringType.FullName, Member.Name, Table.Name);


        throw new ParserException (message);
      }
    }

    public override string ToString ()
    {
      return string.Format ("{0} ({1}.{2})", Column, FromClause.Identifier.Name, Member != null ? Member.Name : "*");
    }
  }
}