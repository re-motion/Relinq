using System.Collections.Generic;

namespace Rubicon.Data.Linq.UnitTests
{
  public class Student
  {
    public string First { get; set; }
    public string Last { get; set; }
    public int ID { get; set; }
    public List<int> Scores { get; set; }
    public string NonDBProperty { get; set; }
    public bool NonDBBoolProperty { get; set; }
    public bool IsOld { get; set; }
    public bool HasDog { get; set; }
    public Student OtherStudent { get; set; }
  }
}