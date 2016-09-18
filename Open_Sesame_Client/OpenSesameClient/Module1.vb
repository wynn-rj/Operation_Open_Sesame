Imports System.Threading
Imports System.Object
Imports System.Net.WebSockets.WebSocket
Imports System.Net.Sockets
Imports System.Text
Imports System.Net.WebSockets
Imports System.Net
Imports System.IO


Module Module1
  Const SENDING_PORT As Integer = 43210
  Const RECEIVING_PORT As Integer = 43211

  Private WithEvents Timer1 As New System.Timers.Timer

  Dim serverTcp As TcpListener
  Dim serverThread As Thread
  Dim serverUDP As New UdpClient(RECEIVING_PORT)
  Dim serverRemoteEndPoint As New IPEndPoint(IPAddress.Any, 0)
  Dim timerInt As Integer
  Dim computerName As String
  Dim listenerIP As String




  Sub Main()
    '' Start server
    readConfig()
    startUpCheckIn()


    serverThread = New Thread(AddressOf serverProc)
    serverThread.Start()
    serverUDP.BeginReceive(AddressOf CheckIn, Nothing)

  End Sub

  Private Sub readConfig()
    Dim splitString(1) As String
    Dim header As New ArrayList
    Dim configDetails As New ArrayList
    Dim hostName As String = Dns.GetHostName

    Try
      If File.Exists("config.txt") Then

        Using configReader = New StreamReader("config.txt")
          While (Not configReader.EndOfStream)
            splitString = configReader.ReadLine().Split(":")
            header.Add(splitString(0))
            configDetails.Add(splitString(1))
          End While
        End Using

        For index = 0 To header.Count
          Select Case header(index).ToString
            Case "Computer_Name"
              If configDetails(index).ToString = "Example.Name" Then
                MsgBox("You need to fix your config file before running the program again", MsgBoxStyle.Exclamation & MsgBoxStyle.MsgBoxSetForeground, "Error!")
                End
              End If
              computerName = configDetails(index).ToString
            Case "ListenerIP"
              If configDetails(index).ToString = "000.000.000.000" Then
                MsgBox("You need to fix your config file before running the program again", MsgBoxStyle.Exclamation & MsgBoxStyle.MsgBoxSetForeground, "Error!")
                End
              End If
              listenerIP = configDetails(index).ToString
            Case Else
              MsgBox("Your config file is messed up", MsgBoxStyle.Exclamation & MsgBoxStyle.MsgBoxSetForeground, "Error!")


          End Select
        Next

      Else

        Using createConfig As StreamWriter = New StreamWriter("config.txt", False)
          createConfig.WriteLine("Computer_Name:Example.Name")
          createConfig.WriteLine("ListenerIP:000.000.000.000")
        End Using
        MsgBox("Your config file was missing. Please fix it", MsgBoxStyle.Exclamation & MsgBoxStyle.MsgBoxSetForeground, "Error!")
        End
      End If
    Catch ex As Exception

    End Try


  End Sub


  Private Sub serverProc()
    '' Listen to port RECEIVING_PORT

    serverTcp = New TcpListener(RECEIVING_PORT)
    serverTcp.Start()



    displayLine("Listen to port RECEIVING_PORT ...")

    '' Accept any connection
    While (True)
      Dim curSocket As Socket = serverTcp.AcceptSocket()
      Dim thread As New Thread(AddressOf clientProc)
      thread.Start(curSocket)
    End While
  End Sub


  Private Sub clientProc(ByVal sck As Socket)
    Dim netStream As New NetworkStream(sck)
    Dim netReader As New IO.StreamReader(netStream)
    Dim netWriter As New IO.StreamWriter(netStream)


    Dim key As String = ""

    '' Tells the console that it has accepted a new connection
    displayLine("Accept new connection ...")

    '' Reading handshake message
    While (True)
      Dim line As String = netReader.ReadLine()
      If line.Length = 0 Then
        Exit While
      End If

      If (line.StartsWith("Sec-WebSocket-Key: ")) Then        'Extracts the key from the handshake message
        key = line.Split(":")(1).Trim()
      End If


      displayLine("Data: " & line)      'Prints all handshake data to the Screen
    End While



    '' Calculate accept-key
    key += "258EAFA5-E914-47DA-95CA-C5AB0DC85B11"
    key = getSHA1Hash(key)

    '' Response handshake message
    Dim response As String
    response = "HTTP/1.1 101 Switching Protocols" & vbCrLf
    response &= "Upgrade: websocket" & vbCrLf
    response &= "Connection: Upgrade" & vbCrLf
    response &= "Sec-WebSocket-Accept: " & key & vbCrLf & vbCrLf
    netWriter.Write(response)
    netWriter.Flush()

    '' Creates Recieving Message Variables
    Dim dataSent As [Byte]() = Nothing
    Dim dataRecieved(1024) As [Byte]
    Dim bytes As Int32

    '' Recieve Message and then send message
    Dim messageConverted As String = Nothing
    Dim convertMessage As String = Nothing
    While (True)

      '' Checks to see if the socket is still connected, if not it tells the console that it has been disconnected and stops listening on that socket
      If (sck.Connected = False) Then
        displayLine("------Socket Disconnected------")
        Exit While
      End If
      bytes = sck.Receive(dataRecieved, dataRecieved.Length, 0)       'Determines amount of bytes recieved

      '' Reads OP-code to decide what to do with recieved message
      Select Case dataRecieved(0)
        Case 136        'Closes Socket
          bytes = 0
          displayLine("!Disconnect Called!")
          sck.Disconnect(True)        'Disconnects the socket

        Case 129       'Recieves message and calls decoder
          While (bytes > 0)
            displayLine("------Recieving Data------")
            displayLine("Amount of data recieved: " & bytes & " bytes")

            '' Safety Check
            bytes = 0
            If (bytes = 0) Then
              deocdeMessage(dataRecieved)     'Calls decoder to decode and display message
              SendMessage(sck, "Someone has been notified")     'Sends a message to the server
            End If

          End While
        Case Else        'Recieves Check in and responds
          MsgBox("AN ERROR OCCURED", MsgBoxStyle.Exclamation & MsgBoxStyle.MsgBoxSetForeground, "Error!")
          End
      End Select

    End While
  End Sub


  Sub startUpCheckIn()
    While True
      Try

        Dim sendingUDPClient As New UdpClient(listenerIP, SENDING_PORT)    'Create a TcpClient
        Dim RemoteIpEndPoint As New IPEndPoint(IPAddress.Any, 0)
        Dim receivedData As Byte()
        Dim receivedDataString As String
        Dim computerNameAndOpCode As String
        'Dim bytes As Int32
        Dim expectedAnswer As String


        '' Prepares to send Message
        receivedDataString = Nothing
        'bytes = 0

        computerNameAndOpCode = "s" & computerName
        Dim data As Byte() = System.Text.Encoding.ASCII.GetBytes(computerNameAndOpCode)      'Converts computer name to a byte array


        '' Sends Computer Name to OOS Logger
        sendingUDPClient.Connect(listenerIP, SENDING_PORT)
        sendingUDPClient.Send(data, data.Length)
        displayLine("Sent: " & computerNameAndOpCode)
        sendingUDPClient.Close()

        AddHandler Timer1.Elapsed, AddressOf tick
        Timer1.Interval = 1000
        timerInt = 0
        Timer1.Enabled = True


        '' Recieves return message from OOS Logger
        receivedData = serverUDP.Receive(RemoteIpEndPoint)
        receivedDataString = System.Text.Encoding.ASCII.GetString(receivedData)
        Timer1.Enabled = False
        displayLine("Recieved: " & receivedDataString)

        '' Double checks to make sure an error didn't occur
        expectedAnswer = "Data Recieved " & computerName
        If receivedDataString <> expectedAnswer Then
          MsgBox("Recieved message is incorrect", MsgBoxStyle.Exclamation & MsgBoxStyle.MsgBoxSetForeground, "Error!")
        End If
        Exit While
      Catch e As Exception
        displayLine(e.ToString)
        System.Threading.Thread.Sleep(15000)

      End Try
    End While

  End Sub


  Sub SendMessage(ByVal sck As Socket, ByVal message As String)
    displayLine("Sending: '" & message & "' to Server")

    '' Converts message into a byte array
    Dim rawData = System.Text.Encoding.UTF8.GetBytes(message)

    Dim frameCount = 0
    Dim frame(10) As Byte

    frame(0) = CByte(129)

    '' Encodes the message being sent to the server
    If rawData.Length <= 125 Then
      frame(1) = CByte(rawData.Length + 1)
      frameCount = 2
    ElseIf rawData.Length >= 126 AndAlso rawData.Length <= 65535 Then
      frame(1) = CByte(126)
      Dim len = CByte(rawData.Length)
      frame(2) = CByte(((len >> 8) & CByte(255)))
      frame(3) = CByte((len & CByte(255)))
      frameCount = 4
    Else
      frame(1) = CByte(127)
      Dim len = CByte(rawData.Length)
      frame(2) = CByte(((len >> 56) & CByte(255)))
      frame(3) = CByte(((len >> 48) & CByte(255)))
      frame(4) = CByte(((len >> 40) & CByte(255)))
      frame(5) = CByte(((len >> 32) & CByte(255)))
      frame(6) = CByte(((len >> 24) & CByte(255)))
      frame(7) = CByte(((len >> 16) & CByte(255)))
      frame(8) = CByte(((len >> 8) & CByte(255)))
      frame(9) = CByte((len & CByte(255)))
      frameCount = 10
    End If

    Dim bLength = frameCount + rawData.Length
    Dim reply(bLength) As Byte
    Dim bLim = 0

    '' Formats Encoded Bytes into a sendable message
    For i = 0 To frameCount - 1
      reply(bLim) = frame(i)
      bLim += 1
    Next

    For i = 0 To rawData.Length - 1
      reply(bLim) = rawData(i)
      bLim += 1
    Next

    '' Sends the encoded message to the server
    sck.Send(reply)
    displayLine("Message sent")
  End Sub


  Sub deocdeMessage(ByVal rawData() As Byte)

    Dim firstMask As Integer
    Dim secondByte As Byte
    Dim length As Byte
    Dim mask(3) As Byte
    Dim firstDataByte As Integer
    Dim j As Integer
    Dim decodedMessageReadable As String


    '' Determines values for variables
    j = 0
    secondByte = rawData(1)         'The second byte which is the encoded form of the length

    '' The first bit of the second byte is always 1 which means the data is masked (= encoded). Messages from the client to the server are always masked. You must change the first bit to 0
    length = (secondByte And &H7F) ' may not be the actual length in the two special cases      
    displayLine("Message length: " & length & " bytes")

    '' Using the length of the message, determines where the first mask starts
    firstMask = 2
    If length = 126 Then
      firstMask = 4

    Else
      If length = 127 Then
        firstMask = 10

      End If
    End If

    '' Seperates the masks (decoding keys) from the message
    mask(0) = rawData(firstMask)
    mask(1) = rawData(firstMask + 1)
    mask(2) = rawData(firstMask + 2)
    mask(3) = rawData(firstMask + 3)

    '' Four bytes further. Where the message starts
    firstDataByte = firstMask + 4

    '' Prints the recieved data to the console with the prefix of what part of the data it is
    For i = 0 To firstDataByte + length - 1
      Select Case (i)
        Case 0
          displayLine("OP-Code: " & rawData(i))
        Case 1
          displayLine("Message Length encoded: " & rawData(i))
        Case firstMask
          displayLine("Mask 1: " & rawData(i))
        Case (firstMask + 1)
          displayLine("Mask 2: " & rawData(i))
        Case (firstMask + 2)
          displayLine("Mask 3: " & rawData(i))
        Case (firstMask + 3)
          displayLine("Mask 4: " & rawData(i))
        Case Is > (firstMask + 3)
          displayLine("Encoded Message Data Byte " & (i - (firstMask + 3)) & ": " & rawData(i))
        Case Else
          displayLine("Encoded Message Length: " & rawData(i))
      End Select
    Next

    '' Creates a byte array to house the decoded message
    Dim decodedMessage(length - 1) As Byte

    '' Decodes the message using the masks
    For i = firstDataByte To firstDataByte + length - 1
      decodedMessage(j) = rawData(i) Xor mask(j Mod 4)

      displayLine("Decoded Message Data Byte " & (j + 1) & " :" & decodedMessage(j))        'Prints decoded bytes
      j = j + 1
    Next

    '' Transform Byte array into string using ASCII conversion table
    decodedMessageReadable = System.Text.Encoding.ASCII.GetString(decodedMessage)

    '' Displays decoded message on cosole, beeps, and makes a message box pop up in front of everything
    displayLine("===" & decodedMessageReadable & "===")
    Beep()
    My.Computer.Audio.Play(My.Resources.openthedoor, AudioPlayMode.Background)
    MsgBox(decodedMessageReadable & " is outside the door", MsgBoxStyle.OkOnly & MsgBoxStyle.MsgBoxSetForeground, "Alert!")     'Halts code until "OK" button pressed
  End Sub


  Sub displayLine(ByVal message As String)
    Console.WriteLine(message)
    Using writer As StreamWriter = New StreamWriter("log.txt", True)
      writer.WriteLine(message)
    End Using
  End Sub


  Sub CheckIn(ByVal ar As IAsyncResult)

    Dim sendingUDPClient As New UdpClient(listenerIP, SENDING_PORT)    'Create a TcpClient
    Dim receivedBytes As Byte()
    Dim receivedDataString As String
    Dim data() As Byte = Nothing

    receivedBytes = serverUDP.EndReceive(ar, serverRemoteEndPoint)
    receivedDataString = Encoding.ASCII.GetString(receivedBytes)
    ' For i As Integer = 0 To receivedBytes.Length
    'data(i) = receivedBytes(i + 1)
    ' Next

    'receivedDataString = System.Text.Encoding.ASCII.GetString(data)
    'If receivedDataString <> computerName Then
    'Dim compNameByte() As Byte = System.Text.Encoding.ASCII.GetBytes(computerName)
    'sendingUDPClient.Connect(listenerIP, SENDING_PORT)
    'sendingUDPClient.Send(compNameByte, compNameByte.Length)
    'displayLine("Sent: " & computerName)
    'sendingUDPClient.Close()
    'Else
    'sendingUDPClient.Connect(listenerIP, SENDING_PORT)
    'sendingUDPClient.Send(data, data.Length)
    'displayLine("Sent: " & receivedDataString)
    'sendingUDPClient.Close()
    'End If

    sendingUDPClient.Connect(listenerIP, SENDING_PORT)
    sendingUDPClient.Send(receivedBytes, receivedBytes.Length)
    displayLine("Sent: " & receivedDataString)
    sendingUDPClient.Close()
    serverUDP.BeginReceive(AddressOf CheckIn, Nothing)

  End Sub


  Private Sub tick(ByVal sender As Object, ByVal e As System.Timers.ElapsedEventArgs)
    timerInt = timerInt + 1
    displayLine(timerInt)
    If timerInt = 30 Then
      timerInt = 0
      Dim receivedDataString As String
      Dim computerNameAndOpCode As String

      '' Prepares to send Message
      receivedDataString = Nothing
      'bytes = 0

      computerNameAndOpCode = "s" & computerName
      Dim data As Byte() = System.Text.Encoding.ASCII.GetBytes(computerNameAndOpCode)
      Dim sendingUDPClient As New UdpClient(listenerIP, SENDING_PORT)
      sendingUDPClient.Connect(listenerIP, SENDING_PORT)
      sendingUDPClient.Send(Data, Data.Length)
      displayLine("Sent: " & computerNameAndOpCode)
      sendingUDPClient.Close()
    End If
  End Sub

  Function getSHA1Hash(ByVal strToHash As String) As String
    Dim sha1Obj As New System.Security.Cryptography.SHA1CryptoServiceProvider
    Dim bytesToHash() As Byte = System.Text.Encoding.ASCII.GetBytes(strToHash)
    Dim result As String

    '' Used to generate return key for handshake message
    bytesToHash = sha1Obj.ComputeHash(bytesToHash)
    result = Convert.ToBase64String(bytesToHash)

    Return result
  End Function

End Module

