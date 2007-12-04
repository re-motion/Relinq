using System;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using Rubicon.Data.DomainObjects.Linq.QueryProviderImplementation;
using Rubicon.Data.DomainObjects.Linq.UnitTests.Parsing;

namespace Rubicon.Data.DomainObjects.Linq.UnitTests
{
  public class StubDatabaseInfo : IDatabaseInfo
  {
    public string GetTableName (Type querySourceType)
    {
      Assert.IsTrue (typeof (IQueryable<Student>).IsAssignableFrom (querySourceType));
      
      return "sourceTable";
    }

    public string GetColumnName (PropertyInfo property)
    {
      return property.Name + "Column";
    }
  }
}