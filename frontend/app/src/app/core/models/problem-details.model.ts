/**
 * Matches the RFC 9457 ProblemDetails shape returned by the backend.
 */
export interface ProblemDetails {
  type?: string;
  title: string;
  status: number;
  detail?: string;
  instance?: string;
  traceId?: string;
  errors?: Record<string, string[]>;
}
