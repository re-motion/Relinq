using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using NUnit.Framework;
using Remotion.Linq.Utilities;

namespace Remotion.Linq.UnitTests.Utilities
{
  [TestFixture]
  public class ItemTypeReflectionUtilityTest
  {
    private class GenericWithIEnumerable<T> : ArrayList
    {
    }

    private class ClosedGenericList : List<int>
    {
    }

    [Test]
    public void TryGetItemTypeOfClosedGenericIEnumerable_ArgumentIsArray_ReturnsTrue ()
    {
      Type itemType;
      var result = ItemTypeReflectionUtility.TryGetItemTypeOfClosedGenericIEnumerable (typeof (double[]), out itemType);
      Assert.That (result, Is.True);
      Assert.That (itemType, Is.SameAs (typeof (double)));
    }

    [Test]
    public void TryGetItemTypeOfClosedGenericIEnumerable_ArgumentIsArray_ReturnsTrue_Strange ()
    {
      Expression<Func<int, IEnumerable<double>>> collectionSelector = x => new double[1];
      Type itemType;
      var result = ItemTypeReflectionUtility.TryGetItemTypeOfClosedGenericIEnumerable (collectionSelector.Body.Type, out itemType);
      Assert.That (result, Is.True);
      Assert.That (itemType, Is.SameAs (typeof (double)));
    }

    [Test]
    public void TryGetItemTypeOfClosedGenericIEnumerable_ArgumentImplementsIEnumerable_ReturnsTrue ()
    {
      Type itemType;
      var result = ItemTypeReflectionUtility.TryGetItemTypeOfClosedGenericIEnumerable (typeof (List<int>), out itemType);
      Assert.That (result, Is.True);
      Assert.That (itemType, Is.SameAs (typeof (int)));
    }

    [Test]
    public void TryGetItemTypeOfClosedGenericIEnumerable_ArgumentImplementsIEnumerableOnBaseClass_ReturnsTrue ()
    {
      Type itemType;
      var result = ItemTypeReflectionUtility.TryGetItemTypeOfClosedGenericIEnumerable (typeof (ClosedGenericList), out itemType);
      Assert.That (result, Is.True);
      Assert.That (itemType, Is.SameAs (typeof (int)));
    }

    [Test]
    public void TryGetItemTypeOfClosedGenericIEnumerable_ArgumentIsIEnumerable_ReturnsTrue ()
    {
      Type itemType;
      var result = ItemTypeReflectionUtility.TryGetItemTypeOfClosedGenericIEnumerable (typeof (IEnumerable<IEnumerable<string>>), out itemType);
      Assert.That (result, Is.True);
      Assert.That (itemType, Is.SameAs (typeof (IEnumerable<string>)));
    }

    [Test]
    public void TryGetItemTypeOfClosedGenericIEnumerable_ArgumentImplementsIEnumerable_NonGeneric_ReturnsFalse ()
    {
      Type itemType;
      var result = ItemTypeReflectionUtility.TryGetItemTypeOfClosedGenericIEnumerable (typeof (ArrayList), out itemType);
      Assert.That (result, Is.False);
      Assert.That (itemType, Is.Null);
    }

    [Test]
    public void TryGetItemTypeOfClosedGenericIEnumerable_ArgumentImplementsIEnumerable_Generic_ReturnsFalse ()
    {
      Type itemType;
      var result = ItemTypeReflectionUtility.TryGetItemTypeOfClosedGenericIEnumerable (typeof (GenericWithIEnumerable<int>), out itemType);
      Assert.That (result, Is.False);
      Assert.That (itemType, Is.Null);
    }

    [Test]
    public void TryGetItemTypeOfClosedGenericIEnumerable_ArgumentImplementsIEnumerable_BothGenericAndNonGeneric_ReturnsTrue ()
    {
      Type itemType;
      var result = ItemTypeReflectionUtility.TryGetItemTypeOfClosedGenericIEnumerable (typeof (int[]), out itemType);
      Assert.That (result, Is.True);
      Assert.That (itemType, Is.SameAs (typeof (int)));
    }

    [Test]
    public void TryGetItemTypeOfClosedGenericIEnumerable_InvalidType_ReturnsFalse ()
    {
      Type itemType;
      var result = ItemTypeReflectionUtility.TryGetItemTypeOfClosedGenericIEnumerable (typeof (int), out itemType);
      Assert.That (result, Is.False);
      Assert.That (itemType, Is.Null);
    }
  }
}