
// Navigation bar functionality
const navbar = document.getElementById('navbar');
let links = Array.from(navbar.getElementsByTagName('a'));
links.forEach(link => {
    link.addEventListener('click', function () {
        const current = navbar.getElementsByClassName('active');
        current[0].className = current[0].className.replace('active', '');
        this.className += 'active';
    })
});

// Function to toggle dark mode and persist in localStorage
function toggleDarkMode() {
    const body = document.body;

    body.classList.toggle('dark-mode'); // Add or remove a class to switch styles
    body.classList.toggle('light-mode'); // Add or remove a class to switch styles

    // Store dark mode preference in localStorage
    const isDarkMode = body.classList.contains('dark-mode');
    localStorage.setItem('darkMode', isDarkMode);
}
