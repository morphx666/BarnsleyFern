Imports System.Threading

' https://en.wikipedia.org/wiki/Barnsley_fern

Public Class FormMain
    Private bmpScale As Integer = 1
    ' --------------------------------

    Private bmp As DirectBitmap
    Private zoom As Double = 1
    Private xOff As Integer = 0
    Private yOff As Integer = 0
    Private mousePos As Point
    Private isDragging As Boolean

    Private w As Integer
    Private h As Integer

    Private autoRefresh As Thread
    Private runAlgorithm As Thread

    Private Sub FormMain_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Me.SetStyle(ControlStyles.AllPaintingInWmPaint, True)
        Me.SetStyle(ControlStyles.OptimizedDoubleBuffer, True)
        Me.SetStyle(ControlStyles.UserPaint, True)

        InitBitmap()

        AddHandler Me.SizeChanged, Sub() InitBitmap()
        AddHandler Me.MouseWheel, Sub(s1 As Object, e1 As MouseEventArgs)
                                      zoom += e1.Delta / 2000
                                      zoom = Math.Max(0.01, Math.Min(8, zoom))
                                  End Sub
        AddHandler Me.MouseDown, Sub(s1 As Object, e1 As MouseEventArgs)
                                     mousePos = e1.Location
                                     isDragging = True
                                 End Sub
        AddHandler Me.MouseMove, Sub(s1 As Object, e1 As MouseEventArgs)
                                     If isDragging Then
                                         xOff += (e1.X - mousePos.X) / zoom
                                         yOff += (e1.Y - mousePos.Y) / zoom
                                         mousePos = e1.Location
                                     End If
                                 End Sub
        AddHandler Me.MouseUp, Sub() isDragging = False
        AddHandler Me.DoubleClick, Sub()
                                       xOff = 0
                                       yOff = 0
                                       zoom = 1
                                   End Sub
    End Sub

    Private Sub StartThreads()
        autoRefresh = New Thread(Sub()
                                     Do
                                         Thread.Sleep(30)
                                         Me.Invalidate()
                                     Loop
                                 End Sub) With {.IsBackground = True}
        autoRefresh.Start()

        runAlgorithm = New Thread(Sub()
                                      Dim p As New PointF(0, 0)
                                      Dim f1 = Function() New PointF(0.0, 0.16 * p.Y)
                                      Dim f2 = Function() New PointF(0.85 * p.X + 0.04 * p.Y, -0.04 * p.X + 0.85 * p.Y + 1.6)
                                      Dim f3 = Function() New PointF(0.2 * p.X + -0.26 * p.Y, 0.23 * p.X + 0.22 * p.Y + 1.6)
                                      Dim f4 = Function() New PointF(-0.15 * p.X + 0.28 * p.Y, 0.26 * p.X + 0.24 * p.Y + 0.44)
                                      Dim r As New Random()
                                      Dim c As New HLSRGB(Color.Red)

                                      Do
                                          Select Case r.NextDouble()
                                              Case <= 0.01 : p = f1()
                                              Case <= 0.85 : p = f2()
                                              Case <= 0.93 : p = f3() ': Thread.Sleep(1)
                                              Case Else : p = f4()
                                          End Select

                                          c.Hue = Map(p.Y, 0, 9.9983, 360)

                                          bmp.FillRectangle(c.Color,
                                                Map(p.X, -2.182, 2.6558, w) * bmpScale,
                                                (h - Map(p.Y, 0, 9.9983, h)) * bmpScale,
                                                1, 1)
                                      Loop
                                  End Sub) With {.IsBackground = True}
        runAlgorithm.Start()
    End Sub

    Private Function Map(v As Double, rmin As Double, rmax As Double, max As Double) As Double
        Return (v - rmin) / (rmax - rmin) * max
    End Function

    Private Sub InitBitmap()
        Static lastW As Integer = 0
        Static lastH As Integer = 0

        Dim tmpW As Integer = Me.DisplayRectangle.Width
        Dim tmpH As Integer = Me.DisplayRectangle.Height

        If lastW <> tmpW OrElse lastH <> tmpH Then
            If autoRefresh IsNot Nothing Then
                autoRefresh.Abort()
                runAlgorithm.Abort()
            End If

            w = tmpW
            h = tmpH
            lastW = w
            lastH = h

            bmp = New Bitmap(w * bmpScale, h * bmpScale)
            bmp.Clear(Color.Black)
            Me.BackColor = Color.DimGray

            StartThreads()
        End If
    End Sub

    Private Sub FormMain_Paint(sender As Object, e As PaintEventArgs) Handles Me.Paint
        If zoom <> 1 Then
            e.Graphics.TranslateTransform((1 - zoom) * bmp.Width / 2, (1 - zoom) * bmp.Height / 2)
            e.Graphics.ScaleTransform(zoom, zoom)
        End If
        e.Graphics.TranslateTransform(xOff, yOff)
        e.Graphics.DrawImageUnscaled(bmp, 0, 0)

        'e.Graphics.DrawRectangle(Pens.White, si * gridSize, sj * gridSize, 2 * (w \ 2 - si) * gridSize, 2 * (h \ 2 - sj) * gridSize)
    End Sub
End Class
