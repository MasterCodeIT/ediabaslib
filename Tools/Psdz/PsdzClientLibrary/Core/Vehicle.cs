﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;
using BMW.Rheingold.CoreFramework.Contracts.Vehicle;
using PsdzClient.Utility;

namespace PsdzClient.Core
{
	public class Vehicle : typeVehicle, INotifyPropertyChanged, IVehicle
	{
		public Vehicle(ClientContext clientContext) : base(clientContext)
        {
            //TransmissionDataType = new TransmissionDataType();
            base.ConnectState = VisibilityType.Collapsed;
            //pKodeList = new ObservableCollectionEx<Fault>();
            //FaultList = new List<Fault>();
            //VirtualFaultInfoList = new BlockingCollection<VirtualFaultInfo>();
            //sessionDataStore = new ParameterContainer();
            //base.Testplan = new TestPlanType();
            diagCodesProgramming = new ObservableCollection<string>();
            IsClosingOperationActive = false;
            validPWFStates = new HashSet<int>(new int[]
            {
                0, 1, 2, 3, 4, 5, 6, 7, 8, 9,
                10, 11, 12, 13, 14, 15, 16
            });
            clamp15MinValue = 0.0;
            clamp30MinValue = 9.95; //new VoltageThreshold(BatteryEnum.Pb).MinError;
            //RxSwin = new RxSwinData();
        }

#if false
        public List<string> PermanentSAEFehlercodesInFaultList()
        {
            List<string> list = new List<string>();
            if (FaultList != null && FaultList.Count != 0)
            {
                foreach (Fault fault in FaultList)
                {
                    if (fault.DTC.FortAsHexString == "S 0751")
                    {
                        list.Add("S 0751");
                    }
                    if (fault.DTC.FortAsHexString == "S 0756")
                    {
                        list.Add("S 0756");
                    }
                }
                return list;
            }
            return new List<string>();
        }
#endif

        [XmlIgnore]
        public bool Sp2021Enabled
        {
            get
            {
                return sp2021Enabled;
            }
            set
            {
                sp2021Enabled = value;
            }
        }

        public string HmiVersion
        {
            get
            {
                return hmiVersion;
            }
            set
            {
                hmiVersion = value;
                OnPropertyChanged("HmiVersion");
            }
        }

        public string EBezeichnungUIText
        {
            get
            {
                return eBezeichnungUIText;
            }
            set
            {
                eBezeichnungUIText = value;
                OnPropertyChanged("EBezeichnungUIText");
            }
        }

        [XmlIgnore]
        public string SalesDesignationBadgeUIText
        {
            get
            {
                return salesDesignationBadgeUIText;
            }
            set
            {
                salesDesignationBadgeUIText = value;
                OnPropertyChanged("SalesDesignationBadgeUIText");
            }
        }

        public string KraftstoffartEinbaulage
        {
            get
            {
                return kraftstoffartEinbaulage;
            }
            set
            {
                if (kraftstoffartEinbaulage != value)
                {
                    kraftstoffartEinbaulage = value;
                    OnPropertyChanged("KraftstoffartEinbaulage");
                }
            }
        }

        public ObservableCollection<string> DiagCodesProgramming => diagCodesProgramming;

#if false
        [XmlIgnore]
        public RxSwinData RxSwin { get; set; }

        [XmlIgnore]
        public List<IRxSwinObject> RxSwinObjectList { get; set; }
#endif
        [XmlIgnore]
        public FA TargetFA
        {
            get
            {
                return targetFA;
            }
            set
            {
                targetFA = value;
            }
        }

        [XmlIgnore]
        public string TargetILevel
        {
            get
            {
                return targetILevel;
            }
            set
            {
                targetILevel = value;
            }
        }

        [XmlIgnore]
        public string SerialGearBox7
        {
            get
            {
                if (!string.IsNullOrEmpty(base.SerialGearBox) && base.SerialGearBox.Length >= 7)
                {
                    return base.SerialGearBox.Substring(0, 7);
                }
                return base.SerialGearBox;
            }
        }

        public string SetVINRangeTypeFromVINRanges()
        {
            PdszDatabase database = ClientContext.GetDatabase(this);
            if (database != null && !"XXXXXXX".Equals(this.VIN7) && !string.IsNullOrEmpty(this.VIN7) && !this.VIN7.Equals(this.vinRangeTypeLastResolvedType, StringComparison.OrdinalIgnoreCase))
            {
                PdszDatabase.VinRanges vinRangesByVin = database.GetVinRangesByVin17(this.VINType, this.VIN7, false);
				if (vinRangesByVin != null)
				{
                    this.vinRangeTypeLastResolvedType = this.VIN7;
					return vinRangesByVin.TypeKey;
				}
			}
			return null;
		}

        [XmlIgnore]
        public string VINRangeType
        {
            get
            {
                return vinRangeType;
            }
            set
            {
                if (vinRangeType != value)
                {
                    vinRangeType = value;
                    OnPropertyChanged("VINRangeType");
                }
            }
        }

        [XmlIgnore]
        public bool IsClosingOperationActive
        {
            get
            {
                return isClosingOperationActive;
            }
            set
            {
                isClosingOperationActive = value;
            }
        }
#if false

        [XmlIgnore]
        public ParameterContainer SessionDataStore => sessionDataStore;
#endif
        public string VIN10Prefix
        {
            get
            {
                try
                {
                    if (string.IsNullOrEmpty(base.VIN17))
                    {
                        return null;
                    }
                    return base.VIN17.Substring(0, 10);
                }
                catch (Exception)
                {
                    //Log.WarningException("Vehicle.VIN10Prefix", exception);
                    return null;
                }
            }
        }

        public string BasisEReihe
        {
            get
            {
                if (!string.IsNullOrEmpty(base.MainSeriesSgbd) && base.MainSeriesSgbd.Length >= 3 && !base.MainSeriesSgbd.Equals("zcs_all"))
                {
                    return base.MainSeriesSgbd.Substring(0, 3);
                }
                return base.Ereihe;
            }
        }

        public string VIN7
        {
            get
            {
                try
                {
                    if (string.IsNullOrEmpty(base.VIN17))
                    {
                        return null;
                    }
                    return base.VIN17.Substring(10, 7);
                }
                catch (Exception)
                {
                    //Log.WarningException("Vehicle.get_VIN7", exception);
                }
                return null;
            }
        }
        // ToDo: Check on update
        public string GMType
        {
            get
            {
                try
                {
                    if (base.FA != null && !string.IsNullOrEmpty(base.FA.TYPE) && base.FA.TYPE.Length == 4)
                    {
                        return base.FA.TYPE;
                    }
                    if (string.IsNullOrEmpty(base.VIN17))
                    {
                        return null;
                    }
                    if (!string.IsNullOrEmpty(VINRangeType))
                    {
                        return VINRangeType;
                    }
                    string text = base.VIN17.Substring(3, 4);
                    if (!string.IsNullOrEmpty(text))
                    {
                        string text2 = text.Substring(0, 3);
                        switch (text[3])
                        {
                            case 'A':
                                return text2 + "1";
                            case 'B':
                                return text2 + "2";
                            case 'C':
                                return text2 + "3";
                            case 'D':
                                return text2 + "4";
                            case 'E':
                                return text2 + "5";
                            case 'F':
                                return text2 + "6";
                            case 'G':
                                return text2 + "7";
                            case 'H':
                                return text2 + "8";
                            default:
                                return text;
                            case 'J':
                                return text2 + "9";
                        }
                    }
                }
                catch (Exception)
                {
                    //Log.WarningException("Vehicle.get_VINType", exception);
                }
                return null;
            }
        }

        public string VINType
        {
            get
            {
                try
                {
                    if (!string.IsNullOrEmpty(base.VIN17) && base.VIN17.Length >= 17)
                    {
                        return base.VIN17.Substring(3, 4);
                    }
                    return null;
                }
                catch (Exception)
                {
                    //Log.WarningException("Vehicle.get_VINType", exception);
                }
                return null;
            }
        }

        public bool IsBusy
        {
            get
            {
                return isBusy;
            }
            set
            {
                isBusy = value;
                OnPropertyChanged("IsBusy");
            }
        }

        public string EMotBaureihe => base.EMotor.EMOTBaureihe;

        public string Produktlinie
        {
            get
            {
                return productLine;
            }
            set
            {
                if (productLine != value)
                {
                    productLine = value;
                    OnPropertyChanged("Produktlinie");
                }
            }
        }

        public string Sicherheitsrelevant
        {
            get
            {
                return securityRelevant;
            }
            set
            {
                if (securityRelevant != value)
                {
                    securityRelevant = value;
                    OnPropertyChanged("Sicherheitsrelevant");
                }
            }
        }

        public string Tueren
        {
            get
            {
                return doorNumber;
            }
            set
            {
                if (doorNumber != value)
                {
                    doorNumber = value;
                    OnPropertyChanged("Tueren");
                }
            }
        }
#if false
		[XmlIgnore]
		public IList<Fault> FaultList
		{
			get
			{
				return this.faultList;
			}
			set
			{
				if (value != null)
				{
					this.faultList = value;
					this.OnPropertyChanged("FaultList");
				}
			}
		}

		[XmlIgnore]
		public BlockingCollection<VirtualFaultInfo> VirtualFaultInfoList
		{
			get
			{
				return this.virtualFaultInfoList;
			}
			set
			{
				this.virtualFaultInfoList = value;
			}
		}

		public IEnumerable<Fault> GetEnrichedFaultList(IFFMDynamicResolver ffmDynamicResolver)
		{
			List<Fault> list = new List<Fault>();
			foreach (Fault fault in this.FaultList)
			{
				fault.ResolveLabels(this, ffmDynamicResolver);
				list.Add(fault);
			}
			return list;
		}

		public ObservableCollectionEx<Fault> PKodeList
		{
			get
			{
				return this.pKodeList;
			}
		}
#endif
        [XmlIgnore]
        public bool IsFastaReadDone { get; set; }

        [XmlIgnore]
        public bool IsProgrammingSessionStartable { get; set; }

        [XmlIgnore]
        public bool IsVehicleTestDone
        {
            get
            {
                return vehicleTestDone;
            }
            set
            {
                if (vehicleTestDone != value)
                {
                    vehicleTestDone = value;
                    OnPropertyChanged("IsVehicleTestDone");
                }
            }
        }

        public bool IsReadingFastaDataFinished
        {
            get
            {
                return isReadingFastaDataFinished;
            }
            set
            {
                isReadingFastaDataFinished = value;
                OnPropertyChanged("IsReadingFastaDataFinished");
            }
        }

        public bool IsNewFaultMemoryActive
        {
            get
            {
                return isNewFaultMemoryActiveField;
            }
            set
            {
                isNewFaultMemoryActiveField = value;
                OnPropertyChanged("IsNewFaultMemoryActive");
            }
        }

        public bool IsNewFaultMemoryExpertModeActive
        {
            get
            {
                return isNewFaultMemoryExpertModeActiveField;
            }
            set
            {
                isNewFaultMemoryExpertModeActiveField = value;
                OnPropertyChanged("IsNewFaultMemoryExpertModeActive");
            }
        }

        [XmlIgnore]
        public bool IsVehicleBreakdownAlreadyShown { get; set; }

        [XmlIgnore]
        public bool IsPowerSafeModeActive
        {
            get
            {
                if (!powerSafeModeByOldEcus)
                {
                    return powerSafeModeByNewEcus;
                }
                return true;
            }
        }

        [XmlIgnore]
        public bool IsPowerSafeModeActiveByOldEcus
        {
            get
            {
                return powerSafeModeByOldEcus;
            }
            set
            {
                //Log.Info("Vehicle.IsPowerSafeModeActiveByOldEcus_set", "Setting vehicle power safe modus from \"{0}\" to \"{1}\".", powerSafeModeByOldEcus, value);
                powerSafeModeByOldEcus = value;
            }
        }

        [XmlIgnore]
        public bool VinNotReadbleFromCarAbort
        {
            get
            {
                return vinNotReadbleFromCarAbort;
            }
            set
            {
                vinNotReadbleFromCarAbort = value;
            }
        }

        [XmlIgnore]
        public bool IsPowerSafeModeActiveByNewEcus
        {
            get
            {
                return powerSafeModeByNewEcus;
            }
            set
            {
                //Log.Info("Vehicle.IsPowerSafeModeActiveByNewEcus_set", "Setting vehicle power safe modus from \"{0}\" to \"{1}\".", powerSafeModeByNewEcus, value);
                powerSafeModeByNewEcus = value;
            }
        }

        [XmlIgnore]
        public int? FaultCodeSum
        {
            get
            {
                return faultCodeSum;
            }
            set
            {
                faultCodeSum = value;
                OnPropertyChanged("FaultCodeSum");
            }
        }

        [XmlIgnore]
        public DateTime? C_DATETIME
        {
            get
            {
                try
                {
                    if (base.FA != null && base.FA.C_DATETIME.HasValue && base.FA.C_DATETIME > DateTime.MinValue)
                    {
                        return base.FA.C_DATETIME;
                    }
                    if (!string.IsNullOrEmpty(base.Modelljahr) && !string.IsNullOrEmpty(base.Modellmonat))
                    {
                        if (!cDatetimeByModelYearMonth.HasValue)
                        {
                            cDatetimeByModelYearMonth = DateTime.Parse(string.Format(CultureInfo.InvariantCulture, "{0}-{1}-01", base.Modelljahr, base.Modellmonat), CultureInfo.InvariantCulture);
                        }
                        return cDatetimeByModelYearMonth;
                    }
                }
                catch (Exception)
                {
                    //Log.WarningException("Vehicle.get_C_DATETIME()", exception);
                }
                return null;
            }
        }
#if false
        [XmlIgnore]
        IEnumerable<ICbsInfo> IVehicle.CBS => base.CBS;

        [XmlIgnore]
        IEnumerable<IDtc> IVehicle.CombinedFaults => base.CombinedFaults;

        [XmlIgnore]
        IEnumerable<IDiagCode> IVehicle.DiagCodes => base.DiagCodes;
#endif
        [XmlIgnore]
        IEnumerable<IEcu> IVehicle.ECU => base.ECU;

        [XmlIgnore]
        IFa IVehicle.FA => base.FA;

        [XmlIgnore]
        IEnumerable<IFfmResult> IVehicle.FFM => base.FFM;

        [XmlIgnore]
        IEnumerable<decimal> IVehicle.InstalledAdapters => base.InstalledAdapters;

        [XmlIgnore]
        IEcu IVehicle.SelectedECU => base.SelectedECU;
#if false
        [XmlIgnore]
        IVciDevice IVehicle.MIB => base.MIB;

        [XmlIgnore]
        IEnumerable<IServiceHistoryEntry> IVehicle.ServiceHistory => base.ServiceHistory;

        [XmlIgnore]
        IEnumerable<ITechnicalCampaign> IVehicle.TechnicalCampaigns => base.TechnicalCampaigns;
#endif
        [XmlIgnore]
        IVciDevice IVehicle.VCI => base.VCI;

#if false
        [XmlIgnore]
        IEnumerable<IZfsResult> IVehicle.ZFS => base.ZFS;
#endif

        [XmlIgnore]
        public double Clamp15MinValue
        {
            get
            {
                return clamp15MinValue;
            }
            set
            {
                if (clamp15MinValue != value)
                {
                    clamp15MinValue = value;
                    OnPropertyChanged("Clamp15MinValue");
                }
            }
        }

        public bool WithLfpBattery
        {
            get
            {
                return withLfpBattery;
            }
            set
            {
                if (withLfpBattery != value)
                {
                    withLfpBattery = value;
                    OnPropertyChanged("WithLfpBattery");
                }
            }
        }

        [XmlIgnore]
        public double Clamp30MinValue
        {
            get
            {
                return clamp30MinValue;
            }
            set
            {
                if (clamp30MinValue != value)
                {
                    clamp30MinValue = value;
                    OnPropertyChanged("Clamp30MinValue");
                }
            }
        }

        [XmlIgnore]
        public HashSet<int> ValidPWFStates
        {
            get
            {
                return validPWFStates;
            }
            set
            {
                if (validPWFStates != value)
                {
                    validPWFStates = value;
                    OnPropertyChanged("ValidPWFStates");
                }
            }
        }

        // ToDo: Check on update
        public override void OnPropertyChanged(string propertyName)
        {
            base.OnPropertyChanged(propertyName);
            switch (propertyName)
            {
                case "Gwsz":
                    base.OnPropertyChanged("DisplayGwsz");
                    break;
                case "SerialGearBox":
                    base.OnPropertyChanged("SerialGearBox7");
                    break;
            }
        }

        // ToDo: Check on update
        public string GetFSCfromUpdateIndex(string updateIndex, string huVariante)
        {
            string[] source = new string[2] { "HU_MGU", "ENAVEVO" };
            try
            {
                int num2 = Convert.ToInt32(updateIndex, 16);
                if (source.Any((string x) => huVariante.Equals(x)))
                {
                    string text = updateIndex.Substring(0, 2);
                    return updateIndex.Substring(2, 2) + "-" + text;
                }
                if (num2 > 45)
                {
                    int months = num2 - 54;
                    DateTime dateTime = new DateTime(2018, 7, 1).AddMonths(months);
                    new DateTime(2017, 10, 1);
                    return dateTime.Month + "-" + dateTime.Year;
                }
                if (num2 > 33)
                {
                    int num3 = 46 - num2;
                    int months2 = -1 * (num3 * 3 - 3);
                    DateTime dateTime2 = new DateTime(2017, 10, 1).AddMonths(months2);
                    return dateTime2.Month + "-" + dateTime2.Year;
                }
                return "-";
            }
            catch
            {
                //Log.Warning("Vehicle.ValidateFSC", "Exception Occurred validating HDDUpdateIndex {0}", updateIndex);
                return "-";
            }
        }

        public object Clone()
        {
            return MemberwiseClone();
        }

        public Vehicle DeepClone()
        {
            try
            {
                XmlSerializer xmlSerializer = new XmlSerializer(typeof(Vehicle));
                using (MemoryStream memoryStream = new MemoryStream())
                {
                    xmlSerializer.Serialize(memoryStream, this);
                    memoryStream.Seek(0L, SeekOrigin.Begin);
                    Vehicle obj = (Vehicle)xmlSerializer.Deserialize(memoryStream);
                    obj.CalculateFaultProperties();
                    return obj;
                }
            }
            catch (Exception)
            {
                //Log.WarningException(Log.CurrentMethod() + "()", exception);
                throw;
            }
        }

        // ToDo: Check on update
        public bool IsVINLessEReihe()
        {
            switch (base.Ereihe)
            {
                case "247":
                case "K599":
                case "248":
                case "259":
                case "259R":
                case "259S":
                case "R22":
                case "R21":
                case "R28":
                case "259C":
                case "K30":
                case "K41":
                case "247E":
                case "K569":
                case "E169":
                case "E189":
                case "K589":
                    return true;
                default:
                    return false;
            }
        }

        public bool IsEreiheValid()
        {
            if (!string.IsNullOrEmpty(base.Ereihe) && !(base.Ereihe == "UNBEK"))
            {
                return true;
            }
            return false;
        }
#if false
        public ECU GetECUbyDTC(decimal id)
        {
            if (base.ECU != null)
            {
                foreach (ECU item in base.ECU)
                {
                    if (item.FEHLER != null)
                    {
                        foreach (DTC item2 in item.FEHLER)
                        {
                            if (id.Equals(item2.Id))
                            {
                                return item;
                            }
                        }
                    }
                    if (item.INFO == null)
                    {
                        continue;
                    }
                    foreach (DTC item3 in item.INFO)
                    {
                        if (id.Equals(item3.Id))
                        {
                            return item;
                        }
                    }
                }
            }
            return null;
        }

        public DTC GetDTC(decimal id)
        {
            if (base.ECU != null)
            {
                foreach (ECU item in base.ECU)
                {
                    if (item.FEHLER != null)
                    {
                        foreach (DTC item2 in item.FEHLER)
                        {
                            if (id.Equals(item2.Id))
                            {
                                return item2;
                            }
                        }
                    }
                    if (item.INFO == null)
                    {
                        continue;
                    }
                    foreach (DTC item3 in item.INFO)
                    {
                        if (id.Equals(item3.Id))
                        {
                            return item3;
                        }
                    }
                }
            }
            if (base.CombinedFaults != null)
            {
                return base.CombinedFaults.FirstOrDefault(delegate (DTC item)
                {
                    decimal? id2 = item.Id;
                    decimal num = id;
                    return (id2.GetValueOrDefault() == num) & id2.HasValue;
                });
            }
            return null;
        }
#endif

        public void CalculateFaultProperties(IFFMDynamicResolver ffmResolver = null)
        {
#if false
			IEnumerable<Fault> collection = CalculateFaultList(this, base.ECU, base.CombinedFaults, base.ZFS, ffmResolver);
            FaultCodeSum = CalculateFaultCodeSum(base.ECU, base.CombinedFaults);
            Log.Info("Vehicle.CalculateFaultProperties()", "FaultCodeSum changed from \"{0}\" to \"{1}\".", FaultList?.Count, FaultCodeSum);
            FaultList = new List<Fault>(collection);
#endif
        }

        public typeECU_Transaction getECUTransaction(ECU transECU, string transId)
        {
            if (transECU == null)
            {
                return null;
            }
            if (string.IsNullOrEmpty(transId))
            {
                return null;
            }
            try
            {
                if (transECU.TAL != null)
                {
                    foreach (typeECU_Transaction item in transECU.TAL)
                    {
                        if (string.Compare(item.transactionId, transId, StringComparison.OrdinalIgnoreCase) == 0)
                        {
                            return item;
                        }
                    }
                }
            }
            catch (Exception)
            {
                //Log.WarningException("Vehicle.getECUTransaction()", exception);
            }
            return null;
        }

        public bool hasBusType(BusType bus)
        {
            if (base.ECU != null)
            {
                foreach (ECU item in base.ECU)
                {
                    if (item.BUS == bus)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public bool hasSA(string checkSA)
        {
            if (string.IsNullOrEmpty(checkSA))
            {
                //Log.Warning("CoreFramework.hasSA()", "checkSA was null or empty");
                return false;
            }
            if (base.FA == null)
            {
                return false;
            }
            FA fA = ((targetFA != null) ? targetFA : base.FA);
            if (fA.SA != null)
            {
                foreach (string item in fA.SA)
                {
                    if (string.Compare(item, checkSA, StringComparison.OrdinalIgnoreCase) != 0)
                    {
                        if (item.Length == 4 && string.Compare(item.Substring(1), checkSA, StringComparison.OrdinalIgnoreCase) == 0)
                        {
                            return true;
                        }
                        continue;
                    }
                    return true;
                }
            }
            if (fA.E_WORT != null)
            {
                foreach (string item2 in fA.E_WORT)
                {
                    if (string.Compare(item2, checkSA, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        return true;
                    }
                }
            }
            if (fA.HO_WORT != null)
            {
                foreach (string item3 in fA.HO_WORT)
                {
                    if (string.Compare(item3, checkSA, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        return true;
                    }
                }
            }
            if (fA.DealerInstalledSA != null && fA.DealerInstalledSA.Any((string item) => string.Equals(item, checkSA, StringComparison.OrdinalIgnoreCase)))
            {
                return true;
            }
            return false;
        }

        public bool HasUnidentifiedECU()
        {
            bool flag = false;
            if (base.ECU != null)
            {
                foreach (ECU item in base.ECU)
                {
                    if (string.IsNullOrEmpty(item.VARIANTE) || !item.COMMUNICATION_SUCCESSFULLY)
                    {
                        flag = true;
                    }
                }
                return flag;
            }
            return true;
        }

        public bool? hasFFM(string checkFFM)
        {
            if (string.IsNullOrEmpty(checkFFM))
            {
                //Log.Warning("CoreFramework.hasFFM()", "checkFFM was null or empty");
                return true;
            }
            if (base.FFM != null)
            {
                foreach (FFMResult item in base.FFM)
                {
                    if (string.Compare(item.Name, checkFFM, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        return item.Result;
                    }
                }
            }
            return null;
        }

        public void AddOrUpdateFFM(FFMResult ffm)
        {
            if (base.FFM == null || ffm == null)
            {
                return;
            }
            foreach (FFMResult item in base.FFM)
            {
                if (string.Compare(item.Name, ffm.Name, StringComparison.OrdinalIgnoreCase) == 0)
                {
                    item.ID = ffm.ID;
                    item.Evaluation = ffm.Evaluation;
                    item.ReEvaluationNeeded = ffm.ReEvaluationNeeded;
                    item.Result = ffm.Result;
                    return;
                }
            }
            base.FFM.Add(ffm);
        }

        public ECU getECU(long? sgAdr)
        {
            try
            {
                foreach (ECU item in base.ECU)
                {
                    if (item.ID_SG_ADR != sgAdr)
                    {
                        if (!string.IsNullOrEmpty(item.ECU_ADR))
                        {
                            string text = string.Empty;
                            if (item.ECU_ADR.Length >= 4 && item.ECU_ADR.Substring(0, 2).ToLower() == "0x")
                            {
                                text = item.ECU_ADR.ToUpper().Substring(2);
                            }
                            if (item.ECU_ADR.Length == 2)
                            {
                                text = item.ECU_ADR.ToUpper();
                            }
                            if (text == string.Format(CultureInfo.InvariantCulture, "{0:X2}", sgAdr))
                            {
                                return item;
                            }
                        }
                        continue;
                    }
                    return item;
                }
            }
            catch (Exception)
            {
                //Log.WarningException("Vehicle.getECU()", exception);
            }
            return null;
        }

        public ECU getECU(long? sgAdr, long? subAddress)
        {
            try
            {
                foreach (ECU item in base.ECU)
                {
                    if (item.ID_SG_ADR == sgAdr && item.ID_LIN_SLAVE_ADR == subAddress)
                    {
                        return item;
                    }
                }
            }
            catch (Exception)
            {
                //Log.WarningException("Vehcile.getECU()", exception);
            }
            return null;
        }

        public ECU getECUbyECU_SGBD(string ECU_SGBD)
        {
            if (string.IsNullOrEmpty(ECU_SGBD))
            {
                return null;
            }
            try
            {
                string[] array = ECU_SGBD.Split('|');
                foreach (string b in array)
                {
                    foreach (ECU item in base.ECU)
                    {
                        if (string.Equals(item.ECU_SGBD, b, StringComparison.OrdinalIgnoreCase) || string.Equals(item.VARIANTE, b, StringComparison.OrdinalIgnoreCase))
                        {
                            return item;
                        }
                    }
                }
            }
            catch (Exception)
            {
                //Log.WarningException("Vehicle.getECUbyECU_SGBD()", exception);
            }
            return null;
        }

        public ECU getECUbyTITLE_ECUTREE(string grobName)
        {
            if (string.IsNullOrEmpty(grobName))
            {
                return null;
            }
            try
            {
                foreach (ECU item in base.ECU)
                {
                    if (string.Compare(item.TITLE_ECUTREE, grobName, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        return item;
                    }
                }
            }
            catch (Exception)
            {
                //Log.WarningException("Vehicle.getECUbyTITLE_ECUTREE()", exception);
            }
            return null;
        }

        public ECU getECUbyECU_GRUPPE(string ECU_GRUPPE)
        {
            if (string.IsNullOrEmpty(ECU_GRUPPE))
            {
                //Log.Warning("Vehicle.getECUbyECU_GRUPPE()", "parameter was null or empty");
                return null;
            }
            if (base.ECU == null)
            {
                //Log.Warning("Vehicle.getECUbyECU_GRUPPE()", "ECU was null");
                return null;
            }
            try
            {
                foreach (ECU item in base.ECU)
                {
                    if (string.IsNullOrEmpty(item.ECU_GRUPPE))
                    {
                        continue;
                    }
                    string[] array = ECU_GRUPPE.Split('|');
                    string[] array2 = item.ECU_GRUPPE.Split('|');
                    foreach (string a in array2)
                    {
                        string[] array3 = array;
                        foreach (string b in array3)
                        {
                            if (string.Equals(a, b, StringComparison.OrdinalIgnoreCase))
                            {
                                return item;
                            }
                        }
                    }
                }
            }
            catch (Exception)
            {
                //Log.WarningException("Vehicle.getECUbyECU_GRUPPE()", exception);
            }
            return null;
        }

        public uint getDiagProtECUCount(typeDiagProtocoll ecuDiag)
        {
            uint num2 = 0u;
            try
            {
                foreach (ECU item in base.ECU)
                {
                    if (item.DiagProtocoll == ecuDiag)
                    {
                        num2++;
                    }
                }
                return num2;
            }
            catch (Exception)
            {
                //Log.WarningException("Vehcile.getECU()", exception);
                return num2;
            }
        }

#if false
        public typeCBSInfo getCBSMeasurementValue(typeCBSMeaurementType mType)
        {
            try
            {
                if (base.CBS == null)
                {
                    return null;
                }
                foreach (typeCBSInfo cB in base.CBS)
                {
                    if (cB.Type == mType)
                    {
                        return cB;
                    }
                }
                return null;
            }
            catch (Exception exception)
            {
                //Log.WarningException("Vehicle.getCBSMeasurementValue()", exception);
            }
            return null;
        }

        public bool addOrUpdateCBSMeasurementValue(typeCBSInfo cbsNew)
        {
            try
            {
                if (cbsNew == null)
                {
                    return false;
                }
                if (base.CBS == null)
                {
                    base.CBS = new ObservableCollection<typeCBSInfo>();
                }
                foreach (typeCBSInfo cB in base.CBS)
                {
                    if (cB.Type == cbsNew.Type)
                    {
                        base.CBS.Remove(cB);
                        base.CBS.Add(cbsNew);
                        return true;
                    }
                }
                base.CBS.Add(cbsNew);
                return true;
            }
            catch (Exception exception)
            {
                //Log.WarningException("Vehicle.addOrUpdateCBSMeasurementValue()", exception);
            }
            return false;
        }

        public bool addOrUpdateCBSMeasurementValues(IList<typeCBSInfo> cbsNewList)
        {
            int num = 1;
            try
            {
                if (cbsNewList == null)
                {
                    return false;
                }
                if (base.CBS == null)
                {
                    base.CBS = new ObservableCollection<typeCBSInfo>();
                }
                foreach (typeCBSInfo cbsNew in cbsNewList)
                {
                    bool flag = false;
                    foreach (typeCBSInfo cB in base.CBS)
                    {
                        if (cB.Type == cbsNew.Type)
                        {
                            int num2 = base.CBS.IndexOf(cB);
                            if (num2 >= 0 && num2 < base.CBS.Count)
                            {
                                base.CBS[num2] = cbsNew;
                            }
                            flag = true;
                        }
                    }
                    if (!flag)
                    {
                        base.CBS.Add(cbsNew);
                    }
                }
                return true;
            }
            catch (Exception exception)
            {
                //Log.WarningException("Vehicle.addOrUpdateCBSMeasurementValue()", exception);
            }
            return false;
        }
#endif
        public void AddEcu(ECU ecu)
        {
            base.ECU.Add(ecu);
        }

        public bool AddOrUpdateECU(ECU nECU)
        {
            try
            {
                if (nECU == null)
                {
                    //Log.Warning("Vehicle.AddOrUpdateECU()", "ecu was null");
                    return false;
                }
                if (base.ECU == null)
                {
                    base.ECU = new ObservableCollection<ECU>();
                }
                foreach (ECU item in base.ECU)
                {
                    if (item.ID_SG_ADR == nECU.ID_SG_ADR)
                    {
                        int num2 = base.ECU.IndexOf(item);
                        if (num2 >= 0 && num2 < base.ECU.Count)
                        {
                            base.ECU[num2] = nECU;
                            //Log.Info("Vehicle.AddOrUpdateECU()", "updating ecu: \"{0:X2}\" (hex.), slave address: \"{1:X2}\" (hex.).", nECU.ID_SG_ADR, nECU.ID_LIN_SLAVE_ADR);
                            return true;
                        }
                    }
                }
                base.ECU.Add(nECU);
                //Log.Info("Vehicle.AddOrUpdateECU()", "adding ecu: \"{0:X2}\" (hex.), slave address: \"{1:X2}\" (hex.).", nECU.ID_SG_ADR, nECU.ID_LIN_SLAVE_ADR);
                return true;
            }
            catch (Exception)
            {
                //Log.WarningException("Vehicle.AddOrUpdateECU()", exception);
            }
            return false;
        }

        public bool getISTACharacteristics(decimal id, out string value, long datavalueId, ValidationRuleInternalResults internalResult)
		{
            PdszDatabase.CharacteristicRoots characteristicRootsById = ClientContext.GetDatabase(this)?.GetCharacteristicRootsById(id.ToString(CultureInfo.InvariantCulture));
			if (characteristicRootsById != null)
			{
				return new VehicleCharacteristicVehicleHelper(this).GetISTACharacteristics(characteristicRootsById, out value, id, this, datavalueId, internalResult);
			}
			value = "???";
			return false;
		}

        public void UpdateStatus(string name, StateType type, double? progress)
        {
            try
            {
                string status_FunctionName = base.Status_FunctionName;
                StateType status_FunctionState = base.Status_FunctionState;
                //Log.Info("Vehicle.UpdateStatus()", "Change state from '{0}/{1}' to '{2}/{3}'", status_FunctionName, status_FunctionState, name, type);
                base.Status_FunctionName = name;
                base.Status_FunctionState = type;
                base.Status_FunctionStateLastChangeTime = DateTime.Now;
                if (progress.HasValue)
                {
                    base.Status_FunctionProgress = progress.Value;
                }
                IsNoVehicleCommunicationRunning = base.Status_FunctionState != StateType.running;
            }
            catch (Exception)
            {
                //Log.WarningException("Vehicle.UpdateStatus()", exception);
            }
        }

        [XmlIgnore]
        public bool IsNoVehicleCommunicationRunning
        {
            get
            {
                return noVehicleCommunicationRunning;
            }
            set
            {
                noVehicleCommunicationRunning = value;
                OnPropertyChanged("IsNoVehicleCommunicationRunning");
            }
        }

        public bool IsVehicleWithOnlyVin7()
        {
            return VIN10Prefix.Equals("FILLER17II", StringComparison.InvariantCultureIgnoreCase);
        }

        // ToDo: Check on update
        public bool evalILevelExpression(string iLevelExpressions)
        {
            bool flag = false;
            bool flag2 = true;
            try
            {
                if (string.IsNullOrEmpty(iLevelExpressions))
                {
                    return true;
                }
                if (string.IsNullOrEmpty(base.ILevel))
                {
                    //Log.Info("Vehicle.evaILevelExpression()", "ILevel unknown; result will be true; expression was: {0}", iLevelExpressions);
                    return true;
                }
                if (iLevelExpressions.Contains("&"))
                {
                    flag2 = false;
                    flag = true;
                }
                //if (CoreFramework.DebugLevel > 0)
                //{
                //    Log.Info("Vehicle.evalILevelExpression()", "expression:{0} vehicle iLEVEL:{1}", iLevelExpressions, base.ILevel);
                //}
                string[] separator = new string[2] { "&", "|" };
                string[] array = iLevelExpressions.Split(separator, StringSplitOptions.RemoveEmptyEntries);
                foreach (string text in array)
                {
                    string[] separator2 = new string[1] { "," };
                    string[] array2 = text.Split(separator2, StringSplitOptions.RemoveEmptyEntries);
                    if (array2.Length != 2)
                    {
                        continue;
                    }
                    //Log.Info("Vehicle.evalILevelExpression()", "expression {0} {1}", base.ILevel, text);
                    if (string.Compare(base.ILevel, 0, array2[1], 0, 4, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        switch (array2[0])
                        {
                            case "<":
                                //if (CoreFramework.DebugLevel > 0 && FormatConverter.ExtractNumericalILevel(base.ILevel) < FormatConverter.ExtractNumericalILevel(array2[1]))
                                //{
                                //    Log.Info("Vehicle.evalILevelExpression()", "< was true");
                                //}
                                flag = ((!flag2) ? (flag & (FormatConverter.ExtractNumericalILevel(base.ILevel) < FormatConverter.ExtractNumericalILevel(array2[1]))) : (flag | (FormatConverter.ExtractNumericalILevel(base.ILevel) < FormatConverter.ExtractNumericalILevel(array2[1]))));
                                break;
                            case "=":
                                //if (CoreFramework.DebugLevel > 0 && FormatConverter.ExtractNumericalILevel(base.ILevel) == FormatConverter.ExtractNumericalILevel(array2[1]))
                                //{
                                //    Log.Info("Vehicle.evalILevelExpression()", "= was true");
                                //}
                                flag = ((!flag2) ? (flag & (FormatConverter.ExtractNumericalILevel(base.ILevel) == FormatConverter.ExtractNumericalILevel(array2[1]))) : (flag | (FormatConverter.ExtractNumericalILevel(base.ILevel) == FormatConverter.ExtractNumericalILevel(array2[1]))));
                                break;
                            case ">=":
                                //if (CoreFramework.DebugLevel > 0 && FormatConverter.ExtractNumericalILevel(base.ILevel) >= FormatConverter.ExtractNumericalILevel(array2[1]))
                                //{
                                //    Log.Info("Vehicle.evalILevelExpression()", ">= was true");
                                //}
                                flag = ((!flag2) ? (flag & (FormatConverter.ExtractNumericalILevel(base.ILevel) >= FormatConverter.ExtractNumericalILevel(array2[1]))) : (flag | (FormatConverter.ExtractNumericalILevel(base.ILevel) >= FormatConverter.ExtractNumericalILevel(array2[1]))));
                                break;
                            case ">":
                                //if (CoreFramework.DebugLevel > 0 && FormatConverter.ExtractNumericalILevel(base.ILevel) > FormatConverter.ExtractNumericalILevel(array2[1]))
                                //{
                                //    Log.Info("Vehicle.evalILevelExpression()", "> was true");
                                //}
                                flag = ((!flag2) ? (flag & (FormatConverter.ExtractNumericalILevel(base.ILevel) > FormatConverter.ExtractNumericalILevel(array2[1]))) : (flag | (FormatConverter.ExtractNumericalILevel(base.ILevel) > FormatConverter.ExtractNumericalILevel(array2[1]))));
                                break;
                            case "<=":
                                //if (CoreFramework.DebugLevel > 0 && FormatConverter.ExtractNumericalILevel(base.ILevel) <= FormatConverter.ExtractNumericalILevel(array2[1]))
                                //{
                                //    Log.Info("Vehicle.evalILevelExpression()", "<= was true");
                                //}
                                flag = ((!flag2) ? (flag & (FormatConverter.ExtractNumericalILevel(base.ILevel) <= FormatConverter.ExtractNumericalILevel(array2[1]))) : (flag | (FormatConverter.ExtractNumericalILevel(base.ILevel) <= FormatConverter.ExtractNumericalILevel(array2[1]))));
                                break;
                            case "!=":
                            case "<>":
                                //if (CoreFramework.DebugLevel > 0 && FormatConverter.ExtractNumericalILevel(base.ILevel) != FormatConverter.ExtractNumericalILevel(array2[1]))
                                //{
                                //    Log.Info("Vehicle.evalILevelExpression()", "!= was true");
                                //}
                                flag = ((!flag2) ? (flag & (FormatConverter.ExtractNumericalILevel(base.ILevel) != FormatConverter.ExtractNumericalILevel(array2[1]))) : (flag | (FormatConverter.ExtractNumericalILevel(base.ILevel) != FormatConverter.ExtractNumericalILevel(array2[1]))));
                                break;
                        }
                    }
                    else
                    {
                        //Log.Warning("Vehicle.evalILevelExpression()", "iLevel main type does not match");
                    }
                }
                return flag;
            }
            catch (Exception)
            {
                //Log.WarningException("Vehicle.evalILevelExpression()", exception);
                return true;
            }
        }

        // ToDo: Check on update
        public bool? HasMSAButton()
        {
            switch (Produktlinie.ToUpper())
            {
                case "PL6-ALT":
                    if (base.FA != null && base.FA.C_DATETIME.HasValue && base.FA.C_DATETIME > LciDateE60)
                    {
                        return true;
                    }
                    return false;
                case "PL5-ALT":
                case "PL3-ALT":
                    return false;
                case "PL5":
                case "PL2":
                case "PL3":
                case "PL7":
                case "PL4":
                case "35LG":
                case "PL6":
                case "PLLI":
                case "PLLU":
                    return true;
                default:
                    return null;
            }
        }

        public bool isECUAlreadyScanned(ECU checkSG)
        {
            try
            {
                foreach (ECU item in base.ECU)
                {
                    if (item.ID_SG_ADR != checkSG.ID_SG_ADR)
                    {
                        if (!string.IsNullOrEmpty(item.ECU_ADR) && !string.IsNullOrEmpty(checkSG.ECU_ADR) && string.Compare(item.ECU_ADR, checkSG.ECU_ADR, StringComparison.OrdinalIgnoreCase) == 0)
                        {
                            return true;
                        }
                        continue;
                    }
                    return true;
                }
            }
            catch (Exception)
            {
                //Log.WarningException("Vehicle.isECUAlreadyScanned()", exception);
            }
            return false;
        }

        // ToDo: Check on update
        public T getResultAs<T>(string resultName)
        {
            try
            {
                Type typeFromHandle = typeof(T);
                if (!string.IsNullOrEmpty(resultName))
                {
                    object obj = null;
                    switch (resultName)
                    {
                        case "/VehicleConfiguration.RootNode/GetGroupListEx/Arguments/Fahrzeugauftrag":
                            obj = base.FA.STANDARD_FA;
                            break;
                        case "/VehicleConfiguration.RootNode/GetGroupListEx/Arguments/BaureihenVerbund":
                            obj = BasisEReihe;
                            break;
                        case "/Result/Baustand":
                            obj = base.FA.C_DATE;
                            break;
                        case "/VehicleConfiguration.RootNode/GetGroupListEx/Arguments/IStufe":
                            obj = base.ILevel;
                            break;
                        case "/Result/HOWortListe":
                            {
                                string text4 = string.Empty;
                                foreach (string item in base.FA.HO_WORT)
                                {
                                    text4 = text4 + item + ",";
                                }
                                text4 = text4.TrimEnd(',');
                                obj = text4;
                                break;
                            }
                        case "/Result/SonderAusstattungsListe":
                            {
                                string text3 = string.Empty;
                                foreach (string item2 in base.FA.SA)
                                {
                                    text3 = text3 + item2 + ",";
                                }
                                text3 = text3.TrimEnd(',');
                                obj = text3;
                                break;
                            }
                        case "/Result/GruppenListe":
                        case "/Result/DList":
                            {
                                string text2 = string.Empty;
                                foreach (ECU item3 in base.ECU)
                                {
                                    text2 = text2 + item3.ECU_GRUPPE + ",";
                                }
                                text2 = text2.TrimEnd(',');
                                obj = text2;
                                break;
                            }
                        case "/Result/EWortListe":
                            {
                                string text = string.Empty;
                                foreach (string item4 in base.FA.E_WORT)
                                {
                                    text = text + item4 + ",";
                                }
                                text = text.TrimEnd(',');
                                obj = text;
                                break;
                            }
                        default:
                            //Log.Error("VehicleHelper.getResultAs<T>", "Unknown resultName '{0}' found!", resultName);
                            break;
                    }
                    if (obj != null)
                    {
                        if (obj.GetType() != typeFromHandle)
                        {
                            return (T)Convert.ChangeType(obj, typeFromHandle);
                        }
                        return (T)obj;
                    }
                }
            }
            catch (Exception)
            {
                //Log.WarningException("Vehicle.getISTAResultAs(string resultName)", exception);
            }
            return default(T);
        }
#if false
		public void AddDiagCode(string diagCodeString, string diagCodeSuffixString, string originatingAblauf, IList<string> reparaturPaketList, bool teileClearingFlag)
        {
            if (!string.IsNullOrEmpty(diagCodeString))
            {
                if (base.DiagCodes == null)
                {
                    base.DiagCodes = new ObservableCollection<typeDiagCode>();
                }
                typeDiagCode typeDiagCode2 = new typeDiagCode();
                typeDiagCode2.DiagnoseCode = diagCodeString;
                typeDiagCode2.DiagnoseCodeSuffix = diagCodeSuffixString;
                typeDiagCode2.Origin = ((originatingAblauf == null) ? string.Empty : originatingAblauf);
                if (reparaturPaketList != null)
                {
                    typeDiagCode2.ReparaturPaket = new ObservableCollection<string>(reparaturPaketList);
                }
                else
                {
                    typeDiagCode2.ReparaturPaket = new ObservableCollection<string>();
                }
                typeDiagCode2.TeileClearing = teileClearingFlag;
                base.DiagCodes.Add(typeDiagCode2);
                if (!string.IsNullOrEmpty(diagCodeString) && !diagCodesProgramming.Contains(diagCodeString))
                {
                    diagCodesProgramming.Add(diagCodeString);
                }
            }
        }
#endif
        // ToDo: Check on update
        public bool IsPreE65Vehicle()
        {
            if (!string.IsNullOrEmpty(base.Ereihe) && (Regex.Match(base.Ereihe, "^E[0-5][0-9]$").Success || Regex.Match(base.Ereihe, "^E6[0-4]$").Success))
            {
                return true;
            }
            return false;
        }

        // ToDo: Check on update
        public bool IsPreDS2Vehicle()
        {
            if (!string.IsNullOrEmpty(base.Ereihe))
            {
                if (Regex.Match(base.Ereihe, "^E[0-3][0-5]$").Success)
                {
                    return true;
                }
                if ("E36".Equals(base.Ereihe))
                {
                    return C_DATETIME < LciDateE36;
                }
            }
            return false;
        }

        // ToDo: Check on update
        public bool IsMotorcycle()
        {
            if (base.BNType != BNType.BN2000_MOTORBIKE && base.BNType != BNType.BN2020_MOTORBIKE && base.BNType != BNType.BNK01X_MOTORBIKE && base.BNType != BNType.BN2000_GIBBS)
            {
                return base.BNType == BNType.BN2020_CAMPAGNA;
            }
            return true;
        }

        // ToDo: Check on update
        public bool IsRRSeries2()
        {
            if (!"RR1".Equals(base.Ereihe) && !"RR2".Equals(base.Ereihe) && !"RR3".Equals(base.Ereihe))
            {
                return false;
            }
            if ("RR1_2020".Equals(base.MainSeriesSgbd))
            {
                return true;
            }
            if (C_DATETIME.HasValue)
            {
                return C_DATETIME > lciRRS2;
            }
            return false;
        }

        // ToDo: Check on update
        public bool IsPowertrainSystemCustomerVehicle()
        {
            if (base.BNType != BNType.BN2000_GIBBS && base.BNType != BNType.BN2000_RODING && base.BNType != BNType.BN2000_WIESMANN)
            {
                return base.BNType == BNType.BN2000_PGO;
            }
            return true;
        }

        IEcu IVehicle.getECU(long? sgAdr)
		{
			return this.getECU(sgAdr);
		}

		IEcu IVehicle.getECU(long? sgAdr, long? subAddress)
		{
			return this.getECU(sgAdr, subAddress);
		}

		IEcu IVehicle.getECUbyECU_GRUPPE(string ECU_GRUPPE)
		{
			return this.getECUbyECU_GRUPPE(ECU_GRUPPE);
		}

		public bool IsVehicleLockedDown()
		{
			return false;
		}

        // ToDo: Check on update
        public bool? IsABSVehicle()
        {
            if (base.ECU != null && base.ECU.Count > 0)
            {
                string[] array = new string[16]
                {
                    "ASCMK20", "absmk4", "absmk4g", "abs5", "abs_uc", "asc4gus", "asc5", "asc57", "asc57r75", "asc5d",
                    "ascmk20", "ascmk4.prg", "ascmk4g", "ascmk4g1", "asc_l22", "asc_t"
                };
                ECU eCU = getECU(86L, null);
                if (eCU != null && eCU.IDENT_SUCCESSFULLY)
                {
                    string[] array2 = array;
                    int num2 = 0;
                    while (true)
                    {
                        if (num2 < array2.Length)
                        {
                            if (array2[num2].Equals(eCU.VARIANTE, StringComparison.OrdinalIgnoreCase))
                            {
                                break;
                            }
                            num2++;
                            continue;
                        }
                        return false;
                    }
                    return true;
                }
                eCU = getECU(41L, null);
                if (eCU != null && eCU.IDENT_SUCCESSFULLY)
                {
                    string[] array2 = array;
                    int num2 = 0;
                    while (true)
                    {
                        if (num2 < array2.Length)
                        {
                            if (array2[num2].Equals(eCU.VARIANTE, StringComparison.OrdinalIgnoreCase))
                            {
                                break;
                            }
                            num2++;
                            continue;
                        }
                        return false;
                    }
                    return true;
                }
                eCU = getECU(54L, null);
                if (eCU != null && eCU.IDENT_SUCCESSFULLY)
                {
                    string[] array2 = array;
                    int num2 = 0;
                    while (true)
                    {
                        if (num2 < array2.Length)
                        {
                            if (array2[num2].Equals(eCU.VARIANTE, StringComparison.OrdinalIgnoreCase))
                            {
                                break;
                            }
                            num2++;
                            continue;
                        }
                        return false;
                    }
                    return true;
                }
			}
			return null;
		}

#if false
        private static ObservableCollection<Fault> CalculateFaultList(Vehicle vehicle, IEnumerable<ECU> ecus, IEnumerable<DTC> combinedFaults, ObservableCollection<ZFSResult> zfs, IFFMDynamicResolver ffmFesolver = null)
        {
            bool flag = true;
            bool flag2 = true;
            if (ConfigSettings.OperationalMode != 0)
            {
                flag = ConfigSettings.getConfigStringAsBoolean("TesterGUI.HideBogusFaults", defaultValue: true);
                flag2 = ConfigSettings.getConfigStringAsBoolean("TesterGUI.HideUnknownFaults", defaultValue: false);
			}
			ObservableCollection<Fault> observableCollection = new ObservableCollection<Fault>();
			try
			{
                if (ecus != null)
                {
                    foreach (ECU item in ecus.Where((ECU item) => item.FEHLER != null))
                    {
                        foreach (DTC item2 in item.FEHLER)
                        {
							Fault fault = new Fault(item, item2, zfs, vehicle.IsNewFaultMemoryActive);
							if (item2.Relevance == true)
							{
                                if (ffmFesolver != null && ConfigSettings.getConfigStringAsBoolean("EnableRelevanceFaultCode", defaultValue: true))
                                {
                                    fault.ResolveRelevanceFaultCode(vehicle, ffmFesolver);
                                    if (fault.DTC.Relevance == true)
                                    {
                                        observableCollection.AddIfNotContains(fault);
                                    }
                                }
                                else
                                {
                                    observableCollection.AddIfNotContains(fault);
                                }
                            }
                            else if (item2.Relevance == false && !flag)
                            {
                                observableCollection.AddIfNotContains(new Fault(item, item2, zfs, vehicle.IsNewFaultMemoryActive));
							}
                            else if (!item2.Relevance.HasValue && !flag2)
                            {
                                observableCollection.AddIfNotContains(new Fault(item, item2, zfs, vehicle.IsNewFaultMemoryActive));
							}
						}
					}
                }
                if (combinedFaults == null)
                {
                    return observableCollection;
                }
                foreach (DTC combinedFault in combinedFaults)
                {
                    Fault fault2 = new Fault(null, combinedFault, null, vehicle.IsNewFaultMemoryActive);
                    fault2.ResolveLabels(vehicle, null);
					observableCollection.AddIfNotContains(fault2);
				}
				return observableCollection;
			}
            catch (Exception exception)
            {
                //Log.ErrorException("Vehicle.CalculateFaultList()", exception);
                return observableCollection;
            }
        }

        private static int? CalculateFaultCodeSum(IEnumerable<IEcu> ecus, IEnumerable<DTC> combinedFaults)
        {
            int num2 = 0;
            bool flag = true;
            bool flag2 = true;
            if (ConfigSettings.OperationalMode != 0)
            {
                flag = ConfigSettings.getConfigStringAsBoolean("TesterGUI.HideBogusFaults", defaultValue: true);
                flag2 = ConfigSettings.getConfigStringAsBoolean("TesterGUI.HideUnknownFaults", defaultValue: false);
            }
            try
            {
                if (ecus != null)
                {
                    foreach (IEcu ecu in ecus)
                    {
                        if (ecu.FEHLER == null)
                        {
                            continue;
                        }
                        foreach (IDtc item in ecu.FEHLER)
                        {
                            bool? relevance = item.Relevance;
                            if (relevance.HasValue)
                            {
                                if (relevance.GetValueOrDefault())
                                {
                                    num2++;
                                }
                                else if (!flag)
                                {
                                    num2++;
                                }
                            }
                            else if (!flag2)
                            {
                                num2++;
                            }
                        }
                    }
                }
                if (combinedFaults != null && combinedFaults.Any())
                {
                    num2 += combinedFaults.Count();
                }
                if (num2 == 0 && (ecus == null || !ecus.Any() || ecus.Any(delegate (IEcu item)
                    {
                        int num3 = 19;
                        return !item.FS_SUCCESSFULLY && !item.BUS.ToString().Contains("VIRTUAL");
                    })))
                {
                    return null;
                }
                return num2;
            }
            catch (Exception exception)
            {
                //Log.WarningException("Vehicle.CalculateFaultCodeSum()", exception);
                return null;
            }
        }

        public void AddCombinedDTC(DTC dtc)
        {
            if (dtc == null)
            {
                //Log.Warning("Vehicle.AddCombinedDTC()", "dtc was null");
            }
            else if (dtc.IsVirtual && dtc.IsCombined && base.CombinedFaults != null)
            {
                base.CombinedFaults.AddIfNotContains(dtc);
            }
        }
#endif
        public bool GetProgrammingEnabledForBn(string bn)
		{
			return Vehicle.GetBnTypes(bn).Contains(base.BNType);
		}

		public bool IsProgrammingSupported(bool considerLogisticBase)
        {
            return true;
            //return (ConfigSettings.IsProgrammingEnabled() || (considerLogisticBase && ConfigSettings.IsLogisticBaseEnabled())) && this.GetProgrammingEnabledForBn(ConfigSettings.getConfigString("BMW.Rheingold.Programming.BN", "BN2020,BN2020_MOTORBIKE")) && ConfigSettings.OperationalMode != OperationalMode.TELESERVICE;
        }

        private static ISet<BNType> GetBnTypes(string bnTypes)
        {
            ISet<BNType> set = new HashSet<BNType>();
            if (string.IsNullOrEmpty(bnTypes))
            {
                return set;
            }
            string[] array = bnTypes.Split(',');
            foreach (string text in array)
            {
                if (Enum.TryParse<BNType>(text, ignoreCase: false, out var result))
                {
                    set.Add(result);
                    continue;
                }
                //Log.Error("Vehicle.GetBnTypes()", "Ignore BN \"{0}\", because of missconfiguration.", text);
            }
            return set;
        }

		// ToDo: Check on update
        public int GetCustomHashCode()
        {
            int num = 37;
            int num2 = 327;
            num = 37 * GetHashCode();
            if (!string.IsNullOrWhiteSpace(base.VIN17))
            {
                num += base.VIN17.GetHashCode();
                num *= num2;
            }
            ObservableCollection<ECU> eCU = base.ECU;
            if (eCU != null && eCU.Any())
            {
                foreach (ECU item in base.ECU)
                {
                    num += item.GetHashCode();
                    num *= num2;
                    if (!string.IsNullOrEmpty(item.VARIANTE))
                    {
                        num += item.VARIANTE.GetHashCode();
                        num *= num2;
                    }
                }
            }
            if (!string.IsNullOrWhiteSpace(base.Ereihe))
            {
                num += base.Ereihe.GetHashCode();
                num *= num2;
            }
            if (!string.IsNullOrWhiteSpace(base.Baureihenverbund))
            {
                num += base.Baureihenverbund.GetHashCode();
                num *= num2;
            }
            if (C_DATETIME.HasValue)
            {
                num += C_DATETIME.GetHashCode();
                num *= num2;
            }
            return num;
        }

        // ToDo: Check on update
        public const string BnProgramming = "BN2020,BN2020_MOTORBIKE";

        private static readonly DateTime LciDateE36 = DateTime.Parse("1998-03-01", CultureInfo.InvariantCulture);

		private static readonly DateTime LciDateE60 = DateTime.Parse("2005-09-01", CultureInfo.InvariantCulture);

		//private readonly ObservableCollectionEx<Fault> pKodeList;

		//private readonly ParameterContainer sessionDataStore;

		private string vinRangeType;

		private string vinRangeTypeLastResolvedType;

		private FA targetFA;

		private bool isBusy;

		private string productLine;

		private string doorNumber;

		private string securityRelevant;

		private DateTime? cDatetimeByModelYearMonth;

		private HashSet<int> validPWFStates;

		private double clamp15MinValue;

		private double clamp30MinValue;

		private bool withLfpBattery;

		private bool isClosingOperationActive;

		private bool powerSafeModeByOldEcus;

		private bool powerSafeModeByNewEcus;

		private bool vehicleTestDone;

		private bool isReadingFastaDataFinished = true;

		private bool vinNotReadbleFromCarAbort;

		private int? faultCodeSum;

		private string targetILevel;

		private readonly ObservableCollection<string> diagCodesProgramming;

		//private IList<Fault> faultList;

		private bool noVehicleCommunicationRunning;

		private string salesDesignationBadgeUIText;

		private string eBezeichnungUIText;

		private const int indexOfFirsHDDAboUpdateInDecimal = 54;

		private bool isNewFaultMemoryActiveField;

		private bool isNewFaultMemoryExpertModeActiveField;

		//private BlockingCollection<VirtualFaultInfo> virtualFaultInfoList;

		private string hmiVersion;

		private bool sp2021Enabled;

        private string kraftstoffartEinbaulage;

		private static readonly DateTime lciRRS2 = DateTime.Parse("2012-05-31", CultureInfo.InvariantCulture);
    }
}
