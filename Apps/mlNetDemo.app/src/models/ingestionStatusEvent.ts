export class IngestionStatusEvent {
  constructor(
    public id: string,
    public fileName : string,
    public creationDate : string,
    public message? : string,
    public modelName? : string, 
    public detectionTime? : number,
  ) {}
}