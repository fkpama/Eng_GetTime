import { Injectable } from "@angular/core";
import StaticConfig from '../environments/environment';

/**
 *
 * @class AppConfigService
 * 
 * @summary
 * Class managing the app configuration
 * 
 */
@Injectable({
    providedIn: 'root'
})
export class AppConfigService
{
    public get serverUrl(): string
    {
        return StaticConfig.serverUrl;
    }
}