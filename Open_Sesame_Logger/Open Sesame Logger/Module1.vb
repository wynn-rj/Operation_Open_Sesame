
Imports System.Net
Imports System.Net.Sockets
Imports System.Text
'Imports System.Threading
Imports System.IO


Module Module1
  '' Constants
  Const LOG_IN_CODE As Integer = 111
  Const CHECK_IN_CODE As Integer = 115


  '' Variables
  Public asyncReceivingUdp As New UdpClient
  Public asyncRemoteEndPoint As New IPEndPoint(0, 0)
  Public pingContPass As Boolean = False
  Public asyncReceivedMessage As String
  Public timerIndex As Integer = 0

  Public myFlag As Boolean

  Sub startUp()
    asyncReceivingUdp = New UdpClient(43210)
    asyncRemoteEndPoint = New IPEndPoint(IPAddress.Any, 43211)
    asyncReceivingUdp.BeginReceive(AddressOf routeMessage, Nothing)

  End Sub


  Sub routeMessage(ByVal ar As IAsyncResult)
    Dim receivedBytes As Byte()
    Dim ipAdd As String

    receivedBytes = asyncReceivingUdp.EndReceive(ar, asyncRemoteEndPoint)

    Dim actualData(receivedBytes.Length - 2) As Byte

    ipAdd = asyncRemoteEndPoint.Address.ToString
    For index As Integer = 0 To receivedBytes.Length - 2
      actualData(index) = receivedBytes(index + 1)
    Next

    asyncReceivedMessage = Encoding.ASCII.GetString(actualData)

    Debug.Print("routeMessage " & asyncReceivedMessage)
    Debug.Print("receivedBytes(0) " & receivedBytes(0))

    Select Case receivedBytes(0)
      Case 111
        pingContPass = True
        '      asyncReceivingUdp.BeginReceive(AddressOf routeMessage, Nothing)

      Case 115
        Connect(asyncReceivedMessage, ipAdd)
        '     asyncReceivingUdp.BeginReceive(AddressOf routeMessage, Nothing)
        'Form1.AddRowData()
        myFlag = True
    End Select


  End Sub




  Public Sub Connect(ByVal receivedDataString As String, ByVal ResponseIP As String)
    'Dim listeningIP As String
    'listeningIP = "192.168.1.200"
    Dim sendingUDPClient As New UdpClient(43211)    'Create a TcpClient
    Dim message As String
    Dim bytes As Int32

    bytes = 0
    Debug.Print("Connect")

    'Try

    Debug.Print("Recieved: {0}", receivedDataString)

    saveNameIP(ResponseIP, receivedDataString)

    sendingUDPClient.Connect(ResponseIP, 43211)
    message = "Data Recieved " & receivedDataString
    Debug.Print("Data Recieved " & receivedDataString)
    Dim data As Byte() = System.Text.Encoding.ASCII.GetBytes(message)
    sendingUDPClient.Send(data, data.Length)
    sendingUDPClient.Close()


    Debug.Print(ResponseIP)
    Debug.Print("Sent: {0}", message)

    'Catch e As Exception
    'MsgBox(e.Message)
    'Debug.Print(e.Message)
    'End Try

  End Sub 'Connect


  Sub saveNameIP(ByVal IPAddress As String, ByVal name As String)   'Overwrites IPs
    Dim namesList As New ArrayList 'Stopes each teacher's name
    Dim IPList As New ArrayList    'Store's each teacher's corresponding IP
    Dim splitString(2) As String



    'Try
    Using reader As StreamReader = New StreamReader("IPReference.txt")
      While (Not reader.EndOfStream)               'This loop makes a list of names with corresponding IPs.
        splitString = reader.ReadLine().Split(":") 'Splits each line up into a name and IP.
        namesList.Add(splitString(0))
        IPList.Add(splitString(1))
      End While
    End Using


    Dim isNameInList As Boolean
    isNameInList = False
    Using writer As StreamWriter = New StreamWriter("IPReference.txt")
      For index As Integer = 0 To namesList.Count - 1 'Finds if and where the name is in the list.
        If IPList(index).ToString = IPAddress Then 'If the name is in the file
          isNameInList = True
          writer.WriteLine(name + ":" + IPAddress)
          Exit For
        Else
          writer.WriteLine(namesList(index) + ":" + IPList(index)) 'Else rewrite original name to file
        End If
      Next
      If Not isNameInList Then 'If name is not in list, write it at the end.
        writer.WriteLine(name + ":" + IPAddress)
      End If
    End Using

    'Catch e As Exception
    'MsgBox(e.Message)
    '
    'Using writer As StreamWriter = New StreamWriter("IPReference.txt", False)
    'writer.WriteLine(name + ":" + IPAddress) 'If file is empty, write the stuff in.
    'End Using

    'End Try
  End Sub


  Sub CheckIn()
    Dim namesList As New ArrayList 'Stopes each teacher's name
    Dim IPList As New ArrayList    'Store's each teacher's corresponding IP
    Dim splitString(2) As String
    Dim sendingUDPClient As New UdpClient(43211)    'Create a TcpClient
    Dim receivingUDPClient As New UdpClient(43210)
    Dim message() As Byte = Nothing
    Dim receivedData As Byte()
    Dim receivedDataString As String


    Using reader As StreamReader = New StreamReader("IPReference.txt")
      While (Not reader.EndOfStream)               'This loop makes a list of names with corresponding IPs.
        splitString = reader.ReadLine().Split(":") 'Splits each line up into a name and IP.
        namesList.Add(splitString(0))
        IPList.Add(splitString(1))
      End While
    End Using

    For index As Integer = 0 To namesList.Count - 1
      sendingUDPClient.Connect(IPList(index), 42311)
      Dim data As Byte() = System.Text.Encoding.ASCII.GetBytes(namesList(index))
      For i As Integer = 1 To data.Length + 1
        message(i) = data(i - 1)
      Next
      sendingUDPClient.Send(message, message.Length)
      sendingUDPClient.Close()
      receivedData = receivingUDPClient.Receive(IPList(index))
      receivedDataString = System.Text.Encoding.ASCII.GetString(receivedData)
      If receivedDataString <> namesList(index) Then
        namesList(index) = receivedDataString
      End If
    Next

    Using writer As StreamWriter = New StreamWriter("IPReference.txt", False)
      For index As Integer = 0 To namesList.Count - 1
        writer.WriteLine(namesList(index) + ":" + IPList(index))
      Next
    End Using

  End Sub


  Sub CleanList()
    Using writer As StreamWriter = New StreamWriter("IPReference.txt", False)
      writer.WriteLine("EXAMPLE---DONT PING:192.168.1.1")
    End Using

  End Sub


  Public Function Ping(ByVal IPAdd As String, ByVal personName As String) As Boolean
    Dim sendingUDPClient As New UdpClient(IPAdd, 5001)    'Create a TcpClient
    Dim data As Byte() = System.Text.Encoding.ASCII.GetBytes("o" & personName)
    pingContPass = False

    sendingUDPClient.Connect(IPAdd, 5001)
    sendingUDPClient.Send(data, data.Length)
    sendingUDPClient.Close()

    timerIndex = 0
    Form1.Timer1.Enabled = True  ''doesnt work
    While pingContPass = False
      If timerIndex > 300 Then
        Form1.Timer1.Enabled = False
        timerIndex = 0
        Return False
      End If
      Application.DoEvents()
    End While

    Return True

  End Function


  'Returns an arraylist of all the people.
  Function ReadFile(ByVal returnValue As Integer)
    Dim splitString(2) As String
    Dim namesList As New ArrayList
    Dim IPList As New ArrayList
    Dim people As New ArrayList
    Using reader As StreamReader = New StreamReader("IPReference.txt")
      While (Not reader.EndOfStream)               'This loop makes a list of names with corresponding IPs.
        splitString = reader.ReadLine().Split(":") 'Splits each line up into a name and IP.
        namesList.Add(splitString(0))
        IPList.Add(splitString(1))
      End While
    End Using
    If returnValue = 0 Then
      Return namesList
    Else
      Return IPList
    End If
  End Function


  'Returns an arraylist of all the people.
  Public Sub ReadFileNew()
    Dim splitString(2) As String
    Dim people As New ArrayList
    Using reader As StreamReader = New StreamReader("IPReference.txt")
      While (Not reader.EndOfStream)               'This loop makes a list of names with corresponding IPs.
        splitString = reader.ReadLine().Split(":") 'Splits each line up into a name and IP.
        Form1.nameList.Add(splitString(0))
        Form1.IPlist.Add(splitString(1))
      End While
    End Using
  End Sub

End Module

