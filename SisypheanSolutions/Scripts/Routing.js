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

        $('.home').on('click', function () {
            NavigateToURL('');
        });

        $('.file-manager').on('click', function () {
            NavigateToURL('file-manager');
        });

        $('.news-feed').on('click', function () {
            NavigateToURL('news-feed');
        });

        $('.about').on('click', function () {
            NavigateToURL('about');
        });

        $('.contact').on('click', function () {
            NavigateToURL('contact');
        });
    });

})(jQuery);