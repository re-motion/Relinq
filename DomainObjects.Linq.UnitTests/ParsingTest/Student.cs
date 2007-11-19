using System.Collections.Generic;

namespace Rubicon.Data.DomainObjects.Linq.UnitTests.Parsing
{
  public class Student
  {
    public string First { get; set; }
    public string Last { get; set; }
    public int ID { get; set; }
    public List<int> Scores;
  }
}