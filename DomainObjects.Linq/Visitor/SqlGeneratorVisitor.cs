using System.Linq;
using System.Collections.Generic;
using Rubicon.Collections;
using Rubicon.Data.DomainObjects.Linq.Clauses;
using System.Reflection;
using Rubicon.Data.DomainObjects.Linq.Parsing;

namespace Rubicon.Data.DomainObjects.Linq.Visitor
{
  public class SqlGeneratorVisitor : IQueryVisitor
  {
    private readonly IDatabaseInfo _databaseInfo;

    public List<Tuple<string, string>> Tables {get; private set;}
    public List<Tuple<string, string>> Columns { get; private set; }

    public SqlGeneratorVisitor (IDatabaseInfo databaseInfo)
    {
      _databaseInfo = databaseInfo;
      Tables = new List<Tuple<string, string>>();
      Columns = new List<Tuple<string, string>>();
    }

    public void VisitQueryExpression (QueryExpression queryExpression)
    {
      queryExpression.FromClause.Accept (this);
      queryExpression.QueryBody.Accept (this);
    }

    public void VisitMainFromClause (MainFromClause fromClause)
    {
      Tuple<string, string> tableEntry = Tuple.NewTuple (_databaseInfo.GetTableName (fromClause.QuerySource.GetType()), fromClause.Identifier.Name);
      Tables.Add (tableEntry);
    }

    public void VisitAdditionalFromClause (AdditionalFromClause fromClause)
    {
      Tuple<string, string> tableEntry = Tuple.NewTuple (_databaseInfo.GetTableName (fromClause.GetQuerySourceType()), fromClause.Identifier.Name);
      Tables.Add (tableEntry);
    }

    public void VisitJoinClause (JoinClause joinClause)
    {
    }

    public void VisitLetClause (LetClause letClause)
    {
    }

    public void VisitWhereClause (WhereClause whereClause)
    {
    }

    public void VisitOrderByClause (OrderByClause orderByClause)
    {
    }

    public void VisitOrderingClause (OrderingClause orderingClause)
    {
    }

    public void VisitSelectClause (SelectClause selectClause)
    {
      SelectProjectionParser projectionParser = new SelectProjectionParser (selectClause, _databaseInfo);
      IEnumerable<Tuple<FromClauseBase, MemberInfo>> selectedFields = projectionParser.SelectedFields;
      
      IEnumerable<Tuple<string, string>> columns =
          from field in selectedFields
          select Tuple.NewTuple (field.A.Identifier.Name, field.B == null ? "*" : _databaseInfo.GetColumnName (field.B));

      Columns.AddRange (columns);
    }

    public void VisitGroupClause (GroupClause groupClause)
    {
    }

    public void VisitQueryBody (QueryBody queryBody)
    {
      foreach (IFromLetWhereClause fromLetWhereClause in queryBody.FromLetWhereClauses)
        fromLetWhereClause.Accept (this);
      if (queryBody.OrderByClause != null)
        queryBody.OrderByClause.Accept (this);
      queryBody.SelectOrGroupClause.Accept (this);
    }
  }
}