using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.VisualBasic.Devices;
using SecMan.Data.SQLCipher;
using System;
using System.Collections.Generic;
using System.Data;
using System.DirectoryServices.ActiveDirectory;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static SecMan.Data.SecManDb;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.ListView;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.StartPanel;

namespace SecMan.Data
{
    public class SuperUser
    {
        private enum Property
        {
            UserName,
            Password
        }
        internal SuperUser(SQLCipher.SuperUser sqlCipherSuperUser)
        {
            Id = sqlCipherSuperUser.Id;
            _userName = sqlCipherSuperUser.UserName;
            _password = sqlCipherSuperUser.Password;
        }

        public ulong Id { get; set; }
        private string _userName = string.Empty;
        public string GetUserName() { return _userName; }
        public bool SetUserName(string val) { return SetProperty(Property.UserName, val); }

        private string _password = string.Empty;
        public string GetPassword() { return _password; }
        public bool SetPassword(string val) { return SetProperty(Property.Password, val); }

        private bool SetProperty(Property property, string val)
        {
            bool ok = false;
            SecManDb.dbLock.EnterWriteLock();
            try
            {
                using SQLCipher.Db db = new();

                if ((db != null) && (db.SuperUsers != null))
                {
                    SQLCipher.SuperUser sqlCipherSuperUser = db.SuperUsers.FirstOrDefault();
                    if (sqlCipherSuperUser != null)
                    {

                        switch (property)
                        {
                            case Property.UserName:
                                _userName = val;
                                sqlCipherSuperUser.UserName = val;
                                break;
                            case Property.Password:
                                _password = val;
                                sqlCipherSuperUser.Password = val;
                                break;
                             default:
                                break;
                        }
                        db.SaveChanges();
                        ok = true;
                    }
                }
            }
            catch
            {

                ok = false;
            }
            finally
            {
                SecManDb.dbLock.ExitWriteLock();
            }
            return ok;
        }
    }
}
