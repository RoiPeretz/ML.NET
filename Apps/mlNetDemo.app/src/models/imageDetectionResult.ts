export interface DetectedObject {
  label: string;
  color: string;
  additionalInfo: string;
}

export interface ImageDetectionResult {
  fileName: string;
  detectionTimeMilliseconds: number;
  detectionSource: string;
  detectedObjects?: DetectedObject[];
}
