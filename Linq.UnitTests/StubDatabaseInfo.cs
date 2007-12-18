using System;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using Rubicon.Data.Linq.QueryProviderImplementation;

namespace Rubicon.Data.Linq.UnitTests
{
  public class StubDatabaseInfo : IDatabaseInfo
  {
    public string GetTableName (Type querySourceType)
    {
      if (typeof (IQueryable<Student>).IsAssignableFrom (querySourceType))
        return "sourceTable";
      else
        return null;
    }

    public string GetColumnName (MemberInfo member)
    {
      if (member.Name == "NonDBProperty")
        return null;
      else
        return member.Name + "Column";
    }
  }
}