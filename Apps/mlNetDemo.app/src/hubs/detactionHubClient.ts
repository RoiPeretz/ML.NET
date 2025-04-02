import { BehaviorSubject, Observable } from 'rxjs';
import { HubClient, type IHubClient } from './hubClientBase';

export interface IDetactionHubClient extends IHubClient {
  detect(fileName: string, fileData: ArrayBuffer, contentType: string): void
 }

export class DetactionHubClient extends HubClient implements IDetactionHubClient {
  private _stationsChanged: BehaviorSubject<number[]> = new BehaviorSubject(new Array<number>());

  constructor() {
    const hupAddr = "http://127.0.0.1:5477";
    const url = `${hupAddr}/Detect`;

    super(url);
   }

  async detect(fileName: string, fileData: ArrayBuffer, contentType: string): Promise<void> {
    const bytes = new Uint8Array(fileData);
    const base64String = btoa(String.fromCharCode(...bytes));

    await this._connection.invoke("Detect", fileName, base64String, contentType).catch((err) => {
      console.error(err);
    });
  }

  public dispose(): void {
    this._stationsChanged.complete();
    super.dispose();
  }
}
