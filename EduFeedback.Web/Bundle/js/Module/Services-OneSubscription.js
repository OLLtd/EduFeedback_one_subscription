
$(document).ready(function () {

    $("#btnSubscribe").click(function (e) {
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
        else {
            var email = $('#Email').val();
            const pattern = /^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$/;
            const isValid = pattern.test(email);
            if (isValid) {
                flag = true;
            }
            else {
                $('#email-error').html('Please enter valid email Id');
                $('#email-error').show();
                flag = false;
            }
        }
        //var response = grecaptcha.getResponse();
        //if (response.length == 0) {
        //    flag = false;
        //    $("#CaptchaSpan").show();
        //}
        if (flag == true) {
            $("#form").submit();

        }
        else {
            alert("Please fill mandatory fields");
        }


    });

    function AssignmentPerWeekOnChange() {
        var AssignmentPerWeek = $('#assignmentPerWeek').val();
        var Course_ID = '1'; //$('#SchoolYear').val();

        $.ajax({
            url: '/Home/GetCoursePriceAsPerAssignment',
            type: "GET",
            dataType: "JSON",
            data: { CourseId: Course_ID, Packs: AssignmentPerWeek },
            success: function (Result) {
                $('#AssignmentPerWeek').val(AssignmentPerWeek);
                $('#PackQuantity').val(AssignmentPerWeek);

            }
        });
    }
});

