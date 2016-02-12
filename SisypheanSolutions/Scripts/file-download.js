$(function () {
    $('#submit-button').on('click', function () {
        var url = '/File/ValidateDownload';

        $.post(url, { uniqueID: $('#file').val(), password: $('#password').val() }, function (data) {
            if (data.success) {
                $('#file-form').submit();
            }

            else {
                var i;
                for (i = 0; i < data.errors.length; i++) {
                    alertify.error(data.errors[i]);
                }
            }
        });
    });
});