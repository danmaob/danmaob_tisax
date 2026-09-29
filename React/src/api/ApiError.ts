export class ApiError extends Error {
  constructor(public readonly status: number, public readonly errorCode: string | null, message: string) {
    super(message);
    this.name = 'ApiError';
  }
}