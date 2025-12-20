import { defineConfig } from 'vite';
import vue from '@vitejs/plugin-vue';
import { fileURLToPath, URL } from 'node:url';
// Determine base path based on environment
var getBasePath = function () {
    if (!process.env.GITHUB_ACTIONS)
        return '/';
    var env = process.env.VITE_ENVIRONMENT || 'prod';
    var repoBase = '/transaction-dispute-portal';
    switch (env) {
        case 'dev': return "".concat(repoBase, "/dev/");
        case 'int': return "".concat(repoBase, "/int/");
        case 'qa': return "".concat(repoBase, "/qa/");
        case 'prod': return "".concat(repoBase, "/prod/");
        default: return "".concat(repoBase, "/");
    }
};
export default defineConfig({
    plugins: [vue()],
    // Dynamic base path for multi-environment GitHub Pages
    base: getBasePath(),
    resolve: {
        alias: {
            '@': fileURLToPath(new URL('./src', import.meta.url))
        }
    },
    server: {
        port: 3000,
        host: true,
        proxy: {
            '/api': {
                target: 'http://localhost:5000',
                changeOrigin: true
            }
        }
    }
});
