import { Component, inject, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, NgForm } from '@angular/forms';

import {
  AdminService,
  Train,
  TrainAdminRequest,
  Station,
  StationAdminRequest,
  RouteStop,
  RouteStopRequest,
  RouteStopAdminRequest,
  Coach,
  CoachRequest,
  CoachAdminRequest,
  SeatRequest,
  Fare,
  FareAdminRequest
} from '../../core/services/admin.service';

export interface Seat {
  id: number;
  coachId: number;
  seatNumber: string;
}

@Component({
  selector: 'app-admin',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './admin.html',
  styleUrl: './admin.css'
})
export class Admin implements OnInit {

  private readonly adminService = inject(AdminService);

  readonly trains = signal<Train[]>([]);
  readonly isLoading = signal(false);
  readonly errorMessage = signal('');

  readonly showAddTrainForm = signal(false);
  readonly isAddingTrain = signal(false);
  readonly addTrainError = signal('');
  readonly editingTrainId = signal<number | null>(null);
  readonly isEditingTrain = signal(false);
  readonly editTrainError = signal('');
  readonly deletingTrainId = signal<number | null>(null);
  readonly deleteTrainError = signal('');

  readonly stations = signal<Station[]>([]);
  readonly isStationLoading = signal(false);
  readonly stationError = signal('');

  readonly showAddStationForm = signal(false);
  readonly isAddingStation = signal(false);
  readonly addStationError = signal('');

  readonly editingStationId = signal<number | null>(null);
  readonly isEditingStation = signal(false);
  readonly editStationError = signal('');
  readonly deletingStationId = signal<number | null>(null);
  readonly deleteStationError = signal('');

  readonly selectedRouteTrain = signal<Train | null>(null);
  readonly routeStops = signal<RouteStop[]>([]);
  readonly isRouteLoading = signal(false);
  readonly routeError = signal('');

  readonly showAddRouteStopForm = signal(false);
  readonly isAddingRouteStop = signal(false);
  readonly addRouteStopError = signal('');

  readonly editingRouteStopId = signal<number | null>(null);
  readonly isEditingRouteStop = signal(false);
  readonly editRouteStopError = signal('');

  readonly deletingRouteStopId = signal<number | null>(null);
  readonly deleteRouteStopError = signal('');

  readonly selectedCoachTrain = signal<Train | null>(null);
  readonly coaches = signal<Coach[]>([]);
  readonly isCoachLoading = signal(false);
  readonly coachError = signal('');
  readonly editingCoachId = signal<number | null>(null);
  readonly isUpdatingCoach = signal(false);
  readonly updateCoachError = signal('');

  readonly deletingCoachId = signal<number | null>(null);
  readonly deleteCoachError = signal('');

  readonly showAddCoachForm = signal(false);
  readonly isAddingCoach = signal(false);
  readonly addCoachError = signal('');

  readonly selectedCoach = signal<Coach | null>(null);
  readonly seats = signal<Seat[]>([]);
  readonly isSeatLoading = signal(false);
  readonly seatError = signal('');
  readonly showAddSeatForm = signal(false);
  readonly isAddingSeat = signal(false);
  readonly addSeatError = signal('');
  readonly editingSeatId = signal<number | null>(null);
  readonly isUpdatingSeat = signal(false);
  readonly updateSeatError = signal('');
  readonly deletingSeatId = signal<number | null>(null);
  readonly deleteSeatError = signal('');

  readonly selectedFareTrain = signal<Train | null>(null);
  readonly fares = signal<Fare[]>([]);
  readonly isFareLoading = signal(false);
  readonly fareError = signal('');
  readonly showFareForm = signal(false);
  readonly isSavingFare = signal(false);
  readonly fareFormError = signal('');
  readonly editingFareId = signal<number | null>(null);
  readonly deletingFareId = signal<number | null>(null);
  readonly deleteFareError = signal('');
  
  fareFromStationId: number | null = null;
  fareToStationId: number | null = null;
  fareCoachType: number | null = null;
  fareAmount: number | null = null;
  
  editRouteStopStationId: number | null = null;
  editRouteStopOrder = 1;
  editRouteStopArrivalTime = '';
  editRouteStopDepartureTime = '';
  
  routeStopStationId: number | null = null;
  routeStopOrder = 1;
  routeStopArrivalTime = '';
  routeStopDepartureTime = '';
  
  
  editStationCode = '';
  editStationName = '';
  
  stationCode = '';
  stationName = '';

  editTrainNumber = '';
  editTrainName = '';

  trainNumber = '';
  trainName = '';

  ngOnInit(): void {
    this.loadTrains();
    this.loadStations();
  }

  loadTrains(): void {
    this.isLoading.set(true);
    this.errorMessage.set('');

    this.adminService.getTrains().subscribe({
      next: trains => {
        this.trains.set(trains);
        this.isLoading.set(false);
      },
      error: error => {
        console.error('ADMIN TRAIN LOAD ERROR:', error);
        this.errorMessage.set('Unable to load trains. Please try again.');
        this.isLoading.set(false);
      }
    });
  }

  openAddTrainForm(): void {
    this.trainNumber = '';
    this.trainName = '';
    this.addTrainError.set('');
    this.showAddTrainForm.set(true);
  }

  closeAddTrainForm(): void {
    if (this.isAddingTrain()) {
      return;
    }

    this.showAddTrainForm.set(false);
    this.addTrainError.set('');
  }

  addTrain(): void {
    const trainNumber = this.trainNumber.trim();
    const trainName = this.trainName.trim();

    if (!trainNumber || !trainName) {
      this.addTrainError.set('Please enter both train number and train name.');
      return;
    }

    const request: TrainAdminRequest = {
      trainNumber,
      name: trainName
    };

    this.isAddingTrain.set(true);
    this.addTrainError.set('');

    this.adminService.createTrain(request).subscribe({
      next: () => {
        this.isAddingTrain.set(false);
        this.showAddTrainForm.set(false);

        this.trainNumber = '';
        this.trainName = '';

        this.loadTrains();
      },
      error: error => {
        console.error('CREATE TRAIN ERROR:', error);

        this.isAddingTrain.set(false);

        this.addTrainError.set(
          error?.error?.message ||
          'Unable to create the train. Please try again.'
        );
      }
    });
  }

  startEditTrain(train: Train): void {
    this.editingTrainId.set(train.id);
    this.editTrainNumber = train.trainNumber;
    this.editTrainName = train.name;
    this.editTrainError.set('');
  }

  cancelEditTrain(): void {
    if (this.isEditingTrain()) {
        return;
    }

    this.editingTrainId.set(null);
    this.editTrainNumber = '';
    this.editTrainName = '';
    this.editTrainError.set('');
  }

  updateTrain(): void {
    const trainId = this.editingTrainId();
    const trainNumber = this.editTrainNumber.trim();
    const trainName = this.editTrainName.trim();

    if (trainId === null) {
        return;
    }

    if (!trainNumber || !trainName) {
        this.editTrainError.set(
        'Please enter both train number and train name.'
        );
        return;
    }

    const request: TrainAdminRequest = {
        trainNumber,
        name: trainName
    };

    this.isEditingTrain.set(true);
    this.editTrainError.set('');

    this.adminService.updateTrain(trainId, request).subscribe({
        next: () => {
        this.isEditingTrain.set(false);
        this.editingTrainId.set(null);

        this.editTrainNumber = '';
        this.editTrainName = '';

        this.loadTrains();
        },
        error: error => {
        console.error('UPDATE TRAIN ERROR:', error);

        this.isEditingTrain.set(false);

        this.editTrainError.set(
            error?.error?.message ||
            'Unable to update the train. Please try again.'
        );
        }
    });
    }
    deleteTrain(train: Train): void {
    const confirmed = window.confirm(
        `Are you sure you want to delete train ${train.trainNumber} - ${train.name}?`
    );

    if (!confirmed) {
        return;
    }

    this.deletingTrainId.set(train.id);
    this.deleteTrainError.set('');

    this.adminService.deleteTrain(train.id).subscribe({
        next: () => {
        this.deletingTrainId.set(null);

        this.trains.update(trains =>
            trains.filter(existingTrain => existingTrain.id !== train.id)
        );
        },
        error: error => {
        console.error('DELETE TRAIN ERROR:', error);

        this.deletingTrainId.set(null);

        this.deleteTrainError.set(
            error?.error?.message ||
            'Unable to delete the train. Please try again.'
        );
        }
    });
    }

    loadStations(): void {
        this.isStationLoading.set(true);
        this.stationError.set('');

        this.adminService.getStations().subscribe({
            next: stations => {
            this.stations.set(stations);
            this.isStationLoading.set(false);
            },
            error: error => {
            console.error('ADMIN STATION LOAD ERROR:', error);

            this.stationError.set(
                'Unable to load stations. Please try again.'
            );

            this.isStationLoading.set(false);
            }
        });
    }
    openAddStationForm(): void {
        this.editingStationId.set(null);
        this.editStationError.set('');

        this.stationCode = '';
        this.stationName = '';
        this.addStationError.set('');
        this.showAddStationForm.set(true);
    }
    closeAddStationForm(): void {
        if (this.isAddingStation()) {
            return;
        }

        this.showAddStationForm.set(false);
        this.addStationError.set('');
    }
    addStation(): void {
    const stationCode = this.stationCode.trim();
    const stationName = this.stationName.trim();

    if (!stationCode || !stationName) {
        this.addStationError.set(
        'Please enter both station code and station name.'
        );
        return;
    }

    const request: StationAdminRequest = {
        code: stationCode,
        name: stationName
    };

    this.isAddingStation.set(true);
    this.addStationError.set('');

    this.adminService.createStation(request).subscribe({
        next: station => {
        this.isAddingStation.set(false);
        this.showAddStationForm.set(false);

        this.stationCode = '';
        this.stationName = '';

        this.stations.update(stations => [
            ...stations,
            station
        ]);
        },
        error: error => {
        console.error('CREATE STATION ERROR:', error);

        this.isAddingStation.set(false);

        this.addStationError.set(
            error?.error?.message ||
            'Unable to create the station. Please try again.'
        );
        }
    });
    }
    startEditStation(station: Station): void {
        this.showAddStationForm.set(false);
        this.addStationError.set('');

        this.editingStationId.set(station.id);
        this.editStationCode = station.code;
        this.editStationName = station.name;
        this.editStationError.set('');
    }
    cancelEditStation(): void {
        if (this.isEditingStation()) {
            return;
        }

        this.editingStationId.set(null);
        this.editStationCode = '';
        this.editStationName = '';
        this.editStationError.set('');
    }
    updateStation(): void {
        const stationId = this.editingStationId();
        const stationCode = this.editStationCode.trim();
        const stationName = this.editStationName.trim();

        if (stationId === null) {
            return;
        }

        if (!stationCode || !stationName) {
            this.editStationError.set(
            'Please enter both station code and station name.'
            );
            return;
        }

        const request: StationAdminRequest = {
            code: stationCode,
            name: stationName
        };

        this.isEditingStation.set(true);
        this.editStationError.set('');

        this.adminService.updateStation(stationId, request).subscribe({
            next: updatedStation => {
            this.isEditingStation.set(false);
            this.editingStationId.set(null);

            this.editStationCode = '';
            this.editStationName = '';

            this.stations.update(stations =>
                stations.map(station =>
                station.id === updatedStation.id
                    ? updatedStation
                    : station
                )
            );
            },
            error: error => {
            console.error('UPDATE STATION ERROR:', error);

            this.isEditingStation.set(false);

            this.editStationError.set(
                error?.error?.message ||
                'Unable to update the station. Please try again.'
            );
            }
        });
    }
    deleteStation(station: Station): void {
        const confirmed = window.confirm(
            `Are you sure you want to delete station ${station.code} - ${station.name}?`
        );

        if (!confirmed) {
            return;
        }

        this.deletingStationId.set(station.id);
        this.deleteStationError.set('');

        this.adminService.deleteStation(station.id).subscribe({
            next: () => {
            this.deletingStationId.set(null);

            this.stations.update(stations =>
                stations.filter(existingStation =>
                existingStation.id !== station.id
                )
            );
            },
            error: error => {
            console.error('DELETE STATION ERROR:', error);

            this.deletingStationId.set(null);

            this.deleteStationError.set(
                error?.error?.message ||
                'Unable to delete the station. Please try again.'
            );
            }
        });
    }
    viewRoute(train: Train): void {
        this.selectedRouteTrain.set(train);
        this.routeStops.set([]);
        this.routeError.set('');
        this.isRouteLoading.set(true);

        this.adminService.getRouteStops(train.id).subscribe({
            next: routeStops => {
            this.routeStops.set(
                [...routeStops].sort((a, b) => a.stopOrder - b.stopOrder)
            );

            this.isRouteLoading.set(false);
            },
            error: error => {
            console.error('LOAD ROUTE STOPS ERROR:', error);

            this.routeError.set(
                error?.error?.message ||
                'Unable to load the route. Please try again.'
            );

            this.isRouteLoading.set(false);
            }
        });
    }
    closeRoute(): void {
        this.selectedRouteTrain.set(null);
        this.routeStops.set([]);
        this.routeError.set('');
    }
    openAddRouteStopForm(): void {
        this.editingRouteStopId.set(null);
        this.editRouteStopError.set('');

        this.routeStopStationId = null;
        this.routeStopOrder = this.routeStops().length + 1;
        this.routeStopArrivalTime = '';
        this.routeStopDepartureTime = '';

        this.addRouteStopError.set('');
        this.showAddRouteStopForm.set(true);
    }
    closeAddRouteStopForm(): void {
        if (this.isAddingRouteStop()) {
            return;
        }

        this.showAddRouteStopForm.set(false);
        this.addRouteStopError.set('');
    }
    addRouteStop(): void {
        const train = this.selectedRouteTrain();

        if (!train) {
            return;
        }

        if (
            this.routeStopStationId === null ||
            !this.routeStopArrivalTime ||
            !this.routeStopDepartureTime
        ) {
            this.addRouteStopError.set(
            'Please provide the station, arrival time and departure time.'
            );
            return;
        }

        if (this.routeStopOrder < 1) {
            this.addRouteStopError.set(
            'Stop order must be at least 1.'
            );
            return;
        }

        const request: RouteStopRequest = {
            stationId: this.routeStopStationId,
            stopOrder: this.routeStopOrder,
            arrivalTime: this.routeStopArrivalTime,
            departureTime: this.routeStopDepartureTime
        };

        this.isAddingRouteStop.set(true);
        this.addRouteStopError.set('');

        this.adminService.createRouteStop(train.id, request).subscribe({
            next: routeStop => {
            this.isAddingRouteStop.set(false);
            this.showAddRouteStopForm.set(false);

            this.routeStops.update(stops =>
                [...stops, routeStop].sort(
                (a, b) => a.stopOrder - b.stopOrder
                )
            );

            this.routeStopStationId = null;
            this.routeStopOrder = 1;
            this.routeStopArrivalTime = '';
            this.routeStopDepartureTime = '';
            },

            error: error => {
            console.error('CREATE ROUTE STOP ERROR:', error);

            this.isAddingRouteStop.set(false);

            this.addRouteStopError.set(
                error?.error?.message ||
                'Unable to add the route stop. Please try again.'
            );
            }
        });
    }
    startEditRouteStop(stop: RouteStop): void {
        this.showAddRouteStopForm.set(false);
        this.addRouteStopError.set('');

        this.editingRouteStopId.set(stop.id);
        this.editRouteStopStationId = stop.stationId;
        this.editRouteStopOrder = stop.stopOrder;
        this.editRouteStopArrivalTime = this.formatTimeForInput(
            stop.arrivalTime
        );
        this.editRouteStopDepartureTime = this.formatTimeForInput(
            stop.departureTime
        );

        this.editRouteStopError.set('');
    }
    private formatTimeForInput(time: string): string {
        return time.substring(0, 5);
    }
    cancelEditRouteStop(): void {
        if (this.isEditingRouteStop()) {
            return;
        }

        this.editingRouteStopId.set(null);
        this.editRouteStopStationId = null;
        this.editRouteStopOrder = 1;
        this.editRouteStopArrivalTime = '';
        this.editRouteStopDepartureTime = '';
        this.editRouteStopError.set('');
    }
    updateRouteStop(): void {
        const routeStopId = this.editingRouteStopId();

        if (routeStopId === null) {
            return;
        }

        if (
            this.editRouteStopStationId === null ||
            !this.editRouteStopArrivalTime ||
            !this.editRouteStopDepartureTime
        ) {
            this.editRouteStopError.set(
            'Please provide the station, arrival time and departure time.'
            );
            return;
        }

        if (this.editRouteStopOrder < 1) {
            this.editRouteStopError.set(
            'Stop order must be at least 1.'
            );
            return;
        }

        const train = this.selectedRouteTrain();

            if (!train) {
            return;
            }

            const request: RouteStopAdminRequest = {
            trainId: train.id,
            stationId: this.editRouteStopStationId,
            stopOrder: this.editRouteStopOrder,
            arrivalTime: this.editRouteStopArrivalTime,
            departureTime: this.editRouteStopDepartureTime
        };

        this.isEditingRouteStop.set(true);
        this.editRouteStopError.set('');

        this.adminService.updateRouteStop(
            routeStopId,
            request
        ).subscribe({
            next: updatedStop => {
            this.isEditingRouteStop.set(false);
            this.editingRouteStopId.set(null);

            this.routeStops.update(stops =>
                stops
                .map(stop =>
                    stop.id === updatedStop.id
                    ? updatedStop
                    : stop
                )
                .sort((a, b) => a.stopOrder - b.stopOrder)
            );

            this.editRouteStopStationId = null;
            this.editRouteStopOrder = 1;
            this.editRouteStopArrivalTime = '';
            this.editRouteStopDepartureTime = '';
            },

            error: error => {
            console.error('UPDATE ROUTE STOP ERROR:', error);

            this.isEditingRouteStop.set(false);

            this.editRouteStopError.set(
                error?.error?.message ||
                'Unable to update the route stop. Please try again.'
            );
            }
        });
    }
    deleteRouteStop(stop: RouteStop): void {
        const confirmed = window.confirm(
            `Are you sure you want to delete the route stop at ${stop.stationName}?`
        );

        if (!confirmed) {
            return;
        }

        this.deletingRouteStopId.set(stop.id);
        this.deleteRouteStopError.set('');

        this.adminService.deleteRouteStop(stop.id).subscribe({
            next: () => {
            this.routeStops.update(stops =>
                stops.filter(routeStop => routeStop.id !== stop.id)
            );

            this.deletingRouteStopId.set(null);
            },

            error: error => {
            console.error('DELETE ROUTE STOP ERROR:', error);

            this.deletingRouteStopId.set(null);

            this.deleteRouteStopError.set(
                error?.error?.message ||
                'Unable to delete the route stop. Please try again.'
            );
            }
        });
    }

    viewCoaches(train: Train) {
        this.selectedCoachTrain.set(train);
        this.coaches.set([]);
        this.coachError.set('');
        this.isCoachLoading.set(true);

        this.adminService.getCoaches(train.id).subscribe({
            next: coaches => {
            this.coaches.set(coaches);
            this.isCoachLoading.set(false);
            },
            error: () => {
            this.coachError.set('Failed to load coaches.');
            this.isCoachLoading.set(false);
            }
        });
    }
    closeCoaches() {
        this.selectedCoachTrain.set(null);
        this.coaches.set([]);
        this.coachError.set('');
    }
    openAddCoachForm() {
    this.showAddCoachForm.set(true);
    this.addCoachError.set('');
    }

    closeAddCoachForm() {
    this.showAddCoachForm.set(false);
    this.addCoachError.set('');
    }
    addCoach(form: NgForm) {
    const train = this.selectedCoachTrain();

    if (!train) {
        return;
    }

    if (form.invalid) {
        form.control.markAllAsTouched();
        return;
    }

    this.isAddingCoach.set(true);
    this.addCoachError.set('');

    const request: CoachRequest = {
        coachNumber: form.value.coachNumber.trim(),
        coachType: Number(form.value.coachType)
    };

    this.adminService.createCoach(train.id, request).subscribe({
        next: () => {
        this.adminService.getCoaches(train.id).subscribe({
            next: coaches => {
            this.coaches.set(coaches);
            this.showAddCoachForm.set(false);
            this.isAddingCoach.set(false);
            form.resetForm();
            },
            error: () => {
            this.isAddingCoach.set(false);
            this.addCoachError.set(
                'Coach was added, but the coach list could not be refreshed.'
            );
            }
        });
        },
        error: error => {
        this.isAddingCoach.set(false);

        this.addCoachError.set(
            error?.error?.message || 'Failed to add coach.'
        );
        }
    });
    }
    getCoachTypeName(coachType: number): string {
        const coachTypes = [
            'General',
            'Sleeper',
            'AC3Tier',
            'AC2Tier',
            'AC1Tier'
        ];

        return coachTypes[coachType] ?? 'Unknown';
    }
    startEditCoach(coach: Coach) {
    this.editingCoachId.set(coach.id);
    this.updateCoachError.set('');
    }
    cancelEditCoach() {
    this.editingCoachId.set(null);
    this.updateCoachError.set('');
    }
    updateCoach(coach: Coach, form: NgForm) {
    const train = this.selectedCoachTrain();

    if (!train) {
        return;
    }

    if (form.invalid) {
        form.control.markAllAsTouched();
        return;
    }

    this.isUpdatingCoach.set(true);
    this.updateCoachError.set('');

    const request: CoachAdminRequest = {
        trainId: train.id,
        coachNumber: form.value.coachNumber.trim(),
        coachType: Number(form.value.coachType)
    };

    this.adminService.updateCoach(coach.id, request).subscribe({
        next: () => {
        this.adminService.getCoaches(train.id).subscribe({
            next: coaches => {
            this.coaches.set(coaches);
            this.editingCoachId.set(null);
            this.isUpdatingCoach.set(false);
            },
            error: () => {
            this.isUpdatingCoach.set(false);
            this.updateCoachError.set(
                'Coach was updated, but the coach list could not be refreshed.'
            );
            }
        });
        },
        error: error => {
        this.isUpdatingCoach.set(false);
        this.updateCoachError.set(
            error?.error?.message || 'Failed to update coach.'
        );
        }
    });
    }
    deleteCoach(coach: Coach) {
    const confirmed = window.confirm(
        `Are you sure you want to delete coach ${coach.coachNumber}?`
    );

    if (!confirmed) {
        return;
    }

    this.deletingCoachId.set(coach.id);
    this.deleteCoachError.set('');

    this.adminService.deleteCoach(coach.id).subscribe({
        next: () => {
        this.coaches.update(coaches =>
            coaches.filter(c => c.id !== coach.id)
        );

        this.deletingCoachId.set(null);
        },
        error: error => {
        this.deletingCoachId.set(null);

        this.deleteCoachError.set(
            error?.error?.message || 'Failed to delete coach.'
        );
        }
    });
    }

    viewSeats(coach: Coach) {
        this.selectedCoach.set(coach);
        this.seats.set([]);
        this.seatError.set('');
        this.isSeatLoading.set(true);

        this.adminService.getSeats(coach.id).subscribe({
            next: seats => {
            this.seats.set(seats);
            this.isSeatLoading.set(false);
            },
            error: () => {
            this.seatError.set('Failed to load seats.');
            this.isSeatLoading.set(false);
            }
        });
    }
    closeSeats() {
        this.selectedCoach.set(null);
        this.seats.set([]);
        this.seatError.set('');
    }
    openAddSeatForm() {
    this.showAddSeatForm.set(true);
    this.addSeatError.set('');
    }
    closeAddSeatForm() {
    this.showAddSeatForm.set(false);
    this.addSeatError.set('');
    }

    addSeat(form: NgForm) {
        const coach = this.selectedCoach();

        if (!coach) {
            return;
        }

        if (form.invalid) {
            form.control.markAllAsTouched();
            return;
        }

        this.isAddingSeat.set(true);
        this.addSeatError.set('');

        const request: SeatRequest = {
            seatNumber: form.value.seatNumber.trim()
        };

        this.adminService.createSeat(coach.id, request).subscribe({
            next: () => {
            this.adminService.getSeats(coach.id).subscribe({
                next: seats => {
                this.seats.set(seats);
                this.showAddSeatForm.set(false);
                this.isAddingSeat.set(false);
                form.resetForm();
                },
                error: () => {
                this.isAddingSeat.set(false);
                this.addSeatError.set(
                    'Seat was added, but the seat list could not be refreshed.'
                );
                }
            });
            },
            error: error => {
            this.isAddingSeat.set(false);

            this.addSeatError.set(
                error?.error?.message || 'Failed to add seat.'
            );
            }
        });
    }

    startEditSeat(seat: Seat) {
        this.editingSeatId.set(seat.id);
        this.updateSeatError.set('');
    }
    cancelEditSeat() {
        this.editingSeatId.set(null);
        this.updateSeatError.set('');
    }

    updateSeat(seat: Seat, form: NgForm) {
        const coach = this.selectedCoach();

        if (!coach) {
            return;
        }

        if (form.invalid) {
            form.control.markAllAsTouched();
            return;
        }

        this.isUpdatingSeat.set(true);
        this.updateSeatError.set('');

        const request: SeatRequest = {
            seatNumber: form.value.seatNumber.trim()
        };

        this.adminService.updateSeat(seat.id, request).subscribe({
            next: () => {
            this.adminService.getSeats(coach.id).subscribe({
                next: seats => {
                this.seats.set(seats);
                this.editingSeatId.set(null);
                this.isUpdatingSeat.set(false);
                },
                error: () => {
                this.isUpdatingSeat.set(false);
                this.updateSeatError.set(
                    'Seat was updated, but the seat list could not be refreshed.'
                );
                }
            });
            },
            error: error => {
            this.isUpdatingSeat.set(false);

            this.updateSeatError.set(
                error?.error?.message || 'Failed to update seat.'
            );
            }
        });
    }
    deleteSeat(seat: Seat) {
        const confirmed = window.confirm(
            `Are you sure you want to delete seat ${seat.seatNumber}?`
        );

        if (!confirmed) {
            return;
        }

        this.deletingSeatId.set(seat.id);
        this.deleteSeatError.set('');

        this.adminService.deleteSeat(seat.id).subscribe({
            next: () => {
            this.seats.update(seats =>
                seats.filter(s => s.id !== seat.id)
            );

            this.deletingSeatId.set(null);
            },
            error: error => {
            this.deletingSeatId.set(null);

            this.deleteSeatError.set(
                error?.error?.message || 'Failed to delete seat.'
            );
            }
        });
    }

    viewFares(train: Train): void {
    this.selectedFareTrain.set(train);
    this.fares.set([]);
    this.fareError.set('');
    this.showFareForm.set(false);
    this.loadFares(train.id);
    }

    loadFares(trainId: number): void {
    this.isFareLoading.set(true);
    this.fareError.set('');

    this.adminService.getFares(trainId).subscribe({
        next: fares => {
        this.fares.set(fares);
        this.isFareLoading.set(false);
        },
        error: error => {
        console.error('LOAD FARES ERROR:', error);
        this.fareError.set(
            error?.error?.message || 'Failed to load fares.'
        );
        this.isFareLoading.set(false);
        }
    });
    }

    closeFares(): void {
    if (this.isSavingFare()) {
        return;
    }

    this.selectedFareTrain.set(null);
    this.fares.set([]);
    this.showFareForm.set(false);
    this.editingFareId.set(null);
    this.fareError.set('');
    this.fareFormError.set('');
    this.deleteFareError.set('');
    }

    openAddFareForm(): void {
    this.editingFareId.set(null);
    this.fareFromStationId = null;
    this.fareToStationId = null;
    this.fareCoachType = null;
    this.fareAmount = null;
    this.fareFormError.set('');
    this.showFareForm.set(true);
    }

    startEditFare(fare: Fare): void {
        console.log('Edit clicked:', fare);
        this.editingFareId.set(fare.id);
        this.fareFromStationId = fare.fromStationId;
        this.fareToStationId = fare.toStationId;
        this.fareCoachType = fare.coachType;
        this.fareAmount = fare.amount;
        this.fareFormError.set('');
        this.showFareForm.set(true);
    }

    cancelFareForm(): void {
    if (this.isSavingFare()) {
        return;
    }

    this.showFareForm.set(false);
    this.editingFareId.set(null);
    this.fareFormError.set('');
    }

    saveFare(): void {
    const train = this.selectedFareTrain();

    if (!train) {
        return;
    }

    if (
        this.fareFromStationId === null ||
        this.fareToStationId === null ||
        this.fareCoachType === null ||
        this.fareAmount === null ||
        !Number.isFinite(this.fareAmount) ||
        this.fareAmount <= 0
    ) {
        this.fareFormError.set(
        'Select both stations and a coach type, and enter a positive fare amount.'
        );
        return;
    }

    if (this.fareFromStationId === this.fareToStationId) {
        this.fareFormError.set(
        'The source and destination stations must be different.'
        );
        return;
    }

    const request: FareAdminRequest = {
        trainId: train.id,
        fromStationId: this.fareFromStationId,
        toStationId: this.fareToStationId,
        coachType: this.fareCoachType,
        amount: this.fareAmount
    };

    this.isSavingFare.set(true);
    this.fareFormError.set('');

    const fareId = this.editingFareId();
    const operation = fareId === null
        ? this.adminService.createFare(request)
        : this.adminService.updateFare(fareId, request);

    operation.subscribe({
        next: () => {
        this.isSavingFare.set(false);
        this.showFareForm.set(false);
        this.editingFareId.set(null);
        this.loadFares(train.id);
        },
        error: error => {
        console.error('SAVE FARE ERROR:', error);
        this.isSavingFare.set(false);
        this.fareFormError.set(
            error?.error?.message || 'Failed to save the fare.'
        );
        }
    });
    }

    deleteFare(fare: Fare): void {
    const confirmed = window.confirm(
        `Delete this fare of ₹${fare.amount}?`
    );

    if (!confirmed) {
        return;
    }

    this.deletingFareId.set(fare.id);
    this.deleteFareError.set('');

    this.adminService.deleteFare(fare.id).subscribe({
        next: () => {
        this.fares.update(fares =>
            fares.filter(existing => existing.id !== fare.id)
        );
        this.deletingFareId.set(null);
        },
        error: error => {
        console.error('DELETE FARE ERROR:', error);
        this.deletingFareId.set(null);
        this.deleteFareError.set(
            error?.error?.message || 'Failed to delete the fare.'
        );
        }
    });
    }

    getStationLabel(stationId: number): string {
    const station = this.stations().find(item => item.id === stationId);
    return station
        ? `${station.code} — ${station.name}`
        : `Station #${stationId}`;
    }

    getFareCoachTypeName(coachType: number): string {
    const coachTypes = [
        'General',
        'Sleeper',
        'AC 3 Tier',
        'AC 2 Tier',
        'AC 1 Tier'
    ];

    return coachTypes[coachType] ?? 'Unknown';
    }
}