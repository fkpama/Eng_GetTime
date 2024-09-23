import { AppConfiguration } from '../lib/AppConfiguration';
import ConfigBase from './environment.development';

const config: AppConfiguration = Object.assign({}, ConfigBase, {
    serverUrl: 'wss://orange-space-eureka-vgwjj5gqjv5fpxp4-8181.app.github.dev/'
});

export default config;