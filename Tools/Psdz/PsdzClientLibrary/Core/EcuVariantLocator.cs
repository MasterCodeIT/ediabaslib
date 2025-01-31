﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace PsdzClient.Core
{
	public class EcuVariantLocator : ISPELocator, IEcuVariantLocator
	{
		public EcuVariantLocator(PdszDatabase.EcuVar ecuVariant)
		{
			this.ecuVariant = ecuVariant;
			//this.children = new ISPELocator[0];
		}

		public static IEcuVariantLocator CreateEcuVariantLocator(string ecuVariant, Vehicle vecInfo, IFFMDynamicResolver ffmResolver)
		{
			PdszDatabase.EcuVar ecuVariantByName = ClientContext.GetDatabase(vecInfo)?.GetEcuVariantByName(ecuVariant);
			if (ecuVariantByName != null)
			{
				return new EcuVariantLocator(ecuVariantByName, vecInfo, ffmResolver);
			}
			return null;
		}

		public EcuVariantLocator(decimal id, Vehicle vec, IFFMDynamicResolver ffmResolver)
		{
            this.vecInfo = vec;
			this.ecuVariant = ClientContext.GetDatabase(this.vecInfo)?.GetEcuVariantById(id.ToString(CultureInfo.InvariantCulture));
			this.ffmResolver = ffmResolver;
		}

		public EcuVariantLocator(PdszDatabase.EcuVar ecuVariant, Vehicle vec, IFFMDynamicResolver ffmResolver)
		{
            this.vecInfo = vec;
			this.ecuVariant = ecuVariant;
			//this.children = new ISPELocator[0];
			this.ffmResolver = ffmResolver;
		}
#if false
		public ISPELocator[] Children
		{
			get
			{
				if (this.children != null && this.children.Length != 0)
				{
					return this.children;
				}
				new List<ISPELocator>();
				ICollection<XEP_FAULTCODE> xepFaultCodeByEcuVariantId = ClientContext.Database?.GetXepFaultCodeByEcuVariantId(this.ecuVariant.Id, this.vecInfo, this.ffmResolver);
				if (xepFaultCodeByEcuVariantId != null)
				{
					foreach (XEP_FAULTCODE xep_FAULTCODE in xepFaultCodeByEcuVariantId)
					{
						new FaultCode();
					}
				}
				return this.children;
			}
		}
#endif
		public string Id
		{
			get
			{
				return this.ecuVariant.Id.ToString(CultureInfo.InvariantCulture);
			}
		}

		public ISPELocator[] Parents
		{
			get
			{
				if (this.parents != null && this.parents.Length != 0)
				{
					return this.parents;
				}
				List<ISPELocator> list = new List<ISPELocator>();
				if (string.IsNullOrEmpty(this.ecuVariant.GroupId))
				{
					PdszDatabase.EcuGroup ecuGroupById = ClientContext.GetDatabase(this.vecInfo)?.GetEcuGroupById(this.ecuVariant.GroupId);
					if (ecuGroupById != null)
					{
						list.Add(new EcuGroupLocator(ecuGroupById, this.vecInfo, this.ffmResolver));
						this.parents = list.ToArray();
					}
				}
				return this.parents;
			}
		}

		public string DataClassName
		{
			get
			{
				return "ECUVariant";
			}
		}

		public string[] OutgoingLinkNames
		{
			get
			{
				return new string[0];
			}
		}

		public string[] IncomingLinkNames
		{
			get
			{
				return new string[0];
			}
		}

		public string[] DataValueNames
		{
			get
			{
				return new string[]
				{
					"ID",
					"FAULTMEMORYDELETEWAITINGTIME",
					"NAME",
					"TITLEID",
					"TITLE_DEDE",
					"TITLE_ENGB",
					"TITLE_ENUS",
					"TITLE_FR",
					"TITLE_TH",
					"TITLE_SV",
					"TITLE_IT",
					"TITLE_ES",
					"TITLE_ID",
					"TITLE_KO",
					"TITLE_EL",
					"TITLE_TR",
					"TITLE_ZHCN",
					"TITLE_RU",
					"TITLE_NL",
					"TITLE_PT",
					"TITLE_ZHTW",
					"TITLE_JA",
					"TITLE_CSCZ",
					"TITLE_PLPL",
					"VALIDFROM",
					"VALIDTO",
					"SICHERHEITSRELEVANT",
					"ECUGROUPID",
					"SORT"
				};
			}
		}

		public string SignedId
		{
			get
			{
				if (this.ecuVariant == null)
				{
					return string.Empty;
				}
				return this.ecuVariant.Id;
			}
		}

		public Exception Exception
		{
			get
			{
				return null;
			}
		}

		public bool HasException
		{
			get
			{
				return false;
			}
		}
#if false
        public string GetDataValue(string name)
        {
            if (ecuVariant != null && !string.IsNullOrEmpty(name))
            {
                switch (name.ToUpperInvariant())
                {
                    case "TITLE_JA":
                        return ecuVariant.Title_ja;
                    case "SORT":
                        if (!ecuVariant.Sort.HasValue)
                        {
                            return "0";
                        }
                        return ecuVariant.Sort.ToString();
                    case "TITLE_ENUS":
                        return ecuVariant.Title_enus;
                    case "NODECLASS":
                        return "5719042";
                    case "TITLE_ENGB":
                        return ecuVariant.Title_engb;
                    case "TITLE_NL":
                        return ecuVariant.Title_nl;
                    case "TITLE_ZHTW":
                        return ecuVariant.Title_zhtw;
                    case "TITLE_ZHCN":
                        return ecuVariant.Title_zhcn;
                    case "TITLE_TH":
                        return ecuVariant.Title_th;
                    case "TITLE_KO":
                        return ecuVariant.Title_ko;
                    case "TITLE":
                        return ecuVariant.Title;
                    case "TITLE_RU":
                        return ecuVariant.Title_ru;
                    case "TITLE_TR":
                        return ecuVariant.Title_tr;
                    case "TITLE_CSCZ":
                        return ecuVariant.Title_cscz;
                    case "TITLE_DEDE":
                        return ecuVariant.Title_dede;
                    case "ID":
                        return ecuVariant.Id.ToString(CultureInfo.InvariantCulture);
                    case "NAME":
                        return ecuVariant.Name;
                    case "FAULTMEMORYDELETEWAITINGTIME":
                        if (!ecuVariant.FaultMemoryDeleteWaitingTime.HasValue)
                        {
                            return string.Empty;
                        }
                        return ecuVariant.FaultMemoryDeleteWaitingTime.ToString();
                    case "TITLE_PLPL":
                        return ecuVariant.Title_plpl;
                    case "VALIDFROM":
                        if (!ecuVariant.ValidFrom.HasValue)
                        {
                            return string.Empty;
                        }
                        return ecuVariant.ValidFrom.ToString();
                    case "SICHERHEITSRELEVANT":
                        if (!ecuVariant.Sicherheitsrelevant.HasValue)
                        {
                            return "0";
                        }
                        return ecuVariant.Sicherheitsrelevant.ToString();
                    case "VALIDTO":
                        if (!ecuVariant.ValidTo.HasValue)
                        {
                            return string.Empty;
                        }
                        return ecuVariant.ValidTo.ToString();
                    case "TITLE_EL":
                        return ecuVariant.Title_el;
                    case "ECUGROUPID":
                        if (!ecuVariant.EcuGroupId.HasValue)
                        {
                            return "0";
                        }
                        return ecuVariant.EcuGroupId.ToString();
                    case "TITLE_SV":
                        return ecuVariant.Title_sv;
                    case "TITLE_IT":
                        return ecuVariant.Title_it;
                    case "TITLE_ES":
                        return ecuVariant.Title_es;
                    case "TITLE_PT":
                        return ecuVariant.Title_pt;
                    case "TITLE_FR":
                        return ecuVariant.Title_fr;
                    case "TITLE_ID":
                        return ecuVariant.Title_id;
                    default:
                        return string.Empty;
                }
            }
            return null;
        }
#endif
        public ISPELocator[] GetIncomingLinks()
		{
			return new ISPELocator[0];
		}

		public ISPELocator[] GetIncomingLinks(string incomingLinkName)
		{
			return this.parents;
		}
#if false
		public ISPELocator[] GetOutgoingLinks()
		{
			return this.children;
		}

		public ISPELocator[] GetOutgoingLinks(string outgoingLinkName)
		{
			return this.children;
		}

        public T GetDataValue<T>(string name)
        {
            try
            {
                if (!string.IsNullOrEmpty(name) && ecuVariant != null)
                {
                    object obj = null;
                    switch (name.ToUpperInvariant())
                    {
                        case "TITLE_JA":
                            obj = ecuVariant.Title_ja;
                            break;
                        case "SORT":
                            obj = ecuVariant.Sort;
                            break;
                        case "TITLE_ENUS":
                            obj = ecuVariant.Title_enus;
                            break;
                        case "NODECLASS":
                            obj = "5719042";
                            break;
                        case "TITLE_ENGB":
                            obj = ecuVariant.Title_engb;
                            break;
                        case "TITLE_NL":
                            obj = ecuVariant.Title_nl;
                            break;
                        case "TITLE_ZHTW":
                            obj = ecuVariant.Title_zhtw;
                            break;
                        case "TITLE_ZHCN":
                            obj = ecuVariant.Title_zhcn;
                            break;
                        case "TITLE_TH":
                            obj = ecuVariant.Title_th;
                            break;
                        case "TITLE_KO":
                            obj = ecuVariant.Title_ko;
                            break;
                        case "TITLE":
                            obj = ecuVariant.Title;
                            break;
                        case "TITLE_RU":
                            obj = ecuVariant.Title_ru;
                            break;
                        case "TITLE_TR":
                            obj = ecuVariant.Title_tr;
                            break;
                        case "TITLE_CSCZ":
                            obj = ecuVariant.Title_cscz;
                            break;
                        case "TITLE_DEDE":
                            obj = ecuVariant.Title_dede;
                            break;
                        case "ID":
                            obj = ecuVariant.Id;
                            break;
                        case "NAME":
                            obj = ecuVariant.Name;
                            break;
                        case "FAULTMEMORYDELETEWAITINGTIME":
                            obj = ecuVariant.FaultMemoryDeleteWaitingTime;
                            break;
                        case "TITLE_PLPL":
                            obj = ecuVariant.Title_plpl;
                            break;
                        case "VALIDFROM":
                            obj = ecuVariant.ValidFrom.HasValue;
                            break;
                        case "SICHERHEITSRELEVANT":
                            obj = ecuVariant.Sicherheitsrelevant;
                            break;
                        case "VALIDTO":
                            obj = ecuVariant.ValidTo.HasValue;
                            break;
                        case "TITLE_EL":
                            obj = ecuVariant.Title_el;
                            break;
                        case "ECUGROUPID":
                            obj = ecuVariant.EcuGroupId;
                            break;
                        case "TITLE_SV":
                            obj = ecuVariant.Title_sv;
                            break;
                        case "TITLE_IT":
                            obj = ecuVariant.Title_it;
                            break;
                        case "TITLE_ES":
                            obj = ecuVariant.Title_es;
                            break;
                        case "TITLE_PT":
                            obj = ecuVariant.Title_pt;
                            break;
                        case "TITLE_FR":
                            obj = ecuVariant.Title_fr;
                            break;
                        case "TITLE_ID":
                            obj = ecuVariant.Title_id;
                            break;
                    }
                    if (obj != null)
                    {
                        return (T)Convert.ChangeType(obj, typeof(T));
                    }
                }
            }
            catch (Exception exception)
            {
                //Log.WarningException("EcuVariantLocator.GetDataValue<T>()", exception);
            }
            return default(T);
        }
#endif
        private readonly PdszDatabase.EcuVar ecuVariant;

		//private readonly ISPELocator[] children;

		private ISPELocator[] parents;

		private readonly Vehicle vecInfo;

		private readonly IFFMDynamicResolver ffmResolver;
	}
}
