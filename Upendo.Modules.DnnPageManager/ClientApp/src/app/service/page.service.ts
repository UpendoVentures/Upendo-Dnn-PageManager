import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { HttpClient } from '@angular/common/http';
import { Context } from './DNN/context.service';
import { getApiName, getParameterName } from '@app/utility';
import { take } from 'rxjs/operators';

@Injectable({
  providedIn: 'root',
})
export class PageService {
  private _routingWebAPI: string;

  constructor(private context: Context, private http: HttpClient) {
    this._routingWebAPI = this.context._properties.routingWebAPI;
  }

  getPages(
    portalId: number,
    searchKey: string,
    pageIndex: number,
    pageSize: number,
    sortBy: string,
    sortType: string
  ): Observable<any> {
    const apiUrl = `Pages/GetPagesList?portalId=${portalId}&searchKey=${searchKey}&pageIndex=${pageIndex}&pageSize=${pageSize}&sortBy=${sortBy}&sortType=${sortType}`;

    return this.http.get(this._routingWebAPI + apiUrl).pipe(take(1));
  }

  updatePageIndexing(
    portalId: number,
    tabId: number,
    indexed: boolean
  ): Observable<any> {
    const apiUrl = `Pages/SetPageIndexed?portalId=${portalId}&tabId=${tabId}&indexed=${indexed}`;

    return this.http.post(this._routingWebAPI + apiUrl, null);
  }

  updatePageVisibility(
    portalId: number,
    tabId: number,
    visible: boolean
  ): Observable<any> {
    const apiUrl = `Pages/SetPageVisibility?portalId=${portalId}&tabId=${tabId}&visible=${visible}`;

    return this.http.post(this._routingWebAPI + apiUrl, null);
  }

  updatePageProperties(
    portalId: number,
    tabId: number,
    propertyName: string,
    propertyValue: string
  ): Observable<any> {
    const apiName = getApiName(propertyName);
    const prameterName = getParameterName(propertyName);

    const apiUrl = `Pages/${apiName}?portalId=${portalId}&tabId=${tabId}&${prameterName}=${propertyValue}`;

    return this.http.post(this._routingWebAPI + apiUrl, null);
  }

  getPermissions(portalId: number, tabId: number): Observable<any> {
    const apiUrl = `Pages/GetPagePermissions?portalId=${portalId}&tabId=${tabId}`;

    return this.http.get<any>(this._routingWebAPI + apiUrl);
  }

  getUrls(portalId: number, tabId: number): Observable<any> {
    const apiUrl = `Pages/GetPageUrls?portalId=${portalId}&tabId=${tabId}`;

    return this.http.get<any>(this._routingWebAPI + apiUrl);
  }

  getPageModules(portalId: number, tabId: number): Observable<any> {
    const apiUrl = `Pages/GetPageModules?portalId=${portalId}&tabId=${tabId}`;

    return this.http.get<any>(this._routingWebAPI + apiUrl);
  }
}
