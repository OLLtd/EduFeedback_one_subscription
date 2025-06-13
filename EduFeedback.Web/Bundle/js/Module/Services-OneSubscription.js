
$(document).ready(function () {

    $("#btnSubscribe").click(function (e) {
        var flag = true;
        debugger;
        e.preventDefault();
        e.stopPropagation();

        $("#Year_IDSpan").hide();
        if ($('#Year').val() == "" || $('#Year').val() == null) {
            flag = false;
            $("#Year_IDSpan").show();
        }
        $("#Subject_IDSpan").hide();
        if ($('#Subject').val() == "" || $('#Subject').val() == null) {
            flag = false;
            $("#Subject_IDSpan").show();
        }
        $("#NameSpan").hide();
        if ($('#Name').val() == "" || $('#Name').val() == null) {
            flag = false;
            $("#NameSpan").show();
        }
        $("#EmailSpan").hide();
        if ($('#Email').val() == "" || $('#Email').val() == null) {
            flag = false;
            $("#EmailSpan").show();
        }
        var response = grecaptcha.getResponse();
        if (response.length == 0) {
            flag = false;
            $("#CaptchaSpan").show();
        }
        if (flag == true) {
            $("#form").submit();

            //var files = $("#fileInput")[0].files;


            //var formData = new FormData();
            //for (var i = 0; i < files.length; i++) {
            //    formData.append("files", files[i]);
            //}
            //formData.append("Year_ID", $('#Year_ID').val());
            //formData.append("Subject_ID", $('#Subject_ID').val());
            //formData.append("ExamDate", $('#ExamDate').val());
            //formData.append("TestName", $('#TestName').val());
            //debugger
            //$.ajax({
            //    url: '/ClientDashboard/BulkUploadFilesNew',// @Url.Action("BulkUploadFilesNew", "ClientDashboard")',
            //    type: 'POST',
            //    data: formData,
            //    contentType: false,
            //    processData: false,
            //    success: function (response) {
            //        alert('Files upload started successfully!');
            //    },
            //    error: function (xhr, status, error) {

            //        console.log("status" + status + "Error" + error);
            //        alert('Error uploading files: ' + error);
            //    },
            //    xhr: function () {
            //        var xhr = new window.XMLHttpRequest();
            //        xhr.upload.addEventListener("progress", function (evt) {
            //            if (evt.lengthComputable) {
            //                var percentComplete = evt.loaded / evt.total * 100;
            //                $("#progress-bar").css("width", percentComplete + "%");
            //                $("#progress-bar").text(Math.round(percentComplete) + "%");
            //            }
            //        }, false);
            //        return xhr;
            //    }
            //});

        }
        else {
            alert("Please fill mandatory fields");
        }


    });


   
});

