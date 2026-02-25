window.AIAnalytics = (function () {
    var _listener = null;

    function initEmbedAuth(iframeElement, embedToken, targetOrigin, theme) {
        dispose();

        _listener = function (event) {
            if (event.origin !== targetOrigin) {
                return;
            }

            if (!event.data || event.data.type !== "PT.EMBED.READY") {
                return;
            }

            iframeElement.contentWindow.postMessage({
                type: "PT.EMBED.AUTH",
                version: 1,
                payload: {
                    embedToken: embedToken,
                    ui: {
                        theme: theme || "light"
                    }
                }
            }, targetOrigin);

            window.removeEventListener("message", _listener);
            _listener = null;
        };

        window.addEventListener("message", _listener);
    }

    function dispose() {
        if (_listener) {
            window.removeEventListener("message", _listener);
            _listener = null;
        }
    }

    return {
        initEmbedAuth: initEmbedAuth,
        dispose: dispose
    };
})();