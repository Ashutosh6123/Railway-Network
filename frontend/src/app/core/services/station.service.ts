import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable, map } from 'rxjs';
import { environment } from '../../../environments/environment';

export interface Station {
  id: number;
  code: string;
  name: string;
}

@Injectable({
  providedIn: 'root'
})
export class StationService {

  private readonly http = inject(HttpClient);

  private readonly apiUrl = `${environment.apiUrl}/api/stations`;

  searchStations(search: string): Observable<Station[]> {
    const params = new HttpParams()
      .set('search', search);

    return this.http.get<Station[]>(this.apiUrl, { params });
  }

  getStationById(stationId: number): Observable<Station | null> {
        return this.searchStations(stationId.toString()).pipe(
            map(stations =>
            stations.find(station => station.id === stationId) ?? null
            )
        );
    }
    getAllStations(): Observable<Station[]> {
        return this.http.get<Station[]>(`${this.apiUrl}`);
    }
}