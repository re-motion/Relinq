// Copyright (c) rubicon IT GmbH, www.rubicon.eu
//
// See the NOTICE file distributed with this work for additional information
// regarding copyright ownership.  rubicon licenses this file to you under 
// the Apache License, Version 2.0 (the "License"); you may not use this 
// file except in compliance with the License.  You may obtain a copy of the 
// License at
//
//   http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software 
// distributed under the License is distributed on an "AS IS" BASIS, WITHOUT 
// WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.  See the 
// License for the specific language governing permissions and limitations
// under the License.
// 

using System;
using System.Linq.Expressions;
using Remotion.Linq.Utilities;
using Remotion.Utilities;

namespace Remotion.Linq.Development.UnitTesting
{

  /// <summary>
  /// Compares two <see cref="Expression"/> trees constructed from <b>System.Linq</b> expressions.
  /// </summary>
  public sealed class ExpressionTreeComparer : ExpressionTreeComparerBase
  {
    public static void CheckAreEqualTrees (Expression expectedTree, Expression actualTree)
    {
      ArgumentUtility.CheckNotNull ("expectedTree", expectedTree);
      ArgumentUtility.CheckNotNull ("actualTree", actualTree);

      var comparer = new ExpressionTreeComparer (
          expectedTree.BuildString(),
          actualTree.BuildString());
      comparer.CheckAreEqualNodes (expectedTree, actualTree);
    }

    private ExpressionTreeComparer (string expectedInitial, string actualInitial)
        : base (expectedInitial, actualInitial, typeof (MemberBinding), typeof (ElementInit))
    {
    }
  }
}