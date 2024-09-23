import { Component, effect, OnInit, Signal } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { PriceCardComponent } from '../price-card/price-card.component';
import { PriceService } from '../../services/price.service';
import { CurrencyConversion } from '../../lib/CurrencyConversion';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [RouterOutlet, CommonModule, PriceCardComponent],
  templateUrl: './app.component.html',
  styleUrl: './app.component.scss'
})
export class AppComponent implements OnInit {
  title = 'engie-gettime';

  public get currencies(): Signal<CurrencyConversion[]>
  {
    return this.service.priceCount;
  }

  public constructor(public service: PriceService)
  {
  }

  ngOnInit(): void {
    console.debug('Starting service');
    this.service.start()
  }

  public getConversionText(idx: number, item: CurrencyConversion)
  {
    return item.text;
  }
}
