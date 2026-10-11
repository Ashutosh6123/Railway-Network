import { Router } from '@angular/router';
import { CommonModule } from '@angular/common';
import { Component, inject } from '@angular/core';
import {
  Subject,
  of
} from 'rxjs';

import {
  catchError,
  debounceTime,
  distinctUntilChanged,
  map,
  switchMap
} from 'rxjs/operators';

import { Station, StationService } from '../../core/services/station.service';
import { TrainService } from '../../core/services/train.service';
import { AuthService } from '../../core/services/auth.service';

@Component({
  selector: 'app-home',
  imports: [CommonModule],
  templateUrl: './home.html',
  styleUrl: './home.css'
})
export class Home {

  private readonly stationService = inject(StationService);
  private readonly trainService = inject(TrainService);
  private readonly router = inject(Router);

  private readonly fromSearchSubject = new Subject<string>();
  private readonly toSearchSubject = new Subject<string>();
  private readonly authService = inject(AuthService);

  fromSearchText = '';
  toSearchText = '';

  fromSuggestions: Station[] = [];
  toSuggestions: Station[] = [];

  selectedFromStation: Station | null = null;
  selectedToStation: Station | null = null;

  isFromFocused = false;
  isToFocused = false;

  constructor() {

    this.fromSearchSubject
      .pipe(
        map(search => search.trim()),
        debounceTime(300),
        distinctUntilChanged(),
        switchMap(search => {
          if (search.length < 2) {
            return of([]);
          }

          return this.stationService.searchStations(search).pipe(
            catchError(() => of([]))
          );
        })
      )
      .subscribe(stations => {
        this.fromSuggestions = stations;
      });

    this.toSearchSubject
      .pipe(
        map(search => search.trim()),
        debounceTime(300),
        distinctUntilChanged(),
        switchMap(search => {
          if (search.length < 2) {
            return of([]);
          }

          return this.stationService.searchStations(search).pipe(
            catchError(() => of([]))
          );
        })
      )
      .subscribe(stations => {
        this.toSuggestions = stations;
      });
  }

  get isAdmin(): boolean {
    return this.authService.getRole() === 'Administrator';
  }

  goToAdmin(): void {
    this.router.navigate(['/admin']);
  }

  onFromInput(event: Event): void {
    const input = event.target as HTMLInputElement;

    this.fromSearchText = input.value;
    this.selectedFromStation = null;

    if (input.value.trim().length < 2) {
      this.fromSuggestions = [];
      return;
    }

    this.fromSearchSubject.next(input.value);
  }

  onToInput(event: Event): void {
    const input = event.target as HTMLInputElement;

    this.toSearchText = input.value;
    this.selectedToStation = null;

    if (input.value.trim().length < 2) {
      this.toSuggestions = [];
      return;
    }

    this.toSearchSubject.next(input.value);
  }

  selectFromStation(station: Station): void {
    this.selectedFromStation = station;
    this.fromSearchText = `${station.name} (${station.code})`;
    this.fromSuggestions = [];
  }

  selectToStation(station: Station): void {
    this.selectedToStation = station;
    this.toSearchText = `${station.name} (${station.code})`;
    this.toSuggestions = [];
  }

  onFromFocus(): void {
    this.isFromFocused = true;
  }

  onToFocus(): void {
    this.isToFocused = true;
  }

  onFromBlur(): void {
    this.isFromFocused = false;

    setTimeout(() => {
      this.fromSuggestions = [];
    }, 150);
  }

  onToBlur(): void {
    this.isToFocused = false;

    setTimeout(() => {
      this.toSuggestions = [];
    }, 150);
  }

  today = this.getTodayDate();

  private getTodayDate(): string {
    const date = new Date();

    const year = date.getFullYear();
    const month = String(date.getMonth() + 1).padStart(2, '0');
    const day = String(date.getDate()).padStart(2, '0');

    return `${year}-${month}-${day}`;
  }

  searchTrains(): void {
    if (!this.selectedFromStation || !this.selectedToStation) {
      return;
    }

    const journeyDate = (
      document.getElementById('journeyDate') as HTMLInputElement
    )?.value;

    if (!journeyDate) {
      return;
    }

    if (journeyDate < this.today) {
      return;
    }

    this.router.navigate(['/trains'], {
      queryParams: {
        fromStationId: this.selectedFromStation.id,
        toStationId: this.selectedToStation.id,
        fromStationName: this.selectedFromStation.name,
        toStationName: this.selectedToStation.name,
        journeyDate
      }
    });
  }
}