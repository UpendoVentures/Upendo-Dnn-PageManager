import {
  AfterViewInit,
  Component,
  OnDestroy,
  OnInit,
  ViewChild,
} from '@angular/core';
import { MatPaginator } from '@angular/material/paginator';
import { MatSort, SortDirection } from '@angular/material/sort';
import { MatDialog } from '@angular/material/dialog';
import { PageModel } from '@app/models';
import {
  EditPageDialogComponent,
  ManagePageModulesDialogComponent,
} from '@app/components';
import { EditPermissionsDialogComponent } from './edit-permissions-dialog/edit-permissions-dialog.component';
import { EMPTY, Observable, Subject } from 'rxjs';
import { catchError, take, takeUntil, tap } from 'rxjs/operators';
import { Context } from '@app/service';
import { Select, Store } from '@ngxs/store';
import {
  GetAllPages,
  PageState,
  SetDefaultState,
  UpdatePageIndexing,
  UpdatePageVisibility,
} from '@app/state';

@Component({
  selector: 'app-manage-pages',
  templateUrl: './manage-pages.component.html',
  styleUrls: ['./manage-pages.component.sass'],
})
export class ManagePagesComponent implements OnInit, OnDestroy, AfterViewInit {
  displayedColumns: Array<string> = [
    'name',
    'title',
    'description',
    'keywords',
    'priority',
    'primaryUrl',
    'permissions',
    'lastUpdated',
    'settings',
    'indexing',
    'visibility',
    'accessibility',
  ];

  @Select(PageState.isLoading)
  isLoading$: Observable<boolean>;

  @Select(PageState.pages)
  dataSource$: Observable<Array<PageModel>>;

  @Select(PageState.total)
  total$: Observable<number>;

  searchText = '';

  showClearSearch = false;

  errorMessage: string = '';

  protected dispose = new Subject<void>();

  @ViewChild(MatPaginator) paginator: MatPaginator;
  @ViewChild(MatSort) sort: MatSort;
  sortActive: string;
  sortDirection: SortDirection;

  constructor(
    private context: Context,
    private dialog: MatDialog,
    private store: Store
  ) {}

  ngOnInit(): void {
    this.store
      .select(PageState.updated)
      .pipe(
        takeUntil(this.dispose),
        tap((updated) => {
          if (updated) {
            this.errorMessage = '';
            this.getPages(this.searchText.length > 0);
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

  ngAfterViewInit(): void {
    setTimeout(() => {
      this.getPages(false);
    }, 1000);

    this.paginator.page
      .pipe(
        takeUntil(this.dispose),
        tap(() => this.getPages(this.searchText.length > 0))
      )
      .subscribe();
  }

  getPages(showClearSearch: boolean): void {
    if (
      this.searchText.length > 0 ||
      (showClearSearch && this.searchText.length === 0)
    ) {
      localStorage.setItem('searchText', this.searchText);
    }

    if (!!this.sort) {
      localStorage.setItem(
        'sortType',
        !!this.sort?.direction ? this.sort?.direction : 'asc'
      );
      localStorage.setItem(
        'sortBy',
        !!this.sort?.active ? this.sort?.active : 'undefined'
      );
    }

    const portalId = this.context._properties.PortalId;
    const sortType = localStorage.getItem('sortType');
    const sortBy = localStorage.getItem('sortBy');
    const searchValue = localStorage.getItem('searchText');

    this.store
      .dispatch(
        new GetAllPages(
          portalId,
          !!searchValue ? searchValue : this.searchText,
          this.paginator ? !!searchValue ? 0 : this.paginator.pageIndex : 0,
          this.paginator ? this.paginator.pageSize : 10,
          !!sortBy ? sortBy : this.sort?.active,
          !!sortType ? sortType : this.sort?.direction
        )
      )
      .pipe(
        take(1),
        tap(() => {
          this.showClearSearch = !!searchValue ? true : showClearSearch;
          if (!!searchValue) {
            this.searchText = searchValue;
          }
          if (!!sortBy && !!sortType) {
            this.sortActive = sortBy;
            this.sortDirection = sortType as SortDirection;
          }
        })
      )
      .subscribe();
  }

  sortData(): void {
    this.getPages(this.searchText.length > 0);
  }

  compare(a: number | string, b: number | string, isAsc: boolean): number {
    return (a < b ? -1 : 1) * (isAsc ? 1 : -1);
  }

  edit(tabId: number, currentValue: string, propertyName: string): void {
    const portalId = this.context._properties.PortalId;

    const dialogRef = this.dialog.open(EditPageDialogComponent, {
      width: '526px',
      data: { currentValue, propertyName, portalId, tabId },
    });

    dialogRef.componentInstance.saved
      .pipe(
        tap((saved) => {
          if (saved) {
            this.getPages(this.searchText.length > 0);
          }
        })
      )
      .subscribe();
  }

  editPermissions(model: PageModel): void {
    const portalId = this.context._properties.PortalId;

    const dialogRef = this.dialog.open(EditPermissionsDialogComponent, {
      width: '526px',
      data: { tabId: model.Id, portalId },
    });

    dialogRef.componentInstance.saved
      .pipe(
        tap((saved) => {
          if (saved) {
            this.getPages(this.searchText.length > 0);
          }
        })
      )
      .subscribe();
  }

  updateIndexing(data: PageModel, value: boolean): void {
    const portalId = this.context._properties.PortalId;

    this.store
      .dispatch(new UpdatePageIndexing(portalId, data.Id, value))
      .pipe(
        catchError((error) => {
          this.errorMessage = error.message;

          return EMPTY;
        })
      )
      .subscribe();
  }

  updateVisibility(data: PageModel, value: boolean): void {
    const portalId = this.context._properties.PortalId;

    this.store
      .dispatch(new UpdatePageVisibility(portalId, data.Id, value))
      .pipe(
        catchError((error) => {
          this.errorMessage = error.message;

          return EMPTY;
        })
      )
      .subscribe();
  }

  isNullOrEmpty(data: String) {
    return data ? data : 'no value';
  }

  clearSearch(): void {
    this.searchText = '';
    localStorage.setItem('searchText', '');
    this.showClearSearch = false;
    this.getPages(false);
  }

  openPageModules(data: PageModel): void {
    const portalId = this.context._properties.PortalId;

    this.dialog.open(ManagePageModulesDialogComponent, {
      width: '526px',
      data: { tabId: data.Id, portalId },
    });
  }

  redirectToPage(data: any): void {
    window.location.href = data.PrimaryUrl;
  }
}
