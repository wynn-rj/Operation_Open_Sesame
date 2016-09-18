<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class Form1
    Inherits System.Windows.Forms.Form

    'Form overrides dispose to clean up the component list.
    <System.Diagnostics.DebuggerNonUserCode()> _
    Protected Overrides Sub Dispose(ByVal disposing As Boolean)
        Try
            If disposing AndAlso components IsNot Nothing Then
                components.Dispose()
            End If
        Finally
            MyBase.Dispose(disposing)
        End Try
    End Sub

    'Required by the Windows Form Designer
  Private components As System.ComponentModel.IContainer

  'NOTE: The following procedure is required by the Windows Form Designer
  'It can be modified using the Windows Form Designer.  
  'Do not modify it using the code editor.
  <System.Diagnostics.DebuggerStepThrough()> _
  Private Sub InitializeComponent()
    Me.components = New System.ComponentModel.Container
    Me.ButtonPing = New System.Windows.Forms.Button
    Me.Timer1 = New System.Windows.Forms.Timer(Me.components)
    Me.Timer2 = New System.Windows.Forms.Timer(Me.components)
    Me.SuspendLayout()
    '
    'ButtonPing
    '
    Me.ButtonPing.Location = New System.Drawing.Point(163, 5)
    Me.ButtonPing.Name = "ButtonPing"
    Me.ButtonPing.Size = New System.Drawing.Size(80, 30)
    Me.ButtonPing.TabIndex = 0
    Me.ButtonPing.Text = "Ping All"
    Me.ButtonPing.UseVisualStyleBackColor = True
    '
    'Timer1
    '
    '
    'Timer2
    '
    '
    'Form1
    '
    Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
    Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
    Me.ClientSize = New System.Drawing.Size(417, 345)
    Me.Controls.Add(Me.ButtonPing)
    Me.Name = "Form1"
    Me.Text = "Open Doors 1.0"
    Me.ResumeLayout(False)

  End Sub
  Friend WithEvents ButtonPing As System.Windows.Forms.Button
  Friend WithEvents Timer1 As System.Windows.Forms.Timer
  Friend WithEvents Timer2 As System.Windows.Forms.Timer

End Class
