import { HubConnection, HubConnectionBuilder, HubConnectionState, LogLevel } from '@microsoft/signalr';
import { BehaviorSubject, Observable, Subject } from 'rxjs';
import { RetryPolicy } from './retryPolicy';

export interface IHubClient {
  get isConnected(): boolean;
  get onConnectionStateChanged(): Observable<boolean>;
  get onConnectionEror(): Observable<Error>;

  startAaync(): Promise<void>;
  stopAsync(): Promise<void>;
}

export class HubClient implements IHubClient {
  private _connectionAttempts: number = 1;
  private _onConnectionEror: Subject<Error> = new Subject();
  private _onConnectionStateChanged: BehaviorSubject<boolean> = new BehaviorSubject(false);

  protected  _connection: HubConnection;

  public get isConnected(): boolean {
    return this._onConnectionStateChanged.value;
  }

  public get onConnectionStateChanged(): Observable<boolean> {
    return this._onConnectionStateChanged.asObservable();
  }

  public get onConnectionEror(): Observable<Error> {
    return this._onConnectionEror.asObservable();
  }

  constructor(url: string) {
    this._connection = new HubConnectionBuilder()
      .withUrl(url, {
        withCredentials: false,
      })
      .withAutomaticReconnect(new RetryPolicy())
      .configureLogging(LogLevel.Information)
      .build();

    this._connection.onreconnected(() => this._onConnectionStateChanged.next(true));
    this._connection.onreconnecting(() => this._onConnectionStateChanged.next(false));
    this._connection.onclose((error) => {
      if (error) {
        this._onConnectionEror.next(error);
      }
      this._onConnectionStateChanged.next(false);
    });
  }

  public async startAaync(): Promise<void> {
    const connectRetryMs = 1000;

    while (this._onConnectionStateChanged.value !== true) {
      try {
        await this._connection.start();
        this._onConnectionStateChanged.next(true);
        console.log('Hub Client connected successfully.');
        this._connectionAttempts = 1;
      } catch {
        console.log(`Hub Client failed to connect. Retrying - Attempt ${this._connectionAttempts++}`);
        this._onConnectionStateChanged.next(false);
        await this.sleep(connectRetryMs);
      }
    }
  }

  public async stopAsync(): Promise<void> {
    if (this._connection.state !== HubConnectionState.Disconnected) await this._connection.stop();
    this._onConnectionStateChanged.next(false);
  }

  public dispose(): void {
    this._connection.stop();
    this._onConnectionStateChanged.complete();
  }

  private sleep(ms: number): Promise<void> {
    return new Promise((resolve) => setTimeout(resolve, ms));
  }
}
