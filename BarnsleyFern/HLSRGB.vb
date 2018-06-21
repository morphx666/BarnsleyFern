<Serializable()>
Public Class HLSRGB
    Private mRed As Byte = 0
    Private mGreen As Byte = 0
    Private mBlue As Byte = 0

    Private mHue As Single = 0
    Private mLuminance As Single = 0
    Private mSaturation As Single = 0
    Private mAlpha As Byte

    Public Structure HueLumSat
        Private mH As Single
        Private mL As Single
        Private mS As Single

        Public Sub New(hue As Single, lum As Single, sat As Single)
            mH = hue
            mL = lum
            mS = sat
        End Sub

        Public Property Hue() As Single
            Get
                Return mH
            End Get
            Set(value As Single)
                mH = value
            End Set
        End Property

        Public Property Lum() As Single
            Get
                Return mL
            End Get
            Set(value As Single)
                mL = value
            End Set
        End Property

        Public Property Sat() As Single
            Get
                Return mS
            End Get
            Set(value As Single)
                mS = value
            End Set
        End Property
    End Structure

    Public Sub New(color As Color)
        mAlpha = color.A
        mRed = color.R
        mGreen = color.G
        mBlue = color.B
        ToHLS()
    End Sub

    Public Sub New(hue As Single, luminance As Single, saturation As Single)
        mAlpha = 255
        mHue = hue
        mLuminance = luminance
        mSaturation = saturation
        ToRGB()
    End Sub

    Public Sub New(alpha As Byte, red As Byte, green As Byte, blue As Byte)
        mAlpha = alpha
        mRed = red
        mGreen = green
        mBlue = blue
    End Sub

    Public Sub New(hlsrgb As HLSRGB)
        mRed = hlsrgb.Red
        mBlue = hlsrgb.Blue
        mGreen = hlsrgb.Green
        mLuminance = hlsrgb.Luminance
        mHue = hlsrgb.Hue
        mSaturation = hlsrgb.Saturation
    End Sub

    Public Sub New()
    End Sub

    Public Property Red() As Byte
        Get
            Return mRed
        End Get
        Set(value As Byte)
            mRed = value
            ToHLS()
        End Set
    End Property

    Public Property Green() As Byte
        Get
            Return mGreen
        End Get
        Set(value As Byte)
            mGreen = value
            ToHLS()
        End Set
    End Property

    Public Property Blue() As Byte
        Get
            Return mBlue
        End Get
        Set(value As Byte)
            mBlue = value
            ToHLS()
        End Set
    End Property

    Public Property Luminance() As Single
        Get
            Return mLuminance
        End Get
        Set(value As Single)
            mLuminance = ChkLum(value)
            ToRGB()
        End Set
    End Property

    Public Property Hue() As Single
        Get
            Return mHue
        End Get
        Set(value As Single)
            mHue = ChkHue(value)
            ToRGB()
        End Set
    End Property

    Public Property Saturation() As Single
        Get
            Return mSaturation
        End Get
        Set(value As Single)
            mSaturation = ChkSat(value)
            ToRGB()
        End Set
    End Property

    Public Property HLS() As HueLumSat
        Get
            Return New HueLumSat(mHue, mLuminance, mSaturation)
        End Get
        Set(value As HueLumSat)
            mHue = ChkHue(value.Hue)
            mLuminance = ChkLum(value.Lum)
            mSaturation = ChkSat(value.Sat)
            ToRGB()
        End Set
    End Property

    Public Property Color() As Color
        Get
            Return Color.FromArgb(mAlpha, mRed, mGreen, mBlue)
        End Get
        Set(value As Color)
            mAlpha = Color.A
            mRed = value.R
            mGreen = value.G
            mBlue = value.B
            ToHLS()
        End Set
    End Property

    Public Sub LightenColor(lightenBy As Single)
        mLuminance *= (1.0F + lightenBy)
        If mLuminance > 1.0F Then Luminance = 1.0F
        ToRGB()
    End Sub

    Public Property Alpha() As Integer
        Get
            Return mAlpha
        End Get
        Set(value As Integer)
            mAlpha = ChkAlpha(value)
        End Set
    End Property

    Public Sub DarkenColor(darkenBy As Single)
        Luminance *= darkenBy
        ToRGB()
    End Sub

    Private Sub ToHLS()
        Dim minval As Byte = Math.Min(mRed, Math.Min(mGreen, mBlue))
        Dim maxval As Byte = Math.Max(mRed, Math.Max(mGreen, mBlue))

        Dim mdiff As Single = CSng(maxval) - CSng(minval)
        Dim msum As Single = CSng(maxval) + CSng(minval)

        mLuminance = msum / 510.0F

        If maxval = minval Then
            mSaturation = 0.0F
            mHue = 0.0F
        Else
            Dim rnorm As Single = (maxval - mRed) / mdiff
            Dim gnorm As Single = (maxval - mGreen) / mdiff
            Dim bnorm As Single = (maxval - mBlue) / mdiff

            If mLuminance <= 0.5F Then
                mSaturation = mdiff / msum
            Else
                mSaturation = mdiff / (510.0F - msum)
            End If

            If mRed = maxval Then mHue = 60.0F * (6.0F + bnorm - gnorm)
            If mGreen = maxval Then mHue = 60.0F * (2.0F + rnorm - bnorm)
            If mBlue = maxval Then mHue = 60.0F * (4.0F + gnorm - rnorm)
            If mHue > 360.0F Then mHue = Hue - 360.0F
        End If
    End Sub

    Private Sub ToRGB()
        If mSaturation = 0.0 Then
            mAlpha = 255
            mRed = CByte(mLuminance * 255.0F)
            mGreen = mRed
            mBlue = mRed
        Else
            Dim rm1 As Single
            Dim rm2 As Single

            If mLuminance <= 0.5F Then
                rm2 = mLuminance + mLuminance * mSaturation
            Else
                rm2 = mLuminance + mSaturation - mLuminance * mSaturation
            End If
            rm1 = 2.0F * mLuminance - rm2
            mRed = ToRGB1(rm1, rm2, mHue + 120.0F)
            mGreen = ToRGB1(rm1, rm2, mHue)
            mBlue = ToRGB1(rm1, rm2, mHue - 120.0F)
        End If
    End Sub

    Private Function ToRGB1(rm1 As Single, rm2 As Single, rh As Single) As Byte
        If rh > 360.0F Then
            rh -= 360.0F
        ElseIf rh < 0.0F Then
            rh += 360.0F
        End If

        If (rh < 60.0F) Then
            rm1 = rm1 + (rm2 - rm1) * rh / 60.0F
        ElseIf (rh < 180.0F) Then
            rm1 = rm2
        ElseIf (rh < 240.0F) Then
            rm1 = rm1 + (rm2 - rm1) * (240.0F - rh) / 60.0F
        End If

        'TODO: Fix this... we shouldn't have to use a Try/Catch
        Try
            Return CByte(rm1 * 255)
        Catch
            Return CByte(255)
        End Try
    End Function

    Private Function ChkHue(value As Single) As Single
        If value < 0.0F Then
            value = Math.Abs((360.0F + value) Mod 360.0F)
        ElseIf value > 360.0F Then
            value = value Mod 360.0F
        End If

        Return value
    End Function

    Private Function ChkLum(value As Single) As Single
        If (value < 0.0F) OrElse (value > 1.0F) Then
            If value < 0.0F Then
                value = 0
            ElseIf value > 1.0F Then
                value = 1.0F
            End If
        End If

        Return value
    End Function

    Private Function ChkSat(value As Single) As Single
        If value < 0.0F Then
            value = 0
        ElseIf value > 1.0F Then
            value = 1.0F
        End If

        Return value
    End Function

    Private Function ChkAlpha(value As Integer) As Integer
        If value < 0 Then
            value = 0
        ElseIf value > 255 Then
            value = 255
        End If

        Return value
    End Function
End Class
