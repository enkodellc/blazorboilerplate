window.jsInterops = {
    setCookie: function (cookie) {
        document.cookie = cookie;
    },
    removeCookie: function (cookieName) {
        document.cookie = cookieName + '=; expires=Thu, 01 Jan 1970 00:00:01 GMT;';
    }
}