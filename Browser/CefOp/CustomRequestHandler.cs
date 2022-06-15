using CefSharp;
using CefSharp.Handler;

namespace Browser.CefOp
{
    public class CustomRequestHandler : RequestHandler
    {
        public delegate void RenderProcessTerminatedEventHandler(string message);
        public event RenderProcessTerminatedEventHandler RenderProcessTerminated;

        public delegate void OnConfigurationChangedHandler(BrowserLib.BrowserConfiguration conf, System.Action<bool> callBack);
        public event OnConfigurationChangedHandler ChangedConfigurationHandler;

        bool pixiSettingEnabled;
        string ServerRedirectUrl;

        public CustomRequestHandler(bool pixiSettingEnabled, string redirectUrl) : base()
        {
            this.pixiSettingEnabled = pixiSettingEnabled;
            this.ServerRedirectUrl  = redirectUrl;
        }

        protected override IResourceRequestHandler GetResourceRequestHandler(IWebBrowser chromiumWebBrowser, IBrowser browser, IFrame frame, IRequest request, bool isNavigation, bool isDownload, string requestInitiator, ref bool disableDefaultHandling)
        {
            var handler = new CustomResourceRequestHandler(this.pixiSettingEnabled, this.ServerRedirectUrl);
            this.ChangedConfigurationHandler = handler.OnConfigurationChanged;

            return handler;
        }

        public void OnConfigurationChanged(BrowserLib.BrowserConfiguration conf, System.Action<bool> callBack)
        {
            this.ChangedConfigurationHandler?.Invoke(conf, callBack);
        }

        /// <summary>
        /// 戻る/進む操作をブロックします。
        /// </summary>
        protected override bool OnBeforeBrowse(IWebBrowser browserControl, IBrowser browser, IFrame frame, IRequest request, bool userGesture, bool isRedirect)
        {
            if ((request.TransitionType & TransitionType.ForwardBack) != 0)
            {
                return true;
            }

            return base.OnBeforeBrowse(browserControl, browser, frame, request, userGesture, isRedirect);
        }

        /// <summary>
        /// 描画プロセスが何らかの理由で落ちた際の処理を行います。
        /// </summary>
        protected override void OnRenderProcessTerminated(IWebBrowser browserControl, IBrowser browser, CefTerminationStatus status)
        {
            // note: out of memory (例外コード: 0xe0000008) でクラッシュした場合、このイベントは呼ばれない
            string ret = "브라우저의 렌더링 프로세스가";
            switch (status)
            {
                case CefTerminationStatus.AbnormalTermination:
                    ret += "정상 종료 되지 않았습니다.";
                    break;
                case CefTerminationStatus.ProcessWasKilled:
                    ret += "다른 프로그램에 의해 종료되었습니다.";
                    break;
                case CefTerminationStatus.ProcessCrashed:
                    ret += "충돌했습니다.";
                    break;
                default:
                    ret += "예기치 않게 종료되었습니다.";
                    break;
            }
            ret += "다시 로드합니다.";
            this.RenderProcessTerminated(ret);
        }
    }
}
