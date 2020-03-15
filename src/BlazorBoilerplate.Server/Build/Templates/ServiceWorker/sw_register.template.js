window.updateAvailable = new Promise(function (resolve, reject) {
    var { hostname } = window.location;
    if (typeof ignoreHosts !== 'undefined') {
        if (ignoreHosts.includes(hostname)) {
            return;
        }
    }
    if ('serviceWorker' in navigator) {
        navigator.serviceWorker.register(serviceWorkerFileName)
            .then(function (registration) {
                console.log('Registration successful, scope is:', registration.scope);
                registration.onupdatefound = () => {
                    const installingWorker = registration.installing;
                    installingWorker.onstatechange = () => {
                        switch (installingWorker.state) {
                            case swInstalledEvent:
                                if (navigator.serviceWorker.controller) {
                                    resolve(true);
                                } else {
                                    resolve(false);
                                }
                                break;
                            default:
                        }
                    };
                };
            })
            .catch(error =>
                console.log('Service worker registration failed, error:', error));
    }
});

