import { Injectable, OnInit, Signal, signal } from "@angular/core";
import { BrowserTabVisibilityService } from "./browser-tab-visibility.service";
import { Subscription } from "rxjs";
import { CurrencyConversion } from "../lib/CurrencyConversion";
import { AppConfigService } from "./app-config.service";



@Injectable({
    providedIn: 'root'
})
export class PriceService
{
    private webSocket: WebSocket | null;

    private _tabSubscription: Subscription | null;
    private _visibilityTimeout: number = 0;

    private _priceCount = signal<CurrencyConversion[]>([]);
    public get priceCount(): Signal<CurrencyConversion[]>
    {
        return this._priceCount;
    }

    public get isRunning()
    {
        return !!this.webSocket
        && (this.webSocket.readyState === WebSocket.CONNECTING
        || this.webSocket.readyState === WebSocket.OPEN);
    }

    constructor(
        private _tabVisibility: BrowserTabVisibilityService,
        private _appConfig: AppConfigService
    )
    {
        this.webSocket = null!;
        this._tabSubscription = null;
    }

    public start()
    {

        let addr = this._appConfig.serverUrl;
        console.info(`Connecting to server: ${addr}`);
        let ws = this.webSocket = new WebSocket(addr);
        this.webSocket.onopen = () => {
            console.debug('WebSocket connected')
        }

        ws.onmessage = (e) => this._onMessage(ws, e);

        ws.onclose = (e) => this._onClosed(ws, e);

        ws.onerror = (e) => {
            console.error(e);
            this._closeSocket(ws);
        }

        if (!this._tabSubscription)
        {
            this._tabSubscription = this._tabVisibility.visibleChange.subscribe(() => {
                console.debug('Tab Visibility changed: ', this._tabVisibility.visible);
                if (!this._tabVisibility.visible)
                {
                    this._visibilityTimeout = <any>setTimeout(() => {
                        if (this._visibilityTimeout > 0){

                            clearTimeout(this._visibilityTimeout);
                            this._visibilityTimeout = -1;
                            if (this.webSocket && !this._tabVisibility.visible)
                            {
                                this._closeSocket(this.webSocket);
                            }
                        }
                    }, 2000);
                }
                else if (!this.isRunning)
                {
                    this.start();
                }
            });
        }
        
    }

    private _closeSocket(ws: WebSocket)
    {
        console.debug('Closing my Socket');
        ws.close();
        this.webSocket = null;
    }

    private _onClosed(ws: WebSocket, evt: CloseEvent)
    {
        if (this)
        console.log('Socket closed');
        this.webSocket = null;
    }

    private async _onMessage(ws: WebSocket, evt: MessageEvent)
    {
        let data: ArrayBuffer = evt.data;
        if (evt.data instanceof Blob)
        {
            data = await evt.data.arrayBuffer();
        }
        
        if (data instanceof ArrayBuffer)
        {
            console.log('Event received: ', data.byteLength);
            let encoder = new TextDecoder();
            let ar = new Uint8Array(data, 0)
            let startIndex = 0;
            while(true)
            {
                let idx = ar.indexOf(0, startIndex);
                if (idx >= 0 && idx <= ar.length - 2 && ar[idx + 1] == 0)
                {
                    let txt = encoder.decode(ar.slice(startIndex, idx));
                    console.log(txt);
                    startIndex = idx += 2;
                    let rateAr = new Float64Array(ar.slice(startIndex, startIndex + 8));
                    let rate = rateAr[0]
                    console.log(rate);
                    startIndex+= rateAr.BYTES_PER_ELEMENT;
                }
                else
                {
                    break;
                }
            }
        }
        else if (typeof data === 'string')
        {
            const currencies : CurrencyConversion[] = JSON.parse(data);
            this._priceCount.set(currencies);
        }
        else

        {
            console.error("Invallid event:", evt.data);
        }
    }
}