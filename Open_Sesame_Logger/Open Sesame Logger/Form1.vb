Public Class Form1
  Public myDataGrid As simpleGrid

  Private setupFlag As Boolean = True
  Public nameList As ArrayList
  Public IPlist As ArrayList


  Private Sub Form1_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load
    Debug.Print("Load")
    Me.DoubleBuffered = True
    Me.Width = 1000
    Me.Height = 500

    myDataGrid = New simpleGrid
    Me.Controls.Add(myDataGrid)

    ' CleanList()
    startUp()
    LoadDataGrid()
    setupFlag = False
    myDataGrid.Item(1, 0).Value = "Test 2"

    Timer2.Interval = 100
    Timer2.Enabled = True

  End Sub


  Private Sub Form1_Resize(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Resize
    If setupFlag = False Then
      ButtonPing.Location = New Point((Me.Width / 2 - 40), 10)
      myDataGrid.Location = New Point(10, 50)
      myDataGrid.Size = New Point(Me.ClientSize.Width - 50, Me.ClientSize.Height - 70)
      myDataGrid.Columns(1).Width = myDataGrid.Size.Width - 120

      For i = 0 To myDataGrid.RowCount - 1
        For j = 0 To myDataGrid.ColumnCount - 1
          Debug.Print(myDataGrid.Item(j, i).Value)
        Next
      Next
    End If

  End Sub

  Sub LoadDataGrid()
    myDataGrid.Visible = False

    nameList = ReadFile(0)
    IPlist = ReadFile(1)

    Dim btn As New DataGridViewButtonColumn
    btn.HeaderText = "Click Data"
    btn.Text = "Ping"
    btn.Name = "btn"
    btn.UseColumnTextForButtonValue = True

    'dataTable.Columns.Add("Status")
    'dataTable.Columns.Add("Name")
    With myDataGrid
      '.DataSource = dataTable
      .ColumnCount = 3
      '.Columns.Insert(2, btn)
      .Size = New Point(Me.ClientSize.Width - 20, Me.ClientSize.Height - 70)
      .AllowUserToAddRows = False
      .AllowUserToDeleteRows = False
      .AllowUserToOrderColumns = False
      .AllowUserToResizeRows = False
      .RowHeadersVisible = False
      .ColumnHeadersVisible = True
      .RowCount = nameList.Count
      .ScrollBars = ScrollBars.Vertical
      .Columns(0).HeaderText = "Status"
      .Columns(1).HeaderText = "Name"
      .Columns(2).HeaderText = Nothing
      .Columns(0).Width = 50
      .Columns(2).Width = 70

      .RowsDefaultCellStyle.SelectionBackColor = .DefaultCellStyle.BackColor
      .RowsDefaultCellStyle.SelectionForeColor = .DefaultCellStyle.ForeColor

      If .Width > 90 Then
        .Columns(1).Width = (.Width - 120)
      Else
        .Columns(1).Width = 100
      End If


    End With
    If nameList.Count > 0 Then
      For index As Integer = 0 To nameList.Count - 1
        'dataTable.Rows.Add("", nameList(index))


        With myDataGrid
          .Item(1, index).Value = nameList(index)
          .Item(0, index).Style.BackColor = Color.Yellow
        End With

      Next
    End If
    Debug.Print(myDataGrid.Columns.Count)

    myDataGrid.Visible = True
    Debug.Print("Load end")
  End Sub


  Sub AddRowData()
    Dim tmp As ArrayList

    tmp = ReadFile(0)
    myDataGrid.RowCount = tmp.Count
    Debug.Print("RowCount " & tmp.Count)

    If tmp.Count > 0 Then
      For index As Integer = 0 To tmp.Count - 1
        With myDataGrid
          .Item(1, index).Value = tmp(index)
          Debug.Print(.Item(1, index).Value)
          .Item(0, index).Style.BackColor = Color.Yellow
        End With

      Next
    End If

    myDataGrid.Refresh()

  End Sub


  Private Sub DataGrid_CellContentClick(ByVal sender As Object, ByVal e As System.Windows.Forms.DataGridViewCellEventArgs)
    If e.ColumnIndex = 2 Then
      myDataGrid.Item(0, e.RowIndex).Style.BackColor = Color.Yellow
      If Ping(IPlist(e.RowIndex), nameList(e.RowIndex)) = True Then
        myDataGrid.Item(0, e.RowIndex).Style.BackColor = Color.Green
      Else
        myDataGrid.Item(0, e.RowIndex).Style.BackColor = Color.Red
      End If

    End If

  End Sub



  Private Sub DataGrid_CellClick(ByVal sender As Object, ByVal e As DataGridViewCellEventArgs)
    If e.ColumnIndex = 2 Then
      myDataGrid.Item(0, e.RowIndex).Style.BackColor = Color.Yellow
      If Ping(IPlist(e.RowIndex), nameList(e.RowIndex)) = True Then
        Timer1.Enabled = False
        myDataGrid.Item(0, e.RowIndex).Style.BackColor = Color.Green
      Else
        myDataGrid.Item(0, e.RowIndex).Style.BackColor = Color.Red
      End If

    End If

  End Sub


  Private Sub Timer1_Tick(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Timer1.Tick
    timerIndex = timerIndex + 1
    Debug.Print(timerIndex)
  End Sub


  Private Sub Timer2_Tick(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Timer2.Tick
    If (myFlag = True) Then
      AddRowData()
      myFlag = False
    End If
  End Sub
End Class



Public Class simpleGrid
  Inherits System.Windows.Forms.DataGridView

  Public Sub New()
    MyBase.New()
    Me.Visible = False
    Me.EditMode = DataGridViewEditMode.EditOnKeystrokeOrF2


    Me.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter
    Me.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter

    Me.RowHeadersVisible = False
    Me.ColumnHeadersVisible = False

    Me.ShowEditingIcon = False
    Me.ShowRowErrors = False
    Me.ShowCellToolTips = False
    Me.ShowCellErrors = False
    Me.AllowUserToAddRows = False
    Me.AllowUserToDeleteRows = False
    Me.AllowUserToResizeColumns = False
    Me.AllowUserToResizeRows = False
    Me.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing
    Me.RowHeadersWidthSizeMode = DataGridViewRowHeadersWidthSizeMode.DisableResizing
    Me.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.Single
    Me.EnableHeadersVisualStyles = False
    Me.AutoSize = False
    Me.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None
    Me.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.None
    Me.MultiSelect = True
    Me.DoubleBuffered = True
    Me.DefaultCellStyle.BackColor = Color.Aquamarine
    Me.DefaultCellStyle.ForeColor = Color.Black

  End Sub

  ' don't do anything if ctrl key is pressed
  ' this fixes the ctrl-click (highlight multiple sections of grid)
  ' which causes problems with smooth 
  Protected Overrides Sub OnCellMouseDown(ByVal e As System.Windows.Forms.DataGridViewCellMouseEventArgs)
    If ((Control.ModifierKeys & Keys.Control) = Keys.Control) Then
      MyBase.OnCellMouseDown(e)
    End If
  End Sub

  ' prevent going to the next row after edit 
  ' replace enter with ctrl-enter
  Protected Overrides Function ProcessCmdKey(ByRef msg As System.Windows.Forms.Message, ByVal keyData As System.Windows.Forms.Keys) As Boolean
    If (keyData = Keys.Enter) Then
      keyData = (Keys.Control Or Keys.Enter)
    End If
    Return MyBase.ProcessCmdKey(msg, keyData)
  End Function

  Private Sub simpleGrid_EditingControlShowing(ByVal sender As Object, ByVal e As System.Windows.Forms.DataGridViewEditingControlShowingEventArgs) Handles Me.EditingControlShowing
    Dim tb As TextBox = CType(e.Control, TextBox)
    AddHandler tb.KeyPress, AddressOf TextBox_KeyPress
  End Sub

  Private Sub TextBox_KeyPress(ByVal sender As System.Object, ByVal e As System.Windows.Forms.KeyPressEventArgs)
    If (Char.IsDigit(e.KeyChar) Or e.KeyChar = "." Or e.KeyChar = "-") Then
      ' all is good 
    ElseIf e.KeyChar = vbBack Then
    Else
      e.Handled = True
    End If
  End Sub

  Protected Overrides Function ProcessDialogKey(ByVal keyData As System.Windows.Forms.Keys) As Boolean
    If (keyData = Keys.Tab Or (keyData = (Keys.Tab Or Keys.Shift))) Then
      ' this only happens at the end of the row
      'Debug.Print("Tab")
    Else
      Return MyBase.ProcessDialogKey(keyData)
    End If

  End Function

End Class