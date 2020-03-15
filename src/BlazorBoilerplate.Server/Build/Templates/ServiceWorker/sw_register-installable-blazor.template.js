
function showAddToHomeScreen() {
    DotNet.invokeMethodAsync(blazorAssembly, blazorInstallMethod)
        .then(function () {  }, function (er) { setTimeout(showAddToHomeScreen, 1000); });
}

window.BlazorPWA = {
    installPWA: function () {
        if (window.PWADeferredPrompt) {
            window.PWADeferredPrompt.prompt();
            window.PWADeferredPrompt.userChoice
                .then(function (choiceResult) {
                    window.PWADeferredPrompt = null;
                });
        }
    }
};
