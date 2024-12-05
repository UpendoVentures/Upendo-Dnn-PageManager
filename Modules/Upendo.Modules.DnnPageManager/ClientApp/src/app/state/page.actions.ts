export class GetAllPages {
  static readonly type = '[USER] Get All Pages';

  constructor(
    public portalId: number,
    public searchKey: string,
    public pageIndex: number,
    public pageSize: number,
    public sortBy: string,
    public sortType: string,
    public filterMetadata: boolean,
    public filterUnpublished: boolean
  ) {}
}

export class GetAllPagesWithoutSEO {
  static readonly type = '[USER] Get All Pages Without SEO';

  constructor(
    public portalId: number,
    public searchKey: string,
    public pageIndex: number,
    public pageSize: number,
    public sortBy: string,
    public sortType: string,
    public filterMetadata: boolean
  ) {}
}

export class GetAllPagesUnpublished {
  static readonly type = '[USER] Get All Pages Unpublished';

  constructor(
    public portalId: number,
    public searchKey: string,
    public pageIndex: number,
    public pageSize: number,
    public sortBy: string,
    public sortType: string,
    public filterUnpublished: boolean
  ) {}
}

export class UpdatePageIndexing {
  static readonly type = '[USER] Update Page Indexing';

  constructor(
    public portalId: number,
    public tabId: number,
    public indexed: boolean
  ) {}
}

export class UpdatePageVisibility {
  static readonly type = '[USER] Update Page Visibility';

  constructor(
    public portalId: number,
    public tabId: number,
    public visible: boolean
  ) {}
}

export class UpdatePageProperties {
  static readonly type = '[USER] Update Page Properties';

  constructor(
    public portalId: number,
    public tabId: number,
    public propertyName: string,
    public propertyValue: string
  ) {}
}

export class GetPermissions {
  static readonly type = '[USER] Get Permissions';

  constructor(public portalId: number, public tabId: number) {}
}

export class GetUrls {
  static readonly type = '[USER] Get Urls';

  constructor(public portalId: number, public tabId: number) {}
}

export class SetDefaultState {
  static readonly type = '[USER] Set Default State';
}

export class GetPageModules {
  static readonly type = '[USER] Get Page Modules';

  constructor(public portalId: number, public tabId: number) {}
}
