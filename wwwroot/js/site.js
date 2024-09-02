document.addEventListener('DOMContentLoaded', function () {
    UsePreferredTheme();
});

function ToggleTheme() {
    var theme = GetTheme();
    const alt = theme === 'light' ? 'dark' : 'light'
    UseTheme(alt);
    SetSwitch(alt);
}

function UseTheme(theme) {
    SetTheme(theme);
    document.documentElement.setAttribute('data-bs-theme', theme);
}

function GetTheme() {
    return localStorage.getItem('theme');
}

function SetTheme(theme) {
    localStorage.setItem('theme', theme);
    document.cookie = `Theme=${theme}; path=/`;
}

function UsePreferredTheme() {
    var theme = GetTheme();
    if (theme == null) {
        theme = window.matchMedia('(prefers-color-scheme: dark)').matches ? 'dark' : 'light';
    }

    UseTheme(theme);
    SetSwitch(theme);
}

function SetSwitch(theme) {
    const light = theme === 'dark' ? false : true;
    document.getElementById('sun').style.display = light ? 'none' : 'inline';
    document.getElementById('moon').style.display = light ? 'inline' : 'none';
}
