import { AppConfiguration } from '../lib/AppConfiguration';
import ConfigBase from './environment';

const appConfig: AppConfiguration = Object.assign({}, ConfigBase, {
    serverUrl: 'localhost:8181'
})

export default appConfig;