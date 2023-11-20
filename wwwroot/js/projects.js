$(document).ready(function () {
    $('.card').addClass('transition');
});

$(window).on('beforeunload', function () {
    $('.card').removeClass('transition');
});