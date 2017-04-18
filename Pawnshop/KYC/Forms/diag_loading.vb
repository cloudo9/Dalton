﻿Imports System.Threading

Public Class diag_loading

    Friend Sub Set_Bar(ByVal val As Integer)
        pbLoading.Maximum = val
    End Sub

    Friend Sub Reset_Bar()
        pbLoading.Value = 0
    End Sub

    Friend Sub Add_Bar()
        Dim th As Thread
        th = New Thread(AddressOf addBar)
        th.Start()
    End Sub

    Private Delegate Sub addBar_callback()
    Private Sub addBar()
        If pbLoading.InvokeRequired Then
            pbLoading.Invoke(New addBar_callback(AddressOf addBar))
        Else
            If pbLoading.Value < pbLoading.Maximum Then
                pbLoading.Value += 1
                lblStatus.Text = String.Format("{0}%", ((pbLoading.Value / pbLoading.Maximum) * 100).ToString("F2"))
            End If
        End If
    End Sub

End Class