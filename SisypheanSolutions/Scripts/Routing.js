$(function () {
    var currentURL = window.location.href;

    if (currentURL.toLowerCase().indexOf("file-manager") >= 0) {
        var url = "/Home/FileManager";
        GetPartial(url);
    }

    else if (currentURL.toLowerCase().indexOf("location") >= 0) {
        var url = "/Home/FileDownload";
        GetPartial(url);
    }

    else if (currentURL.toLowerCase().indexOf("about") >= 0) {
        var url = "/Home/About";
        GetPartial(url);
    }

    else if (currentURL.toLowerCase().indexOf("contact") >= 0) {
        var url = "/Home/Contact";
        GetPartial(url);
    }

    else if (currentURL.toLowerCase().indexOf("news-feed") >= 0) {
        var url = "/Home/News";
        GetPartial(url);
    }

    else {
        var url = "/Home/Home";
        GetPartial(url);
    }

    $('.routing').on('click', '.home', function () {
        var url = "/Home/Home";
        GetPartial(url);
    });

    $('.routing').on('click', '.file-manager', function () {
        var url = "/Home/FileManager";
        GetPartial(url);
    });

    $('.routing').on('click', '.news-feed', function () {
        var url = "/Home/News";
        GetPartial(url);
    });

    $('.routing').on('click', '.about', function () {
        var url = "/Home/About";
        GetPartial(url);
    });

    $('.routing').on('click', '.contact', function () {
        var url = "/Home/Contact";
        GetPartial(url);
    });

    function GetPartial(url) {
        $.get(url, function (data) {
            $('#main-content').html(data);
        });
    }
});