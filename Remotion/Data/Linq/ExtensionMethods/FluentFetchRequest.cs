using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Remotion.Data.Linq.EagerFetching;
using Remotion.Utilities;

namespace Remotion.Data.Linq.ExtensionMethods
{
  /// <summary>
  /// Provides a fluent interface to recursively fetch related objects of objects which themselves are eager-fetched.
  /// </summary>
  /// <typeparam name="TQueried">The type of the objects returned by the query.</typeparam>
  /// <typeparam name="TFetch">The type of object from which the recursive fetch operation should be made.</typeparam>
  public class FluentFetchRequest<TQueried, TFetch> : QueryableBase<TQueried>
  {
    public FluentFetchRequest (QueryProviderBase provider, Expression expression) 
        : base (provider, expression)
    {
    }

    /// <summary>
    /// Specifies that, when the previous fetch request is executed, the relation indicated by <paramref name="relatedObjectSelector"/> should be 
    /// eagerly fetched, too, if supported by the query provider implementation.
    /// </summary>
    /// <typeparam name="TRelated">The type of the next related objects to be eager-fetched.</typeparam>
    /// <param name="relatedObjectSelector">A lambda expression selecting the next related objects to be eager-fetched.</param>
    /// <returns>A <see cref="FluentFetchRequest{TFetch, TQueried}"/> object on which further recursive fetch requests can be made. The subsequent fetches start 
    /// from the related objects fetched by the fetch request created by this method.</returns>
    public FluentFetchRequest<TQueried, TRelated> ThenFetch<TRelated> (Expression<Func<TFetch, IEnumerable<TRelated>>> relatedObjectSelector)
    {
      ArgumentUtility.CheckNotNull ("relatedObjectSelector", relatedObjectSelector);

      var newExpression = new ThenFetchExpression (Expression, relatedObjectSelector);
      return new FluentFetchRequest<TQueried, TRelated> ((QueryProviderBase) Provider, newExpression);
    }
  }
}