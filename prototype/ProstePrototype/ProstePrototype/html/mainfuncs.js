function sendMessageWPF(json) {
    CefSharp.PostMessage(JSON.stringify(json));
}


