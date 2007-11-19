using System;
using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using System.Linq;

namespace Rubicon.Data.DomainObjects.Linq.UnitTests.Parsing
{
  [TestFixture]
  [Ignore ("TODO: Implement parsing")]
  public class SimpleQuery
  {
    private IEnumerable<Student> _sourceExpression;
    private QueryExpression _parsedQuery;

    [SetUp]
    public void SetUp()
    {
      
      _sourceExpression = TestQueryGenerator.CreateSimpleQuery();

    }



    
  }
}