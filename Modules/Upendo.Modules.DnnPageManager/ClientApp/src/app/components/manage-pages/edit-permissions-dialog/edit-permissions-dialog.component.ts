import { Component, EventEmitter, Inject, OnInit } from '@angular/core';
import { MatDialogRef, MAT_DIALOG_DATA } from '@angular/material/dialog';
import { RoleModel, UserModel } from '@app/models';
import { GetPermissions, PageState } from '@app/state';
import { Select, Store } from '@ngxs/store';
import { Observable } from 'rxjs';

@Component({
  selector: 'app-edit-permissions-dialog',
  templateUrl: './edit-permissions-dialog.component.html',
  styleUrls: ['./edit-permissions-dialog.component.sass'],
})
export class EditPermissionsDialogComponent implements OnInit {
  saved = new EventEmitter<boolean>();

  portalId: number;
  tabId: number;
  isEditDisabled: boolean = true;

  @Select(PageState.userPermissions)
  userPermissions$: Observable<Array<UserModel>>;

  @Select(PageState.rolePermissions)
  rolePermissions$: Observable<Array<RoleModel>>;

  displayedRoleColumns: Array<string> = ['rolename', 'view', 'edit'];

  displayedUserColumns: Array<string> = ['username', 'view', 'edit'];

  constructor(
    @Inject(MAT_DIALOG_DATA) public data: any,
    private dialogRef: MatDialogRef<EditPermissionsDialogComponent>,
    private store: Store
  ) {}

  ngOnInit(): void {
    this.portalId = this.data.portalId;
    this.tabId = this.data.tabId;

    this.store
      .dispatch(new GetPermissions(this.portalId, this.tabId))
      .subscribe();
  }

  cancel(): void {
    this.dialogRef.close();
  }

  edit(): void {}
}
