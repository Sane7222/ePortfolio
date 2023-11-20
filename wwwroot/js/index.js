$(document).ready(function () {
    $('h3').on('click', function () {
        $(this).next().slideToggle(1000, function () { });
    });
});

$(window).on('beforeunload', function () {

});