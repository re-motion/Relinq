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
  /// <typeparam name="T">The type of object from which the recursive fetch operation should be made.</typeparam>
  public class FluentFetchRequest<T>
  {
    private readonly FetchRequest<T> _fetchRequest;

    public FluentFetchRequest (FetchRequest<T> fetchRequest)
    {
      ArgumentUtility.CheckNotNull ("fetchRequest", fetchRequest);
      _fetchRequest = fetchRequest;
    }

    /// <summary>
    /// Specifies that, when the former fetch request is executed, the relation indicated by <paramref name="relatedObjectSelector"/> should be eagerly
    /// fetched as well if supported by the query provider implementation.
    /// </summary>
    /// <typeparam name="TRelated">The type of the next related objects to be eager-fetched.</typeparam>
    /// <param name="relatedObjectSelector">A lambda expression selecting the next related objects to be eager-fetched.</param>
    /// <returns>A <see cref="FluentFetchRequest{T}"/> object on which further recursive fetch requests can be made. The subsequent fetches start 
    /// from the related objects fetched by the fetch request created by this method.</returns>
    public FluentFetchRequest<TRelated> Fetch<TRelated> (Expression<Func<T, IEnumerable<TRelated>>> relatedObjectSelector)
    {
      ArgumentUtility.CheckNotNull ("relatedObjectSelector", relatedObjectSelector);
      var nextRequest = _fetchRequest.GetOrAddInnerFetchRequest (relatedObjectSelector);
      return new FluentFetchRequest<TRelated> (nextRequest);
    }
  }
}