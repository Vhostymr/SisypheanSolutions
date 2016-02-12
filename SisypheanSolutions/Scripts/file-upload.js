$(function () {
    $(document).on('change', '.btn-file :file', function () {
        var names = [];
        var input = $(this),
            numFiles = input.get(0).files ? input.get(0).files.length : 1;

        for (var i = 0; i < numFiles; ++i) {
            names.push(" " + $(this).get(0).files[i].name);
        }

        $("#filefeedback").val(names);
    });

    $('#submit-button').on('click', function () {
        var form = $('file-upload-form').serialize();

        if (document.getElementById('browse').files.length === 0) {
            alertify.error('Please select a file or files to be uploaded.');
            return;
        }

        var formData = new FormData();
        var length = document.getElementById('browse').files.length;
        var files = document.getElementById('browse').files;
        var i;
        for (i = 0; i < length; i++) {
            formData.append("files[" + i + "]", files[i]);
        }

        if (document.getElementById('encrypted').checked) {
            var dialogBox = new BootstrapDialog({
                title: "Share File(s)",
                message: 'Please enter a password to encrypt your file(s), and be sure to remember it to decrypt them later. <br/><br/> ' +
                         '<input id="modal-input-password" type="password" class="form-control" placeholder="Password" /> <br/>' +
                         '<input id="modal-input-confirm" type="password" class="form-control" placeholder="Confirm Password" />',
                cssClass: 'dialog-modal',
                closable: true,
                onshown: function () {
                    $('#modal-input').focus();
                },
                buttons: [{
                    label: 'Upload',
                    cssClass: 'btn-primary',
                    hotkey: 13,
                    action: function (dialogBox) {
                        if ($('#modal-input-password').val() === $('#modal-input-confirm').val()) {
                            if ($('#modal-input-password').val() === "") {
                                alertify.error('Please enter a password to encrypt, or press cancel and uncheck "Encrypted".');
                            }

                            else {
                                formData.append("password", $('#modal-input-password').val());
                                ajaxPost(formData);

                                $.each(BootstrapDialog.dialogs, function (id, dialogBox) {
                                    dialogBox.close();
                                });
                            }
                        }

                        else {
                            alertify.error('Password and Confirm Password do not match.');
                        }
                    }
                }, {
                    label: 'Cancel',
                    hotkey: 27,
                    action: function (dialogBox) {
                        $.each(BootstrapDialog.dialogs, function (id, dialogBox) {
                            dialogBox.close();
                        });
                    }

                }]
            });
            dialogBox.setType(BootstrapDialog.TYPE_INFO);
            dialogBox.open();
        }

        else {
            ajaxPost(formData);
        }
    });

    function ajaxPost(formData) {
        $.ajax({
            url: '/File/FileUpload/',
            data: formData,
            cache: false,
            contentType: false,
            processData: false,
            type: 'POST',
            success: function (data) {
                if (data.success) {
                    alertify.success("Your file(s) have been successfully uploaded.");
                }

                else {
                    var i;
                    for (i = 0; i < data.errors.length; i++) {
                        alertify.error(data.errors[i]);
                    }
                }
            }
        });
    }
});