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

    var file = $.sammy('#main-content', function (data) {
        this.get('#/file-manager', function (context) {
            var url = "/Home/FileManager";
            GetPartial(url);
            SetActive('.file-manager');
        });
    });

    var about = $.sammy('#main-content', function (data) {
        this.get('#/about', function (context) {
            var url = "/Home/About";
            GetPartial(url);
            SetActive('.about');
        });
    });

    var contact = $.sammy('#main-content', function (data) {
        this.get('#/contact', function (context) {
            var url = "/Home/Contact";
            GetPartial(url);
            SetActive('.contact');
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
        contact.run('#/contact');

        $('.routing').on('click', '.home', function () {
            NavigateToURL('');
        });

        $('.routing').on('click', '.file-manager', function () {
            NavigateToURL('file-manager');
        });

        $('.routing').on('click', '.news-feed', function () {
            NavigateToURL('news-feed');
        });

        $('.routing').on('click', '.about', function () {
            NavigateToURL('about');
        });

        $('.routing').on('click', '.contact', function () {
            NavigateToURL('contact');
        });
    });

})(jQuery);