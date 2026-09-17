import { Injectable, signal } from '@angular/core';

export interface GeoCoordinates {
  latitude: number;
  longitude: number;
  accuracy?: number;
}

@Injectable({
  providedIn: 'root'
})
export class GeolocationService {
  readonly currentCoords = signal<GeoCoordinates | null>(null);
  readonly isLocating = signal(false);
  readonly errorMessage = signal<string | null>(null);

  getCurrentPosition(): Promise<GeoCoordinates> {
    this.isLocating.set(true);
    this.errorMessage.set(null);

    return new Promise((resolve, reject) => {
      if (typeof navigator === 'undefined' || !navigator.geolocation) {
        const err = 'Geolocation is not supported by this browser.';
        this.errorMessage.set(err);
        this.isLocating.set(false);
        reject(err);
        return;
      }

      navigator.geolocation.getCurrentPosition(
        (pos) => {
          const coords: GeoCoordinates = {
            latitude: pos.coords.latitude,
            longitude: pos.coords.longitude,
            accuracy: pos.coords.accuracy
          };
          this.currentCoords.set(coords);
          this.isLocating.set(false);
          resolve(coords);
        },
        (error) => {
          this.errorMessage.set(error.message);
          this.isLocating.set(false);
          reject(error);
        },
        { enableHighAccuracy: true, timeout: 10000, maximumAge: 60000 }
      );
    });
  }
}
