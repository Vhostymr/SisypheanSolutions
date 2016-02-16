(function ($) {
    var app = $.sammy('#main-content', function () {
        this.get('#/', function (context) {
            var url = '/Home/Home';
            GetPartial(url);
            SetActive('.home');
        });
    });

    var news = $.sammy('#main-content', function (data) {
        this.get('#/news-feed', function (context) {
            var url = "/Home/News";
            GetPartial(url);
            SetActive('.news-feed');
        });
    });

    var about = $.sammy('#main-content', function (data) {
        this.get('#/about', function (context) {
            var url = "/Home/About";
            GetPartial(url);
            SetActive('.about');
        });
    });

    var file = $.sammy('#main-content', function (data) {
        this.get('#/file-manager', function (context) {
            var url = "/File/FileManagerPartial";
            GetPartial(url);
            SetActive('.file-manager');
        });
    });

    var download = $.sammy('#main-content', function (data) {
        this.get('#/file-download', function (context) {
            var uniqueID = context.params['uniqueID'];
            var param = '?uniqueID=';
            var action = '/File/FileDownloadPartial';
            var url = action + param + uniqueID;

            GetPartial(url);
            SetActive('.file-manager');
        });
    });

    var filedownload = $.sammy('#main-content', function (data) {
        this.get('#/file/filedownload', function (context) {
            var uniqueID = context.params['uniqueID'];
            var param = '?uniqueID=';
            var action = '/File/FileDownload';

            window.location = action + param + uniqueID;
        });
    });

    var error = $.sammy('#main-content', function (data) {
        this.get('#/error', function (context) {
            var url = "/Error/";
            GetPartial(url);
        });
    });

    var internalServerError = $.sammy('#main-content', function (data) {
        this.get('#/internal-server', function (context) {
            var url = "/Error/InternalServerPartial";
            GetPartial(url);
        });
    });

    var notFoundError = $.sammy('#main-content', function (data) {
        this.get('#/page-not-found', function (context) {
            var url = "/Error/NotFoundPartial";
            GetPartial(url);
        });
    });

    var fileNotFound = $.sammy('#main-content', function (data) {
        this.get('#/file-not-found', function (context) {
            var url = "/File/FileNotFoundPartial";
            GetPartial(url);
            SetActive('.file-manager');
        });
    });

    var duplicateFile = $.sammy('#main-content', function (data) {
        this.get('#/duplicate-file', function (context) {
            var url = "/Error/DuplicateFilePartial";
            GetPartial(url);
        });
    });

    var unauthorized = $.sammy('#main-content', function (data) {
        this.get('#/unauthorized', function (context) {
            var url = "/Error/UnauthorizedPartial";
            GetPartial(url);
        });
    });

    function GetPartial(url) {
        $.get(url, function (data) {
            $('#main-content').html(data);
        });
    }

    function SetActive(input) {
        $('.navbar-inverse .navbar-nav > li').removeClass('active');
        $('li' + input).addClass('active');
    }

    function NavigateToURL(path) {
        var currentURL = window.location.href;
        var index = currentURL.indexOf("/#/");
        var url = currentURL.substring(0, index) + '/#/' + path;

        window.location.href = url;
    }

    $(function () {
        app.run('#/');
        news.run('#/news-feed');
        file.run('#/file-manager');
        about.run('#/about');

        download.run('#/file-download');
        filedownload.run('#/file/filedownload');
        fileNotFound.run('#/file-not-found');

        error.run('#/error');
        internalServerError.run('#/internal-server-error');
        notFoundError.run('#/page-not-found');
        duplicateFile.run('#/duplicate-file-error');
        unauthorized.run('#/unauthorized');

        $('.routing').on('click', '.home', function () {
            NavigateToURL('');
            $('div.navbar-collapse').removeClass('in');
        });

        $('.routing').on('click', '.file-manager', function () {
            NavigateToURL('file-manager');
            $('div.navbar-collapse').removeClass('in');
        });

        $('.routing').on('click', '.news-feed', function () {
            NavigateToURL('news-feed');
            $('div.navbar-collapse').removeClass('in');
        });

        $('.routing').on('click', '.about', function () {
            NavigateToURL('about');
            $('div.navbar-collapse').removeClass('in');
        });
    });

})(jQuery);