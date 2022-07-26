function sendMessageWPF(json) {

    CefSharp.PostMessage(JSON.stringify(json));
}

function addActive() {
    $(document).on('click', '.btnStyle', function () {
        $('.btnStyle').removeClass('active');// here remove class active from all btnStyle
        $(this).addClass('active');// here apply selected class on clicked btnStyle
    });
}
