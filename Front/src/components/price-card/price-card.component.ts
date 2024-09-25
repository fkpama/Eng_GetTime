import { Component, Input } from "@angular/core";
import { CurrencyConversion } from "../../lib/CurrencyConversion";
import { MatCardModule } from '@angular/material/card';
import { CommonModule } from "@angular/common";

@Component({
    selector: 'price-card',
    templateUrl: './price-card.component.html',
    styleUrls: ['./price-card.component.scss'],
    standalone: true,
    imports: [MatCardModule, CommonModule]
})
export class PriceCardComponent
{
    @Input({ required: true })
    public conversion!: CurrencyConversion;
}