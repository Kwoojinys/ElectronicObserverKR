﻿using DynaJson;
using ElectronicObserver.Utility;
using ElectronicObserver.Utility.Mathematics;
using Nekoxy;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Windows.Forms;

namespace ElectronicObserver.Observer
{


    public sealed class APIObserver
    {


        #region Singleton

        private static readonly APIObserver instance = new APIObserver();

        public static APIObserver Instance => instance;

        #endregion



        public APIDictionary APIList { get; private set; }

        public string ServerAddress { get; private set; }
        public int ProxyPort { get; private set; }

        public delegate void ProxyStartedEventHandler();
        public event ProxyStartedEventHandler ProxyStarted = delegate { };

        private Control UIControl;


        public event APIReceivedEventHandler RequestReceived = delegate { };
        public event APIReceivedEventHandler ResponseReceived = delegate { };


        private APIObserver()
        {

            this.APIList = new APIDictionary
            {
                new kcsapi.api_start2.getData(),
                new kcsapi.api_get_member.basic(),
                new kcsapi.api_get_member.slot_item(),
                new kcsapi.api_get_member.useitem(),
                new kcsapi.api_get_member.kdock(),
                new kcsapi.api_port.port(),
                new kcsapi.api_get_member.ship2(),
                new kcsapi.api_get_member.questlist(),
                new kcsapi.api_get_member.ndock(),
                new kcsapi.api_req_kousyou.getship(),
                new kcsapi.api_req_hokyu.charge(),
                new kcsapi.api_req_kousyou.destroyship(),
                new kcsapi.api_req_kousyou.destroyitem2(),
                new kcsapi.api_req_member.get_practice_enemyinfo(),
                new kcsapi.api_get_member.picture_book(),
                new kcsapi.api_req_mission.start(),
                new kcsapi.api_get_member.ship3(),
                new kcsapi.api_req_kaisou.powerup(),
                new kcsapi.api_req_map.start(),
                new kcsapi.api_req_map.next(),
                new kcsapi.api_req_kousyou.createitem(),
                new kcsapi.api_req_sortie.battle(),
                new kcsapi.api_req_sortie.battleresult(),
                new kcsapi.api_req_battle_midnight.battle(),
                new kcsapi.api_req_battle_midnight.sp_midnight(),
                new kcsapi.api_req_combined_battle.battle(),
                new kcsapi.api_req_combined_battle.midnight_battle(),
                new kcsapi.api_req_combined_battle.sp_midnight(),
                new kcsapi.api_req_combined_battle.airbattle(),
                new kcsapi.api_req_combined_battle.battleresult(),
                new kcsapi.api_req_practice.battle(),
                new kcsapi.api_req_practice.midnight_battle(),
                new kcsapi.api_req_practice.battle_result(),
                new kcsapi.api_get_member.deck(),
                new kcsapi.api_get_member.mapinfo(),
                new kcsapi.api_req_combined_battle.battle_water(),
                new kcsapi.api_req_combined_battle.goback_port(),
                new kcsapi.api_req_kousyou.remodel_slot(),
                new kcsapi.api_get_member.material(),
                new kcsapi.api_req_mission.result(),
                new kcsapi.api_req_ranking.getlist(),
                new kcsapi.api_req_sortie.airbattle(),
                new kcsapi.api_get_member.ship_deck(),
                new kcsapi.api_req_kaisou.marriage(),
                new kcsapi.api_req_hensei.preset_select(),
                new kcsapi.api_req_kaisou.slot_exchange_index(),
                new kcsapi.api_get_member.record(),
                new kcsapi.api_get_member.payitem(),
                new kcsapi.api_req_kousyou.remodel_slotlist(),
                new kcsapi.api_req_sortie.ld_airbattle(),
                new kcsapi.api_req_combined_battle.ld_airbattle(),
                new kcsapi.api_get_member.require_info(),
                new kcsapi.api_get_member.base_air_corps(),
                new kcsapi.api_req_air_corps.set_plane(),
                new kcsapi.api_req_air_corps.set_action(),
                new kcsapi.api_req_air_corps.supply(),
                new kcsapi.api_req_kaisou.slot_deprive(),
                new kcsapi.api_req_air_corps.expand_base(),
                new kcsapi.api_req_combined_battle.ec_battle(),
                new kcsapi.api_req_combined_battle.ec_midnight_battle(),
                new kcsapi.api_req_combined_battle.each_battle(),
                new kcsapi.api_req_combined_battle.each_battle_water(),
                new kcsapi.api_get_member.sortie_conditions(),
                new kcsapi.api_req_sortie.night_to_day(),
                new kcsapi.api_req_combined_battle.ec_night_to_day(),
                new kcsapi.api_req_sortie.goback_port(),
                new kcsapi.api_req_member.itemuse(),
                new kcsapi.api_req_sortie.ld_shooting(),
                new kcsapi.api_req_combined_battle.ld_shooting(),
                new kcsapi.api_req_map.anchorage_repair(),
                new kcsapi.api_get_member.preset_deck(),
                new kcsapi.api_req_kaisou.slotset(),

                new kcsapi.api_req_quest.clearitemget(),
                new kcsapi.api_req_nyukyo.start(),
                new kcsapi.api_req_nyukyo.speedchange(),
                new kcsapi.api_req_kousyou.createship(),
                new kcsapi.api_req_kousyou.createship_speedchange(),
                new kcsapi.api_req_hensei.change(),
                new kcsapi.api_req_member.updatedeckname(),
                new kcsapi.api_req_kaisou.remodeling(),
                new kcsapi.api_req_kaisou.open_exslot(),
                new kcsapi.api_req_map.select_eventmap_rank(),
                new kcsapi.api_req_hensei.combined(),
                new kcsapi.api_req_member.updatecomment(),
                new kcsapi.api_req_air_corps.change_name(),
                new kcsapi.api_req_quest.stop(),
                new kcsapi.api_req_hensei.preset_register(),
                new kcsapi.api_req_hensei.preset_delete(),
            };


            this.ServerAddress = null;


            HttpProxy.AfterSessionComplete += this.HttpProxy_AfterSessionComplete;
        }




        /// <summary>
        /// 通信の受信を開始します。
        /// </summary>
        /// <param name="portID">受信に使用するポート番号。</param>
        /// <param name="UIControl">GUI スレッドで実行するためのオブジェクト。中身は何でもいい</param>
        /// <returns>実際に使用されるポート番号。</returns>
        public int Start(int portID, Control UIControl)
        {

            Utility.Configuration.ConfigurationData.ConfigConnection c = Configuration.Config.Connection;


            this.UIControl = UIControl;


            HttpProxy.Shutdown();
            try
            {

                if (c.UseUpstreamProxy)
                    HttpProxy.UpstreamProxyConfig = new ProxyConfig(ProxyConfigType.SpecificProxy, c.UpstreamProxyAddress, c.UpstreamProxyPort);
                else if (c.UseSystemProxy)
                    HttpProxy.UpstreamProxyConfig = new ProxyConfig(ProxyConfigType.SystemProxy);
                else
                    HttpProxy.UpstreamProxyConfig = new ProxyConfig(ProxyConfigType.DirectAccess);

                HttpProxy.Startup(portID, false, false);
                this.ProxyPort = portID;


                ProxyStarted();

                Logger.Add(LogType.Browser, string.Format("APIObserver: 포트 {0} 번에서 수신을 시작했습니다.", portID));

            }
            catch (Exception ex)
            {

                Logger.Add(LogType.Error, "APIObserver: 수신 시작에 실패했습니다." + ex.Message);
                this.ProxyPort = 0;
            }


            return this.ProxyPort;
        }


        /// <summary>
        /// 通信の受信を停止します。
        /// </summary>
        public void Stop()
        {

            HttpProxy.Shutdown();

            Logger.Add(LogType.Browser, "APIObserver: 수신을 중지했습니다.");
        }



        public APIBase this[string key]
        {
            get
            {
                if (this.APIList.ContainsKey(key)) return this.APIList[key];
                else return null;
            }
        }




        void HttpProxy_AfterSessionComplete(Session session)
        {

            Utility.Configuration.ConfigurationData.ConfigConnection c = Configuration.Config.Connection;

            string baseurl = session.Request.PathAndQuery;

            //debug
            //Utility.Logger.Add( 1, baseurl );


            // request
            if (baseurl.Contains("/kcsapi/"))
            {

                string url = baseurl;
                string body = session.Request.BodyAsString;

                //保存
                if (c.SaveReceivedData && c.SaveRequest)
                {

                    Task.Run((Action)(() =>
                    {
                        this.SaveRequest(url, body);
                    }));
                }


                this.UIControl.BeginInvoke((Action)(() => { this.LoadRequest(url, body); }));
            }



            //response
            //保存

            if (c.SaveReceivedData)
            {

                try
                {

                    if (!Directory.Exists(c.SaveDataPath))
                        Directory.CreateDirectory(c.SaveDataPath);


                    if (c.SaveResponse && baseurl.Contains("/kcsapi/"))
                    {

                        // 非同期で書き出し処理するので取っておく
                        // stringはイミュータブルなのでOK
                        string url = baseurl;
                        string body = session.Response.BodyAsString;

                        Task.Run((Action)(() =>
                        {
                            this.SaveResponse(url, body);
                        }));

                    }
                    else if (baseurl.Contains("/kcs") && c.SaveOtherFile)
                    {

                        string saveDataPath = c.SaveDataPath; // スレッド間の競合を避けるため取っておく

                        string tpath = string.Format("{0}\\{1}", saveDataPath, baseurl.Substring(baseurl.IndexOf("/kcs") + 1).Replace("/", "\\"));
                        //Logger.Add(1, $"{baseurl} $ {tpath}");
                        {
                            int index = tpath.IndexOf("?");
                            if (index != -1)
                            {
                                if (Configuration.Config.Connection.ApplyVersion)
                                {
                                    string over = tpath.Substring(index + 1);
                                    int vindex = over.LastIndexOf("VERSION=", StringComparison.CurrentCultureIgnoreCase);
                                    if (vindex != -1)
                                    {
                                        string version = over.Substring(vindex + 8).Replace('.', '_');
                                        tpath = tpath.Insert(tpath.LastIndexOf('.', index), "_v" + version);
                                        index += version.Length + 2;
                                    }

                                }

                                tpath = tpath.Remove(index);
                            }
                        }

                        // 非同期で書き出し処理するので取っておく
                        byte[] responseCopy = new byte[session.Response.Body.Length];
                        Array.Copy(session.Response.Body, responseCopy, session.Response.Body.Length);

                        Task.Run((Action)(() =>
                        {
                            try
                            {
                                lock (this)
                                {
                                    // 同時に書き込みが走るとアレなのでロックしておく

                                    Directory.CreateDirectory(Path.GetDirectoryName(tpath));

                                    //System.Diagnostics.Debug.WriteLine( oSession.fullUrl + " => " + tpath );
                                    using (var sw = new System.IO.BinaryWriter(File.OpenWrite(tpath)))
                                    {
                                        sw.Write(responseCopy);
                                    }
                                }

                                Logger.Add(LogType.Browser, string.Format("통신에서 {0} 파일을 저장했습니다.", tpath.Remove(0, saveDataPath.Length + 1)));

                            }
                            catch (IOException ex)
                            {   //ファイルがロックされている; 頻繁に出るのでエラーレポートを残さない

                                Logger.Add(LogType.Error, "통신 내용 저장에 실패했습니다." + ex.Message);
                            }
                        }));

                    }

                }
                catch (Exception ex)
                {

                    ErrorReporter.SendErrorReport(ex, "통신 내용 저장에 실패했습니다.");
                }

            }




            if (baseurl.Contains("/kcsapi/") && session.Response.MimeType == "text/plain")
            {

                // 非同期でGUIスレッドに渡すので取っておく
                // stringはイミュータブルなのでOK
                string url = baseurl;
                string body = session.Response.BodyAsString;
                this.UIControl.BeginInvoke((Action)(() => { this.LoadResponse(url, body); }));

            }


            if (this.ServerAddress == null && baseurl.Contains("/kcsapi/"))
            {
                this.ServerAddress = session.Request.Headers.Host;
            }

        }



        public void LoadRequest(string path, string data)
        {

            string shortpath = path.Substring(path.LastIndexOf("/kcsapi/") + 8);

            try
            {

                Logger.Add(LogType.Browser, "Request 를 수신했습니다 : " + shortpath);

                SystemEvents.UpdateTimerEnabled = false;


                var parsedData = new Dictionary<string, string>();

                foreach (string unit in data.Split("&".ToCharArray()))
                {
                    string[] pair = unit.Split("=".ToCharArray());
                    parsedData.Add(HttpUtility.UrlDecode(pair[0]), HttpUtility.UrlDecode(pair[1]));
                }


                RequestReceived(shortpath, parsedData);
                this.APIList.OnRequestReceived(shortpath, parsedData);

            }
            catch (Exception ex)
            {

                ErrorReporter.SendErrorReport(ex, "Request 수신 중 오류가 발생했습니다.", shortpath, data);

            }
            finally
            {

                SystemEvents.UpdateTimerEnabled = true;

            }

        }


        public void LoadResponse(string path, string data)
        {

            string shortpath = path.Substring(path.LastIndexOf("/kcsapi/") + 8);

            try
            {

                Logger.Add(LogType.Browser, "Response를 받았습니다 : " + shortpath);

                SystemEvents.UpdateTimerEnabled = false;


                var json = JsonObject.Parse(data.Substring(7));        //remove "svdata="

                int result = (int)json.api_result;
                if (result != 1)
                {
                    throw new InvalidOperationException("에러코를 발견했습니다. ( 오류 코드 :  " + result + ")");
                }


                if (shortpath == "api_get_member/ship2")
                {
                    ResponseReceived(shortpath, json);
                    this.APIList.OnResponseReceived(shortpath, json);
                }
                else if (json.IsDefined("api_data"))
                {
                    ResponseReceived(shortpath, json.api_data);
                    this.APIList.OnResponseReceived(shortpath, json.api_data);
                }
                else
                {
                    ResponseReceived(shortpath, null);
                    this.APIList.OnResponseReceived(shortpath, null);
                }

            }
            catch (Exception ex)
            {

                ErrorReporter.SendErrorReport(ex, "Response 수신중 오류가 발생했습니다.", shortpath, data);

            }
            finally
            {

                SystemEvents.UpdateTimerEnabled = true;

            }

        }


        private void SaveRequest(string url, string body)
        {

            try
            {

                string tpath = string.Format("{0}\\{1}Q@{2}.json", Configuration.Config.Connection.SaveDataPath, DateTimeHelper.GetTimeStamp(), url.Substring(url.LastIndexOf("/kcsapi/") + 8).Replace("/", "@"));

                using (var sw = new System.IO.StreamWriter(tpath, false, Encoding.UTF8))
                {
                    sw.Write(body);
                }


            }
            catch (Exception ex)
            {

                ErrorReporter.SendErrorReport(ex, "Request 저장에 실패했습니다.");

            }
        }


        private void SaveResponse(string url, string body)
        {

            try
            {

                string tpath = string.Format("{0}\\{1}S@{2}.json", Configuration.Config.Connection.SaveDataPath, DateTimeHelper.GetTimeStamp(), url.Substring(url.LastIndexOf("/kcsapi/") + 8).Replace("/", "@"));

                using (var sw = new System.IO.StreamWriter(tpath, false, Encoding.UTF8))
                {
                    sw.Write(body);
                }

            }
            catch (Exception ex)
            {

                ErrorReporter.SendErrorReport(ex, "Response 저장에 실패했습니다.");

            }



        }

    }



}
