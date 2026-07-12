import { inject, Injectable } from '@angular/core';
import { HttpErrorResponse } from '@angular/common/http';
import { ProblemDetails } from '../models/problem-details.model';

/**
 * Extracts a user-friendly error message from an HttpErrorResponse.
 * Handles ProblemDetails responses from the backend API.
 */
@Injectable({ providedIn: 'root' })
export class ErrorService {
  extractMessage(error: HttpErrorResponse): string {
    const problem = error.error as ProblemDetails | undefined;

    if (problem?.detail) return problem.detail;
    if (problem?.title) return problem.title;

    return 'An unexpected error occurred. Please try again.';
  }

  extractFieldErrors(error: HttpErrorResponse): Record<string, string[]> {
    const problem = error.error as ProblemDetails | undefined;
    return problem?.errors ?? {};
  }
}
