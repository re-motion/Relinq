/* Copyright (C) 2005 - 2008 rubicon informationstechnologie gmbh
 *
 * This program is free software: you can redistribute it and/or modify it under 
 * the terms of the re:motion license agreement in license.txt. If you did not 
 * receive it, please visit http://www.re-motion.org/licensing.
 * 
 * Unless otherwise provided, this software is distributed on an "AS IS" basis, 
 * WITHOUT WARRANTY OF ANY KIND, either express or implied. 
 */

using System.Collections.Generic;

namespace Remotion.Data.UnitTests.Linq
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