using CefSharp;
using CefSharp.Enums;
using System.Collections.Generic;

namespace Browser.CefOp
{
    /// <summary>
    /// (たぶん)ドラッグ&ドロップを無効化します。
    /// </summary>
    public class DragHandler : IDragHandler
    {
        public bool OnDragEnter(IWebBrowser browserControl, IBrowser browser, IDragData dragData, DragOperationsMask mask)
        {
            return true;
        }

        public void OnDraggableRegionsChanged(IWebBrowser chromiumWebBrowser, IBrowser browser, IFrame frame, IList<DraggableRegion> regions)
        {
            // nop
        }
    }
}
