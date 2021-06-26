import { RoleModel } from "./role-model"
import { UserModel } from "./user-model"

export class PermissionsModel {
  Roles: Array<RoleModel>;
  Users: Array<UserModel>
}
