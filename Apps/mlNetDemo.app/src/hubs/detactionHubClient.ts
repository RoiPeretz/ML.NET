import { Observable, Subject } from 'rxjs';
import { HubClient, type IHubClient } from './hubClientBase';
import type { IngestionStatusEvent } from '@/models/ingestionStatusEvent';
import type { ImageDetectionResult } from '@/models/imageDetectionResult';

export interface IBffHubClient extends IHubClient {
  get ingestionStatusAdded(): Observable<IngestionStatusEvent>;
  getCurretStatus(): Promise<Record<string, IngestionStatusEvent[]>>;
  Query(searchTerm: string): Promise<ImageDetectionResult[]>;
  detect(fileName: string, fileData: ArrayBuffer, contentType: string): Promise<void>
 }

export class BffHubClient extends HubClient implements IBffHubClient {
  get ingestionStatusAdded(): Observable<IngestionStatusEvent> {
    return this._ingestionStatusAdded.asObservable();
  }  

  private _ingestionStatusAdded: Subject<IngestionStatusEvent> = new Subject();

  private readonly IngestionStatusAdded: string = 'IngestionStatusAdded';

  constructor() {
    const hupAddr = "http://127.0.0.1:5477";
    const url = `${hupAddr}/Detect`;
    super(url);

    this._connection.on(this.IngestionStatusAdded, (layer) => {
      this._ingestionStatusAdded.next(layer);
    });
   }

  async detect(fileName: string, fileData: ArrayBuffer, contentType: string): Promise<void> {
    const bytes = new Uint8Array(fileData);
    const base64String = btoa(String.fromCharCode(...bytes));

    await this._connection.invoke("Detect", fileName, base64String, contentType).catch((err) => {
      console.error(err);
    });
  }

  async Query(searchTerm: string): Promise<ImageDetectionResult[]> {
    return await this._connection.invoke("Query", searchTerm).catch((err) => {
      console.error(err);
    });
  }

  async getCurretStatus(): Promise<Record<string, IngestionStatusEvent[]>> {
    const result = await this._connection.invoke("GetCurrentStatus").catch((err) => {
      console.error(err);
    });

    return result;
  }

  public dispose(): void {
    this._ingestionStatusAdded.complete();
    super.dispose();
  }
}
