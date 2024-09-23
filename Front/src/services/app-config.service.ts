import { Injectable } from "@angular/core";
import StaticConfig from '../environments/environment';

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