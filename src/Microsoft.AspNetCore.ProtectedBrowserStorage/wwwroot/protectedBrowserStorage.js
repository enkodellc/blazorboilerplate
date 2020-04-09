(function () {
    // We may be able to eliminate this .js file entirely as of preview 8,
    // because it will be possible to invoke things like localStorage.setValue
    // directly from .NET without needing a JS wrapper
    window.protectedBrowserStorage = {
        get: (storeName, key) => window[storeName][key],
        set: (storeName, key, value) => { window[storeName][key] = value; },
        delete: (storeName, key) => { delete window[storeName][key]; }
    };
})();
