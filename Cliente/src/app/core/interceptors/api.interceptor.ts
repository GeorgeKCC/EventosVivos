import { HttpInterceptorFn } from '@angular/common/http';

export const apiInterceptor: HttpInterceptorFn = (req, next) => {
  if (req.url.includes('/Auth/login')) {
    return next(req);
  }

  const cloned = req.clone({
    setHeaders: {
      'X-Api-Key': 'EV-2024-SecureKey-a1b2c3d4e5f6'
    }
  });

  return next(cloned);
};
