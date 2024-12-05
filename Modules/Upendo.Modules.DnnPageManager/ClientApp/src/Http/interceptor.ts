import { Injectable } from '@angular/core';
import {
  HttpEvent,
  HttpInterceptor,
  HttpHandler,
  HttpRequest,
} from '@angular/common/http';
import { Observable, Subject } from 'rxjs';
import { Context } from '@app/service';
import { tap } from 'rxjs/operators';

@Injectable()
export class Interceptor implements HttpInterceptor {
  private tabId: number;
  private antiForgeryToken: string;

  constructor(private context: Context) {
    context.autoConfigure();
    context.all$
      .pipe(
        tap((x) => {
          this.tabId = x.tabId;
          this.antiForgeryToken = x.antiForgeryToken;
        })
      )
      .subscribe();
  }

  intercept(
    req: HttpRequest<any>,
    next: HttpHandler
  ): Observable<HttpEvent<any>> {
    const newReq = req.clone({
      setHeaders: {
        ModuleId: this.context._moduleId?.toString(),
        TabId: this.tabId?.toString(),
        UserId: this.context._userId?.toString(),
        RequestVerificationToken: this.antiForgeryToken,
      },
    });

    return next.handle(newReq);
  }
}
