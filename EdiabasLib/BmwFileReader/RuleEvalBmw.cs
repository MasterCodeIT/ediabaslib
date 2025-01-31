﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Text;
using EdiabasLib;
using Mono.CSharp;

namespace BmwFileReader
{
    public class RuleEvalBmw
    {
        private RulesInfo _rulesInfo { get; }
        private readonly Dictionary<string, List<string>> _propertiesDict = new Dictionary<string, List<string>>();
        private readonly HashSet<string> _unknownNamesHash = new HashSet<string>();
        private string _unknownId;
        private readonly object _lockObject = new object();

        public RuleEvalBmw()
        {
            _rulesInfo = new RulesInfo(this);
        }

        public bool EvaluateRule(string id, bool ecuFuncRule = false)
        {
            lock (_lockObject)
            {
                if (_rulesInfo == null)
                {
                    return false;
                }

                try
                {
                    _unknownNamesHash.Clear();
                    _unknownId = null;

                    bool valid = ecuFuncRule ? _rulesInfo.IsEcuFuncRuleValid(id) : _rulesInfo.IsFaultRuleValid(id);
                    if (_unknownId != null)
                    {
                        return true;
                    }

                    if (_unknownNamesHash.Count > 0)
                    {
                        return true;
                    }

                    return valid;
                }
                catch (Exception)
                {
                    return false;
                }
            }
        }

        public bool SetEvalProperties(DetectVehicleBmw detectVehicleBmw, EcuFunctionStructs.EcuVariant ecuVariant)
        {
            lock (_lockObject)
            {
                try
                {
                    _propertiesDict.Clear();

                    if (detectVehicleBmw == null || !detectVehicleBmw.Valid)
                    {
                        return false;
                    }

                    if (detectVehicleBmw.BrandList != null && detectVehicleBmw.BrandList.Count > 0)
                    {
                        _propertiesDict.Add("Marke".ToUpperInvariant(), detectVehicleBmw.BrandList);
                    }

                    if (!string.IsNullOrWhiteSpace(detectVehicleBmw.TypeKey))
                    {
                        _propertiesDict.Add("Typschl\u00FCssel".ToUpperInvariant(), new List<string> { detectVehicleBmw.TypeKey.Trim() });
                    }

                    if (!string.IsNullOrWhiteSpace(detectVehicleBmw.Series))
                    {
                        _propertiesDict.Add("E-Bezeichnung".ToUpperInvariant(), new List<string> { detectVehicleBmw.Series.Trim() });
                    }

                    if (!string.IsNullOrWhiteSpace(detectVehicleBmw.ConstructYear))
                    {
                        string constructDate = detectVehicleBmw.ConstructYear;
                        if (!string.IsNullOrWhiteSpace(detectVehicleBmw.ConstructMonth))
                        {
                            constructDate += detectVehicleBmw.ConstructMonth;
                        }
                        else
                        {
                            constructDate += "01";
                        }
                        _propertiesDict.Add("Baustand".ToUpperInvariant(), new List<string> { constructDate });

                        string productionDate = constructDate;
                        if (!string.IsNullOrWhiteSpace(detectVehicleBmw.ILevelShip))
                        {
                            string iLevelTrim = detectVehicleBmw.ILevelShip.Trim();
                            string[] levelParts = iLevelTrim.Split("-");
                            if (levelParts.Length == 4)
                            {
                                if (Int32.TryParse(levelParts[1], NumberStyles.Integer, CultureInfo.InvariantCulture, out int iLevelYear) &&
                                    Int32.TryParse(levelParts[2], NumberStyles.Integer, CultureInfo.InvariantCulture, out int iLevelMonth))
                                {
                                    int dateValue = (iLevelYear + 2000) * 100 + iLevelMonth;
                                    productionDate = dateValue.ToString(CultureInfo.InvariantCulture);
                                }
                            }
                        }

                        _propertiesDict.Add("Produktionsdatum".ToUpperInvariant(), new List<string> { productionDate });
                    }

                    if (!string.IsNullOrWhiteSpace(detectVehicleBmw.ILevelCurrent))
                    {
                        string iLevelTrim = detectVehicleBmw.ILevelCurrent.Trim();
                        string[] levelParts = iLevelTrim.Split("-");
                        _propertiesDict.Add("IStufe".ToUpperInvariant(), new List<string> { iLevelTrim.Trim() });
                        if (levelParts.Length == 4 && iLevelTrim.Length == 14)
                        {
                            string iLevelNum = levelParts[1] + levelParts[2] + levelParts[3];
                            if (Int32.TryParse(iLevelNum, NumberStyles.Integer, CultureInfo.InvariantCulture, out int iLevelValue))
                            {
                                _propertiesDict.Add("IStufeX".ToUpperInvariant(), new List<string> { iLevelValue.ToString(CultureInfo.InvariantCulture) });
                            }

                            _propertiesDict.Add("Baureihenverbund".ToUpperInvariant(), new List<string> { levelParts[0] });
                        }
                    }

                    return SetEvalEcuProperties(ecuVariant);
                }
                catch (Exception)
                {
                    return false;
                }
            }
        }

        public bool UpdateEvalEcuProperties(EcuFunctionStructs.EcuVariant ecuVariant)
        {
            lock (_lockObject)
            {
                return SetEvalEcuProperties(ecuVariant);
            }
        }

        private bool SetEvalEcuProperties(EcuFunctionStructs.EcuVariant ecuVariant)
        {
            try
            {
                string keyEcuRep = "EcuRepresentative".ToUpperInvariant();
                _propertiesDict.Remove(keyEcuRep);

                if (ecuVariant != null)
                {
                    string repsName = ecuVariant.EcuClique?.EcuRepsName;
                    if (!string.IsNullOrEmpty(repsName))
                    {
                        _propertiesDict.Add(keyEcuRep, new List<string> { repsName });
                    }
                }
            }
            catch (Exception)
            {
                return false;
            }

            return true;
        }

        public void RuleNotFound(string id)
        {
            _unknownId = id;
        }

        public string RuleString(string name)
        {
            string propertyString = GetPropertyString(name);
            if (string.IsNullOrWhiteSpace(propertyString))
            {
                return string.Empty;
            }
            return propertyString;
        }

        public long RuleNum(string name)
        {
            long? propertyValue = GetPropertyValue(name);
            if (!propertyValue.HasValue)
            {
                return -1;
            }

            return propertyValue.Value;
        }

        public bool IsValidRuleString(string name, string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return false;
            }

            List<string> propertyStrings = GetPropertyStrings(name);
            if (propertyStrings == null)
            {
                return false;
            }

            foreach (string propertyString in propertyStrings)
            {
                if (string.Compare(propertyString.Trim(), value.Trim(), StringComparison.OrdinalIgnoreCase) == 0)
                {
                    return true;
                }
            }

            return false;
        }

        public bool IsValidRuleNum(string name, long value)
        {
            List<long> propertyValues = GetPropertyValues(name);
            if (propertyValues == null)
            {
                return false;
            }

            foreach (long propertyValue in propertyValues)
            {
                if (propertyValue == value)
                {
                    return true;
                }
            }

            return false;
        }

        private string GetPropertyString(string name)
        {
            List<string> stringList = GetPropertyStrings(name);
            if (stringList != null && stringList.Count > 0)
            {
                return stringList[0];
            }

            return string.Empty;
        }

        private List<string> GetPropertyStrings(string name)
        {
            if (_propertiesDict == null)
            {
                return null;
            }

            string key = name.Trim().ToUpperInvariant();
            if (_propertiesDict.TryGetValue(key, out List<string> valueList))
            {
                return valueList;
            }

            _unknownNamesHash.Add(key);
            return null;
        }

        private long? GetPropertyValue(string name)
        {
            List<long> propertyValues = GetPropertyValues(name);
            if (propertyValues != null && propertyValues.Count > 0)
            {
                return propertyValues[0];
            }

            return null;
        }

        private List<long> GetPropertyValues(string name)
        {
            List<string> propertyStrings = GetPropertyStrings(name);
            if (propertyStrings == null)
            {
                return null;
            }

            List<long> valueList = new List<long>();
            foreach (string propertyString in propertyStrings)
            {
                if (long.TryParse(propertyString, NumberStyles.Integer, CultureInfo.InvariantCulture, out long result))
                {
                    if (!valueList.Contains(result))
                    {
                        valueList.Add(result);
                    }
                }
            }

            return valueList;
        }
    }
}
