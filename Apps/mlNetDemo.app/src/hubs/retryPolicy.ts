import type { IRetryPolicy, RetryContext } from '@microsoft/signalr';

export class RetryPolicy implements IRetryPolicy {
  nextRetryDelayInMilliseconds(retryContext: RetryContext): number | null {
    const count = retryContext.previousRetryCount;
    switch (true) {
      case count < 10:
        return 1000;
      case count < 50:
        return 1000 * 30;
      default:
        return 1000 * 60;
    }
  }
}
