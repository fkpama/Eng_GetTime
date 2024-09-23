import { EventEmitter, Injectable } from "@angular/core";

@Injectable({
    providedIn: 'root'
})
export class BrowserTabVisibilityService
{
    private _visible = true;
    public get visible(): boolean
    {
        if (!document.visibilityState)
        {
            return true;
        }

        return document.visibilityState == 'visible';
    }

    public visibleChange = new EventEmitter<boolean>();

    public constructor()
    {
        if (document.visibilityState)
        {
            document.addEventListener('visibilitychange', () => {
                this.visibleChange.emit();
            })
        }
    }
}