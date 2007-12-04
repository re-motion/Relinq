using System.Collections.Generic;
using System.Reflection;
using Rubicon.Collections;
using Rubicon.Data.DomainObjects.Linq.Clauses;
using Rubicon.Utilities;

namespace Rubicon.Data.DomainObjects.Linq.Parsing
{
  public class SelectProjectionParser
  {
    private readonly SelectClause _selectClause;
    private readonly IDatabaseInfo _databaseInfo;

    public SelectProjectionParser (SelectClause selectClause, IDatabaseInfo databaseInfo)
    {
      ArgumentUtility.CheckNotNull ("selectClause", selectClause);
      ArgumentUtility.CheckNotNull ("databaseInfo", databaseInfo);

      _selectClause = selectClause;
      _databaseInfo = databaseInfo;
    }

    public IEnumerable<Tuple<FromClauseBase, PropertyInfo>> GetSelectedFields ()
    {
      FromClauseBase lastFromClause = ClauseFinderHelper.FindClause<FromClauseBase> (_selectClause.PreviousClause);
      yield return Tuple.NewTuple (lastFromClause, (PropertyInfo) null);
    }
  }
}