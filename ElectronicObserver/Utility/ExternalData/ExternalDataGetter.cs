using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using ElectronicObserver.Data;

namespace ElectronicObserver.Utility
{
    public partial class ExternalDataReader
    {
        private Dictionary<string, string> _translatedStrings = new Dictionary<string, string>();

        public string GetTranslation(string jpString, TranslateType type, int id = -1)
        {
            try
            {
                if (string.IsNullOrEmpty(jpString) == false &&
                    this._translatedStrings.TryGetValue(jpString, out string cached) == true)
                {
                    return cached;
                }

                var translationList = this.GetData(type);
                if (translationList == null)
                {
                    return jpString;
                }

                string translated = jpString;
                if (id != -1)
                {
                    if (this.GetTranslation(id, translationList, ref translated) == true)
                    {
                        if (this._translatedStrings.ContainsKey(jpString) == false)
                        {
                            this._translatedStrings.Add(jpString, translated);
                        }

                        return translated;
                    }
                }
                else
                {
                    if (this.GetTranslation(jpString, translationList, ref translated, type) == true)
                    {
                        if (this._translatedStrings.ContainsKey(jpString) == false)
                        {
                            this._translatedStrings.Add(jpString, translated);
                        }

                        return translated;
                    }
                }
            }
            catch (Exception e)
            {
                Logger.Add(LogType.Error, "번역 파일을 불러오는데 실패했습니다. : " + e.Message + ":" + type);
                return jpString;
            }

            return jpString;
        }

        private bool GetTranslation(int id, JObject translationList, ref string translate)
        {
            var founded = translationList.TryGetValue(id.ToString(), out JToken value);
            if (founded == false || value == null)
            {
                return false;
            }

            translate = value.ToString();
            return true;
        }

        private bool GetTranslation(string jpString, JObject translationList, ref string translate, TranslateType type, string shipSuffix = "")
        {
            if (translationList.TryGetValue(jpString, out JToken value) == false || value == null)
            {
                if (type == TranslateType.ShipName)
                {
                    foreach (var suffix in this.GetData(TranslateType.ShipSuffix))
                    {
                        if (jpString.Contains(suffix.Key) == true)
                        {
                            if (suffix.Key.Equals(jpString.Substring(jpString.Length - suffix.Key.Length)) == true)
                            {
                                return this.GetTranslation(jpString.Remove(jpString.Length - suffix.Key.Length), 
                                    translationList, ref translate, TranslateType.ShipName, suffix.Value?.ToString() + shipSuffix);
                            }
                        }
                    }
                }

                translate = jpString;
                return false;
            }

            translate = $"{value}{shipSuffix}";
            return true;
        }

        public bool CheckWeightData(int shipId)
        {
            var fitInfo = this.GetData(TranslateType.FitData)[shipId.ToString()];
            if (fitInfo == null)
                return false;

            if (fitInfo.Type == JTokenType.Integer)
                return true;

            return false;
        }

        public JToken GetExpeditionData(string id)
        {
            var obj = this.GetData(TranslateType.ExpeditionData)["Expedition"] as JArray;
            if (obj == null) return null;

            JToken value = null;
            obj.Foreach(data =>
            {
                if (data["ID"].ToString().Equals(id) == true)
                {
                    value = data;
                }
            });

            return value;
        }

        public void GetFit(Dictionary<string, string> data)
        {
            int shipId  = int.Parse(data["api_id"]);
            int eqId    = int.Parse(data["api_item_id"]);
            if (eqId == -1)
                return;
            
            KCDatabase db = KCDatabase.Instance;
            if (db.Equipments[eqId].MasterEquipment.IsLargeGun == true)
            {
                bool married        = db.Ships[shipId].IsMarried;
                int masterShipId    = db.Ships[shipId].MasterShip.ID;
                int masterEquipId   = db.Equipments[eqId].MasterEquipment.ID;
                if (this.CheckWeightData(masterShipId) == true)
                {
                    masterShipId = this.GetData(TranslateType.FitData)[masterShipId.ToString()].ToObject<int>();
                }

                var WeightData = this.GetData(TranslateType.FitData)[masterShipId.ToString()];
                if (WeightData == null)
                {
                    Window.FormMain.Instance.fInformation.ShowFitInfo(masterShipId, masterEquipId);
                    return;
                }

                var classData       = (JObject)WeightData["weightClass"];
                var accuracyData    = (JObject)WeightData["accuracy"];
                string weightType   = "unknown";
                foreach (var weightClass in classData)
                {
                    int[] weights = weightClass.Value.ToObject<int[]>();
                    if (weights.Contains(masterEquipId))
                    {
                        weightType = weightClass.Key;
                        break;
                    }
                }

                if (weightType.Equals("unknown"))
                {
                    Window.FormMain.Instance.fInformation.ShowFitInfo(masterShipId, masterEquipId);
                }
                else
                {
                    int[] fitBonus = new int[] { accuracyData[weightType]["day"].ToObject<int>(), accuracyData[weightType]["night"].ToObject<int>() };
                    float marryData = 0;
                    if (accuracyData[weightType]["married"] != null)
                        marryData = accuracyData[weightType]["married"].ToObject<float>();

                    Window.FormMain.Instance.fInformation.ShowFitInfo(db.Ships[shipId].Name, db.MasterEquipments[masterEquipId].Name, fitBonus, marryData, married);
                }
            }
        }

        public JObject GetData(TranslateType type)
        {
            return this._externalDatas.Find(i => i.DataType == type).Data;
        }
    }
}
