import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { HttpClient } from '@angular/common/http';
import { Context } from './DNN/context.service';

@Injectable({
  providedIn: 'root'
})
export class DemoService {
  private _routingWebAPI: string;
  private demorouting: string;

  constructor(private context: Context, private http: HttpClient) {
    this._routingWebAPI = this.context._properties.routingWebAPI;
  }

  public getHelloWorld(): Observable<any> {
    let webAPIName = "Test/HelloWorld";
    let getUrl = this._routingWebAPI + webAPIName;
    console.log('​---------------------------------');
    console.log('​Service -> getUrl', getUrl);
    console.log('​---------------------------------');
    return this.http.get<any>(getUrl);
  }
}