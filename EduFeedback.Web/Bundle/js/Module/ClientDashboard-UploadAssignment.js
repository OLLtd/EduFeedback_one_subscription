//(function ($) {
// Code that uses jQuery 1.12.4
var dropzone;
$(document).ready(function () {

    //var studentNameMapping = @Html.Raw(Json.Encode(studentNameMapping));
    PopulateServiceDropdown();

    $('#ExamDate').datepicker({
        format: 'dd-mm-yyyy',
        autoclose: true,
        startDate: '-30d',
        endDate: "today"
    });

    $('#div-assignment-config').hide();

    // Get the template HTML and remove it from the doument.
    //var previewNode = document.querySelector("#template");
    //previewNode.id = "";
    //var previewTemplate = previewNode.parentNode.innerHTML;
    //previewNode.parentNode.removeChild(previewNode);
    Dropzone.autoDiscover = false;
    var maxImageWidth = 520, maxImageHeight = 790;
    // Check if Dropzone is already attached
    if (Dropzone.instances.length > 0) {
        Dropzone.instances.forEach(dz => dz.destroy());
    }
    dropzone = new Dropzone("#DropZone", {
        url: "../ClientDashboard/BulkUploadFiles",
        paramName: 'files',
        autoProcessQueue: true,
        autoQueue: true,
        parallelUploads: 20,
        maxFiles: 20,
        maxFileSize: 200, //defaults to 256 (MB)
        addRemoveLinks: true,
        uploadMultiple: true, // Adding This
        dictRemoveFile: 'X (remove)',
        createImageThumbnails: false,
        previewsContainer: "#dz-preview",// "#previews",//
        //acceptedFiles: ".jpeg,.jpg,.png,.gif,.pdf",
        acceptedFiles: "application/pdf",
        //  previewTemplate: previewTemplate,
        init: function () {
            var myDropzone = this;
            $('#btnBulkUpload').on("click", function (e) {
                var flag = true;
                debugger;
                e.preventDefault();
                e.stopPropagation();


                // Hide all validation messages
                hideValidationMessages();

                // Validate required fields
                var ddlTestName = document.getElementById("DDLTestName");
                var selectedIndex = ddlTestName.selectedIndex;


                if (!validateRequiredField('#Year_ID', '#Year_IDSpan')) flag = false;
                if (!validateRequiredField('#Subject_ID', '#Subject_IDSpan')) flag = false;
                if (!validateRequiredField('#ExamDate', '#ExamDateSpan')) flag = false;
                if (!validateRequiredField('#TestName', '#TestNameSpan')) flag = false;

                if (selectedIndex == 0) {
                    if (!validateRequiredField('#Week', '#WeekNoSpan')) flag = false;
                    // Validate file types and sizes
                    if (!validateRequiredField('#testPaperFileUpload', '#testPaperFileUpload_IDSpan')) flag = false;
                    if (!validateFile('#testPaperFileUpload', ['pdf'], 20 * 1024 * 1024)) flag = false;
                    if (!validateFile('#testPaperSolutionFileUpload', ['pdf'], 20 * 1024 * 1024)) flag = false;
                }

                //$("#Year_IDSpan").hide();
                //if ($('#Year_ID').val() == "" || $('#Year_ID').val() == null) {
                //    flag = false;
                //    $("#Year_IDSpan").show();
                //}
                //$("#Subject_IDSpan").hide();
                //if ($('#Subject_ID').val() == "" || $('#Subject_ID').val() == null) {
                //    flag = false;
                //    $("#Subject_IDSpan").show();
                //}
                //$("#ExamDateSpan").hide();
                //if ($('#ExamDate').val() == "" || $('#ExamDate').val() == null) {
                //    flag = false;
                //    $("#ExamDateSpan").show();
                //}
                //$("#WeekNoSpan").hide();
                //if ($('#Week').val() == "" || $('#Week').val() == null) {
                //    flag = false;
                //    $("#WeekNoSpan").show();
                //}


                //$("#TestNameSpan").hide();
                //if ($('#TestName').val() == "" || $('#TestName').val() == null) {
                //    flag = false;
                //    $("#TestNameSpan").show();
                //}
                //$("#testPaperFileUpload_IDSpan").hide();
                //var testPaperFile = document.getElementById('testPaperFileUpload').files[0];
                //if (!testPaperFile) {
                //    flag = false;
                //    $("#testPaperFileUpload_IDSpan").show();
                //}

                if (myDropzone.getQueuedFiles().length > 0) {

                    if (flag == true) {
                        $('#btnBulkUpload').text('Processing...');
                        $('#btnBulkUpload').prop('disabled', true);
                        $('.modalWait').show();
                        myDropzone.processQueue();
                    }

                }
                else {
                    alert("Please upload atleast 1 file");
                }
            });
            //// Update the total progress bar
            //this.on("totaluploadprogress", function (progress) {
            //    console.log(progress + "%");
            //});
            this.on('addedfile', function (file) {
                if (file.type === "application/pdf") {
                    //alert("adding files");
                    disableGDriveLoading();
                    $('#div-assignment-config').show();
                    $('#btnBulkUpload').css('display', 'block');
                    $('#btnGDriveBulkUpload').css('display', 'none');
                    file.status = "queued";
                    if (this.files.length > 50) {
                        alert("Please upload only 20 PDF assignment");
                        this.removeFile(file);
                        $('#div-assignment-config').show();
                    }
                    else {
                        // Create an input field for the file name
                        //var fileNameInput = document.createElement('input');
                        //fileNameInput.type = 'text';
                        //fileNameInput.value = file.name;
                        //fileNameInput.className = 'file-name-input';
                        //file.previewElement.appendChild(fileNameInput);

                        ////// Store the input field reference in the file object
                        ////file.fileNameInput = fileNameInput;

                        //// Update the file name when the input value changes
                        //fileNameInput.addEventListener('change', function () {
                        //    file.name = fileNameInput.value;
                        //});
                    }
                }

            });

            this.on("totaluploadprogress", function (progress) {

                console.log("Total progress: " + progress + "%");
                //$('.progress').show();
                //setTimeout(function () {
                //    $(".progress-bar").html("Loading...")
                //    document.querySelector(".progress .progress-bar").style.width = progress + "%";
                //}, 1000);

                window.location.href = "/ClientDashboard/UploadingAssignmentStatusList";

            });
            this.on('success', function (file, response) {
                window.location.href = "/ClientDashboard/UploadingAssignmentStatusList";
            });
            // Add mmore data to send along with the file as POST data. (optional)
            this.on("sending", function (file, xhr, formdata) {
                // Show the total progress bar when upload starts
                //document.querySelector(".progress").style.opacity = "1";
            });
            this.on("queuecomplete", function () {
                console.log("All files have been uploaded");
            });
            // Hide the total progress bar when nothing's uploading anymore
            //this.on("queuecomplete", function (progress) {
            //    document.querySelector(".progress").style.opacity = "0";
            //});
            this.on('completemultiple', function (file, xhr, formData) {
                debugger;
                $('.modalWait').hide();
                console.log("completemultiple");
                setTimeout(function () {
                    $('#btnBulkUpload').text('Start Upload');
                    $('#btnBulkUpload').hide()
                    $('#div-assignment-config').hide();
                    $('#upload-status-ui').show();
                    //$("#upload-status").append("All Assignments will be processed in the background!");
                    $('#lnk-view-upload').html("<a href='/ClientDashboard/ViewUploadTestList?testDate=" + $('#ExamDate').val() + "' style='color: blue;text-decoration: underline;' > click here to view your uploads</a>");
                }, 900);

                // var $select = $('$lnk-view-upload');

                //if (confirm("All Assignments will be uploads in the background!")) {
                //    window.location = "/ClientDashboard/AssignmentLists";
                //}

                // this.removeAllFiles(true);

                //    $("#upload-status").html("File uploaded successfully: ");

                //    if (confirm("Thanks, the feedback will be ready in 2 business days!")) {
                //        // alert('Done')

                //        console.log('formData' + formData);
                //    }
                //}
            });

            // Event handler for file removal
            this.on("removedfile", function (file) {
                console.log("File removed: ", file.name);
                // Check if there are no files left in the Dropzone
                //alert("removing files");
                if (this.files.length === 0) {
                    enableGDriveLoading();
                    $('#div-assignment-config').hide();
                    $('#btnBulkUpload').css('display', 'none');
                    $('#btnGDriveBulkUpload').css('display', 'none');
                }

            });

        },
        sending: function (file, xhr, formData) {

            // Update the form data with the modified file name
            //formData.append('modifiedFileName', file.fileNameInput.value);

            formData.append('Year_ID', $('#Year_ID').val());
            formData.append('Subject_ID', $('#Subject_ID').val());
            formData.append('ExamDate', $('#ExamDate').val());
            formData.append('TestName', $('#TestName').val());
            formData.append('Comment', $('#Comment').val());
            formData.append('gDriveLink', $('#txtDriveLink').val());

            var testPaperFile = document.getElementById('testPaperFileUpload').files[0];
            if (testPaperFile) {
                // Validate file type (e.g., only allow PDF files)
                var allowedExtensions = ['pdf'];
                var fileExtension = testPaperFile.name.split('.').pop().toLowerCase();
                if ($.inArray(fileExtension, allowedExtensions) === -1) {
                    alert('Invalid file type. Only PDF files are allowed.');
                } else {
                    // Validate file size (e.g., max 200 MB)
                    var maxFileSize = 20 * 1024 * 1024; // 20 MB in bytes
                    if (testPaperFile.size > maxFileSize) {
                        alert('File size exceeds the maximum limit of 20 MB.');
                    } else {
                        // Append the file to formData if all validations pass
                        formData.append('TestPaperFile', testPaperFile);
                    }
                }

            }

            var testPaperSolutionFile = document.getElementById('testPaperSolutionFileUpload').files[0];
            if (testPaperSolutionFile) {
                // Validate file type (e.g., only allow PDF files)
                var allowedExtensions = ['pdf'];
                var fileExtension = testPaperSolutionFile.name.split('.').pop().toLowerCase();
                if ($.inArray(fileExtension, allowedExtensions) === -1) {
                    alert('Invalid file type. Only PDF files are allowed.');
                } else {
                    // Validate file size (e.g., max 20 MB)
                    var maxFileSize = 20 * 1024 * 1024; // 20 MB in bytes
                    if (testPaperSolutionFile.size > maxFileSize) {
                        alert('File size exceeds the maximum limit of 20 MB.');
                    } else {
                        // Append the file to formData if all validations pass
                        formData.append('TestPaperSolutionFile', testPaperSolutionFile);
                    }
                }
            }
        },

        error: function (file, response) {

            console.log('response=>' + response);
            if (response == "We accept only PDF file type.") {
                alert(response);
                // this.removeFile(file);
                errFlag = true;
            } else if (response == "Your files limit exceeds, we cannot accept more") {
                // this.removeFile(file);
                errFlag = true;
            }
            else {

                // alert("he image resolution is too low. Please make sure the image dimensions are at least 520px X 790px (width X height).  \n 2- No. of uploaded files are more then 5.");

                errFlag = true;
            }
        }, accept: function (file, done) {
            file.acceptDimensions = done;
            file.rejectDimensions = function () { done("Invalid dimension."); };
        }
    });


    //$('#TestName').change(function () {
    //      GetmaskedTestName();
    //});

    //$('#Year_ID').change(function () {
    //    GetmaskedTestName();
    //});

    //$('#TestName').on('paste', function (event) {
    //    // Timeout to ensure the text is pasted before processing
    //    setTimeout(() => {
    //        var pastedText = $(this).val();  // Get the pasted text from the textbox
    //        var processedText = pastedText.replace(/[^a-zA-Z0-9]/g, '');  // Process the text
    //        $(this).val(processedText);  // Update the textbox with the processed text
    //        console.log(processedText);  // Optional: Output to console
    //    }, 50);
    //});

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
        });

    });
    $('#btnReset').click(function () {
        window.location = "/ClientDashboard/UploadAssignment"
    });

    // Function to disable the GDrive Data loading
    function disableGDriveLoading() {
        $('#btnSyncFromDrive').prop('disabled', true);
    }

    // Function to enable the GDrive Data loading
    function enableGDriveLoading() {
        $('#btnSyncFromDrive').prop('disabled', false);
    }


    // GetmaskedTestName();

    updateSubjectsVisibility($('#Year_ID').val());


});

$('#service-dropdown').change(function () {
    var _serviceId = "";
    if ($('#service-dropdown').val() > 0) {
        _year = $('#service-dropdown').val();
        updateYearVisibility(_year);
        if ($('#Year_ID').val() > 0) {
            $('#Year_ID').trigger('change'); // Only trigger if a valid year is selected
        }

        UpdateTestNameList($('#service-dropdown').val());
    }

});

$('#Year_ID').change(function () {
    var _year = "";
    if ($('#Year_ID').val() > 0) {
        _year = $('#Year_ID').val();
        updateSessionValue('SchoolYear', _year);
        updateSubjectsVisibility(_year);
    }

});

function GetmaskedTestName() {

    //XLW18(freetext) +  _16012024_Y4Y10MATHS





    $.ajax({
        url: "/ClientDashboard/GetOrgEmailCode",
        type: "GET",
        //data: { key: key },
        success: function (data) {

            if (data.data) {
                var _date = "";
                var examDate = $('#ExamDate').val();
                if (examDate) {
                    var dateParts = examDate.split('-');
                    var day = dateParts[0];
                    var month = dateParts[1];
                    var year = dateParts[2];
                    _date = day + month + year;
                }
                var _year = "";
                if ($('#Year_ID').val() > 0) {
                    _year = $('#Year_ID').val();
                }

                var orgEmailCode = data.data;
                var _subjectName = $('#Subject_ID option:selected').text();
                var _week = $('#Week').val();
                var testName = orgEmailCode + "W" + _week + "_" + _date + "_" + "Y" + _year + _subjectName;
                $('#TestName').val(testName.toUpperCase().replace(/[^a-zA-Z0-9 _]/g, ''));
            }
        },
        error: function (xhr, status, error) {
            console.log("Error updating session value: " + error);
        }
    });
}

// Function to show or hide subjects based on the selected course yearfunction 
function updateSubjectsVisibility(selectedYear) {
    if (selectedYear != null && selectedYear != "")
        $('#Subject_ID option').each(function () {
            var subjectValue = $(this).text();
            // Add your logic to determine which subjects to show or hide based on the selectedYear
            // For example, hide subjects with value 'Math' if the selected year is less than 7
            if (selectedYear < 7 && (subjectValue === 'Maths' || subjectValue === 'English' || subjectValue === 'Physics' || subjectValue === 'Chemistry' || subjectValue === 'Biology')) {
                $(this).hide();
            } else if (selectedYear < 7 && (subjectValue === 'Creative Writing' || subjectValue === 'Comprehension')) {
                $(this).show();
            } else if (selectedYear >= 7 && (subjectValue === 'Creative Writing' || subjectValue === 'Comprehension')) {
                $(this).hide();
            } else {
                $(this).show();
            }
        });
    // Set the selected index to 0 after updating the visibility
    $('#Subject_ID').prop('selectedIndex', 0);
}


// Function to show or hide subjects based on the selected course yearfunction 
function updateYearVisibility(serviceId) {
    if (serviceId != null && serviceId != "")
        $('#Year_ID option').each(function () {
            var yearId = $(this).text();
            //alert(yearId);
            // Add your logic to determine which subjects to show or hide based on the selectedYear
            // For example, hide subjects with value 'Math' if the selected year is less than 7
            if (serviceId == 2 && (yearId === "School Year 7" || yearId === "School Year 8" || yearId === "School Year 9" || yearId === "School Year 10" || yearId === "School Year 11")) {
                $(this).hide();
            } else if (serviceId == 2 && (yearId === "School Year 2" || yearId === "School Year 3" || yearId === "School Year 4" || yearId === "School Year 5" || yearId === "School Year 6")) {
                $(this).show();
            } else if (serviceId == 1 && (yearId === "School Year 2" || yearId === "School Year 3" || yearId === "School Year 4" || yearId === "School Year 5" || yearId === "School Year 6")) {
                $(this).hide();
            } else {
                $(this).show();
            }
        });
    // Set the selected index to 0 after updating the visibility
    $('#Year_ID').prop('selectedIndex', 0);
}
function updateSessionValue(key, value) {
    //alert("key: " + key + ", value: " + value);

    $.ajax({
        url: "/ClientDashboard/UpdateSessionValue",
        type: "POST",
        data: { key: key, value: value },
        success: function (response) {
            if (response.success) {
                console.log("Session value updated successfully.");
            } else {
                console.log("Failed to update session value.");
            }
        },
        error: function (xhr, status, error) {
            console.log("Error updating session value: " + error);
        }
    });
}
function SelectTestPaper() {
    var ddlTestName = document.getElementById("DDLTestName");
    var selectedIndex = ddlTestName.selectedIndex;

    if (selectedIndex > 0) {
        disableAllControls();
        document.getElementById("TestName").value = $('#DDLTestName option:selected').text();


        $.ajax({
            url: "/ClientDashboard/GetCourseTestDetail",
            type: "GET",
            dataType: "json",
            data: {
                courseTestId: $('#DDLTestName option:selected').val(),
                courseTestName: $('#DDLTestName option:selected').text()
            },
            success: function (Result) {
                //  $('#TestName').val(Result.data);                
                $("#Year_ID").val(Result.data.Year_ID);
                //updateSubjectsVisibility(Result.data.Year_ID);
                if (Result.data.Year_ID < 7) {

                    $("#Subject_ID").prop('selectedIndex', $("#Subject_ID option").length - 1);
                }
                else {
                    $("#Subject_ID").val(Result.data.Subject_ID);
                }

            },
        });
    } else {
        enableAllControls();
    }
}

function disableAllControls() {
    var controls = document.querySelectorAll("input, select");
    controls.forEach(function (control) {
        if (control.id != "DDLTestName" && control.id != "Comment"
            && control.id != "testPaperSolutionFileUpload"
            && control.id != "testPaperFileUpload"
            && control.id != "txtDriveLink"
            && control.id != "organisation-dropdown"
            && control.id != "service-dropdown"            
            && !control.closest('#testPapersList')
            && control.type !== "search") {
            control.disabled = true;
        }
    });
}

function enableAllControls() {
    var controls = document.querySelectorAll("input, select");
    controls.forEach(function (control) {
        control.disabled = false;
    });
}

function isNumberKey(evt) {
    var charCode = (evt.which) ? evt.which : evt.keyCode;
    if (charCode > 31 && (charCode < 48 || charCode > 57)) {
        return false;
    }
    return true;
}
//var inputString = "NFT_Y07_Maths_Week25_25May24_Algebra";
//var outputString = inputString.replace(/[^a-zA-Z0-9]/g, '');