import { HttpInterceptorFn, HttpRequest, HttpHandlerFn } from '@angular/common/http';

/**
 * Attaches the JWT access token from session storage to outgoing API requests.
 * Replace session storage with a proper token service when auth is implemented.
 */
export const authInterceptor: HttpInterceptorFn = (
  req: HttpRequest<unknown>,
  next: HttpHandlerFn
) => {
  const token = sessionStorage.getItem('access_token');

  if (token) {
    const authReq = req.clone({
      setHeaders: { Authorization: `Bearer ${token}` }
    });
    return next(authReq);
  }

  return next(req);
};
