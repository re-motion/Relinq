// This file is part of the re-linq project (relinq.codeplex.com)
// Copyright (C) 2005-2009 rubicon informationstechnologie gmbh, www.rubicon.eu
// 
// re-linq is free software; you can redistribute it and/or modify it under 
// the terms of the GNU Lesser General Public License as published by the 
// Free Software Foundation; either version 2.1 of the License, 
// or (at your option) any later version.
// 
// re-linq is distributed in the hope that it will be useful, 
// but WITHOUT ANY WARRANTY; without even the implied warranty of 
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the 
// GNU Lesser General Public License for more details.
// 
// You should have received a copy of the GNU Lesser General Public License
// along with re-linq; if not, see http://www.gnu.org/licenses.
// 
using System;
using Remotion.Linq.Clauses;
using Remotion.Linq.Clauses.ResultOperators;
using Remotion.Linq.Clauses.StreamedData;

namespace Remotion.Linq.UnitTests.Linq.Core.Clauses.ResultOperators
{
  public class TestSequenceFromSequenceResultOperator : SequenceFromSequenceResultOperatorBase
  {
    public override StreamedSequence ExecuteInMemory<T> (StreamedSequence input)
    {
      return input;
    }

    public override IStreamedDataInfo GetOutputDataInfo (IStreamedDataInfo inputInfo)
    {
      throw new NotImplementedException();
    }

    public override ResultOperatorBase Clone (CloneContext cloneContext)
    {
      throw new NotImplementedException();
    }

    public override void TransformExpressions (Func<System.Linq.Expressions.Expression, System.Linq.Expressions.Expression> transformation)
    {
      throw new NotImplementedException ();
    }
  }
}
