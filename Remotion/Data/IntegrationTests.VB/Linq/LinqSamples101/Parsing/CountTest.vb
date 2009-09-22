Imports NUnit.Framework
Imports Remotion.Data.UnitTests.Linq.IntegrationTests
Imports Remotion.Data.UnitTests.Linq.IntegrationTests.LinqSamples101.TestDomain

Namespace Remotion.Data.IntegrationTests.VB.Linq.LinqSamples101.Parsing

  ''' <summary>
  ''' http://msdn.microsoft.com/en-us/bb737936.aspx
  ''' </summary>
  <TestFixture()> _
  Public Class CountTest
    Inherits TestBase

    <Test()> _
    Public Sub Test_Simple()
      CheckParsedQuery( _
          (Function() QuerySource.Customers.Count()), _
          "Customers => Count()")
    End Sub

  End Class

End Namespace
