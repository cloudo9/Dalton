﻿Imports TOTP

Module otp

    Public tfa As New OneTimePassword

    Friend Sub Generate_QR()
        tfa.Setup("MLNzxczxczxc@gmail.com.com")
        Console.WriteLine(tfa.QRCode_URL)
    End Sub
    Friend Function VerifyPIN(ByVal pin As String) As Boolean
        Dim MOD_NAME As String = "Settings"
        Dim isValid As Boolean = tfa.isCorrect(pin)
        If Not isValid Then Return False

        Dim mySql As String = String.Format("SELECT * FROM TBLOTP WHERE PIN = '{0}'", pin)
        Dim ds As DataSet = LoadSQL(mySql), fillData As String = "tblOTP"

        If ds.Tables(0).Rows.Count = 0 Then
            Dim NewOtp As New ClassOtp(MOD_NAME, pin)

            Return isValid
        End If

        Return False
    End Function
End Module
