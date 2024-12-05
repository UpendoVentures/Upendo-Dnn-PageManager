import { Component } from '@angular/core';
import { HttpErrorResponse } from '@angular/common/http';
import { Context, DemoService } from '@app/service';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.sass']
})
export class AppComponent {
  title = 'Page Manager';
  webapiResult = '';

  constructor(public context: Context, private _demoService: DemoService) {
    context.autoConfigure();
  }

  public getDataFromWebAPI() {
    this._demoService.getHelloWorld().subscribe(data => {
      this.webapiResult = data;
      console.log('​---------------------------------');
      console.log('Call webapi data -> data: ', data);
      console.log('​---------------------------------');
    },
      (err: HttpErrorResponse) => {
        if (err.error instanceof Error) {
          console.log('​---------------------------------');
          console.log('Call webapi error -> ERROR: ', err.error);
          console.log('​---------------------------------');
        } else {
          console.log('​---------------------------------');
          console.log('Call webapi error -> ERROR: ', err.error);
          console.log('​---------------------------------');
        }
      }
    );
  }

  log(par: any): string{
    return JSON.stringify(par).toString();
  }
}
