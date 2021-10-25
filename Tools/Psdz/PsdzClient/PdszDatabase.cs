﻿using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using BMW.Rheingold.Psdz.Model;
using BMW.Rheingold.Psdz.Model.Ecu;

namespace PsdzClient
{
    public class PdszDatabase : IDisposable
    {
        public enum SwiActionSource
        {
            VarId,
            VarGroupId,
            VarPrgEcuId,
            SwiRegister,
        }

        public class EcuTranslation
        {
            public EcuTranslation()
            {
                TextDe = string.Empty;
                TextEn = string.Empty;
                TextFr = string.Empty;
                TextTh = string.Empty;
                TextSv = string.Empty;
                TextIt = string.Empty;
                TextEs = string.Empty;
                TextId = string.Empty;
                TextKo = string.Empty;
                TextEl = string.Empty;
                TextTr = string.Empty;
                TextZh = string.Empty;
                TextRu = string.Empty;
                TextNl = string.Empty;
                TextPt = string.Empty;
                TextJa = string.Empty;
                TextCs = string.Empty;
                TextPl = string.Empty;
            }

            public EcuTranslation(string textDe, string textEn, string textFr, string textTh, string textSv, string textIt,
                string textEs, string textId, string textKo, string textEl, string textTr, string textZh,
                string textRu, string textNl, string textPt, string textJa, string textCs, string textPl)
            {
                TextDe = textDe;
                TextEn = textEn;
                TextFr = textFr;
                TextTh = textTh;
                TextSv = textSv;
                TextIt = textIt;
                TextEs = textEs;
                TextId = textId;
                TextKo = textKo;
                TextEl = textEl;
                TextTr = textTr;
                TextZh = textZh;
                TextRu = textRu;
                TextNl = textNl;
                TextPt = textPt;
                TextJa = textJa;
                TextCs = textCs;
                TextPl = textPl;
            }

            public string GetTitle(string language, string prefix = "Text")
            {
                try
                {
                    if (string.IsNullOrEmpty(language) || language.Length < 2)
                    {
                        return string.Empty;
                    }

                    string titlePropertyName = prefix + language.ToUpperInvariant()[0] + language.ToLowerInvariant()[1];
                    Type objType = GetType();
                    PropertyInfo propertyTitle = objType.GetProperty(titlePropertyName);
                    if (propertyTitle == null)
                    {
                        titlePropertyName = prefix + "En";
                        propertyTitle = objType.GetProperty(titlePropertyName);
                    }

                    if (propertyTitle != null)
                    {
                        string result = propertyTitle.GetValue(this) as string;
                        return result ?? string.Empty;
                    }

                    return string.Empty;
                }
                catch (Exception)
                {
                    return string.Empty;
                }
            }

            public string TextDe { get; set; }
            public string TextEn { get; set; }
            public string TextFr { get; set; }
            public string TextTh { get; set; }
            public string TextSv { get; set; }
            public string TextIt { get; set; }
            public string TextEs { get; set; }
            public string TextId { get; set; }
            public string TextKo { get; set; }
            public string TextEl { get; set; }
            public string TextTr { get; set; }
            public string TextZh { get; set; }
            public string TextRu { get; set; }
            public string TextNl { get; set; }
            public string TextPt { get; set; }
            public string TextJa { get; set; }
            public string TextCs { get; set; }
            public string TextPl { get; set; }
        }

        public class EcuInfo
        {
            public EcuInfo(string name, Int64 address, string description, string sgbd, string grp)
            {
                Name = name;
                Address = address;
                Description = description;
                Sgbd = sgbd;
                Grp = grp;
                VariantId = string.Empty;
                VariantGroupId = string.Empty;
                VariantPrgId = string.Empty;
                VariantPrgName = string.Empty;
                VariantPrgFlashLimit = string.Empty;
                VariantPrgEcuVarId = string.Empty;
                PsdzEcu = null;
                SwiActions = new List<SwiAction>();
            }

            public string Name { get; set; }

            public Int64 Address { get; set; }

            public string Description { get; set; }

            public string Sgbd { get; set; }

            public string Grp { get; set; }

            public string VariantId { get; set; }

            public string VariantGroupId { get; set; }

            public string VariantPrgId { get; set; }

            public string VariantPrgName { get; set; }

            public string VariantPrgFlashLimit { get; set; }

            public string VariantPrgEcuVarId { get; set; }

            public EcuTranslation EcuTranslation { get; set; }

            public IPsdzEcu PsdzEcu { get; set; }

            public List<SwiAction> SwiActions { get; set; }
        }

        public class SwiAction
        {
            public SwiAction(SwiActionSource swiSource, string id, string name, string actionCategory, string selectable, string showInPlan, string executable, string nodeClass,
                EcuTranslation ecuTranslation)
            {
                SwiSource = swiSource;
                Id = id;
                Name = name;
                ActionCategory = actionCategory;
                Selectable = selectable;
                ShowInPlan = showInPlan;
                Executable = executable;
                NodeClass = nodeClass;
                EcuTranslation = ecuTranslation;
            }

            public SwiActionSource SwiSource { get; set; }

            public string Id { get; set; }

            public string Name { get; set; }

            public string ActionCategory { get; set; }

            public string Selectable { get; set; }

            public string ShowInPlan { get; set; }

            public string Executable { get; set; }

            public string NodeClass { get; set; }

            public EcuTranslation EcuTranslation { get; set; }
        }

        public class SwiRegister
        {
            public SwiRegister(string id, string nodeClass, string name, string parentId, string remark, string sort, string versionNum, string identifier,
                EcuTranslation ecuTranslation)
            {
                Id = id;
                NodeClass = nodeClass;
                Name = name;
                ParentId = parentId;
                Remark = remark;
                Sort = sort;
                VersionNum = versionNum;
                Identifier = identifier;
                EcuTranslation = ecuTranslation;
            }

            public string Id { get; set; }

            public string NodeClass { get; set; }

            public string ParentId { get; set; }

            public string Name { get; set; }

            public string Remark { get; set; }

            public string Sort { get; set; }

            public string VersionNum { get; set; }

            public string Identifier { get; set; }

            public EcuTranslation EcuTranslation { get; set; }
        }

        private bool _disposed;
        private SQLiteConnection _mDbConnection;
        private string _rootENameClassId;
        private string _typeKeyClassId;

        public PdszDatabase(string istaFolder)
        {
            string databaseFile = Path.Combine(istaFolder, "SQLiteDBs", "DiagDocDb.sqlite");
            string connection = "Data Source=\"" + databaseFile + "\";";
            _mDbConnection = new SQLiteConnection(connection);

            _mDbConnection.SetPassword("6505EFBDC3E5F324");
            _mDbConnection.Open();

            _rootENameClassId = DatabaseFunctions.GetNodeClassId(_mDbConnection, @"RootEBezeichnung");
            _typeKeyClassId = DatabaseFunctions.GetNodeClassId(_mDbConnection, @"Typschluessel");
        }

        public bool LinkSvtEcus(List<EcuInfo> ecuList, IPsdzSvt psdzSvt)
        {
            try
            {
                foreach (EcuInfo ecuInfo in ecuList)
                {
                    IPsdzEcu psdzEcuMatch = null; 
                    if (ecuInfo.Address >= 0)
                    {
                        foreach (IPsdzEcu psdzEcu in psdzSvt.Ecus)
                        {
                            if (psdzEcu.PrimaryKey.DiagAddrAsInt == ecuInfo.Address)
                            {
                                psdzEcuMatch = psdzEcu;
                                break;
                            }
                        }
                    }
                    ecuInfo.PsdzEcu = psdzEcuMatch;
                }
            }
            catch (Exception)
            {
                return false;
            }

            return true;
        }

        public bool GetEcuVariants(List<EcuInfo> ecuList)
        {
            bool result = true;
            foreach (EcuInfo ecuInfo in ecuList)
            {
                ecuInfo.SwiActions.Clear();
                GetEcuVariant(ecuInfo);
                GetEcuProgrammingVariant(ecuInfo);

                GetSwiActionsForEcuVariant(ecuInfo);
                GetSwiActionsForEcuGroup(ecuInfo);
                GetSwiActionsForEcuProgrammingVariant(ecuInfo);
            }

            return result;
        }

        private bool GetEcuVariant(EcuInfo ecuInfo)
        {
            if (string.IsNullOrEmpty(ecuInfo.Sgbd))
            {
                return false;
            }

            bool result = false;
            string sql = string.Format(@"SELECT ID, " + DatabaseFunctions.SqlTitleItems + ", ECUGROUPID FROM XEP_ECUVARIANTS WHERE (lower(NAME) = '{0}')", ecuInfo.Sgbd.ToLowerInvariant());

            try
            {
                ecuInfo.VariantId = string.Empty;
                ecuInfo.VariantGroupId = string.Empty;
                using (SQLiteCommand command = new SQLiteCommand(sql, _mDbConnection))
                {
                    using (SQLiteDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            ecuInfo.VariantId = reader["ID"].ToString().Trim();
                            ecuInfo.VariantGroupId = reader["ECUGROUPID"].ToString().Trim();
                            ecuInfo.EcuTranslation = GetTranslation(reader);
                            result = true;
                        }
                    }
                }
            }
            catch (Exception)
            {
                return false;
            }

            return result;
        }

        private bool GetEcuProgrammingVariant(EcuInfo ecuInfo)
        {
            if (ecuInfo.PsdzEcu == null || string.IsNullOrEmpty(ecuInfo.PsdzEcu.BnTnName))
            {
                return false;
            }

            bool result = false;
            string sql = string.Format(@"SELECT ID, NAME, FLASHLIMIT, ECUVARIANTID FROM XEP_ECUPROGRAMMINGVARIANT WHERE UPPER(NAME) = UPPER('{0}')", ecuInfo.PsdzEcu.BnTnName);

            try
            {
                ecuInfo.VariantPrgId = string.Empty;
                ecuInfo.VariantPrgName = string.Empty;
                ecuInfo.VariantPrgFlashLimit = string.Empty;
                ecuInfo.VariantPrgEcuVarId = string.Empty;

                using (SQLiteCommand command = new SQLiteCommand(sql, _mDbConnection))
                {
                    using (SQLiteDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            ecuInfo.VariantPrgId = reader["ID"].ToString().Trim();
                            ecuInfo.VariantPrgName = reader["NAME"].ToString().Trim();
                            ecuInfo.VariantPrgFlashLimit = reader["FLASHLIMIT"].ToString().Trim();
                            ecuInfo.VariantPrgEcuVarId = reader["ECUVARIANTID"].ToString().Trim();
                            result = true;
                        }
                    }
                }
            }
            catch (Exception)
            {
                return false;
            }

            return result;
        }

        private bool GetSwiActionsForEcuVariant(EcuInfo ecuInfo)
        {
            if (string.IsNullOrEmpty(ecuInfo.VariantId))
            {
                return false;
            }

            string sql = string.Format(@"SELECT ID, NAME, ACTIONCATEGORY, SELECTABLE, SHOW_IN_PLAN, EXECUTABLE, " + DatabaseFunctions.SqlTitleItems +
                                       ", NODECLASS FROM XEP_SWIACTION WHERE ID IN (SELECT SWI_ACTION_ID FROM XEP_REF_ECUVARIANTS_SWIACTION WHERE ECUVARIANT_ID = {0})",
                                        ecuInfo.VariantId);
            try
            {
                using (SQLiteCommand command = new SQLiteCommand(sql, _mDbConnection))
                {
                    using (SQLiteDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            SwiAction swiAction = ReadXepSwiAction(reader, SwiActionSource.VarId);
                            ecuInfo.SwiActions.Add(swiAction);
                        }
                    }
                }
            }
            catch (Exception)
            {
                return false;
            }

            return true;
        }

        private bool GetSwiActionsForEcuGroup(EcuInfo ecuInfo)
        {
            if (string.IsNullOrEmpty(ecuInfo.VariantGroupId))
            {
                return false;
            }

            string sql = string.Format(@"SELECT ID, NAME, ACTIONCATEGORY, SELECTABLE, SHOW_IN_PLAN, EXECUTABLE, " + DatabaseFunctions.SqlTitleItems +
                                       ", NODECLASS FROM XEP_SWIACTION WHERE ID IN (SELECT SWI_ACTION_ID FROM XEP_REF_ECUGROUPS_SWIACTION WHERE ECUGROUP_ID = {0})",
                ecuInfo.VariantGroupId);
            try
            {
                using (SQLiteCommand command = new SQLiteCommand(sql, _mDbConnection))
                {
                    using (SQLiteDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            SwiAction swiAction = ReadXepSwiAction(reader, SwiActionSource.VarGroupId);
                            ecuInfo.SwiActions.Add(swiAction);
                        }
                    }
                }
            }
            catch (Exception)
            {
                return false;
            }

            return true;
        }

        private bool GetSwiActionsForEcuProgrammingVariant(EcuInfo ecuInfo)
        {
            if (ecuInfo.PsdzEcu == null || string.IsNullOrEmpty(ecuInfo.VariantPrgId))
            {
                return false;
            }

            string sql = string.Format(@"SELECT ID, NAME, ACTIONCATEGORY, SELECTABLE, SHOW_IN_PLAN, EXECUTABLE, " + DatabaseFunctions.SqlTitleItems +
                                       ", NODECLASS FROM XEP_SWIACTION WHERE ID IN (SELECT SWI_ACTION_ID FROM XEP_REF_ECUPRGVARI_SWIACTION WHERE ECUPROGRAMMINGVARIANT_ID = {0})",
                ecuInfo.VariantPrgId);
            try
            {
                using (SQLiteCommand command = new SQLiteCommand(sql, _mDbConnection))
                {
                    using (SQLiteDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            SwiAction swiAction = ReadXepSwiAction(reader, SwiActionSource.VarPrgEcuId);
                            ecuInfo.SwiActions.Add(swiAction);
                        }
                    }
                }
            }
            catch (Exception)
            {
                return false;
            }

            return true;
        }

        private bool GetSwiActionsForSwiRegister(string registerId, List<SwiAction> swiActions)
        {
            if (string.IsNullOrEmpty(registerId))
            {
                return false;
            }

            string sql = string.Format(@"SELECT ID, NAME, ACTIONCATEGORY, SELECTABLE, SHOW_IN_PLAN, EXECUTABLE, " + DatabaseFunctions.SqlTitleItems +
                                       ", NODECLASS FROM XEP_SWIACTION WHERE ID IN (SELECT SWI_ACTION_ID FROM XEP_REF_SWIREGISTER_SWIACTION WHERE SWI_REGISTER_ID = {0})",
                registerId);
            try
            {
                using (SQLiteCommand command = new SQLiteCommand(sql, _mDbConnection))
                {
                    using (SQLiteDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            SwiAction swiAction = ReadXepSwiAction(reader, SwiActionSource.SwiRegister);
                            swiActions.Add(swiAction);
                        }
                    }
                }
            }
            catch (Exception)
            {
                return false;
            }

            return true;
        }

        private List<SwiRegister> GetSwiRegisterByIdentifer(string registerId)
        {
            if (string.IsNullOrEmpty(registerId))
            {
                return null;
            }

            string sql = string.Format(@"SELECT ID, NODECLASS, PARENTID, NAME, REMARK, SORT, TITLEID, " + DatabaseFunctions.SqlTitleItems +
                                       ", VERSIONNUMBER, IDENTIFIER FROM XEP_SWIREGISTER WHERE IDENTIFER = 'REG|{0}'",
                registerId);
            List<SwiRegister> swiRegisterList = new List<SwiRegister>();
            try
            {
                using (SQLiteCommand command = new SQLiteCommand(sql, _mDbConnection))
                {
                    using (SQLiteDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            SwiRegister swiRegister = ReadXepSwiRegister(reader);
                            swiRegisterList.Add(swiRegister);
                        }
                    }
                }
            }
            catch (Exception)
            {
                return null;
            }

            return swiRegisterList;
        }

        private List<SwiRegister> GetSwiRegistersByParentId(string parentId)
        {
            string selection;
            selection = parentId != null ? string.Format(@"= {0}", parentId) : "IS NULL";
            string sql = @"SELECT ID, NODECLASS, PARENTID, NAME, REMARK, SORT, TITLEID, " + DatabaseFunctions.SqlTitleItems +
                                       ", VERSIONNUMBER, IDENTIFIER FROM XEP_SWIREGISTER WHERE PARENTID " + selection;
            List<SwiRegister> swiRegisterList = new List<SwiRegister>();
            try
            {
                using (SQLiteCommand command = new SQLiteCommand(sql, _mDbConnection))
                {
                    using (SQLiteDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            SwiRegister swiRegister = ReadXepSwiRegister(reader);
                            swiRegisterList.Add(swiRegister);
                        }
                    }
                }
            }
            catch (Exception)
            {
                return null;
            }

            return swiRegisterList;
        }

        private static SwiAction ReadXepSwiAction(SQLiteDataReader reader, SwiActionSource swiActionSource)
        {
            string id = reader["ID"].ToString().Trim();
            string name = reader["NAME"].ToString().Trim();
            string actionCategory = reader["ACTIONCATEGORY"].ToString().Trim();
            string selectable = reader["SELECTABLE"].ToString().Trim();
            string showInPlan = reader["SHOW_IN_PLAN"].ToString().Trim();
            string executable = reader["EXECUTABLE"].ToString().Trim();
            string nodeClass = reader["NODECLASS"].ToString().Trim();
            return new SwiAction(swiActionSource, id, name, actionCategory, selectable, showInPlan, executable, nodeClass, GetTranslation(reader));
        }

        private static SwiRegister ReadXepSwiRegister(SQLiteDataReader reader)
        {
            string id = reader["ID"].ToString().Trim();
            string nodeClass = reader["NODECLASS"].ToString().Trim();
            string parentId = reader["PARENTID"].ToString().Trim();
            string name = reader["NAME"].ToString().Trim();
            string remark = reader["REMARK"].ToString().Trim();
            string sort = reader["SORT"].ToString().Trim();
            string versionNum = reader["VERSIONNUMBER"].ToString().Trim();
            string identifer = reader["IDENTIFIER"].ToString().Trim();
            return new SwiRegister(id, nodeClass, parentId, name, remark, sort, versionNum, identifer, GetTranslation(reader));
        }

        private static EcuTranslation GetTranslation(SQLiteDataReader reader, string prefix = "TITLE", string language = null)
        {
            return new EcuTranslation(
                language == null || language.ToLowerInvariant() == "de" ? reader[prefix + "_DEDE"].ToString() : string.Empty,
                language == null || language.ToLowerInvariant() == "en" ? reader[prefix + "_ENUS"].ToString() : string.Empty,
                language == null || language.ToLowerInvariant() == "fr" ? reader[prefix + "_FR"].ToString() : string.Empty,
                language == null || language.ToLowerInvariant() == "th" ? reader[prefix + "_TH"].ToString() : string.Empty,
                language == null || language.ToLowerInvariant() == "sv" ? reader[prefix + "_SV"].ToString() : string.Empty,
                language == null || language.ToLowerInvariant() == "it" ? reader[prefix + "_IT"].ToString() : string.Empty,
                language == null || language.ToLowerInvariant() == "es" ? reader[prefix + "_ES"].ToString() : string.Empty,
                language == null || language.ToLowerInvariant() == "id" ? reader[prefix + "_ID"].ToString() : string.Empty,
                language == null || language.ToLowerInvariant() == "ko" ? reader[prefix + "_KO"].ToString() : string.Empty,
                language == null || language.ToLowerInvariant() == "el" ? reader[prefix + "_EL"].ToString() : string.Empty,
                language == null || language.ToLowerInvariant() == "tr" ? reader[prefix + "_TR"].ToString() : string.Empty,
                language == null || language.ToLowerInvariant() == "zh" ? reader[prefix + "_ZHCN"].ToString() : string.Empty,
                language == null || language.ToLowerInvariant() == "ru" ? reader[prefix + "_RU"].ToString() : string.Empty,
                language == null || language.ToLowerInvariant() == "nl" ? reader[prefix + "_NL"].ToString() : string.Empty,
                language == null || language.ToLowerInvariant() == "pt" ? reader[prefix + "_PT"].ToString() : string.Empty,
                language == null || language.ToLowerInvariant() == "ja" ? reader[prefix + "_JA"].ToString() : string.Empty,
                language == null || language.ToLowerInvariant() == "cs" ? reader[prefix + "_CSCZ"].ToString() : string.Empty,
                language == null || language.ToLowerInvariant() == "pl" ? reader[prefix + "_PLPL"].ToString() : string.Empty
                );
        }

        public void Dispose()
        {
            Dispose(true);
            // This object will be cleaned up by the Dispose method.
            // Therefore, you should call GC.SupressFinalize to
            // take this object off the finalization queue
            // and prevent finalization code for this object
            // from executing a second time.
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            // Check to see if Dispose has already been called.
            if (!_disposed)
            {
                if (_mDbConnection != null)
                {
                    _mDbConnection.Dispose();
                    _mDbConnection = null;
                }

                // If disposing equals true, dispose all managed
                // and unmanaged resources.
                if (disposing)
                {
                }

                // Note disposing has been done.
                _disposed = true;
            }
        }

    }
}