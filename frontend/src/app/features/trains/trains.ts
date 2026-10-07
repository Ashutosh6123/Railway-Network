import { Component, inject, OnInit, signal } from '@angular/core';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { FormsModule } from '@angular/forms';

import {
  Train,
  TrainService,
  RouteStop
} from '../../core/services/train.service';

import {
  AvailabilityService,
  CoachType
} from '../../core/services/availability.service';

@Component({
  selector: 'app-trains',
  imports: [RouterLink, FormsModule],
  templateUrl: './trains.html',
  styleUrl: './trains.css'
})
export class Trains implements OnInit {

  private readonly route = inject(ActivatedRoute);
  private readonly trainService = inject(TrainService);
  private readonly availabilityService = inject(AvailabilityService);
  private readonly router = inject(Router);

  trains = signal<Train[]>([]);

  fromStationId: number | null = null;
  toStationId: number | null = null;
  journeyDate: string | null = null;
  fromStationName: string | null = null;
  toStationName: string | null = null;

  isLoading = signal(false);
  errorMessage = signal('');

  selectedTrain = signal<Train | null>(null);
  routeStops = signal<RouteStop[]>([]);
  isRouteLoading = signal(false);
  routeError = signal('');

  selectedCoachType = signal<CoachType | null>(null);
  availableSeats = signal<number | null>(null);
  isAvailabilityLoading = signal(false);
  availabilityError = signal('');

  trainSearchTerm = '';
  isTrainSearchLoading = signal(false);
  trainSearchError = signal('');
  isTrainSearchMode = signal(false);

  selectedFare = signal<number | null>(null);
  isFareLoading = signal(false);
  fareError = signal('');

  // ngOnInit(): void {
  //   this.route.queryParams.subscribe(params => {

  //     this.fromStationId = Number(params['fromStationId']);
  //     this.toStationId = Number(params['toStationId']);
  //     this.journeyDate = params['journeyDate'] ?? null;
  //     this.fromStationName = params['fromStationName'] ?? null;
  //     this.toStationName = params['toStationName'] ?? null;

  //     if (
  //       this.journeyDate &&
  //       this.journeyDate < this.getTodayDate()
  //     ) {
  //       this.errorMessage.set(
  //         'The journey date cannot be in the past.'
  //       );
  //       return;
  //     }

  //     if (
  //       !this.fromStationId ||
  //       !this.toStationId
  //     ) {
  //       this.errorMessage.set('Invalid train search parameters.');
  //       return;
  //     }

  //     this.loadTrains();
  //   });
  // }

  ngOnInit(): void {
    this.route.queryParams.subscribe(params => {

      this.fromStationId = Number(params['fromStationId']);
      this.toStationId = Number(params['toStationId']);
      this.journeyDate = params['journeyDate'] ?? null;
      this.fromStationName = params['fromStationName'] ?? null;
      this.toStationName = params['toStationName'] ?? null;

      if (
        this.journeyDate &&
        this.journeyDate < this.getTodayDate()
      ) {
        this.errorMessage.set(
          'The journey date cannot be in the past.'
        );
        return;
      }

      const hasJourneySearch =
        !!this.fromStationId &&
        !!this.toStationId;

      if (hasJourneySearch) {
        this.isTrainSearchMode.set(false);
        this.loadTrains();
      } else {
        this.isTrainSearchMode.set(true);
      }
    });
  }

  private getTodayDate(): string {
    const date = new Date();

    const year = date.getFullYear();
    const month = String(date.getMonth() + 1).padStart(2, '0');
    const day = String(date.getDate()).padStart(2, '0');

    return `${year}-${month}-${day}`;
  }

  viewTrain(train: Train): void {
    this.selectedTrain.set(train);
    this.routeStops.set([]);
    this.routeError.set('');
    this.isRouteLoading.set(true);

    this.trainService
      .getRoute(train.id)
      .subscribe({
        next: routeStops => {
          console.log('Route received:', routeStops);

          this.routeStops.set(routeStops);
          this.isRouteLoading.set(false);
        },
        error: error => {
          console.error('Route loading failed:', error);

          this.routeError.set(
            'Unable to load train route. Please try again.'
          );

          this.isRouteLoading.set(false);
        }
      });
  }

  searchTrainByNameOrNumber(): void {
    const searchTerm = this.trainSearchTerm.trim();

    if (!searchTerm) {
      this.trainSearchError.set(
        'Please enter a train number or train name.'
      );
      this.trains.set([]);
      return;
    }

    this.isTrainSearchLoading.set(true);
    this.trainSearchError.set('');
    this.trains.set([]);
    this.selectedTrain.set(null);
    this.routeStops.set([]);
    this.routeError.set('');

    this.trainService
      .searchTrainsByNameOrNumber(searchTerm)
      .subscribe({
        next: trains => {
          this.trains.set(trains);
          this.isTrainSearchLoading.set(false);

          if (trains.length === 0) {
            this.trainSearchError.set(
              'No trains found matching your search.'
            );
          }
        },
        error: error => {
          console.error('TRAIN SEARCH ERROR:', error);

          this.trainSearchError.set(
            'Unable to search trains. Please try again.'
          );

          this.isTrainSearchLoading.set(false);
        }
      });
  }

  checkAvailability(): void {
    if (
      this.selectedTrain() === null ||
      this.selectedCoachType() === null ||
      this.fromStationId === null ||
      this.toStationId === null ||
      this.journeyDate === null
    ) {
      return;
    }

    this.isAvailabilityLoading.set(true);
    this.availabilityError.set('');
    this.availableSeats.set(null);

    this.availabilityService
      .getAvailability(
        this.selectedTrain()!.id,
        this.fromStationId,
        this.toStationId,
        this.journeyDate,
        this.selectedCoachType()!
      )
      .subscribe({
        next: response => {
          console.log(
            'Availability received:',
            response
          );

          this.availableSeats.set(
            response.availableSeats
          );

          this.isAvailabilityLoading.set(false);
        },

        error: error => {
          console.error(
            'Availability check failed:',
            error
          );

          this.availabilityError.set(
            'Unable to check seat availability. Please try again.'
          );

          this.isAvailabilityLoading.set(false);
        }
      });
  }

  // selectCoachType(coachType: CoachType): void {
  //   this.selectedCoachType.set(coachType);

  //   // Clear previous availability when coach type changes.
  //   this.availableSeats.set(null);
  //   this.availabilityError.set('');
  // }

  selectCoachType(coachType: CoachType): void {
    this.selectedCoachType.set(coachType);

    // Clear previous availability and fare when coach type changes.
    this.availableSeats.set(null);
    this.availabilityError.set('');
    this.selectedFare.set(null);
    this.fareError.set('');

    if (
      this.fromStationId === null ||
      this.toStationId === null ||
      this.selectedTrain() === null
    ) {
      return;
    }

    this.isFareLoading.set(true);

    this.trainService
      .getFare(
        this.selectedTrain()!.id,
        this.fromStationId,
        this.toStationId,
        coachType
      )
      .subscribe({
        next: fare => {
          this.selectedFare.set(fare.amount);
          this.isFareLoading.set(false);
        },
        error: error => {
          console.error('Fare loading failed:', error);

          this.fareError.set(
            'Fare is currently unavailable for this coach.'
          );

          this.isFareLoading.set(false);
        }
      });
  }

  private loadTrains(): void {
    this.isLoading.set(true);
    this.errorMessage.set('');

    console.log('Loading trains...');

    this.trainService
      .searchTrains(
        this.fromStationId!,
        this.toStationId!
      )
      .subscribe({
        next: trains => {
          console.log('SUCCESS - trains received:', trains);

          this.trains.set(trains);
          this.isLoading.set(false);

          console.log('isLoading:', this.isLoading());
          console.log('this.trains:', this.trains());
        },
        error: error => {
          console.error('TRAIN SEARCH ERROR:', error);

          this.errorMessage.set(
            'Unable to load trains. Please try again.'
          );

          this.isLoading.set(false);
        }
      });
  }

  bookTicket(): void {
    this.navigateToBooking();
  }

  joinWaitingList(): void {
    this.navigateToBooking();
  }

  private navigateToBooking(): void {
    const train = this.selectedTrain();

    if (
      !train ||
      !this.fromStationId ||
      !this.toStationId ||
      !this.journeyDate ||
      this.selectedCoachType() === null
    ) {
      return;
    }

    this.router.navigate(['/booking'], {
      queryParams: {
        trainId: train.id,
        fromStationId: this.fromStationId,
        toStationId: this.toStationId,
        fromStationName: this.fromStationName,
        toStationName: this.toStationName,
        journeyDate: this.journeyDate,
        coachType: this.selectedCoachType()
      }
    });
  }
}