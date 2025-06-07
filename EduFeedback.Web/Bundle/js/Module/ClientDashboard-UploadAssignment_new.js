
$(document).ready(function () {
    $('#ExamDate').datepicker({
        format: 'dd-mm-yyyy',
        autoclose: true,
        startDate: '-30d',
        endDate: "today"
    });

    $('#div-assignment-config').hide();

    $('#TestName').change(function () {
          GetmaskedTestName();
    });

    $('#Year_ID').change(function () {
        GetmaskedTestName();
    });

    $('#ExamDate').change(function () {

        $.ajax({
            url: "/ClientDashboard/GetExamTestName",
            type: "GET",
            dataType: "json",
            data: { pSearchDate: $(this).val() },
            success: function (Result) {
              //  $('#TestName').val(Result.data);
                const d = new Date();
                let yearCode = d.getFullYear();
                var _year = "";
                if ($('#Year_ID').val() > 0) {
                    _year = "Y" + $('#Year_ID').val();
                }
                var newTextCode = $('#org-name-display').html() + yearCode + _year;

               // var text = $('#TestName').text();
                $('#TestName').val(Result.data.replace(newTextCode, ''));
                GetmaskedTestName();
            },
        })

    });

    $("input[type=file]").change(function () {
        $('#div-assignment-config').show();
    });
    $("#btnBulkUpload").click(function (e) {
        var flag = true;
        debugger;
        e.preventDefault();
        e.stopPropagation();

        $("#Year_IDSpan").hide();
        if ($('#Year_ID').val() == "" || $('#Year_ID').val() == null) {
            flag = false;
            $("#Year_IDSpan").show();
        }
        $("#Subject_IDSpan").hide();
        if ($('#Subject_ID').val() == "" || $('#Subject_ID').val() == null) {
            flag = false;
            $("#Subject_IDSpan").show();
        }
        $("#ExamDateSpan").hide();
        if ($('#ExamDate').val() == "" || $('#ExamDate').val() == null) {
            flag = false;
            $("#ExamDateSpan").show();
        }
        $("#TestNameSpan").hide();
        if ($('#TestName').val() == "" || $('#TestName').val() == null) {
            flag = false;
            $("#TestNameSpan").show();
        }
        if (flag == true) {

            var files = $("#fileInput")[0].files;


            var formData = new FormData();
            for (var i = 0; i < files.length; i++) {
                formData.append("files", files[i]);
            }
            formData.append("Year_ID", $('#Year_ID').val());
            formData.append("Subject_ID", $('#Subject_ID').val());
            formData.append("ExamDate", $('#ExamDate').val());
            formData.append("TestName", $('#TestName').val());
            debugger
            $.ajax({
                url: '/ClientDashboard/BulkUploadFilesNew',// @Url.Action("BulkUploadFilesNew", "ClientDashboard")',
                type: 'POST',
                data: formData,
                contentType: false,
                processData: false,
                success: function (response) {
                    alert('Files upload started successfully!');
                },
                error: function (xhr, status, error) {

                    console.log("status" + status + "Error" + error);
                    alert('Error uploading files: ' + error);
                },
                xhr: function () {
                    var xhr = new window.XMLHttpRequest();
                    xhr.upload.addEventListener("progress", function (evt) {
                        if (evt.lengthComputable) {
                            var percentComplete = evt.loaded / evt.total * 100;
                            $("#progress-bar").css("width", percentComplete + "%");
                            $("#progress-bar").text(Math.round(percentComplete) + "%");
                        }
                    }, false);
                    return xhr;
                }
            });

        }
        else {
            alert("Please fill mandatory fields");
        }


    });


    $('#btnReset').click(function () {
        window.location = "/ClientDashboard/UploadAssignment"
    });

     GetmaskedTestName();
});

function GetmaskedTestName() {

    const d = new Date();
    let yearCode = d.getFullYear();
    var _year = "";
    if ($('#Year_ID').val() > 0) {
        _year = "Y" + $('#Year_ID').val();
    }
    var parseName = $('#org-name-display').html() + yearCode + _year;

    var _parseTestName = $('#TestName').val().toUpperCase().replace(/[^a-zA-Z0-9 _]/g, '');

    $('#TestNamePrefix').val("Preview : " + parseName + _parseTestName);
    $('#TestName').val(_parseTestName);

}