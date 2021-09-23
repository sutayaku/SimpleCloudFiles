module.exports = {
    outputDir: '../wwwroot/client',
    filenameHashing: false,
    devServer: {
        proxy: {
            '/api': {
                target: 'http://localhost:5000/',
                changeOrigin: true,
                ws: true,
            }
        }
    }
}