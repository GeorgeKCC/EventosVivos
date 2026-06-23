import { HttpInterceptorFn } from '@angular/common/http';

export const apiInterceptor: HttpInterceptorFn = (req, next) => {
  if (req.url.includes('/Auth/login')) {
    return next(req);
  }

  const token = localStorage.getItem('auth_token');
  const headers: Record<string, string> = {
    'X-Api-Key': 'EV-2024-SecureKey-a1b2c3d4e5f6'
  };

  if (token) {
    headers['Authorization'] = `Bearer ${token}`;
  }

  const cloned = req.clone({ setHeaders: headers });

  return next(cloned);
};
