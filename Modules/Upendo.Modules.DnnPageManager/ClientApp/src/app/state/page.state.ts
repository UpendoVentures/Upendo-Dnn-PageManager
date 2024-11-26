import { Injectable } from '@angular/core';

import { Observable, of } from 'rxjs';
import { map, switchMap, tap } from 'rxjs/operators';

import { Action, Selector, State, StateContext } from '@ngxs/store';

import {
  GetAllPages,
  UpdatePageIndexing,
  UpdatePageVisibility,
  UpdatePageProperties,
  GetUrls,
  GetPermissions,
  SetDefaultState,
  GetPageModules,
  GetAllPagesWithoutSEO,
} from './page.actions';
import {
  PageModel,
  PageModuleModel,
  PageUrlModel,
  PermissionsModel,
  RoleModel,
  UserModel,
} from '@app/models';
import { PageService } from '@app/service';

export interface PageStateModel {
  isLoading: boolean;
  pages: Array<PageModel>;
  urls: Array<PageUrlModel>;
  permissions: PermissionsModel;
  total: number;
  isWithoutSEO: boolean;
  updated: boolean;
  modules: Array<PageModuleModel>;
}

@State<PageStateModel>({
  name: 'pages',
  defaults: {
    isLoading: false,
    total: 0,
    isWithoutSEO: false,
    pages: [],
    urls: [],
    permissions: {
      Roles: [],
      Users: [],
    },
    updated: false,
    modules: [],
  },
})
@Injectable()
export class PageState {
  constructor(private pageService: PageService) {}

  @Selector()
  static pages(state: PageStateModel): Array<PageModel> {
    return state?.pages ?? [];
  }

  @Selector()
  static modules(state: PageStateModel): Array<PageModuleModel> {
    return state?.modules ?? [];
  }

  @Selector()
  static total(state: PageStateModel): number {
    return state?.total;
  }

  @Selector()
  static totalWithoutSEO(state: PageStateModel): boolean {
    return state?.isWithoutSEO;
  }

  @Selector()
  static urls(state: PageStateModel): Array<PageUrlModel> {
    return state?.urls ?? [];
  }

  @Selector()
  static permissions(state: PageStateModel): PermissionsModel {
    return state?.permissions;
  }

  @Selector()
  static userPermissions(state: PageStateModel): Array<UserModel> {
    return state?.permissions.Users ?? [];
  }

  @Selector()
  static rolePermissions(state: PageStateModel): Array<RoleModel> {
    return state?.permissions.Roles ?? [];
  }

  @Selector()
  static isLoading(state: PageStateModel): boolean {
    return state.isLoading;
  }

  @Selector()
  static updated(state: PageStateModel): boolean {
    return state.updated;
  }

  @Action(GetAllPages, { cancelUncompleted: true })
  getAllPages(
    { patchState }: StateContext<PageStateModel>,
    action: GetAllPages
  ): Observable<any> {
    patchState({ isLoading: true });

    return this.pageService
      .getPages(
        action.portalId,
        action.searchKey,
        action.pageIndex,
        action.pageSize,
        action.sortBy,
        action.sortType,
        action.filterMetadata
      )
      .pipe(
        tap((x) => {
          patchState({
            total: x.Total,
            pages: x.result,
            isLoading: false,
          });
        })
      );
  }

  @Action(GetAllPagesWithoutSEO, { cancelUncompleted: true })
  getAllPagesWithoutSEO(
    { patchState }: StateContext<PageStateModel>,
    action: GetAllPages
  ): Observable<any> {
    patchState({ isLoading: true });

    return this.pageService
      .getPages(
        action.portalId,
        action.searchKey,
        action.pageIndex,
        action.pageSize,
        action.sortBy,
        action.sortType,
        action.filterMetadata
      )
      .pipe(
        tap((x) => {
          var pages = Math.ceil(x.Total / action.pageSize);
          localStorage.setItem('pages', pages.toString());
          console.log('pages', pages);
          patchState({
            isWithoutSEO: x.Total > 0,
          });
        })
      );
  }

  @Action(GetPermissions)
  getPermissions(
    { patchState }: StateContext<PageStateModel>,
    action: GetPermissions
  ): Observable<any> {
    return this.pageService.getPermissions(action.portalId, action.tabId).pipe(
      tap((x) => {
        patchState({
          permissions: x,
        });
      })
    );
  }

  @Action(GetUrls)
  getUrls(
    { patchState }: StateContext<PageStateModel>,
    action: GetUrls
  ): Observable<any> {
    return this.pageService.getUrls(action.portalId, action.tabId).pipe(
      tap((x) => {
        patchState({
          urls: x,
        });
      })
    );
  }

  @Action(UpdatePageIndexing)
  updatePageIndexing(
    { patchState }: StateContext<PageStateModel>,
    action: UpdatePageIndexing
  ): Observable<any> {
    patchState({
      updated: false,
    });

    return this.pageService
      .updatePageIndexing(action.portalId, action.tabId, action.indexed)
      .pipe(
        tap(() => {
          patchState({
            updated: true,
          });
        })
      );
  }

  @Action(UpdatePageProperties)
  updatePageProperties(
    { patchState }: StateContext<PageStateModel>,
    action: UpdatePageProperties
  ): Observable<any> {
    patchState({
      updated: false,
    });

    return this.pageService
      .updatePageProperties(
        action.portalId,
        action.tabId,
        action.propertyName,
        action.propertyValue
      )
      .pipe(
        tap(() => {
          patchState({
            updated: true,
          });
        })
      );
  }

  @Action(UpdatePageVisibility)
  updatePageVisibility(
    { patchState }: StateContext<PageStateModel>,
    action: UpdatePageVisibility
  ): Observable<any> {
    patchState({
      updated: false,
    });

    return this.pageService
      .updatePageVisibility(action.portalId, action.tabId, action.visible)
      .pipe(
        tap(() => {
          patchState({
            updated: true,
          });
        })
      );
  }

  @Action(SetDefaultState)
  setDefaultState({ patchState }: StateContext<PageStateModel>): void {
    patchState({
      updated: false,
    });
  }

  @Action(GetPageModules)
  getPageModules(
    { patchState }: StateContext<PageStateModel>,
    action: GetPageModules
  ): Observable<any> {
    return this.pageService.getPageModules(action.portalId, action.tabId).pipe(
      tap((x) => {
        patchState({
          modules: x,
        });
      })
    );
  }
}
