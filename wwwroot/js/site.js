
document.addEventListener('DOMContentLoaded', function () { // Add theme transition after DOM
    document.querySelector('body').classList.add('transition');
    document.querySelector('header').classList.add('transition');
    document.querySelector('#dark-mode-toggle').classList.add('transition');
    document.querySelectorAll('a').forEach(function (element) {
        element.classList.add('transition');
    });
    document.querySelectorAll('i[class*="media-link"]').forEach(function (element) {
        element.classList.add('transition');
    });
});

window.addEventListener('beforeunload', function () { // Prevent theme transition on refresh
    document.querySelector('body').classList.remove('transition');
    document.querySelector('header').classList.remove('transition');
    document.querySelector('#dark-mode-toggle').classList.remove('transition');
    document.querySelectorAll('a').forEach(function (element) {
        element.classList.remove('transition');
    });
    document.querySelectorAll('i[class*="media-link"]').forEach(function (element) {
        element.classList.remove('transition');
    });
});

function toggleDarkMode() { // Manage theme setting in local storage
    const body = document.body;

    body.classList.toggle('dark-mode'); // Add or remove a class to switch styles
    body.classList.toggle('light-mode');

    // Store dark mode preference in localStorage
    const isDarkMode = body.classList.contains('dark-mode');
    localStorage.setItem('darkMode', isDarkMode);
}
