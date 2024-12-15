namespace SecMan
{
    partial class Form1
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            tabSecMan = new TabControl();
            tpSuperUser = new TabPage();
            txtSuperUserValidate = new Button();
            btnSetSuperUser = new Button();
            txtSuperUserPassword = new TextBox();
            txtSuperUserUserName = new TextBox();
            label13 = new Label();
            label12 = new Label();
            tpSysFeats = new TabPage();
            dgvSys = new DataGridView();
            SysPropId = new DataGridViewTextBoxColumn();
            NameX = new DataGridViewTextBoxColumn();
            Desc = new DataGridViewTextBoxColumn();
            Cat = new DataGridViewTextBoxColumn();
            Posn = new DataGridViewTextBoxColumn();
            Type = new DataGridViewTextBoxColumn();
            Min = new DataGridViewTextBoxColumn();
            Max = new DataGridViewTextBoxColumn();
            ValX = new DataGridViewTextBoxColumn();
            lblSysFeat = new Label();
            cbSys = new ComboBox();
            tpDevDefs = new TabPage();
            dgvDevPermDefs = new DataGridView();
            DevPermDefId = new DataGridViewTextBoxColumn();
            DevPermDefVers = new DataGridViewTextBoxColumn();
            DevPermDefName = new DataGridViewTextBoxColumn();
            DevPermDefDesc = new DataGridViewTextBoxColumn();
            DevPermDefCat = new DataGridViewTextBoxColumn();
            DevPermDefPosn = new DataGridViewTextBoxColumn();
            label2 = new Label();
            label1 = new Label();
            dgvDevPolDefs = new DataGridView();
            DevDefPolId = new DataGridViewTextBoxColumn();
            DevDefPolVers = new DataGridViewTextBoxColumn();
            DevDefPolName = new DataGridViewTextBoxColumn();
            DevDefPolDesc = new DataGridViewTextBoxColumn();
            DevDefPolCat = new DataGridViewTextBoxColumn();
            DevDefPolPosn = new DataGridViewTextBoxColumn();
            DevDefPolValType = new DataGridViewTextBoxColumn();
            DevDefPolValMin = new DataGridViewTextBoxColumn();
            DevDefPolValMax = new DataGridViewTextBoxColumn();
            DevDefPolValDflt = new DataGridViewTextBoxColumn();
            DevDefPolUnits = new DataGridViewTextBoxColumn();
            cbDevDefs = new ComboBox();
            lblDevDef = new Label();
            tpUsers = new TabPage();
            groupBox3 = new GroupBox();
            txtUserAddPassword = new TextBox();
            lblUserAddPassword = new Label();
            txtUserAddUsername = new TextBox();
            lblUserAddUsername = new Label();
            btnUserAdd = new Button();
            btnUserRemoveRole = new Button();
            btnUserAddRole = new Button();
            cboUserRemoveRole = new ComboBox();
            cboUserAddRole = new ComboBox();
            dgvUserRoles = new DataGridView();
            UserRoles = new DataGridViewTextBoxColumn();
            btnUserDelete = new Button();
            dgvUsers = new DataGridView();
            Property = new DataGridViewTextBoxColumn();
            Value = new DataGridViewTextBoxColumn();
            cbUsers = new ComboBox();
            tpRoles = new TabPage();
            btnRoleRename = new Button();
            txtRoleRename = new TextBox();
            btnAddRole = new Button();
            tbRole = new TextBox();
            label5 = new Label();
            btnDeleteRole = new Button();
            btnRoleRemoveUser = new Button();
            btnRoleAddUser = new Button();
            cbRoleUser = new ComboBox();
            label4 = new Label();
            label3 = new Label();
            dgvRoleUsers = new DataGridView();
            User = new DataGridViewTextBoxColumn();
            cbRoles = new ComboBox();
            tpDevs = new TabPage();
            btnDevsDeallocateZone = new Button();
            groupBox2 = new GroupBox();
            btnDevsAllocateZone = new Button();
            cbDevsZones = new ComboBox();
            label28 = new Label();
            txtDevsAllcatedZone = new TextBox();
            label27 = new Label();
            txtDevsDevDef = new TextBox();
            label26 = new Label();
            groupBox1 = new GroupBox();
            btnAddDevice = new Button();
            cbNewDevDevDef = new ComboBox();
            txtNewDevName = new TextBox();
            label22 = new Label();
            label21 = new Label();
            btnDeleteDevice = new Button();
            label14 = new Label();
            cbDevs = new ComboBox();
            tpZones = new TabPage();
            dgvZoneAllocatedRoles = new DataGridView();
            ZoneRoleIds = new DataGridViewTextBoxColumn();
            ZoneRoleNames = new DataGridViewTextBoxColumn();
            dgvZoneAllocatedDevs = new DataGridView();
            ZoneDevIds = new DataGridViewTextBoxColumn();
            ZoneDevs = new DataGridViewTextBoxColumn();
            btnZoneDeallocateDev = new Button();
            btnZoneAllocateDev = new Button();
            btnZoneDeallocateRole = new Button();
            btnZoneAllocateRole = new Button();
            btnDeleteZone = new Button();
            btnAddZone = new Button();
            txtNewZone = new TextBox();
            cbZonesDevs = new ComboBox();
            label25 = new Label();
            cbZonesRoles = new ComboBox();
            label24 = new Label();
            cbZonesZones = new ComboBox();
            label23 = new Label();
            tpZoneDevPols = new TabPage();
            label11 = new Label();
            lblZone = new Label();
            cbZoneDevPolsDevDefs = new ComboBox();
            dgvZoneDevPols = new DataGridView();
            ZoneDevPolId = new DataGridViewTextBoxColumn();
            ZoneDevPolName = new DataGridViewTextBoxColumn();
            ZoneDevPolDesc = new DataGridViewTextBoxColumn();
            ZoneDevPolCat = new DataGridViewTextBoxColumn();
            ZoneDevPolPosn = new DataGridViewTextBoxColumn();
            ZoneDevPolValType = new DataGridViewTextBoxColumn();
            ZoneDevPolValMin = new DataGridViewTextBoxColumn();
            ZoneDevPolValMax = new DataGridViewTextBoxColumn();
            ZoneDevPolValDflt = new DataGridViewTextBoxColumn();
            ZoneDevPolUnits = new DataGridViewTextBoxColumn();
            ZoneDevPolVal = new DataGridViewTextBoxColumn();
            cbZoneDevPolsZones = new ComboBox();
            tpZoneDevSigs = new TabPage();
            label16 = new Label();
            label15 = new Label();
            dgvZoneDevSigs = new DataGridView();
            ZoneDevSigsId = new DataGridViewTextBoxColumn();
            ZoneDevSigsPerm = new DataGridViewTextBoxColumn();
            ZoneDevSigsSign = new DataGridViewTextBoxColumn();
            ZoneDevSigsAuth = new DataGridViewTextBoxColumn();
            ZoneDevSigsNote = new DataGridViewTextBoxColumn();
            cbZoneDevSigsDevDefs = new ComboBox();
            cbZoneDevSigsZones = new ComboBox();
            tpZoneRolePerms = new TabPage();
            label19 = new Label();
            label18 = new Label();
            label17 = new Label();
            cbZoneRolePermsRoles = new ComboBox();
            cbZoneRolePermsDevDefs = new ComboBox();
            cbZoneRolePermsZones = new ComboBox();
            dgvZonePerms = new DataGridView();
            ZoneDevPermId = new DataGridViewTextBoxColumn();
            ZoneDevPerm = new DataGridViewTextBoxColumn();
            ZoneDevPermVal = new DataGridViewTextBoxColumn();
            tpAppPerms = new TabPage();
            label20 = new Label();
            btnGetAppPerms = new Button();
            dgvAppPerms = new DataGridView();
            AppPerms = new DataGridViewTextBoxColumn();
            cboApps = new ComboBox();
            txtAppsUserName = new TextBox();
            label6 = new Label();
            tpDevPerms = new TabPage();
            dgvDevPerms = new DataGridView();
            DevPerms = new DataGridViewTextBoxColumn();
            btnGetDevPerms = new Button();
            label10 = new Label();
            cbDevPermDevs = new ComboBox();
            txtDevPermUserName = new TextBox();
            label8 = new Label();
            label7 = new Label();
            txtSysFeatCommon = new TextBox();
            tabSecMan.SuspendLayout();
            tpSuperUser.SuspendLayout();
            tpSysFeats.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dgvSys).BeginInit();
            tpDevDefs.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dgvDevPermDefs).BeginInit();
            ((System.ComponentModel.ISupportInitialize)dgvDevPolDefs).BeginInit();
            tpUsers.SuspendLayout();
            groupBox3.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dgvUserRoles).BeginInit();
            ((System.ComponentModel.ISupportInitialize)dgvUsers).BeginInit();
            tpRoles.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dgvRoleUsers).BeginInit();
            tpDevs.SuspendLayout();
            groupBox2.SuspendLayout();
            groupBox1.SuspendLayout();
            tpZones.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dgvZoneAllocatedRoles).BeginInit();
            ((System.ComponentModel.ISupportInitialize)dgvZoneAllocatedDevs).BeginInit();
            tpZoneDevPols.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dgvZoneDevPols).BeginInit();
            tpZoneDevSigs.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dgvZoneDevSigs).BeginInit();
            tpZoneRolePerms.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dgvZonePerms).BeginInit();
            tpAppPerms.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dgvAppPerms).BeginInit();
            tpDevPerms.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dgvDevPerms).BeginInit();
            SuspendLayout();
            // 
            // tabSecMan
            // 
            tabSecMan.Controls.Add(tpSuperUser);
            tabSecMan.Controls.Add(tpSysFeats);
            tabSecMan.Controls.Add(tpDevDefs);
            tabSecMan.Controls.Add(tpUsers);
            tabSecMan.Controls.Add(tpRoles);
            tabSecMan.Controls.Add(tpDevs);
            tabSecMan.Controls.Add(tpZones);
            tabSecMan.Controls.Add(tpZoneDevPols);
            tabSecMan.Controls.Add(tpZoneDevSigs);
            tabSecMan.Controls.Add(tpZoneRolePerms);
            tabSecMan.Controls.Add(tpAppPerms);
            tabSecMan.Controls.Add(tpDevPerms);
            tabSecMan.Dock = DockStyle.Fill;
            tabSecMan.Location = new Point(0, 0);
            tabSecMan.Name = "tabSecMan";
            tabSecMan.SelectedIndex = 0;
            tabSecMan.Size = new Size(1245, 585);
            tabSecMan.TabIndex = 6;
            // 
            // tpSuperUser
            // 
            tpSuperUser.Controls.Add(txtSuperUserValidate);
            tpSuperUser.Controls.Add(btnSetSuperUser);
            tpSuperUser.Controls.Add(txtSuperUserPassword);
            tpSuperUser.Controls.Add(txtSuperUserUserName);
            tpSuperUser.Controls.Add(label13);
            tpSuperUser.Controls.Add(label12);
            tpSuperUser.Location = new Point(4, 24);
            tpSuperUser.Name = "tpSuperUser";
            tpSuperUser.Size = new Size(1237, 557);
            tpSuperUser.TabIndex = 11;
            tpSuperUser.Text = "Super User";
            tpSuperUser.UseVisualStyleBackColor = true;
            // 
            // txtSuperUserValidate
            // 
            txtSuperUserValidate.Location = new Point(109, 101);
            txtSuperUserValidate.Name = "txtSuperUserValidate";
            txtSuperUserValidate.Size = new Size(75, 23);
            txtSuperUserValidate.TabIndex = 5;
            txtSuperUserValidate.Text = "Validate";
            txtSuperUserValidate.UseVisualStyleBackColor = true;
            txtSuperUserValidate.Click += txtSuperUserValidate_Click;
            // 
            // btnSetSuperUser
            // 
            btnSetSuperUser.Location = new Point(28, 101);
            btnSetSuperUser.Name = "btnSetSuperUser";
            btnSetSuperUser.Size = new Size(75, 23);
            btnSetSuperUser.TabIndex = 4;
            btnSetSuperUser.Text = "Set";
            btnSetSuperUser.UseVisualStyleBackColor = true;
            btnSetSuperUser.Click += btnSetSuperUser_Click;
            // 
            // txtSuperUserPassword
            // 
            txtSuperUserPassword.Location = new Point(108, 48);
            txtSuperUserPassword.Name = "txtSuperUserPassword";
            txtSuperUserPassword.Size = new Size(100, 23);
            txtSuperUserPassword.TabIndex = 3;
            // 
            // txtSuperUserUserName
            // 
            txtSuperUserUserName.Location = new Point(108, 15);
            txtSuperUserUserName.Name = "txtSuperUserUserName";
            txtSuperUserUserName.Size = new Size(100, 23);
            txtSuperUserUserName.TabIndex = 2;
            // 
            // label13
            // 
            label13.AutoSize = true;
            label13.Location = new Point(28, 48);
            label13.Name = "label13";
            label13.Size = new Size(57, 15);
            label13.TabIndex = 1;
            label13.Text = "Password";
            // 
            // label12
            // 
            label12.AutoSize = true;
            label12.Location = new Point(23, 18);
            label12.Name = "label12";
            label12.Size = new Size(62, 15);
            label12.TabIndex = 0;
            label12.Text = "UserName";
            // 
            // tpSysFeats
            // 
            tpSysFeats.Controls.Add(txtSysFeatCommon);
            tpSysFeats.Controls.Add(label7);
            tpSysFeats.Controls.Add(dgvSys);
            tpSysFeats.Controls.Add(lblSysFeat);
            tpSysFeats.Controls.Add(cbSys);
            tpSysFeats.Location = new Point(4, 24);
            tpSysFeats.Name = "tpSysFeats";
            tpSysFeats.Size = new Size(1237, 557);
            tpSysFeats.TabIndex = 5;
            tpSysFeats.Text = "SysFeats";
            tpSysFeats.UseVisualStyleBackColor = true;
            // 
            // dgvSys
            // 
            dgvSys.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgvSys.Columns.AddRange(new DataGridViewColumn[] { SysPropId, NameX, Desc, Cat, Posn, Type, Min, Max, ValX });
            dgvSys.Dock = DockStyle.Right;
            dgvSys.Location = new Point(283, 0);
            dgvSys.Name = "dgvSys";
            dgvSys.Size = new Size(954, 557);
            dgvSys.TabIndex = 2;
            // 
            // SysPropId
            // 
            SysPropId.HeaderText = "Id";
            SysPropId.Name = "SysPropId";
            SysPropId.ReadOnly = true;
            SysPropId.Width = 50;
            // 
            // NameX
            // 
            NameX.HeaderText = "Name";
            NameX.Name = "NameX";
            NameX.ReadOnly = true;
            // 
            // Desc
            // 
            Desc.HeaderText = "Desc";
            Desc.Name = "Desc";
            // 
            // Cat
            // 
            Cat.HeaderText = "Cat";
            Cat.Name = "Cat";
            // 
            // Posn
            // 
            Posn.HeaderText = "Posn";
            Posn.Name = "Posn";
            // 
            // Type
            // 
            Type.HeaderText = "Type";
            Type.Name = "Type";
            // 
            // Min
            // 
            Min.HeaderText = "Min";
            Min.Name = "Min";
            // 
            // Max
            // 
            Max.HeaderText = "Max";
            Max.Name = "Max";
            // 
            // ValX
            // 
            ValX.HeaderText = "Val";
            ValX.Name = "ValX";
            // 
            // lblSysFeat
            // 
            lblSysFeat.AutoSize = true;
            lblSysFeat.Location = new Point(8, 10);
            lblSysFeat.Name = "lblSysFeat";
            lblSysFeat.Size = new Size(90, 15);
            lblSysFeat.TabIndex = 1;
            lblSysFeat.Text = "System Feature:";
            // 
            // cbSys
            // 
            cbSys.FormattingEnabled = true;
            cbSys.Location = new Point(104, 7);
            cbSys.Name = "cbSys";
            cbSys.Size = new Size(165, 23);
            cbSys.TabIndex = 0;
            // 
            // tpDevDefs
            // 
            tpDevDefs.Controls.Add(dgvDevPermDefs);
            tpDevDefs.Controls.Add(label2);
            tpDevDefs.Controls.Add(label1);
            tpDevDefs.Controls.Add(dgvDevPolDefs);
            tpDevDefs.Controls.Add(cbDevDefs);
            tpDevDefs.Controls.Add(lblDevDef);
            tpDevDefs.Location = new Point(4, 24);
            tpDevDefs.Name = "tpDevDefs";
            tpDevDefs.Size = new Size(1237, 557);
            tpDevDefs.TabIndex = 6;
            tpDevDefs.Text = "DevDefs";
            tpDevDefs.UseVisualStyleBackColor = true;
            // 
            // dgvDevPermDefs
            // 
            dgvDevPermDefs.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            dgvDevPermDefs.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgvDevPermDefs.Columns.AddRange(new DataGridViewColumn[] { DevPermDefId, DevPermDefVers, DevPermDefName, DevPermDefDesc, DevPermDefCat, DevPermDefPosn });
            dgvDevPermDefs.Location = new Point(8, 197);
            dgvDevPermDefs.Name = "dgvDevPermDefs";
            dgvDevPermDefs.Size = new Size(1221, 352);
            dgvDevPermDefs.TabIndex = 5;
            // 
            // DevPermDefId
            // 
            DevPermDefId.HeaderText = "Id";
            DevPermDefId.Name = "DevPermDefId";
            // 
            // DevPermDefVers
            // 
            DevPermDefVers.HeaderText = "Vers";
            DevPermDefVers.Name = "DevPermDefVers";
            // 
            // DevPermDefName
            // 
            DevPermDefName.HeaderText = "Name";
            DevPermDefName.Name = "DevPermDefName";
            // 
            // DevPermDefDesc
            // 
            DevPermDefDesc.HeaderText = "Desc";
            DevPermDefDesc.Name = "DevPermDefDesc";
            // 
            // DevPermDefCat
            // 
            DevPermDefCat.HeaderText = "Cat";
            DevPermDefCat.Name = "DevPermDefCat";
            // 
            // DevPermDefPosn
            // 
            DevPermDefPosn.HeaderText = "Posn";
            DevPermDefPosn.Name = "DevPermDefPosn";
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(8, 179);
            label2.Name = "label2";
            label2.Size = new Size(70, 15);
            label2.TabIndex = 4;
            label2.Text = "Permissions";
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(8, 40);
            label1.Name = "label1";
            label1.Size = new Size(47, 15);
            label1.TabIndex = 3;
            label1.Text = "Policies";
            // 
            // dgvDevPolDefs
            // 
            dgvDevPolDefs.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            dgvDevPolDefs.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgvDevPolDefs.Columns.AddRange(new DataGridViewColumn[] { DevDefPolId, DevDefPolVers, DevDefPolName, DevDefPolDesc, DevDefPolCat, DevDefPolPosn, DevDefPolValType, DevDefPolValMin, DevDefPolValMax, DevDefPolValDflt, DevDefPolUnits });
            dgvDevPolDefs.Location = new Point(8, 58);
            dgvDevPolDefs.Name = "dgvDevPolDefs";
            dgvDevPolDefs.Size = new Size(1221, 105);
            dgvDevPolDefs.TabIndex = 2;
            // 
            // DevDefPolId
            // 
            DevDefPolId.HeaderText = "Id";
            DevDefPolId.Name = "DevDefPolId";
            // 
            // DevDefPolVers
            // 
            DevDefPolVers.HeaderText = "Vers";
            DevDefPolVers.Name = "DevDefPolVers";
            // 
            // DevDefPolName
            // 
            DevDefPolName.HeaderText = "Name";
            DevDefPolName.Name = "DevDefPolName";
            // 
            // DevDefPolDesc
            // 
            DevDefPolDesc.HeaderText = "Desc";
            DevDefPolDesc.Name = "DevDefPolDesc";
            // 
            // DevDefPolCat
            // 
            DevDefPolCat.HeaderText = "Cat";
            DevDefPolCat.Name = "DevDefPolCat";
            // 
            // DevDefPolPosn
            // 
            DevDefPolPosn.HeaderText = "Posn";
            DevDefPolPosn.Name = "DevDefPolPosn";
            // 
            // DevDefPolValType
            // 
            DevDefPolValType.HeaderText = "ValType";
            DevDefPolValType.Name = "DevDefPolValType";
            // 
            // DevDefPolValMin
            // 
            DevDefPolValMin.HeaderText = "ValMin";
            DevDefPolValMin.Name = "DevDefPolValMin";
            // 
            // DevDefPolValMax
            // 
            DevDefPolValMax.HeaderText = "ValMax";
            DevDefPolValMax.Name = "DevDefPolValMax";
            // 
            // DevDefPolValDflt
            // 
            DevDefPolValDflt.HeaderText = "ValDflt";
            DevDefPolValDflt.Name = "DevDefPolValDflt";
            // 
            // DevDefPolUnits
            // 
            DevDefPolUnits.HeaderText = "Units";
            DevDefPolUnits.Name = "DevDefPolUnits";
            // 
            // cbDevDefs
            // 
            cbDevDefs.FormattingEnabled = true;
            cbDevDefs.Location = new Point(114, 5);
            cbDevDefs.Name = "cbDevDefs";
            cbDevDefs.Size = new Size(145, 23);
            cbDevDefs.TabIndex = 1;
            // 
            // lblDevDef
            // 
            lblDevDef.AutoSize = true;
            lblDevDef.Location = new Point(8, 8);
            lblDevDef.Name = "lblDevDef";
            lblDevDef.Size = new Size(100, 15);
            lblDevDef.TabIndex = 0;
            lblDevDef.Text = "Device Definition:";
            // 
            // tpUsers
            // 
            tpUsers.Controls.Add(groupBox3);
            tpUsers.Controls.Add(btnUserRemoveRole);
            tpUsers.Controls.Add(btnUserAddRole);
            tpUsers.Controls.Add(cboUserRemoveRole);
            tpUsers.Controls.Add(cboUserAddRole);
            tpUsers.Controls.Add(dgvUserRoles);
            tpUsers.Controls.Add(btnUserDelete);
            tpUsers.Controls.Add(dgvUsers);
            tpUsers.Controls.Add(cbUsers);
            tpUsers.Location = new Point(4, 24);
            tpUsers.Name = "tpUsers";
            tpUsers.Padding = new Padding(3);
            tpUsers.Size = new Size(1237, 557);
            tpUsers.TabIndex = 0;
            tpUsers.Text = "Users";
            tpUsers.UseVisualStyleBackColor = true;
            // 
            // groupBox3
            // 
            groupBox3.Controls.Add(txtUserAddPassword);
            groupBox3.Controls.Add(lblUserAddPassword);
            groupBox3.Controls.Add(txtUserAddUsername);
            groupBox3.Controls.Add(lblUserAddUsername);
            groupBox3.Controls.Add(btnUserAdd);
            groupBox3.Location = new Point(18, 82);
            groupBox3.Name = "groupBox3";
            groupBox3.Size = new Size(202, 106);
            groupBox3.TabIndex = 19;
            groupBox3.TabStop = false;
            groupBox3.Text = "New User";
            // 
            // txtUserAddPassword
            // 
            txtUserAddPassword.Location = new Point(75, 45);
            txtUserAddPassword.Name = "txtUserAddPassword";
            txtUserAddPassword.Size = new Size(100, 23);
            txtUserAddPassword.TabIndex = 16;
            // 
            // lblUserAddPassword
            // 
            lblUserAddPassword.AutoSize = true;
            lblUserAddPassword.Location = new Point(12, 51);
            lblUserAddPassword.Name = "lblUserAddPassword";
            lblUserAddPassword.Size = new Size(57, 15);
            lblUserAddPassword.TabIndex = 15;
            lblUserAddPassword.Text = "Password";
            // 
            // txtUserAddUsername
            // 
            txtUserAddUsername.Location = new Point(75, 16);
            txtUserAddUsername.Name = "txtUserAddUsername";
            txtUserAddUsername.Size = new Size(100, 23);
            txtUserAddUsername.TabIndex = 14;
            // 
            // lblUserAddUsername
            // 
            lblUserAddUsername.AutoSize = true;
            lblUserAddUsername.Location = new Point(6, 19);
            lblUserAddUsername.Name = "lblUserAddUsername";
            lblUserAddUsername.Size = new Size(63, 15);
            lblUserAddUsername.TabIndex = 13;
            lblUserAddUsername.Text = "Username:";
            // 
            // btnUserAdd
            // 
            btnUserAdd.Location = new Point(100, 74);
            btnUserAdd.Name = "btnUserAdd";
            btnUserAdd.Size = new Size(75, 23);
            btnUserAdd.TabIndex = 10;
            btnUserAdd.Text = "Add";
            btnUserAdd.UseVisualStyleBackColor = true;
            btnUserAdd.Click += btnUserAdd_Click;
            // 
            // btnUserRemoveRole
            // 
            btnUserRemoveRole.Location = new Point(575, 64);
            btnUserRemoveRole.Name = "btnUserRemoveRole";
            btnUserRemoveRole.Size = new Size(108, 23);
            btnUserRemoveRole.TabIndex = 18;
            btnUserRemoveRole.Text = "Remove Role";
            btnUserRemoveRole.UseVisualStyleBackColor = true;
            btnUserRemoveRole.Click += btnUserRemoveRole_Click;
            // 
            // btnUserAddRole
            // 
            btnUserAddRole.Location = new Point(575, 35);
            btnUserAddRole.Name = "btnUserAddRole";
            btnUserAddRole.Size = new Size(108, 23);
            btnUserAddRole.TabIndex = 17;
            btnUserAddRole.Text = "Add Role";
            btnUserAddRole.UseVisualStyleBackColor = true;
            btnUserAddRole.Click += btnUserAddRole_Click;
            // 
            // cboUserRemoveRole
            // 
            cboUserRemoveRole.FormattingEnabled = true;
            cboUserRemoveRole.Location = new Point(691, 65);
            cboUserRemoveRole.Name = "cboUserRemoveRole";
            cboUserRemoveRole.Size = new Size(171, 23);
            cboUserRemoveRole.TabIndex = 16;
            // 
            // cboUserAddRole
            // 
            cboUserAddRole.FormattingEnabled = true;
            cboUserAddRole.Location = new Point(689, 36);
            cboUserAddRole.Name = "cboUserAddRole";
            cboUserAddRole.Size = new Size(173, 23);
            cboUserAddRole.TabIndex = 15;
            // 
            // dgvUserRoles
            // 
            dgvUserRoles.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Right;
            dgvUserRoles.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgvUserRoles.Columns.AddRange(new DataGridViewColumn[] { UserRoles });
            dgvUserRoles.Location = new Point(868, 36);
            dgvUserRoles.Name = "dgvUserRoles";
            dgvUserRoles.Size = new Size(361, 513);
            dgvUserRoles.TabIndex = 14;
            // 
            // UserRoles
            // 
            UserRoles.HeaderText = "Roles";
            UserRoles.Name = "UserRoles";
            // 
            // btnUserDelete
            // 
            btnUserDelete.Location = new Point(214, 34);
            btnUserDelete.Name = "btnUserDelete";
            btnUserDelete.Size = new Size(75, 23);
            btnUserDelete.TabIndex = 8;
            btnUserDelete.Text = "Delete";
            btnUserDelete.UseVisualStyleBackColor = true;
            btnUserDelete.Click += btnUserDelete_Click;
            // 
            // dgvUsers
            // 
            dgvUsers.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left;
            dgvUsers.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgvUsers.Columns.AddRange(new DataGridViewColumn[] { Property, Value });
            dgvUsers.Location = new Point(305, 28);
            dgvUsers.Name = "dgvUsers";
            dgvUsers.Size = new Size(264, 515);
            dgvUsers.TabIndex = 7;
            // 
            // Property
            // 
            Property.HeaderText = "Property";
            Property.Name = "Property";
            // 
            // Value
            // 
            Value.HeaderText = "Value";
            Value.Name = "Value";
            // 
            // cbUsers
            // 
            cbUsers.FormattingEnabled = true;
            cbUsers.Location = new Point(8, 35);
            cbUsers.Name = "cbUsers";
            cbUsers.Size = new Size(200, 23);
            cbUsers.TabIndex = 6;
            // 
            // tpRoles
            // 
            tpRoles.Controls.Add(btnRoleRename);
            tpRoles.Controls.Add(txtRoleRename);
            tpRoles.Controls.Add(btnAddRole);
            tpRoles.Controls.Add(tbRole);
            tpRoles.Controls.Add(label5);
            tpRoles.Controls.Add(btnDeleteRole);
            tpRoles.Controls.Add(btnRoleRemoveUser);
            tpRoles.Controls.Add(btnRoleAddUser);
            tpRoles.Controls.Add(cbRoleUser);
            tpRoles.Controls.Add(label4);
            tpRoles.Controls.Add(label3);
            tpRoles.Controls.Add(dgvRoleUsers);
            tpRoles.Controls.Add(cbRoles);
            tpRoles.Location = new Point(4, 24);
            tpRoles.Name = "tpRoles";
            tpRoles.Padding = new Padding(3);
            tpRoles.Size = new Size(1237, 557);
            tpRoles.TabIndex = 1;
            tpRoles.Text = "Roles";
            tpRoles.UseVisualStyleBackColor = true;
            // 
            // btnRoleRename
            // 
            btnRoleRename.Location = new Point(252, 107);
            btnRoleRename.Name = "btnRoleRename";
            btnRoleRename.Size = new Size(75, 23);
            btnRoleRename.TabIndex = 16;
            btnRoleRename.Text = "Rename";
            btnRoleRename.UseVisualStyleBackColor = true;
            btnRoleRename.Click += btnRoleRename_Click;
            // 
            // txtRoleRename
            // 
            txtRoleRename.Location = new Point(42, 106);
            txtRoleRename.Name = "txtRoleRename";
            txtRoleRename.Size = new Size(200, 23);
            txtRoleRename.TabIndex = 15;
            // 
            // btnAddRole
            // 
            btnAddRole.Location = new Point(247, 36);
            btnAddRole.Name = "btnAddRole";
            btnAddRole.Size = new Size(75, 23);
            btnAddRole.TabIndex = 14;
            btnAddRole.Text = "Add";
            btnAddRole.UseVisualStyleBackColor = true;
            btnAddRole.Click += btnAddRole_Click;
            // 
            // tbRole
            // 
            tbRole.Location = new Point(41, 36);
            tbRole.Name = "tbRole";
            tbRole.Size = new Size(200, 23);
            tbRole.TabIndex = 13;
            // 
            // label5
            // 
            label5.AutoSize = true;
            label5.Location = new Point(6, 40);
            label5.Name = "label5";
            label5.Size = new Size(30, 15);
            label5.TabIndex = 12;
            label5.Text = "Role";
            // 
            // btnDeleteRole
            // 
            btnDeleteRole.Location = new Point(247, 5);
            btnDeleteRole.Name = "btnDeleteRole";
            btnDeleteRole.Size = new Size(75, 23);
            btnDeleteRole.TabIndex = 11;
            btnDeleteRole.Text = "Delete";
            btnDeleteRole.UseVisualStyleBackColor = true;
            btnDeleteRole.Click += btnDeleteRole_Click;
            // 
            // btnRoleRemoveUser
            // 
            btnRoleRemoveUser.Location = new Point(329, 65);
            btnRoleRemoveUser.Name = "btnRoleRemoveUser";
            btnRoleRemoveUser.Size = new Size(75, 23);
            btnRoleRemoveUser.TabIndex = 10;
            btnRoleRemoveUser.Text = "Remove";
            btnRoleRemoveUser.UseVisualStyleBackColor = true;
            btnRoleRemoveUser.Click += btnRoleRemoveUser_Click;
            // 
            // btnRoleAddUser
            // 
            btnRoleAddUser.Location = new Point(247, 64);
            btnRoleAddUser.Name = "btnRoleAddUser";
            btnRoleAddUser.Size = new Size(75, 23);
            btnRoleAddUser.TabIndex = 9;
            btnRoleAddUser.Text = "Add";
            btnRoleAddUser.UseVisualStyleBackColor = true;
            btnRoleAddUser.Click += btnRoleAddUser_Click;
            // 
            // cbRoleUser
            // 
            cbRoleUser.FormattingEnabled = true;
            cbRoleUser.Location = new Point(41, 65);
            cbRoleUser.Name = "cbRoleUser";
            cbRoleUser.Size = new Size(200, 23);
            cbRoleUser.TabIndex = 8;
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Location = new Point(6, 68);
            label4.Name = "label4";
            label4.Size = new Size(33, 15);
            label4.TabIndex = 7;
            label4.Text = " User";
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new Point(6, 9);
            label3.Name = "label3";
            label3.Size = new Size(30, 15);
            label3.TabIndex = 6;
            label3.Text = "Role";
            // 
            // dgvRoleUsers
            // 
            dgvRoleUsers.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Right;
            dgvRoleUsers.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgvRoleUsers.Columns.AddRange(new DataGridViewColumn[] { User });
            dgvRoleUsers.Location = new Point(410, 6);
            dgvRoleUsers.Name = "dgvRoleUsers";
            dgvRoleUsers.Size = new Size(819, 548);
            dgvRoleUsers.TabIndex = 5;
            // 
            // User
            // 
            User.HeaderText = "User";
            User.Name = "User";
            User.Width = 300;
            // 
            // cbRoles
            // 
            cbRoles.FormattingEnabled = true;
            cbRoles.Location = new Point(41, 6);
            cbRoles.Name = "cbRoles";
            cbRoles.Size = new Size(200, 23);
            cbRoles.TabIndex = 3;
            // 
            // tpDevs
            // 
            tpDevs.Controls.Add(btnDevsDeallocateZone);
            tpDevs.Controls.Add(groupBox2);
            tpDevs.Controls.Add(txtDevsAllcatedZone);
            tpDevs.Controls.Add(label27);
            tpDevs.Controls.Add(txtDevsDevDef);
            tpDevs.Controls.Add(label26);
            tpDevs.Controls.Add(groupBox1);
            tpDevs.Controls.Add(btnDeleteDevice);
            tpDevs.Controls.Add(label14);
            tpDevs.Controls.Add(cbDevs);
            tpDevs.Location = new Point(4, 24);
            tpDevs.Name = "tpDevs";
            tpDevs.Size = new Size(1237, 557);
            tpDevs.TabIndex = 2;
            tpDevs.Text = "Devs";
            tpDevs.UseVisualStyleBackColor = true;
            // 
            // btnDevsDeallocateZone
            // 
            btnDevsDeallocateZone.Location = new Point(344, 74);
            btnDevsDeallocateZone.Name = "btnDevsDeallocateZone";
            btnDevsDeallocateZone.Size = new Size(75, 23);
            btnDevsDeallocateZone.TabIndex = 12;
            btnDevsDeallocateZone.Text = "Deallocate";
            btnDevsDeallocateZone.UseVisualStyleBackColor = true;
            btnDevsDeallocateZone.Click += btnDevsDeallocateZone_Click;
            // 
            // groupBox2
            // 
            groupBox2.Controls.Add(btnDevsAllocateZone);
            groupBox2.Controls.Add(cbDevsZones);
            groupBox2.Controls.Add(label28);
            groupBox2.Location = new Point(12, 194);
            groupBox2.Name = "groupBox2";
            groupBox2.Size = new Size(688, 62);
            groupBox2.TabIndex = 11;
            groupBox2.TabStop = false;
            groupBox2.Text = "Allocate to Zone";
            // 
            // btnDevsAllocateZone
            // 
            btnDevsAllocateZone.Location = new Point(332, 22);
            btnDevsAllocateZone.Name = "btnDevsAllocateZone";
            btnDevsAllocateZone.Size = new Size(75, 23);
            btnDevsAllocateZone.TabIndex = 2;
            btnDevsAllocateZone.Text = "Allocate";
            btnDevsAllocateZone.UseVisualStyleBackColor = true;
            btnDevsAllocateZone.Click += btnDevsAllocateZone_Click;
            // 
            // cbDevsZones
            // 
            cbDevsZones.FormattingEnabled = true;
            cbDevsZones.Location = new Point(101, 22);
            cbDevsZones.Name = "cbDevsZones";
            cbDevsZones.Size = new Size(225, 23);
            cbDevsZones.TabIndex = 1;
            // 
            // label28
            // 
            label28.AutoSize = true;
            label28.Location = new Point(8, 21);
            label28.Name = "label28";
            label28.Size = new Size(34, 15);
            label28.TabIndex = 0;
            label28.Text = "Zone";
            // 
            // txtDevsAllcatedZone
            // 
            txtDevsAllcatedZone.Enabled = false;
            txtDevsAllcatedZone.Location = new Point(113, 78);
            txtDevsAllcatedZone.Name = "txtDevsAllcatedZone";
            txtDevsAllcatedZone.Size = new Size(225, 23);
            txtDevsAllcatedZone.TabIndex = 10;
            // 
            // label27
            // 
            label27.AutoSize = true;
            label27.Location = new Point(10, 78);
            label27.Name = "label27";
            label27.Size = new Size(85, 15);
            label27.TabIndex = 9;
            label27.Text = "Allocated zone";
            // 
            // txtDevsDevDef
            // 
            txtDevsDevDef.Enabled = false;
            txtDevsDevDef.Location = new Point(113, 43);
            txtDevsDevDef.Name = "txtDevsDevDef";
            txtDevsDevDef.Size = new Size(225, 23);
            txtDevsDevDef.TabIndex = 8;
            // 
            // label26
            // 
            label26.AutoSize = true;
            label26.Location = new Point(10, 46);
            label26.Name = "label26";
            label26.Size = new Size(97, 15);
            label26.TabIndex = 7;
            label26.Text = "Device Definition";
            // 
            // groupBox1
            // 
            groupBox1.Controls.Add(btnAddDevice);
            groupBox1.Controls.Add(cbNewDevDevDef);
            groupBox1.Controls.Add(txtNewDevName);
            groupBox1.Controls.Add(label22);
            groupBox1.Controls.Add(label21);
            groupBox1.Location = new Point(10, 121);
            groupBox1.Name = "groupBox1";
            groupBox1.Size = new Size(690, 61);
            groupBox1.TabIndex = 6;
            groupBox1.TabStop = false;
            groupBox1.Text = "New Device";
            // 
            // btnAddDevice
            // 
            btnAddDevice.Location = new Point(600, 19);
            btnAddDevice.Name = "btnAddDevice";
            btnAddDevice.Size = new Size(75, 23);
            btnAddDevice.TabIndex = 4;
            btnAddDevice.Text = "Add";
            btnAddDevice.UseVisualStyleBackColor = true;
            btnAddDevice.Click += btnAddDevice_Click;
            // 
            // cbNewDevDevDef
            // 
            cbNewDevDevDef.FormattingEnabled = true;
            cbNewDevDevDef.Location = new Point(437, 20);
            cbNewDevDevDef.Name = "cbNewDevDevDef";
            cbNewDevDevDef.Size = new Size(157, 23);
            cbNewDevDevDef.TabIndex = 3;
            // 
            // txtNewDevName
            // 
            txtNewDevName.Location = new Point(102, 17);
            txtNewDevName.Name = "txtNewDevName";
            txtNewDevName.Size = new Size(226, 23);
            txtNewDevName.TabIndex = 2;
            // 
            // label22
            // 
            label22.AutoSize = true;
            label22.Location = new Point(334, 23);
            label22.Name = "label22";
            label22.Size = new Size(97, 15);
            label22.TabIndex = 1;
            label22.Text = "Device Definition";
            // 
            // label21
            // 
            label21.AutoSize = true;
            label21.Location = new Point(9, 20);
            label21.Name = "label21";
            label21.Size = new Size(39, 15);
            label21.TabIndex = 0;
            label21.Text = "Name";
            // 
            // btnDeleteDevice
            // 
            btnDeleteDevice.Location = new Point(344, 13);
            btnDeleteDevice.Name = "btnDeleteDevice";
            btnDeleteDevice.Size = new Size(75, 23);
            btnDeleteDevice.TabIndex = 5;
            btnDeleteDevice.Text = "Delete";
            btnDeleteDevice.UseVisualStyleBackColor = true;
            btnDeleteDevice.Click += btnDeleteDevice_Click;
            // 
            // label14
            // 
            label14.AutoSize = true;
            label14.Location = new Point(10, 14);
            label14.Name = "label14";
            label14.Size = new Size(42, 15);
            label14.TabIndex = 4;
            label14.Text = "Device";
            // 
            // cbDevs
            // 
            cbDevs.FormattingEnabled = true;
            cbDevs.Location = new Point(113, 14);
            cbDevs.Name = "cbDevs";
            cbDevs.Size = new Size(225, 23);
            cbDevs.TabIndex = 0;
            // 
            // tpZones
            // 
            tpZones.Controls.Add(dgvZoneAllocatedRoles);
            tpZones.Controls.Add(dgvZoneAllocatedDevs);
            tpZones.Controls.Add(btnZoneDeallocateDev);
            tpZones.Controls.Add(btnZoneAllocateDev);
            tpZones.Controls.Add(btnZoneDeallocateRole);
            tpZones.Controls.Add(btnZoneAllocateRole);
            tpZones.Controls.Add(btnDeleteZone);
            tpZones.Controls.Add(btnAddZone);
            tpZones.Controls.Add(txtNewZone);
            tpZones.Controls.Add(cbZonesDevs);
            tpZones.Controls.Add(label25);
            tpZones.Controls.Add(cbZonesRoles);
            tpZones.Controls.Add(label24);
            tpZones.Controls.Add(cbZonesZones);
            tpZones.Controls.Add(label23);
            tpZones.Location = new Point(4, 24);
            tpZones.Name = "tpZones";
            tpZones.Size = new Size(1237, 557);
            tpZones.TabIndex = 12;
            tpZones.Text = "Zones";
            tpZones.UseVisualStyleBackColor = true;
            // 
            // dgvZoneAllocatedRoles
            // 
            dgvZoneAllocatedRoles.Anchor = AnchorStyles.Top | AnchorStyles.Bottom;
            dgvZoneAllocatedRoles.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgvZoneAllocatedRoles.Columns.AddRange(new DataGridViewColumn[] { ZoneRoleIds, ZoneRoleNames });
            dgvZoneAllocatedRoles.Location = new Point(726, 3);
            dgvZoneAllocatedRoles.Name = "dgvZoneAllocatedRoles";
            dgvZoneAllocatedRoles.Size = new Size(374, 542);
            dgvZoneAllocatedRoles.TabIndex = 15;
            // 
            // ZoneRoleIds
            // 
            ZoneRoleIds.HeaderText = "Id";
            ZoneRoleIds.Name = "ZoneRoleIds";
            ZoneRoleIds.Width = 50;
            // 
            // ZoneRoleNames
            // 
            ZoneRoleNames.HeaderText = "Roles";
            ZoneRoleNames.Name = "ZoneRoleNames";
            ZoneRoleNames.Width = 250;
            // 
            // dgvZoneAllocatedDevs
            // 
            dgvZoneAllocatedDevs.Anchor = AnchorStyles.Top | AnchorStyles.Bottom;
            dgvZoneAllocatedDevs.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgvZoneAllocatedDevs.Columns.AddRange(new DataGridViewColumn[] { ZoneDevIds, ZoneDevs });
            dgvZoneAllocatedDevs.Location = new Point(342, 4);
            dgvZoneAllocatedDevs.Name = "dgvZoneAllocatedDevs";
            dgvZoneAllocatedDevs.Size = new Size(378, 541);
            dgvZoneAllocatedDevs.TabIndex = 14;
            // 
            // ZoneDevIds
            // 
            ZoneDevIds.HeaderText = "Id";
            ZoneDevIds.Name = "ZoneDevIds";
            ZoneDevIds.Width = 50;
            // 
            // ZoneDevs
            // 
            ZoneDevs.HeaderText = "Devices";
            ZoneDevs.Name = "ZoneDevs";
            ZoneDevs.Width = 250;
            // 
            // btnZoneDeallocateDev
            // 
            btnZoneDeallocateDev.Location = new Point(261, 115);
            btnZoneDeallocateDev.Name = "btnZoneDeallocateDev";
            btnZoneDeallocateDev.Size = new Size(75, 23);
            btnZoneDeallocateDev.TabIndex = 13;
            btnZoneDeallocateDev.Text = "Deallocate";
            btnZoneDeallocateDev.UseVisualStyleBackColor = true;
            btnZoneDeallocateDev.Click += btnZoneDeallocateDev_Click;
            // 
            // btnZoneAllocateDev
            // 
            btnZoneAllocateDev.Location = new Point(180, 115);
            btnZoneAllocateDev.Name = "btnZoneAllocateDev";
            btnZoneAllocateDev.Size = new Size(75, 23);
            btnZoneAllocateDev.TabIndex = 12;
            btnZoneAllocateDev.Text = "Allocate";
            btnZoneAllocateDev.UseVisualStyleBackColor = true;
            btnZoneAllocateDev.Click += btnZoneAllocateDev_Click;
            // 
            // btnZoneDeallocateRole
            // 
            btnZoneDeallocateRole.Location = new Point(261, 80);
            btnZoneDeallocateRole.Name = "btnZoneDeallocateRole";
            btnZoneDeallocateRole.Size = new Size(75, 23);
            btnZoneDeallocateRole.TabIndex = 11;
            btnZoneDeallocateRole.Text = "Deallocate";
            btnZoneDeallocateRole.UseVisualStyleBackColor = true;
            btnZoneDeallocateRole.Click += btnZoneDeallocateRole_Click;
            // 
            // btnZoneAllocateRole
            // 
            btnZoneAllocateRole.Location = new Point(180, 80);
            btnZoneAllocateRole.Name = "btnZoneAllocateRole";
            btnZoneAllocateRole.Size = new Size(75, 23);
            btnZoneAllocateRole.TabIndex = 10;
            btnZoneAllocateRole.Text = "Allocate";
            btnZoneAllocateRole.UseVisualStyleBackColor = true;
            btnZoneAllocateRole.Click += btnZoneAllocateRole_Click;
            // 
            // btnDeleteZone
            // 
            btnDeleteZone.Location = new Point(180, 14);
            btnDeleteZone.Name = "btnDeleteZone";
            btnDeleteZone.Size = new Size(75, 23);
            btnDeleteZone.TabIndex = 9;
            btnDeleteZone.Text = "Delete";
            btnDeleteZone.UseVisualStyleBackColor = true;
            btnDeleteZone.Click += btnDeleteZone_Click;
            // 
            // btnAddZone
            // 
            btnAddZone.Location = new Point(182, 49);
            btnAddZone.Name = "btnAddZone";
            btnAddZone.Size = new Size(75, 23);
            btnAddZone.TabIndex = 8;
            btnAddZone.Text = "Add";
            btnAddZone.UseVisualStyleBackColor = true;
            btnAddZone.Click += btnAddZone_Click;
            // 
            // txtNewZone
            // 
            txtNewZone.Location = new Point(56, 46);
            txtNewZone.Name = "txtNewZone";
            txtNewZone.Size = new Size(120, 23);
            txtNewZone.TabIndex = 7;
            // 
            // cbZonesDevs
            // 
            cbZonesDevs.FormattingEnabled = true;
            cbZonesDevs.Location = new Point(55, 115);
            cbZonesDevs.Name = "cbZonesDevs";
            cbZonesDevs.Size = new Size(121, 23);
            cbZonesDevs.TabIndex = 5;
            // 
            // label25
            // 
            label25.AutoSize = true;
            label25.Location = new Point(7, 118);
            label25.Name = "label25";
            label25.Size = new Size(42, 15);
            label25.TabIndex = 4;
            label25.Text = "Device";
            // 
            // cbZonesRoles
            // 
            cbZonesRoles.FormattingEnabled = true;
            cbZonesRoles.Location = new Point(56, 80);
            cbZonesRoles.Name = "cbZonesRoles";
            cbZonesRoles.Size = new Size(121, 23);
            cbZonesRoles.TabIndex = 3;
            // 
            // label24
            // 
            label24.AutoSize = true;
            label24.Location = new Point(7, 83);
            label24.Name = "label24";
            label24.Size = new Size(30, 15);
            label24.TabIndex = 2;
            label24.Text = "Role";
            // 
            // cbZonesZones
            // 
            cbZonesZones.FormattingEnabled = true;
            cbZonesZones.Location = new Point(53, 14);
            cbZonesZones.Name = "cbZonesZones";
            cbZonesZones.Size = new Size(121, 23);
            cbZonesZones.TabIndex = 1;
            // 
            // label23
            // 
            label23.AutoSize = true;
            label23.Location = new Point(7, 14);
            label23.Name = "label23";
            label23.Size = new Size(34, 15);
            label23.TabIndex = 0;
            label23.Text = "Zone";
            // 
            // tpZoneDevPols
            // 
            tpZoneDevPols.Controls.Add(label11);
            tpZoneDevPols.Controls.Add(lblZone);
            tpZoneDevPols.Controls.Add(cbZoneDevPolsDevDefs);
            tpZoneDevPols.Controls.Add(dgvZoneDevPols);
            tpZoneDevPols.Controls.Add(cbZoneDevPolsZones);
            tpZoneDevPols.Location = new Point(4, 24);
            tpZoneDevPols.Name = "tpZoneDevPols";
            tpZoneDevPols.Padding = new Padding(3);
            tpZoneDevPols.Size = new Size(1237, 557);
            tpZoneDevPols.TabIndex = 3;
            tpZoneDevPols.Text = "Zone Dev Pols";
            tpZoneDevPols.UseVisualStyleBackColor = true;
            // 
            // label11
            // 
            label11.AutoSize = true;
            label11.Location = new Point(6, 40);
            label11.Name = "label11";
            label11.Size = new Size(97, 15);
            label11.TabIndex = 6;
            label11.Text = "Device Definition";
            // 
            // lblZone
            // 
            lblZone.AutoSize = true;
            lblZone.Location = new Point(6, 6);
            lblZone.Name = "lblZone";
            lblZone.Size = new Size(34, 15);
            lblZone.TabIndex = 5;
            lblZone.Text = "Zone";
            // 
            // cbZoneDevPolsDevDefs
            // 
            cbZoneDevPolsDevDefs.FormattingEnabled = true;
            cbZoneDevPolsDevDefs.Location = new Point(112, 40);
            cbZoneDevPolsDevDefs.Name = "cbZoneDevPolsDevDefs";
            cbZoneDevPolsDevDefs.Size = new Size(133, 23);
            cbZoneDevPolsDevDefs.TabIndex = 4;
            // 
            // dgvZoneDevPols
            // 
            dgvZoneDevPols.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            dgvZoneDevPols.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgvZoneDevPols.Columns.AddRange(new DataGridViewColumn[] { ZoneDevPolId, ZoneDevPolName, ZoneDevPolDesc, ZoneDevPolCat, ZoneDevPolPosn, ZoneDevPolValType, ZoneDevPolValMin, ZoneDevPolValMax, ZoneDevPolValDflt, ZoneDevPolUnits, ZoneDevPolVal });
            dgvZoneDevPols.Location = new Point(251, 6);
            dgvZoneDevPols.Name = "dgvZoneDevPols";
            dgvZoneDevPols.Size = new Size(978, 543);
            dgvZoneDevPols.TabIndex = 3;
            // 
            // ZoneDevPolId
            // 
            ZoneDevPolId.HeaderText = "Id";
            ZoneDevPolId.Name = "ZoneDevPolId";
            ZoneDevPolId.Width = 50;
            // 
            // ZoneDevPolName
            // 
            ZoneDevPolName.HeaderText = "Name";
            ZoneDevPolName.Name = "ZoneDevPolName";
            ZoneDevPolName.Width = 200;
            // 
            // ZoneDevPolDesc
            // 
            ZoneDevPolDesc.HeaderText = "Desc";
            ZoneDevPolDesc.Name = "ZoneDevPolDesc";
            ZoneDevPolDesc.Width = 50;
            // 
            // ZoneDevPolCat
            // 
            ZoneDevPolCat.HeaderText = "Cat";
            ZoneDevPolCat.Name = "ZoneDevPolCat";
            ZoneDevPolCat.Width = 50;
            // 
            // ZoneDevPolPosn
            // 
            ZoneDevPolPosn.HeaderText = "Posn";
            ZoneDevPolPosn.Name = "ZoneDevPolPosn";
            ZoneDevPolPosn.Width = 50;
            // 
            // ZoneDevPolValType
            // 
            ZoneDevPolValType.HeaderText = "ValType";
            ZoneDevPolValType.Name = "ZoneDevPolValType";
            ZoneDevPolValType.Width = 50;
            // 
            // ZoneDevPolValMin
            // 
            ZoneDevPolValMin.HeaderText = "ValMin";
            ZoneDevPolValMin.Name = "ZoneDevPolValMin";
            ZoneDevPolValMin.Width = 50;
            // 
            // ZoneDevPolValMax
            // 
            ZoneDevPolValMax.HeaderText = "ValMax";
            ZoneDevPolValMax.Name = "ZoneDevPolValMax";
            ZoneDevPolValMax.Width = 50;
            // 
            // ZoneDevPolValDflt
            // 
            ZoneDevPolValDflt.HeaderText = "ValDflt";
            ZoneDevPolValDflt.Name = "ZoneDevPolValDflt";
            ZoneDevPolValDflt.Width = 50;
            // 
            // ZoneDevPolUnits
            // 
            ZoneDevPolUnits.HeaderText = "Units";
            ZoneDevPolUnits.Name = "ZoneDevPolUnits";
            ZoneDevPolUnits.Width = 50;
            // 
            // ZoneDevPolVal
            // 
            ZoneDevPolVal.HeaderText = "Val";
            ZoneDevPolVal.Name = "ZoneDevPolVal";
            ZoneDevPolVal.Width = 50;
            // 
            // cbZoneDevPolsZones
            // 
            cbZoneDevPolsZones.FormattingEnabled = true;
            cbZoneDevPolsZones.Location = new Point(112, 6);
            cbZoneDevPolsZones.Name = "cbZoneDevPolsZones";
            cbZoneDevPolsZones.Size = new Size(133, 23);
            cbZoneDevPolsZones.TabIndex = 0;
            // 
            // tpZoneDevSigs
            // 
            tpZoneDevSigs.Controls.Add(label16);
            tpZoneDevSigs.Controls.Add(label15);
            tpZoneDevSigs.Controls.Add(dgvZoneDevSigs);
            tpZoneDevSigs.Controls.Add(cbZoneDevSigsDevDefs);
            tpZoneDevSigs.Controls.Add(cbZoneDevSigsZones);
            tpZoneDevSigs.Location = new Point(4, 24);
            tpZoneDevSigs.Name = "tpZoneDevSigs";
            tpZoneDevSigs.Size = new Size(1237, 557);
            tpZoneDevSigs.TabIndex = 10;
            tpZoneDevSigs.Text = "Zone Dev Sigs";
            tpZoneDevSigs.UseVisualStyleBackColor = true;
            // 
            // label16
            // 
            label16.AutoSize = true;
            label16.Location = new Point(8, 32);
            label16.Name = "label16";
            label16.Size = new Size(97, 15);
            label16.TabIndex = 4;
            label16.Text = "Device Definition";
            // 
            // label15
            // 
            label15.AutoSize = true;
            label15.Location = new Point(8, 9);
            label15.Name = "label15";
            label15.Size = new Size(34, 15);
            label15.TabIndex = 3;
            label15.Text = "Zone";
            // 
            // dgvZoneDevSigs
            // 
            dgvZoneDevSigs.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            dgvZoneDevSigs.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgvZoneDevSigs.Columns.AddRange(new DataGridViewColumn[] { ZoneDevSigsId, ZoneDevSigsPerm, ZoneDevSigsSign, ZoneDevSigsAuth, ZoneDevSigsNote });
            dgvZoneDevSigs.Location = new Point(239, 9);
            dgvZoneDevSigs.Name = "dgvZoneDevSigs";
            dgvZoneDevSigs.Size = new Size(990, 540);
            dgvZoneDevSigs.TabIndex = 2;
            // 
            // ZoneDevSigsId
            // 
            ZoneDevSigsId.HeaderText = "Id";
            ZoneDevSigsId.Name = "ZoneDevSigsId";
            ZoneDevSigsId.Width = 50;
            // 
            // ZoneDevSigsPerm
            // 
            ZoneDevSigsPerm.HeaderText = "Perm";
            ZoneDevSigsPerm.Name = "ZoneDevSigsPerm";
            ZoneDevSigsPerm.Width = 200;
            // 
            // ZoneDevSigsSign
            // 
            ZoneDevSigsSign.HeaderText = "Sign";
            ZoneDevSigsSign.Name = "ZoneDevSigsSign";
            ZoneDevSigsSign.Width = 50;
            // 
            // ZoneDevSigsAuth
            // 
            ZoneDevSigsAuth.HeaderText = "Auth";
            ZoneDevSigsAuth.Name = "ZoneDevSigsAuth";
            ZoneDevSigsAuth.Width = 50;
            // 
            // ZoneDevSigsNote
            // 
            ZoneDevSigsNote.HeaderText = "Note";
            ZoneDevSigsNote.Name = "ZoneDevSigsNote";
            ZoneDevSigsNote.Width = 50;
            // 
            // cbZoneDevSigsDevDefs
            // 
            cbZoneDevSigsDevDefs.FormattingEnabled = true;
            cbZoneDevSigsDevDefs.Location = new Point(112, 35);
            cbZoneDevSigsDevDefs.Name = "cbZoneDevSigsDevDefs";
            cbZoneDevSigsDevDefs.Size = new Size(121, 23);
            cbZoneDevSigsDevDefs.TabIndex = 1;
            // 
            // cbZoneDevSigsZones
            // 
            cbZoneDevSigsZones.FormattingEnabled = true;
            cbZoneDevSigsZones.Location = new Point(112, 6);
            cbZoneDevSigsZones.Name = "cbZoneDevSigsZones";
            cbZoneDevSigsZones.Size = new Size(121, 23);
            cbZoneDevSigsZones.TabIndex = 0;
            // 
            // tpZoneRolePerms
            // 
            tpZoneRolePerms.Controls.Add(label19);
            tpZoneRolePerms.Controls.Add(label18);
            tpZoneRolePerms.Controls.Add(label17);
            tpZoneRolePerms.Controls.Add(cbZoneRolePermsRoles);
            tpZoneRolePerms.Controls.Add(cbZoneRolePermsDevDefs);
            tpZoneRolePerms.Controls.Add(cbZoneRolePermsZones);
            tpZoneRolePerms.Controls.Add(dgvZonePerms);
            tpZoneRolePerms.Location = new Point(4, 24);
            tpZoneRolePerms.Name = "tpZoneRolePerms";
            tpZoneRolePerms.Size = new Size(1237, 557);
            tpZoneRolePerms.TabIndex = 9;
            tpZoneRolePerms.Text = "Zone Role Perms";
            tpZoneRolePerms.UseVisualStyleBackColor = true;
            // 
            // label19
            // 
            label19.AutoSize = true;
            label19.Location = new Point(8, 72);
            label19.Name = "label19";
            label19.Size = new Size(30, 15);
            label19.TabIndex = 11;
            label19.Text = "Role";
            // 
            // label18
            // 
            label18.AutoSize = true;
            label18.Location = new Point(8, 39);
            label18.Name = "label18";
            label18.Size = new Size(97, 15);
            label18.TabIndex = 10;
            label18.Text = "Device Definition";
            // 
            // label17
            // 
            label17.AutoSize = true;
            label17.Location = new Point(8, 9);
            label17.Name = "label17";
            label17.Size = new Size(34, 15);
            label17.TabIndex = 9;
            label17.Text = "Zone";
            // 
            // cbZoneRolePermsRoles
            // 
            cbZoneRolePermsRoles.FormattingEnabled = true;
            cbZoneRolePermsRoles.Location = new Point(116, 69);
            cbZoneRolePermsRoles.Name = "cbZoneRolePermsRoles";
            cbZoneRolePermsRoles.Size = new Size(121, 23);
            cbZoneRolePermsRoles.TabIndex = 8;
            // 
            // cbZoneRolePermsDevDefs
            // 
            cbZoneRolePermsDevDefs.FormattingEnabled = true;
            cbZoneRolePermsDevDefs.Location = new Point(116, 36);
            cbZoneRolePermsDevDefs.Name = "cbZoneRolePermsDevDefs";
            cbZoneRolePermsDevDefs.Size = new Size(121, 23);
            cbZoneRolePermsDevDefs.TabIndex = 7;
            // 
            // cbZoneRolePermsZones
            // 
            cbZoneRolePermsZones.FormattingEnabled = true;
            cbZoneRolePermsZones.Location = new Point(116, 6);
            cbZoneRolePermsZones.Name = "cbZoneRolePermsZones";
            cbZoneRolePermsZones.Size = new Size(121, 23);
            cbZoneRolePermsZones.TabIndex = 6;
            // 
            // dgvZonePerms
            // 
            dgvZonePerms.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            dgvZonePerms.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgvZonePerms.Columns.AddRange(new DataGridViewColumn[] { ZoneDevPermId, ZoneDevPerm, ZoneDevPermVal });
            dgvZonePerms.Location = new Point(243, 9);
            dgvZonePerms.Name = "dgvZonePerms";
            dgvZonePerms.Size = new Size(986, 540);
            dgvZonePerms.TabIndex = 5;
            // 
            // ZoneDevPermId
            // 
            ZoneDevPermId.HeaderText = "Id";
            ZoneDevPermId.Name = "ZoneDevPermId";
            ZoneDevPermId.Width = 50;
            // 
            // ZoneDevPerm
            // 
            ZoneDevPerm.HeaderText = "Permission";
            ZoneDevPerm.Name = "ZoneDevPerm";
            ZoneDevPerm.Width = 300;
            // 
            // ZoneDevPermVal
            // 
            ZoneDevPermVal.HeaderText = "Val";
            ZoneDevPermVal.Name = "ZoneDevPermVal";
            ZoneDevPermVal.Width = 50;
            // 
            // tpAppPerms
            // 
            tpAppPerms.Controls.Add(label20);
            tpAppPerms.Controls.Add(btnGetAppPerms);
            tpAppPerms.Controls.Add(dgvAppPerms);
            tpAppPerms.Controls.Add(cboApps);
            tpAppPerms.Controls.Add(txtAppsUserName);
            tpAppPerms.Controls.Add(label6);
            tpAppPerms.Location = new Point(4, 24);
            tpAppPerms.Name = "tpAppPerms";
            tpAppPerms.Padding = new Padding(3);
            tpAppPerms.Size = new Size(1237, 557);
            tpAppPerms.TabIndex = 7;
            tpAppPerms.Text = "User App Perms";
            tpAppPerms.UseVisualStyleBackColor = true;
            // 
            // label20
            // 
            label20.AutoSize = true;
            label20.Location = new Point(210, 21);
            label20.Name = "label20";
            label20.Size = new Size(29, 15);
            label20.TabIndex = 7;
            label20.Text = "App";
            // 
            // btnGetAppPerms
            // 
            btnGetAppPerms.Location = new Point(8, 44);
            btnGetAppPerms.Name = "btnGetAppPerms";
            btnGetAppPerms.Size = new Size(166, 23);
            btnGetAppPerms.TabIndex = 6;
            btnGetAppPerms.Text = "Get App Permissions";
            btnGetAppPerms.UseVisualStyleBackColor = true;
            btnGetAppPerms.Click += btnGetAppPerms_Click;
            // 
            // dgvAppPerms
            // 
            dgvAppPerms.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            dgvAppPerms.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgvAppPerms.Columns.AddRange(new DataGridViewColumn[] { AppPerms });
            dgvAppPerms.Location = new Point(245, 51);
            dgvAppPerms.Name = "dgvAppPerms";
            dgvAppPerms.Size = new Size(984, 498);
            dgvAppPerms.TabIndex = 5;
            // 
            // AppPerms
            // 
            AppPerms.HeaderText = "Permissions";
            AppPerms.Name = "AppPerms";
            AppPerms.Width = 300;
            // 
            // cboApps
            // 
            cboApps.FormattingEnabled = true;
            cboApps.Location = new Point(245, 21);
            cboApps.Name = "cboApps";
            cboApps.Size = new Size(340, 23);
            cboApps.TabIndex = 4;
            // 
            // txtAppsUserName
            // 
            txtAppsUserName.Location = new Point(74, 15);
            txtAppsUserName.Name = "txtAppsUserName";
            txtAppsUserName.Size = new Size(100, 23);
            txtAppsUserName.TabIndex = 1;
            // 
            // label6
            // 
            label6.AutoSize = true;
            label6.Location = new Point(8, 18);
            label6.Name = "label6";
            label6.Size = new Size(60, 15);
            label6.TabIndex = 0;
            label6.Text = "Username";
            // 
            // tpDevPerms
            // 
            tpDevPerms.Controls.Add(dgvDevPerms);
            tpDevPerms.Controls.Add(btnGetDevPerms);
            tpDevPerms.Controls.Add(label10);
            tpDevPerms.Controls.Add(cbDevPermDevs);
            tpDevPerms.Controls.Add(txtDevPermUserName);
            tpDevPerms.Controls.Add(label8);
            tpDevPerms.Location = new Point(4, 24);
            tpDevPerms.Name = "tpDevPerms";
            tpDevPerms.Size = new Size(1237, 557);
            tpDevPerms.TabIndex = 8;
            tpDevPerms.Text = "User Dev Perms";
            tpDevPerms.UseVisualStyleBackColor = true;
            // 
            // dgvDevPerms
            // 
            dgvDevPerms.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            dgvDevPerms.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgvDevPerms.Columns.AddRange(new DataGridViewColumn[] { DevPerms });
            dgvDevPerms.Location = new Point(327, 12);
            dgvDevPerms.Name = "dgvDevPerms";
            dgvDevPerms.Size = new Size(902, 537);
            dgvDevPerms.TabIndex = 7;
            // 
            // DevPerms
            // 
            DevPerms.HeaderText = "Permissions";
            DevPerms.Name = "DevPerms";
            DevPerms.Width = 300;
            // 
            // btnGetDevPerms
            // 
            btnGetDevPerms.Location = new Point(76, 68);
            btnGetDevPerms.Name = "btnGetDevPerms";
            btnGetDevPerms.Size = new Size(245, 23);
            btnGetDevPerms.TabIndex = 6;
            btnGetDevPerms.Text = "Get Device Permissions";
            btnGetDevPerms.UseVisualStyleBackColor = true;
            btnGetDevPerms.Click += btnGetDevPerms_Click;
            // 
            // label10
            // 
            label10.AutoSize = true;
            label10.Location = new Point(8, 42);
            label10.Name = "label10";
            label10.Size = new Size(42, 15);
            label10.TabIndex = 5;
            label10.Text = "Device";
            // 
            // cbDevPermDevs
            // 
            cbDevPermDevs.FormattingEnabled = true;
            cbDevPermDevs.Location = new Point(76, 39);
            cbDevPermDevs.Name = "cbDevPermDevs";
            cbDevPermDevs.Size = new Size(245, 23);
            cbDevPermDevs.TabIndex = 4;
            // 
            // txtDevPermUserName
            // 
            txtDevPermUserName.Location = new Point(76, 10);
            txtDevPermUserName.Name = "txtDevPermUserName";
            txtDevPermUserName.Size = new Size(245, 23);
            txtDevPermUserName.TabIndex = 1;
            // 
            // label8
            // 
            label8.AutoSize = true;
            label8.Location = new Point(8, 13);
            label8.Name = "label8";
            label8.Size = new Size(62, 15);
            label8.TabIndex = 0;
            label8.Text = "UserName";
            // 
            // label7
            // 
            label7.AutoSize = true;
            label7.Location = new Point(12, 44);
            label7.Name = "label7";
            label7.Size = new Size(58, 15);
            label7.TabIndex = 3;
            label7.Text = "Common";
            // 
            // txtSysFeatCommon
            // 
            txtSysFeatCommon.Enabled = false;
            txtSysFeatCommon.Location = new Point(106, 46);
            txtSysFeatCommon.Name = "txtSysFeatCommon";
            txtSysFeatCommon.Size = new Size(100, 23);
            txtSysFeatCommon.TabIndex = 4;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1245, 585);
            Controls.Add(tabSecMan);
            Name = "Form1";
            Text = "Form1";
            Load += Form1_Load;
            tabSecMan.ResumeLayout(false);
            tpSuperUser.ResumeLayout(false);
            tpSuperUser.PerformLayout();
            tpSysFeats.ResumeLayout(false);
            tpSysFeats.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)dgvSys).EndInit();
            tpDevDefs.ResumeLayout(false);
            tpDevDefs.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)dgvDevPermDefs).EndInit();
            ((System.ComponentModel.ISupportInitialize)dgvDevPolDefs).EndInit();
            tpUsers.ResumeLayout(false);
            groupBox3.ResumeLayout(false);
            groupBox3.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)dgvUserRoles).EndInit();
            ((System.ComponentModel.ISupportInitialize)dgvUsers).EndInit();
            tpRoles.ResumeLayout(false);
            tpRoles.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)dgvRoleUsers).EndInit();
            tpDevs.ResumeLayout(false);
            tpDevs.PerformLayout();
            groupBox2.ResumeLayout(false);
            groupBox2.PerformLayout();
            groupBox1.ResumeLayout(false);
            groupBox1.PerformLayout();
            tpZones.ResumeLayout(false);
            tpZones.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)dgvZoneAllocatedRoles).EndInit();
            ((System.ComponentModel.ISupportInitialize)dgvZoneAllocatedDevs).EndInit();
            tpZoneDevPols.ResumeLayout(false);
            tpZoneDevPols.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)dgvZoneDevPols).EndInit();
            tpZoneDevSigs.ResumeLayout(false);
            tpZoneDevSigs.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)dgvZoneDevSigs).EndInit();
            tpZoneRolePerms.ResumeLayout(false);
            tpZoneRolePerms.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)dgvZonePerms).EndInit();
            tpAppPerms.ResumeLayout(false);
            tpAppPerms.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)dgvAppPerms).EndInit();
            tpDevPerms.ResumeLayout(false);
            tpDevPerms.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)dgvDevPerms).EndInit();
            ResumeLayout(false);
        }

        #endregion

        private TabControl tabSecMan;
        private TabPage tpUsers;
        private DataGridView dgvUsers;
        private ComboBox cbUsers;
        private TabPage tpRoles;
        private ComboBox cbRoles;
        private DataGridView dgvRoleUsers;
        private DataGridViewTextBoxColumn User;
        private TabPage tpDevs;
        private ComboBox cbDevs;
        private TabPage tpZoneDevPols;
        private ComboBox cbZoneDevPolsZones;
        private DataGridView dgvZoneDevPols;
        private TabPage tpSysFeats;
        private ComboBox cbSys;
        private DataGridView dgvSys;
        private Label lblSysFeat;
        private Button btnUserDelete;
        private DataGridViewTextBoxColumn SysPropId;
        private DataGridViewTextBoxColumn NameX;
        private DataGridViewTextBoxColumn Desc;
        private DataGridViewTextBoxColumn Cat;
        private DataGridViewTextBoxColumn Posn;
        private DataGridViewTextBoxColumn Type;
        private DataGridViewTextBoxColumn Min;
        private DataGridViewTextBoxColumn Max;
        private DataGridViewTextBoxColumn ValX;
        private TabPage tpDevDefs;
        private ComboBox cbDevDefs;
        private Label lblDevDef;
        private DataGridView dgvDevPolDefs;
        private DataGridViewTextBoxColumn DevDefPolId;
        private DataGridViewTextBoxColumn DevDefPolVers;
        private DataGridViewTextBoxColumn DevDefPolName;
        private DataGridViewTextBoxColumn DevDefPolDesc;
        private DataGridViewTextBoxColumn DevDefPolCat;
        private DataGridViewTextBoxColumn DevDefPolPosn;
        private DataGridViewTextBoxColumn DevDefPolValType;
        private DataGridViewTextBoxColumn DevDefPolValMin;
        private DataGridViewTextBoxColumn DevDefPolValMax;
        private DataGridViewTextBoxColumn DevDefPolValDflt;
        private DataGridViewTextBoxColumn DevDefPolUnits;
        private Label label2;
        private Label label1;
        private DataGridView dgvDevPermDefs;
        private DataGridViewTextBoxColumn DevPermDefId;
        private DataGridViewTextBoxColumn DevPermDefVers;
        private DataGridViewTextBoxColumn DevPermDefName;
        private DataGridViewTextBoxColumn DevPermDefDesc;
        private DataGridViewTextBoxColumn DevPermDefCat;
        private DataGridViewTextBoxColumn DevPermDefPosn;
        private Label label4;
        private Label label3;
        private Button btnRoleRemoveUser;
        private Button btnRoleAddUser;
        private ComboBox cbRoleUser;
        private Button btnAddRole;
        private TextBox tbRole;
        private Label label5;
        private Button btnDeleteRole;
        private DataGridView dgvUserRoles;
        private DataGridViewTextBoxColumn UserRoles;
        private DataGridViewTextBoxColumn Property;
        private DataGridViewTextBoxColumn Value;
        private Button btnUserRemoveRole;
        private Button btnUserAddRole;
        private ComboBox cboUserRemoveRole;
        private ComboBox cboUserAddRole;
        private TabPage tpAppPerms;
        private ComboBox cboApps;
        private TextBox txtAppsUserName;
        private Label label6;
        private Button btnGetAppPerms;
        private DataGridView dgvAppPerms;
        private DataGridViewTextBoxColumn AppPerms;
        private TabPage tpDevPerms;
        private ComboBox cbDevPermDevs;
        private TextBox txtDevPermUserName;
        private Label label8;
        private DataGridView dgvDevPerms;
        private DataGridViewTextBoxColumn DevPerms;
        private Button btnGetDevPerms;
        private Label label10;
        private TabPage tpZoneRolePerms;
        private TabPage tpZoneDevSigs;
        private DataGridView dgvZonePerms;
        private ComboBox cbZoneDevSigsZones;
        private ComboBox cbZoneRolePermsZones;
        private Label label11;
        private Label lblZone;
        private ComboBox cbZoneDevPolsDevDefs;
        private ComboBox cbZoneRolePermsDevDefs;
        private ComboBox cbZoneRolePermsRoles;
        private DataGridViewTextBoxColumn ZoneDevPermId;
        private DataGridViewTextBoxColumn ZoneDevPerm;
        private DataGridViewTextBoxColumn ZoneDevPermVal;
        private ComboBox cbZoneDevSigsDevDefs;
        private DataGridView dgvZoneDevSigs;
        private DataGridViewTextBoxColumn ZoneDevPolId;
        private DataGridViewTextBoxColumn ZoneDevPolName;
        private DataGridViewTextBoxColumn ZoneDevPolDesc;
        private DataGridViewTextBoxColumn ZoneDevPolCat;
        private DataGridViewTextBoxColumn ZoneDevPolPosn;
        private DataGridViewTextBoxColumn ZoneDevPolValType;
        private DataGridViewTextBoxColumn ZoneDevPolValMin;
        private DataGridViewTextBoxColumn ZoneDevPolValMax;
        private DataGridViewTextBoxColumn ZoneDevPolValDflt;
        private DataGridViewTextBoxColumn ZoneDevPolUnits;
        private DataGridViewTextBoxColumn ZoneDevPolVal;
        private DataGridViewTextBoxColumn ZoneDevSigsId;
        private DataGridViewTextBoxColumn ZoneDevSigsPerm;
        private DataGridViewTextBoxColumn ZoneDevSigsSign;
        private DataGridViewTextBoxColumn ZoneDevSigsAuth;
        private DataGridViewTextBoxColumn ZoneDevSigsNote;
        private TabPage tpSuperUser;
        private Button btnSetSuperUser;
        private TextBox txtSuperUserPassword;
        private TextBox txtSuperUserUserName;
        private Label label13;
        private Label label12;
        private Button txtSuperUserValidate;
        private Label label14;
        private Label label16;
        private Label label15;
        private Label label19;
        private Label label18;
        private Label label17;
        private Label label20;
        private GroupBox groupBox1;
        private Button btnDeleteDevice;
        private Label label22;
        private Label label21;
        private Button btnAddDevice;
        private ComboBox cbNewDevDevDef;
        private TextBox txtNewDevName;
        private TabPage tpZones;
        private Label label25;
        private ComboBox cbZonesRoles;
        private Label label24;
        private ComboBox cbZonesZones;
        private Label label23;
        private Button btnZoneDeallocateRole;
        private Button btnZoneAllocateRole;
        private Button btnDeleteZone;
        private Button btnAddZone;
        private TextBox txtNewZone;
        private ComboBox cbZonesDevs;
        private DataGridView dgvZoneAllocatedDevs;
        private Button btnZoneDeallocateDev;
        private Button btnZoneAllocateDev;
        private DataGridView dgvZoneAllocatedRoles;
        private DataGridViewTextBoxColumn ZoneRoleIds;
        private DataGridViewTextBoxColumn ZoneRoleNames;
        private DataGridViewTextBoxColumn ZoneDevIds;
        private DataGridViewTextBoxColumn ZoneDevs;
        private TextBox txtDevsDevDef;
        private Label label26;
        private TextBox txtDevsAllcatedZone;
        private Label label27;
        private GroupBox groupBox2;
        private Button btnDevsDeallocateZone;
        private Button btnDevsAllocateZone;
        private ComboBox cbDevsZones;
        private Label label28;
        private GroupBox groupBox3;
        private TextBox txtUserAddPassword;
        private Label lblUserAddPassword;
        private TextBox txtUserAddUsername;
        private Label lblUserAddUsername;
        private Button btnUserAdd;
        private Button btnRoleRename;
        private TextBox txtRoleRename;
        private TextBox txtSysFeatCommon;
        private Label label7;
    }
}
