Imports NUnit.Framework
Imports Remotion.Data.Linq.UnitTests.Linq.Core.IntegrationTests
Imports Remotion.Data.Linq.UnitTests.Linq.Core.IntegrationTests.LinqSamples101.TestDomain

Namespace Remotion.Data.IntegrationTests.VB.Linq.LinqSamples101.Parsing

  ''' <summary>
  ''' http://msdn.microsoft.com/en-us/bb737944.aspx
  ''' </summary>
  <TestFixture()> _
  Public Class WhereTest
    Inherits TestBase

    <Test()> _
    Public Sub Test_01()
      CheckParsedQuery( _
          (Function() From c In QuerySource.Customers Where c.City = "London"), _
          "from Customer c in Customers where VBCompareString((c.City = ""London""), False) select [c]")
    End Sub

  End Class

End Namespace
