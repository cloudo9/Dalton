﻿Imports System.Data.Odbc
Imports System.IO
Imports System.Text
Public Class frmAdminPanel
    Const fn As String = "\Post_Log.dat"
    Dim dt As New DataTable

    Private mySql As String
    Private fillData As String
    Private SpecSave As ItemSpecs
    Dim ds As New DataSet

    Private Scheme As Hashtable
    Private SelectedItem As ItemClass

    Dim fromOtherForm As Boolean = False
    Dim frmOrig As formSwitch.FormName

    Private Sub frmAdminPanel_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load
        clearfields()
        txtClassification.Focus()

        LoadScheme()
    End Sub

    Friend Sub Load_ItemSpecification(ByVal Item As ItemClass)
        SelectedItem = Item
        txtClassification.Text = Item.ClassName
        txtCategory.Text = Item.Category
        txtDescription.Text = Item.Description
        txtPrintLayout.Text = Item.PrintLayout
        cboSchemename.Text = GetSchemeByID(Item.InterestScheme.SchemeID)

        If Item.isRenewable = "True" Then
            rbYes.Checked = True
            rbNo.Checked = False
        Else
            rbYes.Checked = False
            rbNo.Checked = True
        End If

        SelectedItem = Item
        LoadSpec(Item.ID)
        btnUpdate.Enabled = True
    End Sub

    Friend Sub LoadSpec(ByVal ID As Integer)
        Dim da As New OdbcDataAdapter
        Dim mySql As String = "SELECT * FROM TBLSPECS WHERE ItemID = '" & ID & "'"
        Console.WriteLine("SQL: " & mySql)
        Dim ds As DataSet = LoadSQL(mySql)
        Dim dr As DataRow

        dgSpecs.Rows.Clear()
        For Each dr In ds.Tables(0).Rows
            AddItemSpecs(dr)
        Next
        reaDOnlyTrue()
        For a As Integer = 0 To dgSpecs.Rows.Count - 1
            dgSpecs.Rows(a).ReadOnly = True
        Next
        btnSave.Enabled = False
    End Sub

    Private Sub AddItemSpecs(ByVal ItemSpecs As DataRow)
        Dim tmpItem As New ItemSpecs
        tmpItem.LoadItemSpecs_row(ItemSpecs)
        dgSpecs.Rows.Add(tmpItem.SpecID, tmpItem.ShortCode, tmpItem.SpecName, tmpItem.SpecType.ToString, tmpItem.SpecLayout.ToString, tmpItem.UnitOfMeasure, tmpItem.isRequired.ToString)
    End Sub

    Private Sub LoadScheme()
        Dim mySql As String = "SELECT * FROM TBLINTSCHEMES"
        Dim ds As DataSet = LoadSQL(mySql)

        Scheme = New Hashtable
        cboSchemename.Items.Clear()
        Dim tmpName As String, tmpID As Integer

        For Each dr As DataRow In ds.Tables(0).Rows
            With dr
                tmpID = .Item("schemeID")
                tmpName = .Item("SCHEMENAME")
            End With
            Scheme.Add(tmpID, tmpName)
            cboSchemename.Items.Add(tmpName)
        Next

    End Sub

    Private Function GetSchemeByID(ByVal id As Integer) As String
        For Each el As DictionaryEntry In Scheme
            If el.Key = id Then
                Return el.Value
            End If
        Next

        Return "N/A"
    End Function

    Private Function GetSchemeID(ByVal name As String) As Integer
        For Each el As DictionaryEntry In Scheme
            If el.Value = name Then
                Return el.Key
            End If
        Next

        Return 0
    End Function

    Friend Sub clearfields()
        txtCategory.Text = ""
        txtClassification.Text = ""
        txtDescription.Text = ""
        txtPrintLayout.Text = ""
        'txtSearch.Text = ""
        'txtReferenceNumber.Text = ""
        cboModuleName.Text = ""
        dgSpecs.Rows.Clear()
        btnUpdate.Enabled = False

    End Sub

    Private Function isValid() As Boolean

        If txtClassification.Text = "" Then txtClassification.Focus() : Return False
        If txtCategory.Text = "" Then txtCategory.Focus() : Return False
        If txtDescription.Text = "" Then txtDescription.Focus() : Return False
        If txtPrintLayout.Text = "" Then txtPrintLayout.Focus() : Return False
        If dgSpecs.CurrentCell.Value Is Nothing Then dgSpecs.Focus() : Return False
        If cboSchemename.Text = "" Then cboSchemename.Focus() : Return False

        Return True
    End Function

    Public Function IsDataGridViewEmpty(ByRef dataGridView As DataGridView) As Boolean
        Dim isEmpty As Boolean = True
        For Each row As DataGridViewRow In From row1 As DataGridViewRow In dataGridView.Rows _
        Where (From cell As DataGridViewCell In row1.Cells Where Not String.IsNullOrEmpty(cell.Value)).Any(Function(cell) _
        Not String.IsNullOrEmpty(Trim(cell.Value.ToString())))
            isEmpty = False
        Next
        Return isEmpty
    End Function

    Private Sub btnSave_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnSave.Click
        If btnSave.Text = "&Save" Then
            SaveItems()
        Else
            ModifyItems()
        End If
       
    End Sub

    Private Sub btnUpdate_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnUpdate.Click
        If btnUpdate.Text = "&Edit" Then
            btnUpdate.Text = "&Cancel"
            btnSave.Enabled = True
            btnSave.Text = "&Update"

            ReadOnlyFalse()
            txtClassification.Enabled = False
        Else
            Dim ans As DialogResult = MsgBox("Do you want Cancel?", MsgBoxStyle.YesNo + MsgBoxStyle.DefaultButton2 + MsgBoxStyle.Information)
            If ans = Windows.Forms.DialogResult.No Then Exit Sub
            btnUpdate.Text = "&Edit"
            btnSave.Enabled = False
            btnSave.Text = "&Save"
            ReadOnlyTrue()
        End If
    End Sub

    Private Sub btnSearch_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnSearch.Click
        Dim secured_str As String = txtSearch.Text
        secured_str = DreadKnight(secured_str)
        frmItemList.SearchSelect(secured_str, FormName.frmPawningV2_SpecsValue)
        frmItemList.Show()
    End Sub

    Private Sub ReadOnlyTrue()
        txtCategory.ReadOnly = True
        txtClassification.ReadOnly = True
        txtDescription.ReadOnly = True
        txtPrintLayout.ReadOnly = True
        cboSchemename.Enabled = False
        rbNo.Enabled = False
        rbYes.Enabled = False
        For a As Integer = 0 To dgSpecs.Rows.Count - 1
            dgSpecs.Rows(a).ReadOnly = True
        Next
    End Sub

    Friend Sub ReadOnlyFalse()
        txtCategory.ReadOnly = False
        ' txtClassifiction.ReadOnly = False
        txtDescription.ReadOnly = False
        txtPrintLayout.ReadOnly = False
        cboSchemename.Enabled = True
        rbNo.Enabled = True
        rbYes.Enabled = True
        For a As Integer = 0 To dgSpecs.Rows.Count - 1
            dgSpecs.Rows(a).ReadOnly = False
        Next
    End Sub

    Private Sub txtSearch_KeyDown(ByVal sender As System.Object, ByVal e As System.Windows.Forms.KeyEventArgs)
        If e.KeyCode = Keys.Enter Then
            btnSearch.PerformClick()
        End If
    End Sub

    '"""""""""""""""""""""""""""""export""""""""""""""""""""""""""""""""""""""""
    Private Sub cboModuleName_SelectedIndexChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles cboModuleName.SelectedIndexChanged
        If cboModuleName.Text = "" And cboModuleName.Visible Then Exit Sub
        If cboModuleName.Visible Then
            Select Case cboModuleName.Text
                Case "Money Transfer"
                    ExportModType = ModuleType.MoneyTransfer
                Case "Branch"
                    ExportModType = ModuleType.Branch
                Case "Cash"
                    ExportModType = ModuleType.Cash
                Case "Item"
                    ExportModType = ModuleType.ITEM
                Case "Interest"
                    ExportModType = ModuleType.Interest
                Case "Currency"
                    ExportModType = ModuleType.Currency

            End Select
        End If
        GenerateModule()
        'lvModule.View = View.Details
        'lvModule.CheckBoxes = True
        'lvModule.Columns(1).DisplayIndex = lvModule.Columns.Count - 1

    End Sub

    Enum ModuleType As Integer
        MoneyTransfer = 0
        Branch = 1
        Cash = 2
        ITEM = 3
        Interest = 4
        Currency = 5
    End Enum

    Friend ExportModType As ModuleType = ModuleType.MoneyTransfer

    Private Sub GenerateModule()
        Select Case ExportModType
            Case ModuleType.MoneyTransfer
                ModCharge()
            Case ModuleType.Branch
                ModBranches()
            Case ModuleType.Cash
                Modcash()
            Case ModuleType.ITEM
                ModITEM()
            Case ModuleType.Interest
                ModRate()
            Case ModuleType.Currency
                ModCurrency()
        End Select
    End Sub

#Region "Procedures"

    Private Sub ModBranches()
        fillData = "tblBranches"
        mySql = "SELECT * FROM " & fillData
        mySql &= " ORDER BY BranchID ASC"

        ds = LoadSQL(mySql, fillData)
        'dgvPawnshop.DataSource = ds.Tables(fillData)
    End Sub

    Private Sub Modcash()
        fillData = "tblCash"
        mySql = "SELECT * FROM " & fillData
        mySql &= " WHERE CashID <> 0"
        mySql &= " ORDER BY CashID ASC"

        ds = LoadSQL(mySql, fillData)
        ' dgvPawnshop.DataSource = ds.Tables(fillData)
    End Sub

    Private Sub ModCharge()
        fillData = "tblCharge"
        mySql = "SELECT * FROM " & fillData
        mySql &= " ORDER BY ID ASC"

        ds = LoadSQL(mySql, fillData)
        'dgvPawnshop.DataSource = ds.Tables(fillData)
    End Sub

    Private Sub ModRate()
        mySql = "SELECT * FROM TBLINTSCHEMES"
        ds = LoadSQL(mySql, "TBLINTSCHEMES")
        mySql = "SELECT * FROM TBLINTSCHEME_DETAILS"
        Dim tblIntSchDetails As DataSet = LoadSQL(mySql, "TBLINTSCHEME_DETAILS")

        Dim otherTBL As New DataTable
        otherTBL = tblIntSchDetails.Tables("TBLINTSCHEME_DETAILS")
        ds.Tables.Add(otherTBL.Copy)
    End Sub

    Private Sub ModCurrency()
       
        fillData = "tblCurrency"
        mySql = "SELECT * FROM " & fillData
        mySql &= " ORDER BY CurrencyID ASC"

        ds = LoadSQL(mySql, fillData)
        'dgvPawnshop.DataSource = ds.Tables(fillData)
    End Sub

    Private Sub ModITEM()
        mySql = "SELECT * FROM tblItem"
        ds = LoadSQL(mySql, "tblItem")
        mySql = "SELECT * FROM tblSpecs"
        Dim tblIntSchDetails As DataSet = LoadSQL(mySql, "tblSpecs")

        Dim otherTBL As New DataTable
        otherTBL = tblIntSchDetails.Tables("tblSpecs")
        ds.Tables.Add(otherTBL.Copy)
    End Sub

#End Region

    Private Sub SFD_FileOk(ByVal sender As System.Object, ByVal e As System.ComponentModel.CancelEventArgs) Handles SFD.FileOk
        Dim ans As DialogResult = MsgBox("Do you want to save this?", MsgBoxStyle.YesNo + MsgBoxStyle.DefaultButton2 + MsgBoxStyle.Information)
        If ans = Windows.Forms.DialogResult.No Then Exit Sub

        Dim fn As String = SFD.FileName
        ExportConfig(fn, ds)
        MsgBox("Data Exported", MsgBoxStyle.Information)
    End Sub

    Private Sub oFd_FileOk(ByVal sender As System.Object, ByVal e As System.ComponentModel.CancelEventArgs) Handles oFd.FileOk
        Dim fn As String = oFd.FileName
        FileChecker(fn)
        'dgPawnshop2.DataSource = FileChecker(fn)
    End Sub

    Sub ExportConfig(ByVal url As String, ByVal serialDS As DataSet)
        If System.IO.File.Exists(url) Then System.IO.File.Delete(url)

        Dim fsEsk As New System.IO.FileStream(url, IO.FileMode.CreateNew)
        Dim esk As New Runtime.Serialization.Formatters.Binary.BinaryFormatter
        esk.Serialize(fsEsk, serialDS)
        fsEsk.Close()
    End Sub

    Sub FileChecker(ByVal url As String)
        Dim fs As New System.IO.FileStream(url, IO.FileMode.Open)
        Dim bf As New Runtime.Serialization.Formatters.Binary.BinaryFormatter()

        Dim serialDS As DataSet
        Try
            serialDS = bf.Deserialize(fs)
        Catch ex As Exception
            MsgBox("It seems the file is being tampered.", MsgBoxStyle.Critical)
            fs.Close()
        End Try
        fs.Close()
        'Dim ds As DataSet = serialDS
        'dgvPawnshop.DataSource = ds.Tables(0)
        'dgPawnshop2.DataSource = ds.Tables(1)
    End Sub

    Private Sub ShowDataInLvw(ByVal data As DataTable, ByVal lvw As ListView)
        lvw.View = View.Details
        lvw.GridLines = True
        lvw.Columns.Clear()
        lvw.Items.Clear()
        For Each col As DataColumn In data.Columns
            lvw.Columns.Add(col.ToString)
        Next
        For Each row As DataRow In data.Rows
            Dim lst As ListViewItem
            lst = lvw.Items.Add(If(row(0) IsNot Nothing, row(0).ToString, ""))
            For i As Integer = 1 To data.Columns.Count - 1
                lst.SubItems.Add(If(row(i) IsNot Nothing, row(i).ToString, ""))
            Next
        Next
    End Sub

    Private Sub txtSearch_KeyDown_1(ByVal sender As System.Object, ByVal e As System.Windows.Forms.KeyEventArgs) Handles txtSearch.KeyDown
        If e.KeyCode = Keys.Enter Then
            btnSearch.PerformClick()
        End If
    End Sub

    Private Sub btnExport_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnExport.Click
        'If txtReferenceNumber.Text = "" Then txtReferenceNumber.Focus() : Exit Sub
        'If cmbModuleName.Text = "" Then cmbModuleName.Focus() : Exit Sub
        'If lvModule.Items.Count <= 0 Then Exit Sub
        'If lblCount.Text = "Count: 0" Then Exit Sub

        'For Each item As ListViewItem In Me.lvModule.Items
        '    If item.Checked = False Then
        '        item.Remove()
        '    End If
        'Next

        'Console.WriteLine("Item Count: " & lvModule.Items.Count)

        'FromListView(dt, lvModule)

        'Dim path As String = String.Format("{1}{0}.dat", fn, str)
        'If Not File.Exists(path) Then
        '    Dim a As FileStream
        '    a = File.Create(path)
        '    a.Dispose()
        'End If

        'SFD.ShowDialog()



        'txtReferenceNumber.Text = ""
        'cmbModuleName.SelectedItem = Nothing

        'lvModule.Columns.Clear()
        'lvModule.Items.Clear()
        If ds.Tables.Count < 1 Then MsgBox("No Module Found!", MsgBoxStyle.Critical) : Exit Sub
        SFD.ShowDialog()
        saveModname()
    End Sub

    Public Sub FromListView(ByVal table As DataTable, ByVal lvw As ListView)
        table.Clear()
        dt.Columns.Clear()
        dt.Rows.Clear()
        Dim columns = lvw.Columns.Count

        For Each column As ColumnHeader In lvw.Columns
            table.Columns.Add(column.Text)
        Next

        For Each item As ListViewItem In lvw.Items
            Dim cells = New Object(columns - 1) {}
            For i As VariantType = 0 To columns - 1
                cells(i) = item.SubItems(i).Text
            Next
            table.Rows.Add(cells)
        Next
    End Sub

    Private str As String = My.Computer.FileSystem.SpecialDirectories.Desktop
    Private path As String = String.Format("{1}{0}.dat", fn, str)

    Private Sub saveModname()
        If txtRef.Text = Nothing Then
            Exit Sub
        Else
            Dim Post_log As String = _
          String.Format("[{0}] ", Now.ToString("MM/dd/yyyy HH:mm:ss"))

            File.AppendAllText(path, "Date Exported: " & Post_log & vbCrLf & "Reference No: " & txtRef.Text & vbCrLf & _
                               "Module Name: " & cmbModuleName.Text & vbCrLf & "User: " & POSuser.UserName & vbCrLf)
        End If
    End Sub

    Private Sub btnBrowse_Click(ByVal sender As System.Object, ByVal e As System.EventArgs)
        oFd.ShowDialog()

        'lvModule.View = View.Details
        'lvModule.CheckBoxes = True
        'lvModule.Columns(1).DisplayIndex = lvModule.Columns.Count - 1

    End Sub

    Private Sub btnClose_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnClose.Click
        Me.Close()
    End Sub

    Private Sub ModifyItems()
        If Not isValid() Then Exit Sub

        ReadOnlyFalse()
        txtClassification.Enabled = False

        Dim ans As DialogResult = MsgBox("Do you want to Update Item Class?", MsgBoxStyle.YesNo + MsgBoxStyle.DefaultButton2 + MsgBoxStyle.Information)
        If ans = Windows.Forms.DialogResult.No Then Exit Sub

        Dim ColItemsSpecs As New CollectionItemSpecs
        Dim ItemModify As New ItemClass
        With ItemModify
            .ClassName = txtClassification.Text
            .Category = txtCategory.Text
            .Description = txtDescription.Text
            .ID = SelectedItem.ID

            If rdbYes.Checked Then
                .isRenewable = 1
            Else
                .isRenewable = 0
            End If

            .PrintLayout = txtPrintLayout.Text
            .InterestScheme.SchemeID = GetSchemeID(cboSchemename.Text)
            .updated_at = CurrentDate
        End With

        Dim SpecModify As New ItemSpecs
        For Each row As DataGridViewRow In dgSpecs.Rows

            With SpecModify
                .SpecID = row.Cells(0).Value
                .ShortCode = row.Cells(1).Value
                .SpecName = row.Cells(2).Value
                .SpecType = row.Cells(3).Value
                .SpecLayout = row.Cells(4).Value
                .UnitOfMeasure = row.Cells(5).Value
                .isRequired = row.Cells(6).Value

                If .SpecName Is Nothing Or .SpecType Is Nothing _
                    Or .ShortCode Is Nothing Or .SpecLayout Is Nothing Then
                    Exit For
                End If

            End With
            SpecModify.ItemID = SelectedItem.ID
            SpecModify.UpdateSpecs()
        Next
        ItemModify.Update()

        MsgBox("Item Class Updated", MsgBoxStyle.Information)

        btnSave.Enabled = True
        rdbNo.Checked = False
        txtClassification.Focus()
        txtClassification.Enabled = True
        clearfields()
        LoadScheme()
        btnUpdate.Text = "&Edit"
        btnSave.Text = "&Save"
    End Sub

    Private Sub SaveItems()
        If Not isValid() Then Exit Sub
        Dim ans As DialogResult = MsgBox("Do you want to save this Item Class?", MsgBoxStyle.YesNo + MsgBoxStyle.DefaultButton2 + MsgBoxStyle.Information)
        If ans = Windows.Forms.DialogResult.No Then Exit Sub

        Dim ItemSave As New ItemClass
        Dim ColItemsSpecs As New CollectionItemSpecs
        With ItemSave
            .ClassName = txtClassification.Text
            .Category = txtCategory.Text
            .Description = txtDescription.Text
            .ClassName = txtClassification.Text

            If rbYes.Checked Then
                .isRenewable = 1
            Else
                .isRenewable = 0
            End If
            .PrintLayout = txtPrintLayout.Text
            .created_at = CurrentDate
            .InterestScheme.SchemeID = GetSchemeID(cboSchemename.Text)
        End With

        For Each row As DataGridViewRow In dgSpecs.Rows
            SpecSave = New ItemSpecs
            With SpecSave
                .ShortCode = row.Cells(1).Value
                .SpecName = row.Cells(2).Value
                .SpecType = row.Cells(3).Value
                .SpecLayout = row.Cells(4).Value
                .UnitOfMeasure = row.Cells(5).Value
                .isRequired = row.Cells(6).Value

                If .SpecName Is Nothing Or .SpecType Is Nothing _
                    Or .ShortCode Is Nothing Or .SpecLayout Is Nothing Then
                    Exit For
                End If
            End With
            SpecSave.SaveSpecs()
            ColItemsSpecs.Add(SpecSave)
        Next
        ItemSave.ItemSpecifications = ColItemsSpecs
        ItemSave.Save_ItemClass()

        MsgBox("Item Class Saved", MsgBoxStyle.Information)
        rdbNo.Checked = False
        txtClassification.Focus()
        clearfields()
        LoadScheme()
    End Sub
End Class