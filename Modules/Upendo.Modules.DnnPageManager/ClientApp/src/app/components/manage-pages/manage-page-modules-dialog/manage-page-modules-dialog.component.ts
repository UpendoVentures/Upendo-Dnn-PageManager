import { Component, Inject, OnInit } from '@angular/core';
import { MatDialogRef, MAT_DIALOG_DATA } from '@angular/material/dialog';
import { PageModuleModel } from '@app/models';
import { GetPageModules, PageState } from '@app/state';
import { Select, Store } from '@ngxs/store';
import { Observable } from 'rxjs';

@Component({
  selector: 'app-manage-page-modules-dialog',
  templateUrl: './manage-page-modules-dialog.component.html',
  styleUrls: ['./manage-page-modules-dialog.component.sass'],
})
export class ManagePageModulesDialogComponent implements OnInit {
  portalId: number;
  tabId: number;

  @Select(PageState.modules)
  pageModules$: Observable<Array<PageModuleModel>>;

  displayedPageModuleColumns: Array<string> = ['Title', 'ModuleType'];

  constructor(
    @Inject(MAT_DIALOG_DATA) public data: any,
    private dialogRef: MatDialogRef<ManagePageModulesDialogComponent>,
    private store: Store
  ) {}

  ngOnInit(): void {
    this.portalId = this.data.portalId;
    this.tabId = this.data.tabId;

    this.store.dispatch(new GetPageModules(this.portalId, this.tabId));
  }

  cancel(): void {
    this.dialogRef.close();
  }
}
