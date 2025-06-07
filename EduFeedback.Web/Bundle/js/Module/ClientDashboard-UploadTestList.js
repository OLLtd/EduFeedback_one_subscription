$(document).ready(function () {
    PopulateServiceDropdown();

    // Initialize DataTable with empty data to show column headers
    $("#ExamAssignmentList").DataTable({
        "processing": true,
        "filter": true,
        "pageLength": 10,
        "data": [], // Empty data initially
        "columns": [
            { "title": "Test Name", "autoWidth": true },
            { "title": "Subject", "autoWidth": true },
            { "title": "Year", "autoWidth": true },
            {
                "className": 'details-control',
                "orderable": false,
                "title": "Question Paper",
                "defaultContent": 'Not Available'
            },
            {
                "className": 'details-control',
                "orderable": false,
                "title": "Solution Paper",
                "defaultContent": 'Not Available'
            },
            {
                "title": "Total Uploads",
                "autoWidth": true,
                "defaultContent": ''
            },
            {
                "title": "Completed Feedback",
                "autoWidth": true,
                "defaultContent": ''
            }
        ],
        "order": [[0, 'asc']]
    });
});

// Button click event to reinitialize and load DataTable with data
$('#btnSearchExam').click(function () {
    // Destroy the existing DataTable
    if ($.fn.DataTable.isDataTable('#ExamAssignmentList')) {
        $("#ExamAssignmentList").DataTable().destroy();
    }

    // Initialize DataTable with AJAX data source
    $("#ExamAssignmentList").DataTable({
        "processing": true,
        "filter": true,
        "pageLength": 10,
        "ajax": {
            "url": "../ClientDashboard/GetExamListAPI",
            "type": "Get",
            "datatype": "json",
            "data": function (d) {
                d.examDate = $('#ExamDate').val();
                d.service = $('#service-dropdown').val();
            }
        },
        "columns": [
            {
                "data": "examName", "title": "Test Name", "autoWidth": true
            },
            { "data": "subject", "title": "Subject", "autoWidth": true },
            { "data": "year", "title": "Year", "autoWidth": true },
            {
                "className": 'details-control',
                "orderable": false,
                "data": '',
                "title": "Question Paper",
                "defaultContent": '',
                "render": function (data, type, row) {
                    if (row.questionPaperName == 'Not Available' || row.questionPaperName == '') {
                        return 'Not Available';
                    } else {
                        return '<a href="#" class="download-link-QPaper" target="_blank" data-file="' + row.exam_ID + '">Download</a>';
                    }
                }
            },
            {
                "className": 'details-control',
                "orderable": false,
                "data": '',
                "title": "Solution Paper",
                "defaultContent": '',
                "render": function (data, type, row) {
                    if (row.solutionFileName == 'Not Available' || row.solutionFileName == '') {
                        return 'Not Available';
                    } else {
                        return '<a href="#" class="download-link-SPaper" target="_blank" data-file="' + row.exam_ID + '">Download</a>';
                    }
                }
            },
            {
                "data": "totalAssignment", "title": "Total Uploads", "autoWidth": true,
                render: function (data, type, row) {
                    if (row.totalAssignment > 0) {
                        return '<span><a href="/ClientDashboard/ViewUploadedFileList?Test_ID=' + row.exam_ID + '&SubjectId=' + row.subject_ID + '">' + row.totalAssignment + '   View</a></span>';
                    } else {
                        return '';
                    }
                }
            },
            {
                "data": "totalFeedbackStatus", "title": "Completed Feedback", "autoWidth": true,
                render: function (data) {
                    return data;
                }
            }
        ],
        "order": [[0, 'asc']]
    });

    // Event handlers for download links
    $('#ExamAssignmentList tbody').on('click', 'a.download-link-QPaper', function (e) {
        e.preventDefault();
        var fileId = $(this).data('file');
        window.location = "/ClientDashboard/DownloadQuestionSolutionPaper?CourseTestId=" + fileId + "&fileType=QuestionPaper";
    });

    $('#ExamAssignmentList tbody').on('click', 'a.download-link-SPaper', function (e) {
        e.preventDefault();
        var fileId = $(this).data('file');
        window.location = "/ClientDashboard/DownloadQuestionSolutionPaper?CourseTestId=" + fileId + "&fileType=SolutionPaper";
    });
});

function PopulateServiceDropdown() {
    var dropdown = $('#service-dropdown');
    dropdown.empty();
    dropdown.append($('<option></option>').attr('value', 0).text("Select Service"));
    dropdown.append($('<option></option>').attr('value', 1).text("GCSE"));
    dropdown.append($('<option></option>').attr('value', 2).text("L2W"));
}

function onServiceChange() {
    var service = $('#service-dropdown').val();
    if (service == 1) { // GCSE
        updateSessionValue('SchoolYear', 7);
    } else { // L2W
        updateSessionValue('SchoolYear', 6);
    }

    // Bind exam date list
    $.ajax({
        url: '/ClientDashboard/GetTestActiveDateList_ajax',
        type: 'GET',
        success: function (data) {
            var examDateDropdown = $('#ExamDate');
            examDateDropdown.empty();
            if (data.data.length == 0) {
                examDateDropdown.append($('<option></option>').attr('value', "No date available").text("No date available"));
            } else {
                $.each(data.data, function (index, item) {
                    var date = new Date(parseInt(item.replace('/Date(', '').replace(')/', '')));
                    var formattedDate = moment(date).format('DD-MM-YYYY');
                    examDateDropdown.append($('<option></option>').attr('value', formattedDate).text(formattedDate));
                });
            }
        },
        error: function (xhr, status, error) {
            console.error('Error fetching exam dates:', error);
        }
    });
}