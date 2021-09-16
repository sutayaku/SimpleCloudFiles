const api = (function() {
    var server = '';
    var _ = {
        checkLogin: `${server}/api/Login/CheckLogin`,
        login: `${server}/api/Login/Login`,
        logout: `${server}/api/Login/Logout`,

        accountInfo: `${server}/api/Account/Info`,
        accountUpdate: `${server}/api/Account/Update`,

        upload: `${server}/api/File/Upload`,
        download: `${server}/api/File`,

        getList: `${server}/api/Space/GetList`,
        createDir: `${server}/api/Space/CreateDir`,
        delete: `${server}/api/Space/Delete`
    };
    return _;
})();

export default api;