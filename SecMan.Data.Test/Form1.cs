namespace SecMan
{
    using Microsoft.Extensions.Logging;
    using Microsoft.VisualBasic.ApplicationServices;
    using SecMan.Data;
    using SecMan.Data.SQLCipher;
    using System.Collections.Generic;
    using System.Data;
    using System.DirectoryServices.ActiveDirectory;
    using System.Net.Sockets;
    using System.Security.Policy;
    using static SecMan.Data.SecManDb;
    using static System.Windows.Forms.VisualStyles.VisualStyleElement.ListView;
    using static System.Windows.Forms.VisualStyles.VisualStyleElement.StartPanel;

    public partial class Form1 : Form
    {
        #region Initialise
        SecManDb secManDb = new();

        // User tab
        private int userIdx_Domain;
        private int userIdx_UserName;
        private int userIdx_Password;
        private int userIdx_PasswordDate;
        private int userIdx_ChangePassword;
        private int userIdx_PasswordExpiryEnable;
        private int userIdx_PasswordExpiryDate;
        private int userIdx_LastLoginDate;
        private int userIdx_RFI;
        private int userIdx_RFIDate;
        private int userIdx_Biometric;
        private int userIdx_BiometricDate;
        private int userIdx_FirstlName;
        private int userIdx_LastName;
        private int userIdx_Description;
        private int userIdx_Language;
        private int userIdx_Email;
        private int userIdx_Enabled;
        private int userIdx_EnabledDate;
        private int userIdx_Retired;
        private int userIdx_RetiredDate;
        private int userIdx_Locked;
        private int userIdx_LockedReason;
        private int userIdx_LockedDate;
        private int userIdx_LastLogoutDate;
        private int userIdx_SessionId;
        private int userIdx_SessionExpiry;
        private int userIdx_LegacySupport;

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            RefreshAll();

            // App permissions
            txtAppsUserName.Text = "Admin01";

            // Dev Perms
            txtDevPermUserName.Text = "Admin01";

            // Enable events
            Events(true);

        }
        private void RefreshAll()
        {
            //// Disable events
            //Events(false);

            // SuperUser
            RefreshSuperUser();

            // SysFeats
            RefreshSysFeats();
            RefreshSysFeatProps();

            // DevDefs
            RefreshDevDefs();
            RefreshDevDefProps();

            // Users
            RefreshUsers();
            RefreshUserProps();

            // Roles
            RefreshRoles();
            RefreshRoleProps();

            // Devs
            RefreshDevs();
            RefreshDevsDevProps();
            RefreshDevsZones();

            // Zones
            RefreshZonesZones();
            RefreshZonesDevs();
            RefreshZonesRoles();
            RefreshZonesAllocatedDevs();
            RefreshZonesAllocatedRoles();

            // Zone Device Definition Policies
            RefreshZoneDevPolZones();
            RefreshZoneDevPolsDevDefs();
            RefreshZoneDevPolsVals();

            // Zone Device Definition Signatures
            RefreshZoneDevSigsZones();
            RefreshZoneDevSigsDevDefs();
            RefreshZoneDevSigProps();

            // Zone Device Definition Role Permissions
            RefreshZoneRolePermsZones();
            RefreshZoneRolePermsDevDefs();
            RefreshZoneRolePermsRoles();
            RefreshZoneRolePerms();

            // Dev Perms
            RefreshDevPerms();
        }

        private void Events(bool enable)
        {
            if (enable)
            {
                dgvSys.CellValueChanged += dgvSys_CellValueChanged;
                cbSys.SelectedIndexChanged += cbSysCom_SelectedIndexChanged;
                cbDevDefs.SelectedIndexChanged += cbDevDefs_SelectedIndexChanged;
                dgvUsers.CellValueChanged += dgvUsers_CellValueChanged;
                cbUsers.SelectedIndexChanged += cbUsers_SelectedIndexChanged;
                cbRoles.SelectedIndexChanged += cbRoles_SelectedIndexChanged;
                cbDevs.SelectedIndexChanged += cbDevs_SelectedIndexChanged;
                dgvZoneDevPols.CellValueChanged += dgvZoneDevPols_CellValueChanged;
                cbZoneDevPolsZones.SelectedIndexChanged += cbDevPolsZones_SelectedIndexChanged;
                dgvZoneDevSigs.CellValueChanged += dgvZoneDevSigs_CellValueChanged;
                cbZoneDevSigsDevDefs.SelectedIndexChanged += cbZoneDevSigsDevDefs_SelectedIndexChanged;
                cbZoneDevSigsZones.SelectedIndexChanged += cbZoneDevSigsZones_SelectedIndexChanged;
                cbZoneRolePermsDevDefs.SelectedIndexChanged += cbZoneRolePermsDevDefs_SelectedIndexChanged;
                cbZoneRolePermsRoles.SelectedIndexChanged += cbZoneRolePermsRoles_SelectedIndexChanged;
                cbZoneRolePermsZones.SelectedIndexChanged += cbZoneRolePermsZones_SelectedIndexChanged;
                dgvZonePerms.CellValueChanged += dgvZonePerms_CellValueChanged;
                cboApps.SelectedIndexChanged += cboApps_SelectedIndexChanged;
                cbZonesZones.SelectedIndexChanged += cbZonesZones_SelectedIndexChanged;
                cbZoneDevPolsDevDefs.SelectedIndexChanged += cbZoneDevPolsDevDefs_SelectedIndexChanged;
            }
            else
            {
                dgvSys.CellValueChanged -= dgvSys_CellValueChanged;
                cbSys.SelectedIndexChanged -= cbSysCom_SelectedIndexChanged;
                cbDevDefs.SelectedIndexChanged -= cbDevDefs_SelectedIndexChanged;
                dgvUsers.CellValueChanged -= dgvUsers_CellValueChanged;
                cbUsers.SelectedIndexChanged -= cbUsers_SelectedIndexChanged;
                cbRoles.SelectedIndexChanged -= cbRoles_SelectedIndexChanged;
                cbDevs.SelectedIndexChanged -= cbDevs_SelectedIndexChanged;
                dgvZoneDevPols.CellValueChanged -= dgvZoneDevPols_CellValueChanged;
                cbZoneDevPolsZones.SelectedIndexChanged -= cbDevPolsZones_SelectedIndexChanged;
                dgvZoneDevSigs.CellValueChanged -= dgvZoneDevSigs_CellValueChanged;
                cbZoneDevSigsDevDefs.SelectedIndexChanged -= cbZoneDevSigsDevDefs_SelectedIndexChanged;
                cbZoneDevSigsZones.SelectedIndexChanged -= cbZoneDevSigsZones_SelectedIndexChanged;
                cbZoneRolePermsDevDefs.SelectedIndexChanged -= cbZoneRolePermsDevDefs_SelectedIndexChanged;
                cbZoneRolePermsRoles.SelectedIndexChanged -= cbZoneRolePermsRoles_SelectedIndexChanged;
                cbZoneRolePermsZones.SelectedIndexChanged -= cbZoneRolePermsZones_SelectedIndexChanged;
                dgvZonePerms.CellValueChanged -= dgvZonePerms_CellValueChanged;
                cboApps.SelectedIndexChanged -= cboApps_SelectedIndexChanged;
                cbZonesZones.SelectedIndexChanged -= cbZonesZones_SelectedIndexChanged;
                cbZoneDevPolsDevDefs.SelectedIndexChanged -= cbZoneDevPolsDevDefs_SelectedIndexChanged;
            }
        }
        #endregion
        #region Devs

        void RefreshDevs()
        {
            int idx = cbDevs.SelectedIndex;
            Dictionary<string, Data.Dev> dic1 = new();
            secManDb.GetDevs().ForEach(o => dic1.Add(o.Name, o));
            cbDevs.DataSource = new BindingSource(dic1, null);
            cbDevs.DisplayMember = "Key";
            if (cbDevs.Items.Count > 0) { cbDevs.SelectedIndex = ((idx >= 0) && (idx < cbDevs.Items.Count)) ? idx : 0; ; }

            idx = cbNewDevDevDef.SelectedIndex;
            Dictionary<string, Data.DevDef> dic2 = new();
            secManDb.GetDevDefs("").ForEach(o => dic2.Add(o.Name, o));
            cbNewDevDevDef.DataSource = new BindingSource(dic2, null);
            cbNewDevDevDef.DisplayMember = "Key";
            if (cbNewDevDevDef.Items.Count > 0) { cbNewDevDevDef.SelectedIndex = ((idx >= 0) && (idx < cbNewDevDevDef.Items.Count)) ? idx : 0; ; }


        }
        void RefreshDevsZones()
        {
            int idx = cbDevsZones.SelectedIndex;
            Dictionary<string, Data.Zone> dic = new();
            secManDb.GetZones().ForEach(o => dic.Add(o.Name, o));
            cbDevsZones.DataSource = new BindingSource(dic, null);
            cbDevsZones.DisplayMember = "Key";
            if (cbDevsZones.Items.Count > 0) { cbDevsZones.SelectedIndex = ((idx >= 0) && (idx < cbDevsZones.Items.Count)) ? idx : 0; ; }
        }
        void RefreshDevsDevProps()
        {
            if (cbDevs.SelectedItem != null)
            {
                KeyValuePair<string, Data.Dev> kvp = (KeyValuePair<string, Data.Dev>)cbDevs.SelectedItem;
                Data.Dev dev = kvp.Value;
                if (dev != null)
                {
                    txtDevsDevDef.Text = dev.DevDef.Name;
                    if (dev.Zone != null)
                    {
                        txtDevsAllcatedZone.Text = dev.Zone.Name;
                    }
                    else
                    {
                        txtDevsAllcatedZone.Text = string.Empty;
                    }
                }
                else
                {
                    txtDevsDevDef.Text = string.Empty;
                    txtDevsAllcatedZone.Text = string.Empty;
                }
            }
        }

        private void cbDevs_SelectedIndexChanged(object sender, EventArgs e)
        {
            Events(false);
            RefreshAll();
            Events(true);
        }

        private void btnDeleteDevice_Click(object sender, EventArgs e)
        {
            Events(false);
            if (cbDevs.SelectedItem != null)
            {
                KeyValuePair<string, Data.Dev> kvp = (KeyValuePair<string, Data.Dev>)cbDevs.SelectedItem;
                if (kvp.Value != null)
                {
                    Data.Dev dev = kvp.Value;
                    if (secManDb.DelDev(dev.Id) != SecManDb.ReturnCode.Ok)
                    {
                        MessageBox.Show("Failed to delete device");

                    }
                    else
                    {
                        RefreshAll();
                    }
                }
                else
                {
                    MessageBox.Show("Failed to delete device");
                }
            }
            Events(true);
        }

        private void btnAddDevice_Click(object sender, EventArgs e)
        {
            Events(false);
            if (secManDb.AddDev(cbNewDevDevDef.Text, txtNewDevName.Text) == null)
            {
                MessageBox.Show("Failed to add device");
            }
            else
            {
                RefreshAll();
            }
            Events(true);
        }
        private void btnDevsDeallocateZone_Click(object sender, EventArgs e)
        {
            Events(false);
            if (cbDevs.SelectedItem != null)
            {
                KeyValuePair<string, Data.Dev> kvp = (KeyValuePair<string, Data.Dev>)cbDevs.SelectedItem;
                Data.Dev dev = kvp.Value;
                if (dev.RemZone())
                {
                    RefreshAll();
                }
                else
                {
                    MessageBox.Show("Failed to deallocate dev from zone");
                }
            }
            Events(true);
        }

        private void btnDevsAllocateZone_Click(object sender, EventArgs e)
        {
            Events(false);
            if ((cbDevs.SelectedItem != null) && (cbDevsZones.SelectedItem != null))
            {
                KeyValuePair<string, Data.Dev> kvp1 = (KeyValuePair<string, Data.Dev>)cbDevs.SelectedItem;
                Data.Dev dev = kvp1.Value;
                KeyValuePair<string, Data.Zone> kvp2 = (KeyValuePair<string, Data.Zone>)cbDevsZones.SelectedItem;
                Data.Zone zone = kvp2.Value;
                if (dev.AddZone(zone.Id))
                {
                    RefreshAll();
                }
                else
                {
                    MessageBox.Show("Failed to deallocate dev from zone");
                }
            }
            Events(true);
        }

        #endregion
        #region ZonePols

        private void RefreshZoneDevPolZones()
        {
            int idx = cbZoneDevPolsZones.SelectedIndex;
            Dictionary<string, Data.Zone> dic1 = new();
            secManDb.GetZones().ForEach(o => dic1.Add(o.Name, o));
            cbZoneDevPolsZones.DataSource = new BindingSource(dic1, null);
            cbZoneDevPolsZones.DisplayMember = "Key";
            if (cbZoneDevPolsZones.Items.Count > 0) { cbZoneDevPolsZones.SelectedIndex = ((idx >= 0) && (idx < cbZoneDevPolsZones.Items.Count)) ? idx : 0; ; }
        }

        private void RefreshZoneDevPolsDevDefs()
        {
            cbZoneDevPolsDevDefs.DataSource = null;
            cbZoneDevPolsDevDefs.Items.Clear();
            dgvZoneDevPols.Rows.Clear();
            if (cbZoneDevPolsZones.SelectedItem != null)
            {
                KeyValuePair<string, Data.Zone> kvp1 = (KeyValuePair<string, Data.Zone>)cbZoneDevPolsZones.SelectedItem;
                if (kvp1.Value != null)
                {
                    // Update combo box
                    Data.Zone zone = kvp1.Value;
                    int idx = cbZoneDevPolsDevDefs.SelectedIndex;
                    Dictionary<string, Data.DevDef> dic = new();
                    zone.GetDevDefs("").ForEach(o => dic.Add(o.Name, o));
                    if (dic.Count > 0)
                    {
                        cbZoneDevPolsDevDefs.DataSource = new BindingSource(dic, null);
                        cbZoneDevPolsDevDefs.DisplayMember = "Key";
                        if (cbZoneDevPolsDevDefs.Items.Count > 0)
                        {
                            cbZoneDevPolsDevDefs.SelectedIndex = ((idx >= 0) && (idx < cbZoneDevPolsDevDefs.Items.Count)) ? idx : 0;

                        }
                    }
                }
            }
        }
        private void RefreshZoneDevPolsVals()
        {
            dgvZoneDevPols.Rows.Clear();
            if (cbZoneDevPolsZones.SelectedItem != null)
            {
                KeyValuePair<string, Data.Zone> kvp1 = (KeyValuePair<string, Data.Zone>)cbZoneDevPolsZones.SelectedItem;
                if (kvp1.Value != null)
                {
                    // Update combo box
                    Data.Zone zone = kvp1.Value;
                    if (cbZoneDevPolsDevDefs.SelectedItem != null)
                    {
                        KeyValuePair<string, Data.DevDef> kvp2 = (KeyValuePair<string, Data.DevDef>)cbZoneDevPolsDevDefs.SelectedItem;
                        Data.DevDef devDef = kvp2.Value;
                        List<Data.DevPolVal> devPolVals = zone.GetDevDefPolVals(devDef.Id);
                        foreach (Data.DevPolVal devPolVal in devPolVals)
                        {
                            int i = dgvZoneDevPols.Rows.Add();
                            dgvZoneDevPols.Rows[i].Cells[0].Value = devPolVal.Id;
                            dgvZoneDevPols.Rows[i].Cells[1].Value = devPolVal.Name;
                            dgvZoneDevPols.Rows[i].Cells[2].Value = devPolVal.Desc;
                            dgvZoneDevPols.Rows[i].Cells[3].Value = devPolVal.Cat;
                            dgvZoneDevPols.Rows[i].Cells[4].Value = devPolVal.Posn;
                            dgvZoneDevPols.Rows[i].Cells[5].Value = devPolVal.ValType;
                            dgvZoneDevPols.Rows[i].Cells[6].Value = devPolVal.ValMin;
                            dgvZoneDevPols.Rows[i].Cells[7].Value = devPolVal.ValMax;
                            dgvZoneDevPols.Rows[i].Cells[8].Value = devPolVal.ValDflt;
                            dgvZoneDevPols.Rows[i].Cells[9].Value = devPolVal.Units;
                            dgvZoneDevPols.Rows[i].Cells[10].Value = devPolVal.Val;
                        }
                    }
                }
            }
        }
        private void cbDevPolsZones_SelectedIndexChanged(object sender, EventArgs e)
        {
            Events(false);
            RefreshZoneDevPolsDevDefs();
            RefreshZoneDevPolsVals();
            Events(true);
        }
        private void cbZoneDevPolsDevDefs_SelectedIndexChanged(object sender, EventArgs e)
        {
            Events(false);
            RefreshZoneDevPolsVals();
            Events(true);
        }

        private void dgvZoneDevPols_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            Events(false);
            bool ok = false;

            if ((e.RowIndex >= 0) && (e.ColumnIndex == 10))
            {
                try
                {
                    ulong id = ulong.Parse(dgvZoneDevPols.Rows[e.RowIndex].Cells[0].Value.ToString());
                    string? val = dgvZoneDevPols.Rows[e.RowIndex].Cells[10].Value.ToString();
                    if (val != null)
                    {
                        SecManDb.ReturnCode retCode = secManDb.SetDevPolVal(id, val);
                        if (retCode != SecManDb.ReturnCode.Ok)
                        {
                            MessageBox.Show("Failed to update policy: " + retCode.ToString());
                        }
                    }
                    else
                    {
                        MessageBox.Show("Failed to update policy: No value");
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Failed to update policy: " + ex.Message);
                }
            }
            RefreshAll();
            Events(true);
        }

        #endregion
        #region ZonePerms
        private void RefreshZoneRolePermsZones()
        {
            int idx = cbZoneRolePermsZones.SelectedIndex;
            Dictionary<string, Data.Zone> dic1 = new();
            secManDb.GetZones().ForEach(o => dic1.Add(o.Name, o));
            cbZoneRolePermsZones.DataSource = new BindingSource(dic1, null);
            cbZoneRolePermsZones.DisplayMember = "Key";
            if (cbZoneRolePermsZones.Items.Count > 0) { cbZoneRolePermsZones.SelectedIndex = ((idx >= 0) && (idx < cbZoneRolePermsZones.Items.Count)) ? idx : 0; ; }
        }


        private void RefreshZoneRolePermsDevDefs()
        {
            cbZoneRolePermsDevDefs.DataSource = null;
            cbZoneRolePermsDevDefs.Items.Clear();
            if (cbZoneRolePermsZones.SelectedItem != null)
            {
                KeyValuePair<string, Data.Zone> kvp1 = (KeyValuePair<string, Data.Zone>)cbZoneRolePermsZones.SelectedItem;
                if (kvp1.Value != null)
                {
                    // Update combo box
                    Data.Zone zone = kvp1.Value;
                    int idx = cbZoneRolePermsDevDefs.SelectedIndex;
                    Dictionary<string, Data.DevDef> dic = new();
                    zone.GetDevDefs("").ForEach(o => dic.Add(o.Name, o));
                    if (dic.Count > 0)
                    {
                        cbZoneRolePermsDevDefs.DataSource = new BindingSource(dic, null);
                        cbZoneRolePermsDevDefs.DisplayMember = "Key";
                        if (cbZoneRolePermsDevDefs.Items.Count > 0)
                        {
                            cbZoneRolePermsDevDefs.SelectedIndex = ((idx >= 0) && (idx < cbZoneRolePermsDevDefs.Items.Count)) ? idx : 0;
                        }
                    }
                }
            }
        }

        private void RefreshZoneRolePermsRoles()
        {
            cbZoneRolePermsRoles.DataSource = null;
            cbZoneRolePermsRoles.Items.Clear();
            if (cbZoneRolePermsZones.SelectedItem != null)
            {
                KeyValuePair<string, Data.Zone> kvp = (KeyValuePair<string, Data.Zone>)cbZoneRolePermsZones.SelectedItem;
                if (kvp.Value != null)
                {
                    // Update combo box
                    Data.Zone zone = kvp.Value;
                    int idx = cbZoneRolePermsRoles.SelectedIndex;
                    Dictionary<string, RoleData> dic = new();
                    zone.Roles.ForEach(o => dic.Add(o.Name, o));
                    if (dic.Count > 0)
                    {
                        cbZoneRolePermsRoles.DataSource = new BindingSource(dic, null);
                        cbZoneRolePermsRoles.DisplayMember = "Key";
                        if (cbZoneRolePermsRoles.Items.Count > 0)
                        {
                            cbZoneRolePermsRoles.SelectedIndex = ((idx >= 0) && (idx < cbZoneRolePermsRoles.Items.Count)) ? idx : 0;
                        }
                    }
                }
            }
        }

        private void RefreshZoneRolePerms()
        {
            dgvZonePerms.Rows.Clear();
            if (cbZoneRolePermsZones.SelectedItem != null)
            {
                KeyValuePair<string, Data.Zone> kvp1 = (KeyValuePair<string, Data.Zone>)cbZoneRolePermsZones.SelectedItem;
                if (kvp1.Value != null)
                {
                    // Get Zone
                    Data.Zone zone = kvp1.Value;
                    if (cbZoneRolePermsDevDefs.SelectedItem != null)
                    {
                        KeyValuePair<string, Data.DevDef> kvp2 = (KeyValuePair<string, Data.DevDef>)cbZoneRolePermsDevDefs.SelectedItem;
                        if (kvp2.Value != null)
                        {
                            // Get DevDef
                            Data.DevDef devDef = kvp2.Value;
                            if ((cbZoneRolePermsRoles.SelectedItem != null) && (cbZoneRolePermsRoles.Items.Count > 0))
                            {
                                KeyValuePair<string, RoleData> kvp3 = (KeyValuePair<string, RoleData>)cbZoneRolePermsRoles.SelectedItem;
                                if (kvp3.Value != null)
                                {
                                    // Get Role
                                    RoleData role = kvp3.Value;

                                    // Get permissions
                                    List<Data.DevPermVal> devPermVals = zone.GetDevDefRolePerms(devDef.Id, role.Id);

                                    foreach (Data.DevPermVal devPermVal in devPermVals)
                                    {
                                        int i = dgvZonePerms.Rows.Add();
                                        dgvZonePerms.Rows[i].Cells[0].Value = devPermVal.Id;
                                        dgvZonePerms.Rows[i].Cells[1].Value = devPermVal.Name;
                                        dgvZonePerms.Rows[i].Cells[2].Value = devPermVal.Val;
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
        private void cbZoneRolePermsZones_SelectedIndexChanged(object sender, EventArgs e)
        {
            Events(false);
            RefreshZoneRolePermsDevDefs();
            RefreshZoneRolePermsRoles();
            RefreshZoneRolePerms();
            Events(true);
        }

        private void cbZoneRolePermsDevDefs_SelectedIndexChanged(object sender, EventArgs e)
        {
            Events(false);
            RefreshZoneRolePerms();
            Events(true);
        }

        private void cbZoneRolePermsRoles_SelectedIndexChanged(object sender, EventArgs e)
        {
            Events(false);
            RefreshZoneRolePerms();
            Events(true);
        }

        private void dgvZonePerms_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            Events(false);

            bool ok = false;

            if ((e.RowIndex >= 0) && (e.ColumnIndex == 2))
            {
                try
                {
                    ulong permValId = ulong.Parse(dgvZonePerms.Rows[e.RowIndex].Cells[0].Value.ToString());

                    bool val = false;
                    if (dgvZonePerms.Rows[e.RowIndex].Cells[2].Value != null)
                    {
                        val = bool.Parse(dgvZonePerms.Rows[e.RowIndex].Cells[2].Value.ToString());
                    }
                    SecManDb.ReturnCode retCode = secManDb.SetPermVal(permValId, val);
                    if (retCode != SecManDb.ReturnCode.Ok)
                    {
                        MessageBox.Show("Failed to update permissions");
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Failed to update permissions: " + ex.Message);
                }
            }
            RefreshZoneRolePerms();
            Events(true);
        }

        #endregion
        #region ZoneSigs
        private void RefreshZoneDevSigsZones()
        {
            int idx = cbZoneDevSigsZones.SelectedIndex;
            Dictionary<string, Data.Zone> dic1 = new();
            secManDb.GetZones().ForEach(o => dic1.Add(o.Name, o));
            cbZoneDevSigsZones.DataSource = new BindingSource(dic1, null);
            cbZoneDevSigsZones.DisplayMember = "Key";
            if (cbZoneDevSigsZones.Items.Count > 0) { cbZoneDevSigsZones.SelectedIndex = ((idx >= 0) && (idx < cbZoneDevSigsZones.Items.Count)) ? idx : 0; ; }
        }

        private void RefreshZoneDevSigsDevDefs()
        {
            cbZoneDevSigsDevDefs.DataSource = null;
            cbZoneDevSigsDevDefs.Items.Clear();
            if (cbZoneDevSigsZones.SelectedItem != null)
            {
                KeyValuePair<string, Data.Zone> kvp1 = (KeyValuePair<string, Data.Zone>)cbZoneDevSigsZones.SelectedItem;
                if (kvp1.Value != null)
                {
                    // Update combo box
                    Data.Zone zone = kvp1.Value;
                    int idx = cbZoneDevSigsDevDefs.SelectedIndex;
                    Dictionary<string, Data.DevDef> dic = new();
                    zone.GetDevDefs("").ForEach(o => dic.Add(o.Name, o));
                    if (dic.Count > 0)
                    {
                        cbZoneDevSigsDevDefs.DataSource = new BindingSource(dic, null);
                        cbZoneDevSigsDevDefs.DisplayMember = "Key";
                        if (cbZoneDevSigsDevDefs.Items.Count > 0)
                        {
                            cbZoneDevSigsDevDefs.SelectedIndex = ((idx >= 0) && (idx < cbZoneDevSigsDevDefs.Items.Count)) ? idx : 0;
                        }
                    }
                }
            }
        }

        private void RefreshZoneDevSigProps()
        {
            dgvZoneDevSigs.Rows.Clear();
            if (cbZoneDevSigsZones.SelectedItem != null)
            {
                KeyValuePair<string, Data.Zone> kvp1 = (KeyValuePair<string, Data.Zone>)cbZoneDevSigsZones.SelectedItem;
                if (kvp1.Value != null)
                {
                    Data.Zone zone = kvp1.Value;
                    if (cbZoneDevSigsDevDefs.SelectedItem != null)
                    {
                        KeyValuePair<string, Data.DevDef> kvp2 = (KeyValuePair<string, Data.DevDef>)cbZoneDevSigsDevDefs.SelectedItem;
                        Data.DevDef devDef = kvp2.Value;
                        List<Data.DevSigVal> devSigVals = zone.GetDevDefSigVals(devDef.Id);
                        foreach (Data.DevSigVal devSigVal in devSigVals)
                        {
                            int i = dgvZoneDevSigs.Rows.Add();
                            dgvZoneDevSigs.Rows[i].Cells[0].Value = devSigVal.Id;
                            dgvZoneDevSigs.Rows[i].Cells[1].Value = devSigVal.Name;
                            dgvZoneDevSigs.Rows[i].Cells[2].Value = devSigVal.Sign;
                            dgvZoneDevSigs.Rows[i].Cells[3].Value = devSigVal.Auth;
                            dgvZoneDevSigs.Rows[i].Cells[4].Value = devSigVal.Note;
                        }
                    }
                }
            }
        }
        private void cbZoneDevSigsDevDefs_SelectedIndexChanged(object sender, EventArgs e)
        {
            Events(false);
            RefreshZoneDevSigProps();
            Events(true);
        }

        private void cbZoneDevSigsZones_SelectedIndexChanged(object sender, EventArgs e)
        {
            Events(false);
            RefreshZoneDevSigsDevDefs();
            RefreshZoneDevSigProps();
            Events(true);
        }

        private void dgvZoneDevSigs_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            bool ok = false;

            if ((e.RowIndex >= 0) && ((e.ColumnIndex == 2) || (e.ColumnIndex == 3) || (e.ColumnIndex == 4)))
            {
                try
                {
                    ulong sigValId = ulong.Parse(dgvZoneDevSigs.Rows[e.RowIndex].Cells[0].Value.ToString());

                    bool sign = false;
                    if (dgvZoneDevSigs.Rows[e.RowIndex].Cells[2].Value != null)
                    {
                        sign = bool.Parse(dgvZoneDevSigs.Rows[e.RowIndex].Cells[2].Value.ToString());
                    }
                    bool auth = false;
                    if (dgvZoneDevSigs.Rows[e.RowIndex].Cells[3].Value != null)
                    {
                        auth = bool.Parse(dgvZoneDevSigs.Rows[e.RowIndex].Cells[3].Value.ToString());
                    }
                    bool note = false;
                    if (dgvZoneDevSigs.Rows[e.RowIndex].Cells[4].Value != null)
                    {
                        note = bool.Parse(dgvZoneDevSigs.Rows[e.RowIndex].Cells[4].Value.ToString());
                    }
                    SecManDb.ReturnCode retCode = secManDb.SetSigVal(sigValId, sign, auth, note);
                    if (retCode != SecManDb.ReturnCode.Ok)
                    {
                        MessageBox.Show("Failed to update signatues: " + retCode.ToString());
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Failed to update signatues: " + ex.Message);
                }
            }

        }



        #endregion
        #region SysFeats
        private void RefreshSysFeats()
        {
            int idx = cbSys.SelectedIndex;
            Dictionary<string, Data.SysFeat> dicSysFeats = new();
            secManDb.GetSysFeats("").ForEach(o => dicSysFeats.Add(o.Name, o));
            cbSys.DataSource = new BindingSource(dicSysFeats, null);
            cbSys.DisplayMember = "Key";
            if (cbSys.Items.Count > 0) { cbSys.SelectedIndex = ((idx >= 0) && (idx < cbSys.Items.Count)) ? idx : 0; ; }
        }
        private void RefreshSysFeatProps()
        {
            dgvSys.Rows.Clear();
            if (cbSys.SelectedItem != null)
            {
                KeyValuePair<string, Data.SysFeat> kvp = (KeyValuePair<string, Data.SysFeat>)cbSys.SelectedItem;
                if (kvp.Value != null)
                {
                    Data.SysFeat sysFeat = kvp.Value;
                    txtSysFeatCommon.Text = sysFeat.Common ? "True" : "False";
                    foreach (Data.SysFeatProp sysFeatProp in sysFeat.SysFeatProps)
                    {
                        int i = dgvSys.Rows.Add();
                        dgvSys.Rows[i].Cells[0].Value = sysFeatProp.Id;
                        dgvSys.Rows[i].Cells[1].Value = sysFeatProp.Name;
                        dgvSys.Rows[i].Cells[2].Value = sysFeatProp.Desc;
                        dgvSys.Rows[i].Cells[3].Value = sysFeatProp.Cat;
                        dgvSys.Rows[i].Cells[4].Value = sysFeatProp.Posn;
                        dgvSys.Rows[i].Cells[5].Value = sysFeatProp.ValType;
                        dgvSys.Rows[i].Cells[6].Value = sysFeatProp.ValMin;
                        dgvSys.Rows[i].Cells[7].Value = sysFeatProp.ValMax;
                        dgvSys.Rows[i].Cells[8].Value = sysFeatProp.Val;
                    }
                }
            }
        }

        private void cbSysCom_SelectedIndexChanged(object sender, EventArgs e)
        {
            Events(false);
            RefreshSysFeatProps();
            Events(true);

        }
        private void dgvSys_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            Events(false);
            bool ok = false;

            if (e.RowIndex >= 0)
            {
                string? sId = dgvSys.Rows[e.RowIndex].Cells[0].Value.ToString();
                if (sId != null)
                {
                    ulong id = ulong.Parse(sId);
                    string? valType = dgvSys.Rows[e.RowIndex].Cells[5].Value.ToString();
                    string? val = dgvSys.Rows[e.RowIndex].Cells[8].Value.ToString();
                    if ((valType != null) & (val != null) && ValidateInput(valType, val))
                    {
                        if (secManDb.SetSysFeatPropVal(id, val))
                        {
                            ok = true;
                        }
                        else
                        {
                            MessageBox.Show("Update Failed");
                        }
                    }
                    else
                    {
                        MessageBox.Show("Invalid input");
                    }
                }

                // If not ok reset to default
                if (!ok)
                {
                    dgvSys.Rows[e.RowIndex].Cells[8].Value = dgvSys.Rows[e.RowIndex].Cells[7].Value;
                }
            }
            Events(true);
        }

        #endregion
        #region ValidateInput
        private bool ValidateInput(string valType, string val)
        {
            bool ok = false;
            try
            {
                ValType vt = ValType.None;
                Enum.TryParse(valType, out vt);
                switch (vt)
                {
                    case SecManDb.ValType.Str:
                        ok = true;
                        break;
                    case SecManDb.ValType.Int:
                        long l = long.Parse(val);
                        ok = true;
                        break;
                    case SecManDb.ValType.Bool:
                        bool b = bool.Parse(val);
                        ok = true;
                        break;
                    case SecManDb.ValType.IP:
                        ok = true;
                        break;
                    default:
                        break;
                }
            }
            catch { ok = false; }
            return ok;
        }
        #endregion
        #region Users
        private void RefreshUsers()
        {
            cbUsers.DataSource = null;
            cbUsers.Items.Clear();
            int idx = cbUsers.SelectedIndex;
            Dictionary<string, Data.User> dicUsers = new();
            secManDb.GetUsers().ForEach(o => dicUsers.Add(o.GetUserName(), o));
            if (dicUsers.Count > 0)
            {
                cbUsers.DataSource = new BindingSource(dicUsers, null);
                cbUsers.DisplayMember = "Key";
                if (cbUsers.Items.Count > 0) { cbUsers.SelectedIndex = ((idx >= 0) && (idx < cbUsers.Items.Count)) ? idx : 0; ; }
            }
        }
        private void RefreshUserProps()
        {
            dgvUsers.Rows.Clear();
            dgvUserRoles.Rows.Clear();
            if (cbUsers.SelectedItem != null)
            {
                // Get user
                KeyValuePair<string, Data.User> kvp = (KeyValuePair<string, Data.User>)cbUsers.SelectedItem;
                Data.User user = kvp.Value;

                // Output account properties
                dgvUsers.RowCount = 1;
                userIdx_Domain = dgvUsers.Rows.Add();
                dgvUsers.Rows[userIdx_Domain].Cells[0].Value = "Domain";
                dgvUsers.Rows[userIdx_Domain].Cells[1].Value = user.GetDomain();
                userIdx_UserName = dgvUsers.Rows.Add();
                dgvUsers.Rows[userIdx_UserName].Cells[0].Value = "UserName";
                dgvUsers.Rows[userIdx_UserName].Cells[1].Value = user.GetUserName();
                userIdx_Password = dgvUsers.Rows.Add();
                dgvUsers.Rows[userIdx_Password].Cells[0].Value = "Password";
                dgvUsers.Rows[userIdx_Password].Cells[1].Value = user.GetPassword();
                userIdx_PasswordDate = dgvUsers.Rows.Add();
                dgvUsers.Rows[userIdx_PasswordDate].Cells[0].Value = "PasswordDate";
                dgvUsers.Rows[userIdx_PasswordDate].Cells[1].Value = user.GetPasswordDate();
                userIdx_ChangePassword = dgvUsers.Rows.Add();
                dgvUsers.Rows[userIdx_ChangePassword].Cells[0].Value = "ChangePassword";
                dgvUsers.Rows[userIdx_ChangePassword].Cells[1].Value = user.GetChangePassword();
                userIdx_PasswordExpiryEnable = dgvUsers.Rows.Add();
                dgvUsers.Rows[userIdx_PasswordExpiryEnable].Cells[0].Value = "PasswordExpiryEnable";
                dgvUsers.Rows[userIdx_PasswordExpiryEnable].Cells[1].Value = user.GetPasswordExpiryEnable();
                userIdx_PasswordExpiryDate = dgvUsers.Rows.Add();
                dgvUsers.Rows[userIdx_PasswordExpiryDate].Cells[0].Value = "PasswordExpiryDate";
                dgvUsers.Rows[userIdx_PasswordExpiryDate].Cells[1].Value = user.GetPasswordExpiryDate();
                userIdx_LastLoginDate = dgvUsers.Rows.Add();
                dgvUsers.Rows[userIdx_LastLoginDate].Cells[0].Value = "LastLoginDate";
                dgvUsers.Rows[userIdx_LastLoginDate].Cells[1].Value = user.GetLastLoginDate();
                userIdx_RFI = dgvUsers.Rows.Add();
                dgvUsers.Rows[userIdx_RFI].Cells[0].Value = "RFI";
                dgvUsers.Rows[userIdx_RFI].Cells[1].Value = user.GetRFI();
                userIdx_RFIDate = dgvUsers.Rows.Add();
                dgvUsers.Rows[userIdx_RFIDate].Cells[0].Value = "RFIDate";
                dgvUsers.Rows[userIdx_RFIDate].Cells[1].Value = user.GetRfiDate();
                userIdx_Biometric = dgvUsers.Rows.Add();
                dgvUsers.Rows[userIdx_Biometric].Cells[0].Value = "Biometric";
                dgvUsers.Rows[userIdx_Biometric].Cells[1].Value = user.GetBiometric();
                userIdx_BiometricDate = dgvUsers.Rows.Add();
                dgvUsers.Rows[userIdx_BiometricDate].Cells[0].Value = "BiometricDate";
                dgvUsers.Rows[userIdx_BiometricDate].Cells[1].Value = user.GetBiometricDate();
                userIdx_FirstlName = dgvUsers.Rows.Add();
                dgvUsers.Rows[userIdx_FirstlName].Cells[0].Value = "FirstlName";
                dgvUsers.Rows[userIdx_FirstlName].Cells[1].Value = user.GetFirstName();
                userIdx_LastName = dgvUsers.Rows.Add();
                dgvUsers.Rows[userIdx_LastName].Cells[0].Value = "LastName";
                dgvUsers.Rows[userIdx_LastName].Cells[1].Value = user.GetLastName();
                userIdx_Description = dgvUsers.Rows.Add();
                dgvUsers.Rows[userIdx_Description].Cells[0].Value = "Description";
                dgvUsers.Rows[userIdx_Description].Cells[1].Value = user.GetDescription();
                userIdx_Language = dgvUsers.Rows.Add();
                dgvUsers.Rows[userIdx_Language].Cells[0].Value = "Language";
                dgvUsers.Rows[userIdx_Language].Cells[1].Value = user.GetLanguage();
                userIdx_Email = dgvUsers.Rows.Add();
                dgvUsers.Rows[userIdx_Email].Cells[0].Value = "Email";
                dgvUsers.Rows[userIdx_Email].Cells[1].Value = user.GetEmail();
                userIdx_Enabled = dgvUsers.Rows.Add();
                dgvUsers.Rows[userIdx_Enabled].Cells[0].Value = "Enabled";
                dgvUsers.Rows[userIdx_Enabled].Cells[1].Value = user.GetEnabled();
                userIdx_EnabledDate = dgvUsers.Rows.Add();
                dgvUsers.Rows[userIdx_EnabledDate].Cells[0].Value = "EnabledDate";
                dgvUsers.Rows[userIdx_EnabledDate].Cells[1].Value = user.GetEnabledDate();
                userIdx_Retired = dgvUsers.Rows.Add();
                dgvUsers.Rows[userIdx_Retired].Cells[0].Value = "Retired";
                dgvUsers.Rows[userIdx_Retired].Cells[1].Value = user.GetRetired();
                userIdx_RetiredDate = dgvUsers.Rows.Add();
                dgvUsers.Rows[userIdx_RetiredDate].Cells[0].Value = "RetiredDate";
                dgvUsers.Rows[userIdx_RetiredDate].Cells[1].Value = user.GetRetiredDate();
                userIdx_Locked = dgvUsers.Rows.Add();
                dgvUsers.Rows[userIdx_Locked].Cells[0].Value = "Locked";
                dgvUsers.Rows[userIdx_Locked].Cells[1].Value = user.GetLocked();
                userIdx_LockedReason = dgvUsers.Rows.Add();
                dgvUsers.Rows[userIdx_LockedReason].Cells[0].Value = "LockedReason";
                dgvUsers.Rows[userIdx_LockedReason].Cells[1].Value = user.GetLockedReason();
                userIdx_LockedDate = dgvUsers.Rows.Add();
                dgvUsers.Rows[userIdx_LockedDate].Cells[0].Value = "LockedDate";
                dgvUsers.Rows[userIdx_LockedDate].Cells[1].Value = user.GetLockedDate();
                userIdx_LastLogoutDate = dgvUsers.Rows.Add();
                dgvUsers.Rows[userIdx_LastLogoutDate].Cells[0].Value = "LastLogoutDate";
                dgvUsers.Rows[userIdx_LastLogoutDate].Cells[1].Value = user.GetLastLogoutDate();
                userIdx_SessionId = dgvUsers.Rows.Add();
                dgvUsers.Rows[userIdx_SessionId].Cells[0].Value = "SessionId";
                dgvUsers.Rows[userIdx_SessionId].Cells[1].Value = user.GetSessionId();
                userIdx_SessionExpiry = dgvUsers.Rows.Add();
                dgvUsers.Rows[userIdx_SessionExpiry].Cells[0].Value = "SessionExpiry";
                dgvUsers.Rows[userIdx_SessionExpiry].Cells[1].Value = user.GetSessionExpiry();
                userIdx_LegacySupport = dgvUsers.Rows.Add();
                dgvUsers.Rows[userIdx_LegacySupport].Cells[0].Value = "LegacySupport";
                dgvUsers.Rows[userIdx_LegacySupport].Cells[1].Value = user.GetLegacySupport();

                if (user.Roles.Count > 0)
                {
                    dgvUserRoles.RowCount = 1;
                    foreach (RoleData role in user.Roles)
                    {
                        int i = dgvUserRoles.Rows.Add();
                        dgvUserRoles.Rows[i].Cells[0].Value = role.Name;
                    }
                }

                // Removable Roles
                Dictionary<string, RoleData> dicRoles = new();
                user.Roles.ForEach(o => dicRoles.Add(o.Name, o));
                cboUserRemoveRole.DataSource = new BindingSource(dicRoles, null);
                cboUserRemoveRole.DisplayMember = "Key";
                if (cboUserRemoveRole.Items.Count > 0) { cboUserRemoveRole.SelectedIndex = 0; }


                // Addable Roles
                dicRoles = new();
                secManDb.GetRoles().ForEach(o => dicRoles.Add(o.Name, o));
                cboUserAddRole.DataSource = new BindingSource(dicRoles, null);
                cboUserAddRole.DisplayMember = "Key";
                if (cboUserAddRole.Items.Count > 0) { cboUserAddRole.SelectedIndex = 0; }
            }
        }

        private void cbUsers_SelectedIndexChanged(object sender, EventArgs e)
        {
            Events(false);
            RefreshUserProps();
            Events(true);
        }


        private void btnUserAdd_Click(object sender, EventArgs e)
        {

            if (secManDb.AddUser(txtUserAddUsername.Text, txtUserAddPassword.Text) != null)
            {
                MessageBox.Show("New user succeeded");
                RefreshUsers();
                RefreshRoles();
            }
            else
            {
                MessageBox.Show("Update Failed");
            }
        }

        private void btnUserDelete_Click(object sender, EventArgs e)
        {
            if (cbUsers.SelectedItem != null)
            {
                KeyValuePair<string, Data.User> kvp = (KeyValuePair<string, Data.User>)cbUsers.SelectedItem;
                if (kvp.Value != null)
                {
                    Data.User user = kvp.Value;
                    if ((cbUsers.SelectedItem != null) && secManDb.DelUser(user.Id))
                    {
                        RefreshUsers();
                        RefreshRoles();
                    }
                    else
                    {
                        MessageBox.Show("Failed to delete user");
                    }
                }
                else
                {
                    MessageBox.Show("Failed to delete user");
                }
            }
        }

        private void btnUserAddRole_Click(object sender, EventArgs e)
        {
            dgvUsers.Rows.Clear();
            dgvUserRoles.Rows.Clear();
            if ((cbUsers.SelectedItem != null) && (cboUserAddRole.SelectedItem != null))
            {
                // Get user
                KeyValuePair<string, Data.User> kvpUser = (KeyValuePair<string, Data.User>)cbUsers.SelectedItem;
                Data.User user = kvpUser.Value;

                // Get role
                KeyValuePair<string, Data.RoleData> kvpRole = (KeyValuePair<string, Data.RoleData>)cboUserAddRole.SelectedItem;
                Data.RoleData role = kvpRole.Value;

                if (user.AddRole(role.Id))
                {
                    RefreshUsers();
                    RefreshRoles();
                    RefreshRoleProps();
                }
                else
                {
                    MessageBox.Show("User add role Failed");
                }
            }
        }

        private void btnUserRemoveRole_Click(object sender, EventArgs e)
        {
            dgvUsers.Rows.Clear();
            dgvUserRoles.Rows.Clear();
            if ((cbUsers.SelectedItem != null) && (cboUserRemoveRole.SelectedItem != null))
            {
                // Get user
                KeyValuePair<string, Data.User> kvpUser = (KeyValuePair<string, Data.User>)cbUsers.SelectedItem;
                Data.User user = kvpUser.Value;

                // Get role
                KeyValuePair<string, Data.RoleData> kvpRole = (KeyValuePair<string, Data.RoleData>)cboUserRemoveRole.SelectedItem;
                Data.RoleData role = kvpRole.Value;

                if (user.RemRole(role.Id))
                {
                    RefreshUsers();
                    RefreshRoles();
                    RefreshRoleProps();
                }
                else
                {
                    MessageBox.Show("User add role Failed");
                }
            }
        }

        private void dgvUsers_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                string? val = dgvUsers.Rows[e.RowIndex].Cells[1].Value.ToString();

                if ((cbUsers.SelectedItem != null) && (val != null))
                {
                    // Get user
                    KeyValuePair<string, Data.User> kvpUser = (KeyValuePair<string, Data.User>)cbUsers.SelectedItem;
                    Data.User user = kvpUser.Value;
                    if (e.RowIndex == userIdx_Domain) { }
                    else if (e.RowIndex == userIdx_UserName) { if (!user.SetFirstName(val)) { MessageBox.Show("Failed to update property"); } }
                    else if (e.RowIndex == userIdx_Password) { if (!user.SetPassword(val)) { MessageBox.Show("Failed to update property"); } }
                    else if (e.RowIndex == userIdx_PasswordDate) { if (!user.SetPasswordDate(val)) { MessageBox.Show("Failed to update property"); } }
                    else if (e.RowIndex == userIdx_ChangePassword) { if (!user.SetChangePassword(val)) { MessageBox.Show("Failed to update property"); } }
                    else if (e.RowIndex == userIdx_PasswordExpiryEnable) { if (!user.SetPasswordExpiryEnable(val)) { MessageBox.Show("Failed to update property"); } }
                    else if (e.RowIndex == userIdx_PasswordExpiryDate) { if (!user.SetPasswordExpiryDate(val)) { MessageBox.Show("Failed to update property"); } }
                    else if (e.RowIndex == userIdx_LastLoginDate) { if (!user.SetLastLoginDate(val)) { MessageBox.Show("Failed to update property"); } }
                    else if (e.RowIndex == userIdx_RFI) { if (!user.SetRFI(val)) { MessageBox.Show("Failed to update property"); } }
                    else if (e.RowIndex == userIdx_RFIDate) { if (!user.SetRfiDate(val)) { MessageBox.Show("Failed to update property"); } }
                    else if (e.RowIndex == userIdx_Biometric) { if (!user.SetBiometric(val)) { MessageBox.Show("Failed to update property"); } }
                    else if (e.RowIndex == userIdx_BiometricDate) { if (!user.SetbiometricDate(val)) { MessageBox.Show("Failed to update property"); } }
                    else if (e.RowIndex == userIdx_FirstlName) { if (!user.SetFirstName(val)) { MessageBox.Show("Failed to update property"); } }
                    else if (e.RowIndex == userIdx_LastName) { if (!user.SetLastName(val)) { MessageBox.Show("Failed to update property"); } }
                    else if (e.RowIndex == userIdx_Description) { if (!user.SetDescription(val)) { MessageBox.Show("Failed to update property"); } }
                    else if (e.RowIndex == userIdx_Language) { if (!user.Setlanguage(val)) { MessageBox.Show("Failed to update property"); } }
                    else if (e.RowIndex == userIdx_Email) { if (!user.SetEmail(val)) { MessageBox.Show("Failed to update property"); } }
                    else if (e.RowIndex == userIdx_Enabled) { if (!user.SetEnabled(val)) { MessageBox.Show("Failed to update property"); } }
                    else if (e.RowIndex == userIdx_EnabledDate) { if (!user.SetEnabledDate(val)) { MessageBox.Show("Failed to update property"); } }
                    else if (e.RowIndex == userIdx_Retired) { if (!user.SetRetired(val)) { MessageBox.Show("Failed to update property"); } }
                    else if (e.RowIndex == userIdx_RetiredDate) { if (!user.SetretiredDate(val)) { MessageBox.Show("Failed to update property"); } }
                    else if (e.RowIndex == userIdx_Locked) { if (!user.SetLocked(val)) { MessageBox.Show("Failed to update property"); } }
                    else if (e.RowIndex == userIdx_LockedReason) { if (!user.SetLockedReason(val)) { MessageBox.Show("Failed to update property"); } }
                    else if (e.RowIndex == userIdx_LockedDate) { if (!user.SetLockedDate(val)) { MessageBox.Show("Failed to update property"); } }
                    else if (e.RowIndex == userIdx_LastLogoutDate) { if (!user.SetLockedDate(val)) { MessageBox.Show("Failed to update property"); } }
                    else if (e.RowIndex == userIdx_SessionId) { if (!user.SetSessionId(val)) { MessageBox.Show("Failed to update property"); } }
                    else if (e.RowIndex == userIdx_SessionExpiry) { if (!user.SetSessionExpiry(val)) { MessageBox.Show("Failed to update property"); } }
                    else if (e.RowIndex == userIdx_LegacySupport) { if (!user.SetLegacySupport(val)) { MessageBox.Show("Failed to update property"); } }
                    else { MessageBox.Show("Update Failed"); }
                }
            }
        }


        #endregion
        #region DevDefs

        private void RefreshDevDefs()
        {
            int idx = cbDevDefs.SelectedIndex;
            Dictionary<string, Data.DevDef> dicDevDefs = new();
            secManDb.GetDevDefs("").ForEach(o => dicDevDefs.Add(o.Name, o));
            cbDevDefs.DataSource = new BindingSource(dicDevDefs, null);
            cbDevDefs.DisplayMember = "Key";
            if (cbDevDefs.Items.Count > 0) { cbDevDefs.SelectedIndex = ((idx >= 0) && (idx < cbDevDefs.Items.Count)) ? idx : 0; ; }
        }

        private void RefreshDevDefProps()
        {
            dgvDevPolDefs.Rows.Clear();
            dgvDevPermDefs.Rows.Clear();
            if (cbDevDefs.SelectedItem != null)
            {
                KeyValuePair<string, Data.DevDef> kvp1 = (KeyValuePair<string, Data.DevDef>)cbDevDefs.SelectedItem;
                if ((kvp1.Value != null) && (kvp1.Value.DevPolDefs != null))
                {
                    foreach (Data.DevPolDef devPolDef in kvp1.Value.DevPolDefs)
                    {
                        int i = dgvDevPolDefs.Rows.Add();
                        dgvDevPolDefs.Rows[i].Cells[0].Value = devPolDef.Id;
                        dgvDevPolDefs.Rows[i].Cells[1].Value = devPolDef.Vers;
                        dgvDevPolDefs.Rows[i].Cells[2].Value = devPolDef.Name;
                        dgvDevPolDefs.Rows[i].Cells[3].Value = devPolDef.Desc;
                        dgvDevPolDefs.Rows[i].Cells[4].Value = devPolDef.Cat;
                        dgvDevPolDefs.Rows[i].Cells[5].Value = devPolDef.Posn;
                        dgvDevPolDefs.Rows[i].Cells[6].Value = devPolDef.ValType;
                        dgvDevPolDefs.Rows[i].Cells[7].Value = devPolDef.ValMin;
                        dgvDevPolDefs.Rows[i].Cells[8].Value = devPolDef.ValMax;
                        dgvDevPolDefs.Rows[i].Cells[9].Value = devPolDef.ValDflt;
                        dgvDevPolDefs.Rows[i].Cells[10].Value = devPolDef.Units;
                    }
                }

                KeyValuePair<string, Data.DevDef> kvp2 = (KeyValuePair<string, Data.DevDef>)cbDevDefs.SelectedItem;
                if ((kvp2.Value != null) && (kvp2.Value.DevPermDefs != null))
                {
                    foreach (Data.DevPermDef devPermDef in kvp2.Value.DevPermDefs)
                    {
                        int i = dgvDevPermDefs.Rows.Add();
                        dgvDevPermDefs.Rows[i].Cells[0].Value = devPermDef.Id;
                        dgvDevPermDefs.Rows[i].Cells[1].Value = devPermDef.Vers;
                        dgvDevPermDefs.Rows[i].Cells[2].Value = devPermDef.Name;
                        dgvDevPermDefs.Rows[i].Cells[3].Value = devPermDef.Desc;
                        dgvDevPermDefs.Rows[i].Cells[4].Value = devPermDef.Cat;
                        dgvDevPermDefs.Rows[i].Cells[5].Value = devPermDef.Posn;
                    }
                }
            }
        }


        private void cbDevDefs_SelectedIndexChanged(object sender, EventArgs e)
        {
            RefreshDevDefProps();
        }
        #endregion
        #region Roles
        private void RefreshRoles(int idx = -1)
        {
            if (idx < 0)
            {
                idx = cbRoles.SelectedIndex;
            }
            Dictionary<string, RoleData> dicRoles = new();
            secManDb.GetRoles().ForEach(o => dicRoles.Add(o.Name, o));
            cbRoles.DataSource = new BindingSource(dicRoles, null);
            cbRoles.DisplayMember = "Key";
            if (cbRoles.Items.Count > 0) { cbRoles.SelectedIndex = ((idx >= 0) && (idx < cbRoles.Items.Count)) ? idx : 0; }

            idx = cbRoleUser.SelectedIndex;
            Dictionary<string, Data.User> dicUsers = new();
            secManDb.GetUsers().ForEach(o => dicUsers.Add(o.GetUserName(), o));
            cbRoleUser.DataSource = new BindingSource(dicUsers, null);
            cbRoleUser.DisplayMember = "Key";
            if (cbRoleUser.Items.Count > 0) { cbRoleUser.SelectedIndex = ((idx >= 0) && (idx < cbRoleUser.Items.Count)) ? idx : 0; ; }
        }

        private void RefreshRoleProps()
        {
            dgvRoleUsers.Rows.Clear();
            if (cbRoles.SelectedItem != null)
            {
                KeyValuePair<string, RoleData> kvp = (KeyValuePair<string, RoleData>)cbRoles.SelectedItem;
                if ((kvp.Value != null) && (kvp.Value.Users != null))
                {
                    foreach (Data.User user in kvp.Value.Users)
                    {
                        int i = dgvRoleUsers.Rows.Add();
                        dgvRoleUsers.Rows[i].Cells[0].Value = user.GetUserName();
                    }
                }
            }
        }


        private void cbRoles_SelectedIndexChanged(object sender, EventArgs e)
        {
            Events(false);
            RefreshRoleProps();
            Events(true);
        }


        private void btnRoleRemoveUser_Click(object sender, EventArgs e)
        {
            if ((cbRoles.SelectedItem != null) && (cbRoleUser.SelectedItem != null))
            {
                KeyValuePair<string, RoleData> kvpRole = (KeyValuePair<string, RoleData>)cbRoles.SelectedItem;
                KeyValuePair<string, Data.User> kvpUser = (KeyValuePair<string, Data.User>)cbRoleUser.SelectedItem;
                if (kvpRole.Value.RemUser(kvpUser.Value.Id))
                {
                    RefreshUsers();
                    RefreshRoles();
                }
                else
                {
                    MessageBox.Show("Failed to remove user");
                }
            }
        }

        private void btnRoleAddUser_Click(object sender, EventArgs e)
        {
            if ((cbRoles.SelectedItem != null) && (cbRoleUser.SelectedItem != null))
            {
                KeyValuePair<string, RoleData> kvpRole = (KeyValuePair<string, RoleData>)cbRoles.SelectedItem;
                KeyValuePair<string, Data.User> kvpUser = (KeyValuePair<string, Data.User>)cbRoleUser.SelectedItem;
                if (kvpRole.Value.AddUser(kvpUser.Value.Id))
                {
                    RefreshUsers();
                    RefreshRoles();
                }
                else
                {
                    MessageBox.Show("Failed to remove user");
                }
            }
        }

        private void btnAddRole_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(tbRole.Text))
            {
                if (secManDb.AddRole(tbRole.Text) != null)
                {
                    int idx = -1;
                    foreach (RoleData role in secManDb.GetRoles())
                    {
                        idx++;
                        if (role.Name == tbRole.Text) break;
                    }
                    RefreshUsers();
                    RefreshRoles(idx);
                }
            }
            else
            {
                MessageBox.Show("Failed to add role");
            }
        }

        private void btnDeleteRole_Click(object sender, EventArgs e)
        {
            if (cbRoles.SelectedItem != null)
            {
                KeyValuePair<string, RoleData> kvp = (KeyValuePair<string, RoleData>)cbRoles.SelectedItem;
                if (kvp.Value != null)
                {
                    RoleData role = kvp.Value;

                    if (secManDb.DelRole(role.Id))
                    {
                        MessageBox.Show("Role remove succeeded");
                        RefreshUsers();
                        RefreshRoles();
                    }
                    else
                    {
                        MessageBox.Show("Failed to remove role");
                    }
                }
                else
                {
                    MessageBox.Show("Failed to remove role");
                }
            }
        }

        private void btnRoleRename_Click(object sender, EventArgs e)
        {
            Events(false);
            string roleRename = txtRoleRename.Text.Trim();
            if ((cbRoles.SelectedItem != null) && (!string.IsNullOrEmpty(roleRename)))
            {
                KeyValuePair<string, RoleData> kvp = (KeyValuePair<string, RoleData>)cbRoles.SelectedItem;
                if (kvp.Value != null)
                {
                    RoleData role = kvp.Value;
                    if (role.SetName(roleRename) == SecManDb.ReturnCode.Ok)
                    {
                        MessageBox.Show("Rename tole succeeded");
                        RefreshUsers();
                        RefreshUserProps();
                        RefreshRoles();
                        RefreshRoleProps();
                        RefreshZonesRoles();
                        RefreshZonesAllocatedRoles();
                        RefreshZoneRolePermsZones();
                        RefreshZoneRolePermsRoles();
                    }
                    else
                    {
                        MessageBox.Show("Rename tole failed");
                    }
                }
            }
            else
            {
                MessageBox.Show("Rename tole failed");
            }
            Events(true);
        }
        #endregion
        #region AppPerms

        private void btnGetAppPerms_Click(object sender, EventArgs e)
        {
            // Authenticate the user
            string Username = txtAppsUserName.Text;
            Data.User? user = secManDb.GetUser(Username);

            // Get the user's permissions
            if (user != null)
            {
                Dictionary<string, List<string>>? userAppPerms = user.GetAppPerms();

                cboApps.DataSource = null;
                cboApps.Items.Clear();
                dgvAppPerms.Rows.Clear();
                if ((userAppPerms != null) && (userAppPerms.Count > 0))
                {
                    cboApps.DataSource = new BindingSource(userAppPerms, null);
                    cboApps.DisplayMember = "Key";
                }
            }
            else
            {
                MessageBox.Show("Invalid user");
            }
        }

        private void cboApps_SelectedIndexChanged(object sender, EventArgs e)
        {
            dgvAppPerms.Rows.Clear();
            if (cboApps.SelectedItem != null)
            {
                KeyValuePair<string, List<string>> kvp = (KeyValuePair<string, List<string>>)cboApps.SelectedItem;
                foreach (string perm in kvp.Value)
                {
                    int i = dgvAppPerms.Rows.Add();
                    dgvAppPerms.Rows[i].Cells[0].Value = perm;
                }
            }
        }
        #endregion AppPerms
        #region DevPerms

        private void RefreshDevPerms()
        {
            int idx = cbDevPermDevs.SelectedIndex;
            Dictionary<string, Data.Dev> dicDevs = new();
            secManDb.GetDevs().ForEach(o => dicDevs.Add(o.Name, o));
            cbDevPermDevs.DataSource = new BindingSource(dicDevs, null);
            cbDevPermDevs.DisplayMember = "Key";
            if (cbDevPermDevs.Items.Count > 0) { cbDevPermDevs.SelectedIndex = ((idx >= 0) && (idx < cbDevPermDevs.Items.Count)) ? idx : 0; ; }
        }
        private void btnGetDevPerms_Click(object sender, EventArgs e)
        {
            // Authenticate the user
            string Username = txtDevPermUserName.Text;
            Data.User? user = secManDb.GetUser(Username);

            // Get the user's permissions
            if (user != null)
            {
                if (cbDevPermDevs.SelectedItem != null)
                {
                    KeyValuePair<string, Data.Dev> kvp = (KeyValuePair<string, Data.Dev>)cbDevPermDevs.SelectedItem;

                    if (kvp.Value != null)
                    {
                        Data.Dev dev = kvp.Value;
                        List<string> userDevPerms = user.GetDevPerms(dev.Id);

                        dgvDevPerms.Rows.Clear();
                        if ((userDevPerms != null) && (userDevPerms.Count > 0))
                        {
                            foreach (string perm in userDevPerms)
                            {
                                int i = dgvDevPerms.Rows.Add();
                                dgvDevPerms.Rows[i].Cells[0].Value = perm;
                            }
                        }
                    }
                    else
                    {
                        MessageBox.Show("Invalid device");
                    }
                }
            }
            else
            {
                MessageBox.Show("Invalid user");
            }
        }

        #endregion DevPerms
        #region SuperUser
        private void RefreshSuperUser()
        {
            Data.SuperUser? superUser = secManDb.GetSuperUser();
            if (superUser != null)
            {
                txtSuperUserUserName.Text = superUser.GetUserName();
                txtSuperUserPassword.Text = superUser.GetPassword();
            }
        }

        private void btnSetSuperUser_Click(object sender, EventArgs e)
        {
            Data.SuperUser? superUser = secManDb.GetSuperUser();
            if (superUser != null)
            {
                if (superUser.SetUserName(txtSuperUserUserName.Text.Trim()) && superUser.SetPassword(txtSuperUserPassword.Text.Trim()))
                {
                    MessageBox.Show("Set Super User succeeded");
                }
                else
                {
                    MessageBox.Show("Failed to set Super User");
                }
            }
            else
            {
                MessageBox.Show("Failed to set Super User");
            }
        }

        private void txtSuperUserValidate_Click(object sender, EventArgs e)
        {
            Data.SuperUser? superUser = secManDb.GetSuperUser();
            if ((superUser.GetUserName().ToUpper() == txtSuperUserUserName.Text.Trim().ToUpper()) &&
                (superUser.GetPassword().ToUpper() == txtSuperUserPassword.Text.Trim().ToUpper()))
            {
                MessageBox.Show("Validated");
            }
            else
            {
                MessageBox.Show("Not Validated");
            }
        }
        #endregion
        #region Zones
        private void RefreshZonesZones()
        {
            int idx = cbZonesZones.SelectedIndex;
            Dictionary<string, Data.Zone> dic = new();
            secManDb.GetZones().ForEach(o => dic.Add(o.Name, o));
            cbZonesZones.DataSource = new BindingSource(dic, null);
            cbZonesZones.DisplayMember = "Key";
            if (cbZonesZones.Items.Count > 0) { cbZonesZones.SelectedIndex = ((idx >= 0) && (idx < cbZonesZones.Items.Count)) ? idx : 0; ; }
        }

        private void RefreshZonesDevs()
        {
            int idx = cbZonesDevs.SelectedIndex;
            Dictionary<string, Data.Dev> dic = new();
            secManDb.GetDevs().ForEach(o => dic.Add(o.Name, o));
            cbZonesDevs.DataSource = new BindingSource(dic, null);
            cbZonesDevs.DisplayMember = "Key";
            if (cbZonesDevs.Items.Count > 0) { cbZonesDevs.SelectedIndex = ((idx >= 0) && (idx < cbZonesDevs.Items.Count)) ? idx : 0; ; }
        }

        private void RefreshZonesRoles()
        {
            int idx = cbZonesRoles.SelectedIndex;
            Dictionary<string, Data.RoleData> dic = new();
            secManDb.GetRoles().ForEach(o => dic.Add(o.Name, o));
            cbZonesRoles.DataSource = new BindingSource(dic, null);
            cbZonesRoles.DisplayMember = "Key";
            if (cbZonesRoles.Items.Count > 0) { cbZonesRoles.SelectedIndex = ((idx >= 0) && (idx < cbZonesRoles.Items.Count)) ? idx : 0; ; }
        }

        private void RefreshZonesAllocatedDevs()
        {
            dgvZoneAllocatedDevs.Rows.Clear();
            if (cbZonesZones.SelectedItem != null)
            {
                KeyValuePair<string, Data.Zone> kvp = (KeyValuePair<string, Data.Zone>)cbZonesZones.SelectedItem;
                Data.Zone zone = kvp.Value;
                foreach (Data.Dev dev in zone.Devs)
                {
                    int i = dgvZoneAllocatedDevs.Rows.Add();
                    dgvZoneAllocatedDevs.Rows[i].Cells[0].Value = dev.Id;
                    dgvZoneAllocatedDevs.Rows[i].Cells[1].Value = dev.Name;
                }
            }
        }

        private void RefreshZonesAllocatedRoles()
        {
            dgvZoneAllocatedRoles.Rows.Clear();
            if (cbZonesZones.SelectedItem != null)
            {
                KeyValuePair<string, Data.Zone> kvp = (KeyValuePair<string, Data.Zone>)cbZonesZones.SelectedItem;
                Data.Zone zone = kvp.Value;
                foreach (RoleData role in zone.Roles)
                {
                    int i = dgvZoneAllocatedRoles.Rows.Add();
                    dgvZoneAllocatedRoles.Rows[i].Cells[0].Value = role.Id;
                    dgvZoneAllocatedRoles.Rows[i].Cells[1].Value = role.Name;
                }
            }
        }

        private void cbZonesZones_SelectedIndexChanged(object sender, EventArgs e)
        {
            Events(false);
            RefreshAll();
            Events(true);
        }

        private void btnDeleteZone_Click(object sender, EventArgs e)
        {
            Events(false);
            if (cbZonesZones.SelectedItem != null)
            {
                KeyValuePair<string, Data.Zone> kvp = (KeyValuePair<string, Data.Zone>)cbZonesZones.SelectedItem;
                if (kvp.Value != null)
                {
                    Data.Zone zone = kvp.Value;
                    if (secManDb.DelZone(zone.Id))
                    {
                        MessageBox.Show("Delete zone succeeded");
                        RefreshAll();
                    }
                    else
                    {
                        MessageBox.Show("Failed to delete zone");
                    }
                }
            }
            Events(true);

        }

        private void btnAddZone_Click(object sender, EventArgs e)
        {
            Events(false);
            Data.Zone zone = secManDb.AddZone(txtNewZone.Text);
            if (zone != null)
            {
                RefreshAll();
            }
            else
            {
                MessageBox.Show("Failed to add zone");
            }
            Events(true);

        }

        private void btnZoneAllocateRole_Click(object sender, EventArgs e)
        {
            Events(false);
            if ((cbZonesZones.SelectedItem != null) && (cbZonesRoles.SelectedItem != null))
            {
                KeyValuePair<string, Data.Zone> kvp1 = (KeyValuePair<string, Data.Zone>)cbZonesZones.SelectedItem;
                Data.Zone zone = kvp1.Value;
                KeyValuePair<string, Data.RoleData> kvp2 = (KeyValuePair<string, Data.RoleData>)cbZonesRoles.SelectedItem;
                Data.RoleData role = kvp2.Value;
                if (zone.AddRole(role.Id))
                {
                    RefreshAll();
                }
                else
                {
                    MessageBox.Show("Failed to allocate role to zone");
                }
            }
            Events(true);
        }

        private void btnZoneDeallocateRole_Click(object sender, EventArgs e)
        {
            Events(false);
            if ((cbZonesZones.SelectedItem != null) && (cbZonesRoles.SelectedItem != null))
            {
                KeyValuePair<string, Data.Zone> kvp1 = (KeyValuePair<string, Data.Zone>)cbZonesZones.SelectedItem;
                Data.Zone zone = kvp1.Value;
                KeyValuePair<string, Data.RoleData> kvp2 = (KeyValuePair<string, Data.RoleData>)cbZonesRoles.SelectedItem;
                Data.RoleData role = kvp2.Value;
                if (zone.RemRole(role.Id))
                {
                    RefreshAll();
                }
                else
                {
                    MessageBox.Show("Failed to allocate role to zone");
                }
            }
            Events(true);
        }

        private void btnZoneAllocateDev_Click(object sender, EventArgs e)
        {
            Events(false);
            if ((cbZonesZones.SelectedItem != null) && (cbZonesDevs.SelectedItem != null))
            {
                KeyValuePair<string, Data.Zone> kvp1 = (KeyValuePair<string, Data.Zone>)cbZonesZones.SelectedItem;
                Data.Zone zone = kvp1.Value;
                KeyValuePair<string, Data.Dev> kvp2 = (KeyValuePair<string, Data.Dev>)cbZonesDevs.SelectedItem;
                Data.Dev dev = kvp2.Value;
                if (zone.AddDev(dev.Id))
                {
                    RefreshAll();
                }
                else
                {
                    MessageBox.Show("Failed to allocate dev to zone");
                }
            }
            Events(true);
        }

        private void btnZoneDeallocateDev_Click(object sender, EventArgs e)
        {
            Events(false);
            if ((cbZonesZones.SelectedItem != null) && (cbZonesDevs.SelectedItem != null))
            {
                KeyValuePair<string, Data.Zone> kvp1 = (KeyValuePair<string, Data.Zone>)cbZonesZones.SelectedItem;
                Data.Zone zone = kvp1.Value;
                KeyValuePair<string, Data.Dev> kvp2 = (KeyValuePair<string, Data.Dev>)cbZonesDevs.SelectedItem;
                Data.Dev dev = kvp2.Value;
                if (zone.RemDev(dev.Id))
                {
                    RefreshAll();
                }
                else
                {
                    MessageBox.Show("Failed to allocate dev to zone");
                }
            }
            Events(true);
        }
        #endregion
    }
}
