import {
  Component,
  EventEmitter,
  Inject,
  OnDestroy,
  OnInit,
} from '@angular/core';
import { MatDialogRef, MAT_DIALOG_DATA } from '@angular/material/dialog';
import { PageUrlModel } from '@app/models';
import { GetUrls, PageState, SetDefaultState, UpdatePageProperties } from '@app/state';
import { getPriorities } from '@app/utility';
import { Select, Store } from '@ngxs/store';
import { EMPTY, Observable, Subject } from 'rxjs';
import { catchError, takeUntil, tap } from 'rxjs/operators';

@Component({
  selector: 'app-edit-page-dialog',
  templateUrl: './edit-page-dialog.component.html',
  styleUrls: ['./edit-page-dialog.component.sass'],
})
export class EditPageDialogComponent implements OnInit, OnDestroy {
  saved = new EventEmitter<boolean>();
  currentValue: string;
  propertyName: string;
  portalId: number;
  tabId: number;

  @Select(PageState.urls)
  pageUrls$: Observable<Array<PageUrlModel>>;

  displayedPrimaryUrlColumns: Array<string> = ['Url', 'UrlType', 'GeneratedBy'];
  priorities: Array<{ name: string; value: number }> = getPriorities();
  errorMessage: string = '';

  protected dispose = new Subject<void>();

  constructor(
    @Inject(MAT_DIALOG_DATA) public data: any,
    private dialogRef: MatDialogRef<EditPageDialogComponent>,
    private store: Store
  ) {}

  ngOnInit(): void {
    this.currentValue = this.data.currentValue;
    this.propertyName = this.data.propertyName;
    this.portalId = this.data.portalId;
    this.tabId = this.data.tabId;

    if (this.propertyName == 'PrimaryUrl') {
      this.store.dispatch(new GetUrls(this.portalId, this.tabId));
    }

    this.store
      .select(PageState.updated)
      .pipe(
        takeUntil(this.dispose),
        tap((updated) => {
          if (updated) {
            this.dialogRef.close();
            this.saved.emit(true);

            this.store.dispatch(new SetDefaultState());
          }
        })
      )
      .subscribe();
  }

  ngOnDestroy(): void {
    this.dispose.next();
    this.dispose.complete();
  }

  cancel(): void {
    this.dialogRef.close();
  }

  save(): void {
    this.errorMessage = '';

    if (this.propertyName == 'Name') {
      if (!this.currentValue) {
        return;
      }

      if (this.currentValue.length === 0) {
        return;
      }

      if (!this.validateExpression()) {
        return;
      }
    }

    this.store
      .dispatch(
        new UpdatePageProperties(
          this.portalId,
          this.tabId,
          this.propertyName,
          this.currentValue
        )
      )
      .pipe(
        catchError((error) => {
          this.errorMessage = error.message;

          return EMPTY;
        })
      )
      .subscribe();
  }

  validateExpression(): boolean {
    var res =
      this.currentValue.toUpperCase().match(/^LPT[1-9]$|^COM[1-9]$/g) ||
      this.currentValue
        .toUpperCase()
        .match(
          /^AUX$|^CON$|^NUL$|^SITEMAP$|^LINKCLICK$|^KEEPALIVE$|^DEFAULT$|^ERRORPAGE$|^LOGIN$|^REGISTER$/g
        );

    if (!!res && res.length > 0) {
      this.errorMessage = 'Invalid Name.';
    }

    return this.errorMessage.length === 0;
  }
}
