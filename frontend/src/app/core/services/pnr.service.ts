import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';

export interface PnrStatusResponse {
  pnr: string;
  status: number;
  journeyDate: string;
}

@Injectable({
  providedIn: 'root'
})
export class PnrService {
  private readonly http = inject(HttpClient);
  private readonly apiUrl = `${environment.apiUrl}/api/pnr`;

  getPnrStatus(pnr: string): Observable<PnrStatusResponse> {
    return this.http.get<PnrStatusResponse>(
      `${this.apiUrl}/${pnr}`
    );
  }
}