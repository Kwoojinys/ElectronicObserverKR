using CefSharp;
using CefSharp.Handler;

namespace Browser.CefOp
{
    public class CustomResourceRequestHandler : ResourceRequestHandler
    {
        private bool _pixiSettingEnabled { get; set; }
        private string ServerRedirectingUrl { get; set; }


        public CustomResourceRequestHandler(bool pixiSettingEnabled, string redirectUrl) : base()
        {
            this._pixiSettingEnabled    = pixiSettingEnabled;
            this.ServerRedirectingUrl   = redirectUrl;
        }

        public void OnConfigurationChanged(BrowserLib.BrowserConfiguration conf, System.Action<bool> callBack)
        {
            if (conf == null || string.IsNullOrEmpty(conf.RedirectServerUrl) == true)
            {
                callBack?.Invoke(false);
                return;
            }

            this.ServerRedirectingUrl = conf.RedirectServerUrl;
            callBack?.Invoke(true);
        }

        /// <summary>
        /// レスポンスの置換制御を行います。
        /// </summary>
        protected override IResponseFilter GetResourceResponseFilter(IWebBrowser chromiumWebBrowser, IBrowser browser, IFrame frame, IRequest request, IResponse response)
        {
            if (this._pixiSettingEnabled && request.Url.Contains(@"/kcs2/index.php"))
                return new ResponseFilterPixiSetting();

            return base.GetResourceResponseFilter(chromiumWebBrowser, browser, frame, request, response);
        }

        /// <summary>
        /// 特定の通信をブロックします。
        /// </summary>
        protected override CefReturnValue OnBeforeResourceLoad(IWebBrowser chromiumWebBrowser, IBrowser browser, IFrame frame, IRequest request, IRequestCallback callback)
        {
            // ログイン直後に勝手に遷移させられ、ブラウザがホワイトアウトすることがあるためブロックする
            //if (request.Url.Contains(@"/rt.gsspat.jp/"))
            //{
            //    return CefReturnValue.Cancel;
            //}

            request.Url = request.Url.Replace("203.104.209.7/gadget_html5", $"{this.ServerRedirectingUrl}/gadget_html5");
            request.Url = request.Url.Replace("203.104.209.7/html", $"{this.ServerRedirectingUrl}/html");

            //System.IO.File.WriteAllText($"debug{System.DateTime.Now}.txt", request.Url);

            //request.Url = request.Url.Replace("203.104.209.7/gadget_html5", "luckyjervis.com/gadget_html5");
            //request.Url = request.Url.Replace("203.104.209.7/html", "luckyjervis.com/html");

            return base.OnBeforeResourceLoad(chromiumWebBrowser, browser, frame, request, callback);
        }

    }
}
