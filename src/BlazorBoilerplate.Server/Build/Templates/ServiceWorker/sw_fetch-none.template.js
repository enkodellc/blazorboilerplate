self.addEventListener(networkFetchEvent, event => {
    return fetch(event.request);
});
