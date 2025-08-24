import { HttpInterceptor, HttpRequest, HttpHandler, HttpEvent } from '@angular/common/http';


export class AuthInterceptor implements HttpInterceptor {
  intercept(req: HttpRequest<any>, next: HttpHandler) {
    const token = sessionStorage.getItem('token');
    if (token) {
      const cloned = req.clone({
        setHeaders: { Authorization: `Bearer ${token}` }
      });
      return next.handle(cloned);
    }
    return next.handle(req);
  }
}