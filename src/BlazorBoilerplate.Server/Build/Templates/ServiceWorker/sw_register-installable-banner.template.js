
function showAddToHomeScreen() {
    var pwaInstallPrompt = document.createElement('div');
    var pwaInstallButton = document.createElement('button');
    var pwaCancelButton = document.createElement('button');

    pwaInstallPrompt.id = 'pwa-install-prompt';
    pwaInstallPrompt.style.position = 'absolute';
    pwaInstallPrompt.style.bottom = '0.1rem';
    pwaInstallPrompt.style.left = '0.1rem';
    pwaInstallPrompt.style.right = '0.1rem';
    pwaInstallPrompt.style.padding = '0.5rem';
    pwaInstallPrompt.style.display = 'flex';
    pwaInstallPrompt.style.backgroundColor = 'lightslategray';
    pwaInstallPrompt.style.color = 'white';
    pwaInstallPrompt.style.fontFamily = 'sans-serif';
    pwaInstallPrompt.style.fontSize = '1.3rem';
    pwaInstallPrompt.style.borderRadius = '4px';

    pwaInstallButton.style.marginLeft = 'auto';
    pwaInstallButton.style.width = '4em';
    pwaInstallButton.style.backgroundColor = '#00796B';
    pwaInstallButton.style.color = 'white';
    pwaInstallButton.style.border = 'none';
    pwaInstallButton.style.borderRadius = '25px';

    pwaCancelButton.style.marginLeft = '0.3rem';
    pwaCancelButton.style.width = '4em';
    pwaCancelButton.style.backgroundColor = '#9d0d0d';
    pwaCancelButton.style.color = 'white';
    pwaCancelButton.style.border = 'none';
    pwaCancelButton.style.borderRadius = '25px';

    pwaInstallPrompt.innerText = 'Add to your homescreen?';
    pwaInstallButton.innerText = 'ok';
    pwaCancelButton.innerText = 'no';

    pwaInstallPrompt.appendChild(pwaInstallButton);
    pwaInstallPrompt.appendChild(pwaCancelButton);
    document.body.appendChild(pwaInstallPrompt);

    pwaInstallButton.addEventListener('click', addToHomeScreen);
    pwaCancelButton.addEventListener('click', hideAddToHomeScreen);
    setTimeout(hideAddToHomeScreen, 10000);
}

function hideAddToHomeScreen() {
    var pwa = document.getElementById('pwa-install-prompt');
    if (pwa) document.body.removeChild(pwa);
}

function addToHomeScreen(s, e) {
    hideAddToHomeScreen();
    if (window.PWADeferredPrompt) {
        window.PWADeferredPrompt.prompt();
        window.PWADeferredPrompt.userChoice
            .then(function (choiceResult) {
                window.PWADeferredPrompt = null;
            });
    }
}
