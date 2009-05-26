// This file is part of the re-motion Core Framework (www.re-motion.org)
// Copyright (C) 2005-2009 rubicon informationstechnologie gmbh, www.rubicon.eu
// 
// The re-motion Core Framework is free software; you can redistribute it 
// and/or modify it under the terms of the GNU Lesser General Public License 
// version 3.0 as published by the Free Software Foundation.
// 
// re-motion is distributed in the hope that it will be useful, 
// but WITHOUT ANY WARRANTY; without even the implied warranty of 
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the 
// GNU Lesser General Public License for more details.
// 
// You should have received a copy of the GNU Lesser General Public License
// along with re-motion; if not, see http://www.gnu.org/licenses.
// 
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Remotion.Utilities;

namespace Remotion.Data.Linq.Parsing.Structure.IntermediateModel
{
  /// <summary>
  /// Represents a <see cref="MethodCallExpression"/> for 
  /// <see cref="Queryable.SelectMany{TSource,TCollection,TResult}(System.Linq.IQueryable{TSource},System.Linq.Expressions.Expression{System.Func{TSource,System.Collections.Generic.IEnumerable{TCollection}}},System.Linq.Expressions.Expression{System.Func{TSource,TCollection,TResult}})"/>.
  /// </summary>
  public class SelectManyExpressionNode : ExpressionNodeBase
  {
    public static readonly MethodInfo[] SupportedMethods = new[]
                                                           {
                                                               GetSupportedMethod (
                                                                   () => Queryable.SelectMany<object, object[], object> (null, o => null, null))
                                                           };

    public SelectManyExpressionNode (IExpressionNode source, LambdaExpression collectionSelector, LambdaExpression resultSelector)
    {
      ArgumentUtility.CheckNotNull ("source", source);
      ArgumentUtility.CheckNotNull ("collectionSelector", collectionSelector);
      ArgumentUtility.CheckNotNull ("resultSelector", resultSelector);

      Source = source;
      CollectionSelector = collectionSelector;
      ResultSelector = resultSelector;
    }

    public IExpressionNode Source { get; private set; }
    public LambdaExpression CollectionSelector { get; private set; }
    public LambdaExpression ResultSelector { get; private set; }
  }
}