
$(document).ready(function () {
    $('.form-control').addClass('transition'); // Add theme transition after DOM
    $('.form-check-input').addClass('transition');
    $('span').addClass('transition');
    $('button[type="submit"]').addClass('transition');

    $('#contact-form').submit(function (event) { // AJAX form submission
        event.preventDefault();

        let form = this;

        if (form.checkValidity()) {
            form.classList.remove('was-validated');
            $.ajax({
                url: 'https://formsubmit.co/ajax/6b1cb0b25aa8e9f478552683fc1eb005',
                method: 'POST',
                dataType: 'json',
                accepts: 'application/json',
                data: {
                    Name: $('#Name').val(),
                    Email: $('#Email').val(),
                    Message: $('#Message').val()
                },
                success: function (response) {
                    setHeader('Success');
                },
                error: function (xhr, status, error) {
                    window.location.href = "https://matias-moseley.azurewebsites.net/Error";
                }
            });

            setHeader('Processing');
            $('#contact-form').slideUp(1000, function () { });
        } else {
            form.classList.add('was-validated');
        }
    });
});

$(window).on('beforeunload', function () {
    $('.form-control').removeClass('transition'); // Prevent theme transition on refresh
    $('.form-check-input').removeClass('transition');
    $('span').removeClass('transition');
    $('button[type="submit"]').removeClass('transition');
});

function setHeader(text) {
    $('h1').slideUp(1000, function () {
        $(this).text(text).slideDown(1000, function () { });
    })
}
