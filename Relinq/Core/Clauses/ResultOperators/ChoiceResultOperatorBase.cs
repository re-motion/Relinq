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
using Remotion.Linq.Clauses.StreamedData;
using Remotion.Linq.Utilities;

namespace Remotion.Linq.Clauses.ResultOperators
{
  /// <summary>
  /// Represents a <see cref="ValueFromSequenceResultOperatorBase"/> that is executed on a sequence, choosing a single item for its result.
  /// </summary>
  public abstract class ChoiceResultOperatorBase : ValueFromSequenceResultOperatorBase
  {
    protected ChoiceResultOperatorBase (bool returnDefaultWhenEmpty)
    {
      ReturnDefaultWhenEmpty = returnDefaultWhenEmpty;
    }

    public bool ReturnDefaultWhenEmpty { get; set; }

    public override IStreamedDataInfo GetOutputDataInfo (IStreamedDataInfo inputInfo)
    {
      var inputSequenceInfo = ArgumentUtility.CheckNotNullAndType<StreamedSequenceInfo> ("inputInfo", inputInfo);
      return new StreamedSingleValueInfo (inputSequenceInfo.ItemExpression.Type, ReturnDefaultWhenEmpty);
    }
  }
}
