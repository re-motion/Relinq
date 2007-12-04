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
      throw new System.NotImplementedException();
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
      throw new System.NotImplementedException();
    }

    public void VisitLetClause (LetClause letClause)
    {
      throw new System.NotImplementedException();
    }

    public void VisitWhereClause (WhereClause whereClause)
    {
      throw new System.NotImplementedException();
    }

    public void VisitOrderByClause (OrderByClause orderByClause)
    {
      throw new System.NotImplementedException();
    }

    public void VisitOrderingClause (OrderingClause orderingClause)
    {
      throw new System.NotImplementedException();
    }

    public void VisitSelectClause (SelectClause selectClause)
    {
      SelectProjectionParser projectionParser = new SelectProjectionParser (selectClause, _databaseInfo);
      IEnumerable<Tuple<FromClauseBase, PropertyInfo>> selectedFields = projectionParser.GetSelectedFields();
      
      IEnumerable<Tuple<string, string>> columns =
          from field in selectedFields
          select Tuple.NewTuple (field.A.Identifier.Name, field.B == null ? "*" : _databaseInfo.GetColumnName (field.B));

      Columns.AddRange (columns);
    }

    public void VisitGroupClause (GroupClause groupClause)
    {
      throw new System.NotImplementedException();
    }

    public void VisitQueryBody (QueryBody queryBody)
    {
      throw new System.NotImplementedException();
    }
  }
}