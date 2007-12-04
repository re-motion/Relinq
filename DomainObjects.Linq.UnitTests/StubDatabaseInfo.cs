using System;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using Rubicon.Data.DomainObjects.Linq.QueryProviderImplementation;

namespace Rubicon.Data.DomainObjects.Linq.UnitTests
{
  public class StubDatabaseInfo : IDatabaseInfo
  {
    public string GetTableName (Type querySourceType)
    {
      Assert.IsTrue (typeof (IQueryable<Student>).IsAssignableFrom (querySourceType));
      
      return "sourceTable";
    }

    public string GetColumnName (MemberInfo member)
    {
      return member.Name + "Column";
    }

    public bool IsDbColumn (MemberInfo member)
    {
      return member.Name != "NonDBProperty";
    }
  }
}