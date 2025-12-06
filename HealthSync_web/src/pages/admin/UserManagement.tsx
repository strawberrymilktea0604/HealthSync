import { useState, useEffect } from "react";
import AdminLayout from "@/components/admin/AdminLayout";
import { DataTable } from 'primereact/datatable';
import { Column } from 'primereact/column';
import { Button } from 'primereact/button';
import { InputText } from 'primereact/inputtext';
import { Dropdown } from 'primereact/dropdown';
import { Tag } from 'primereact/tag';
import { Avatar } from 'primereact/avatar';
import { Dialog } from 'primereact/dialog';
import { adminService, AdminUserListDto } from "@/services/adminService";
import { toast } from 'sonner';
import { Can } from "@/components/PermissionGuard";
import { usePermissionCheck } from "@/hooks/usePermissionCheck";
import { Permission } from "@/types/rbac";
import 'primeflex/primeflex.css';

export default function UserManagement() {
  const [searchTerm, setSearchTerm] = useState("");
  const [selectedRole, setSelectedRole] = useState("All Roles");
  const [users, setUsers] = useState<AdminUserListDto[]>([]);
  const [loading, setLoading] = useState(true);
  const [selectedUser, setSelectedUser] = useState<number | null>(null);
  const [showRoleModal, setShowRoleModal] = useState(false);
  const [showDeleteModal, setShowDeleteModal] = useState(false);
  const [newRole, setNewRole] = useState("");
  const { checkAndExecute } = usePermissionCheck();

  useEffect(() => {
    fetchUsers();
  // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [searchTerm, selectedRole]);

  const fetchUsers = async () => {
    try {
      setLoading(true);
      const response = await adminService.getAllUsers(1, 50, searchTerm, selectedRole);
      setUsers(response.users);
    } catch {
      toast.error("Failed to load users");
    } finally {
      setLoading(false);
    }
  };

  const handleUpdateRole = async () => {
    if (!selectedUser || !newRole) return;

    checkAndExecute(
      Permission.UPDATE_USER_ROLES,
      async () => {
        try {
          await adminService.updateUserRole(selectedUser, newRole);
          toast.success("Cập nhật vai trò người dùng thành công");
          setShowRoleModal(false);
          setSelectedUser(null);
          setNewRole("");
          fetchUsers();
        } catch {
          toast.error("Không thể cập nhật vai trò người dùng");
        }
      },
      { errorMessage: "Bạn không có quyền cập nhật vai trò người dùng" }
    );
  };

  const handleDeleteUser = async () => {
    if (!selectedUser) return;

    checkAndExecute(
      Permission.DELETE_USERS,
      async () => {
        try {
          await adminService.deleteUser(selectedUser);
          toast.success("Xóa người dùng thành công");
          setShowDeleteModal(false);
          setSelectedUser(null);
          fetchUsers();
        } catch {
          toast.error("Không thể xóa người dùng");
        }
      },
      { errorMessage: "Bạn không có quyền xóa người dùng" }
    );
  };

  const userIdBodyTemplate = (rowData: AdminUserListDto) => {
    return `USR${rowData.userId.toString().padStart(3, "0")}`;
  };

  const nameBodyTemplate = (rowData: AdminUserListDto) => {
    const initials = rowData.fullName
      ? rowData.fullName.split(" ").map((n) => n[0]).join("")
      : rowData.email[0].toUpperCase();
    
    return (
      <div className="flex align-items-center gap-3">
        <Avatar 
          label={initials}
          size="large" 
          style={{ backgroundColor: '#4A6C6F', color: 'white' }}
          shape="circle"
        />
        <span className="font-medium">{rowData.fullName || "N/A"}</span>
      </div>
    );
  };

  const roleBodyTemplate = (rowData: AdminUserListDto) => {
    return (
      <Tag 
        value={rowData.role} 
        severity={rowData.role === "Admin" ? "info" : "success"}
        style={{ 
          backgroundColor: rowData.role === "Admin" ? "#4A6C6F" : "#F5F5F5", 
          color: rowData.role === "Admin" ? "white" : "#3D3B30",
          borderRadius: '50px',
          padding: '0.25rem 0.75rem'
        }}
      />
    );
  };

  const statusBodyTemplate = (rowData: AdminUserListDto) => {
    return (
      <Tag 
        value={rowData.isActive ? "Active" : "Inactive"} 
        severity={rowData.isActive ? "success" : "danger"}
        style={{ borderRadius: '50px', padding: '0.25rem 0.75rem' }}
      />
    );
  };

  const joinDateBodyTemplate = (rowData: AdminUserListDto) => {
    return new Date(rowData.createdAt).toLocaleDateString('en-US');
  };

  const actionsBodyTemplate = (rowData: AdminUserListDto) => {
    return (
      <div className="flex gap-2">
        <Can permission={Permission.UPDATE_USER_ROLES}>
          <Button
            label="Edit Role"
            size="small"
            style={{ backgroundColor: '#4A6C6F', border: 'none', borderRadius: '6px', fontSize: '0.75rem' }}
            onClick={() => {
              setSelectedUser(rowData.userId);
              setNewRole(rowData.role);
              setShowRoleModal(true);
            }}
          />
        </Can>
        <Can permission={Permission.DELETE_USERS}>
          <Button
            label="Delete"
            size="small"
            severity="danger"
            style={{ borderRadius: '6px', fontSize: '0.75rem' }}
            onClick={() => {
              setSelectedUser(rowData.userId);
              setShowDeleteModal(true);
            }}
          />
        </Can>
      </div>
    );
  };

  const roleOptions = [
    { label: 'All Roles', value: 'All Roles' },
    { label: 'Admin', value: 'Admin' },
    { label: 'Customer', value: 'Customer' }
  ];

  const roleUpdateOptions = [
    { label: 'Customer', value: 'Customer' },
    { label: 'Admin', value: 'Admin' }
  ];

  return (
    <AdminLayout>
      <div className="p-4">
        {/* Header with Search and Filters */}
        <div className="surface-card border-round-xl shadow-2 p-4 mb-4">
          <div className="flex flex-column md:flex-row gap-3 align-items-start md:align-items-center justify-content-between">
            <div className="flex-1 w-full md:w-auto">
              <span className="p-input-icon-left w-full">
                <i className="pi pi-search" />
                <InputText
                  placeholder="Search users..."
                  value={searchTerm}
                  onChange={(e) => setSearchTerm(e.target.value)}
                  className="w-full"
                  style={{ paddingLeft: '2.5rem', borderRadius: '8px' }}
                />
              </span>
            </div>
            <Dropdown
              value={selectedRole}
              options={roleOptions}
              onChange={(e) => setSelectedRole(e.value)}
              placeholder="All Roles"
              className="w-full md:w-auto"
              style={{ minWidth: '180px', borderRadius: '8px' }}
            />
            <Button
              label="Add New User"
              icon="pi pi-plus"
              className="w-full md:w-auto"
              style={{ backgroundColor: '#4A6C6F', border: 'none', borderRadius: '8px' }}
            />
          </div>
        </div>

        {/* Data Table */}
        <div className="surface-card border-round-xl shadow-2">
          <DataTable
            value={users}
            loading={loading}
            paginator
            rows={10}
            dataKey="userId"
            emptyMessage="No users found"
            className="p-datatable-gridlines"
            style={{ borderRadius: '12px', overflow: 'hidden' }}
          >
            <Column 
              field="userId" 
              header="USER ID" 
              body={userIdBodyTemplate}
              headerStyle={{ 
                backgroundColor: '#F5F5F5', 
                color: '#3D3B30', 
                textTransform: 'uppercase', 
                fontSize: '0.875rem', 
                fontWeight: 600,
                borderBottom: '1px solid #E5E5E5'
              }}
            />
            <Column 
              header="NAME" 
              body={nameBodyTemplate}
              headerStyle={{ 
                backgroundColor: '#F5F5F5', 
                color: '#3D3B30', 
                textTransform: 'uppercase', 
                fontSize: '0.875rem', 
                fontWeight: 600,
                borderBottom: '1px solid #E5E5E5'
              }}
            />
            <Column 
              field="email" 
              header="EMAIL"
              headerStyle={{ 
                backgroundColor: '#F5F5F5', 
                color: '#3D3B30', 
                textTransform: 'uppercase', 
                fontSize: '0.875rem', 
                fontWeight: 600,
                borderBottom: '1px solid #E5E5E5'
              }}
            />
            <Column 
              header="ROLE" 
              body={roleBodyTemplate}
              headerStyle={{ 
                backgroundColor: '#F5F5F5', 
                color: '#3D3B30', 
                textTransform: 'uppercase', 
                fontSize: '0.875rem', 
                fontWeight: 600,
                borderBottom: '1px solid #E5E5E5'
              }}
            />
            <Column 
              header="STATUS" 
              body={statusBodyTemplate}
              headerStyle={{ 
                backgroundColor: '#F5F5F5', 
                color: '#3D3B30', 
                textTransform: 'uppercase', 
                fontSize: '0.875rem', 
                fontWeight: 600,
                borderBottom: '1px solid #E5E5E5'
              }}
            />
            <Column 
              header="JOIN DATE" 
              body={joinDateBodyTemplate}
              headerStyle={{ 
                backgroundColor: '#F5F5F5', 
                color: '#3D3B30', 
                textTransform: 'uppercase', 
                fontSize: '0.875rem', 
                fontWeight: 600,
                borderBottom: '1px solid #E5E5E5'
              }}
            />
            <Column 
              header="ACTIONS" 
              body={actionsBodyTemplate}
              headerStyle={{ 
                backgroundColor: '#F5F5F5', 
                color: '#3D3B30', 
                textTransform: 'uppercase', 
                fontSize: '0.875rem', 
                fontWeight: 600,
                borderBottom: '1px solid #E5E5E5'
              }}
            />
          </DataTable>
        </div>
      </div>

      {/* Role Update Dialog */}
      <Dialog
        visible={showRoleModal}
        onHide={() => {
          setShowRoleModal(false);
          setSelectedUser(null);
          setNewRole("");
        }}
        header="Update User Role"
        style={{ width: '450px' }}
        modal
      >
        <div className="flex flex-column gap-4">
          <div className="flex flex-column gap-2">
            <label htmlFor="role" className="font-medium">Select Role</label>
            <Dropdown
              id="role"
              value={newRole}
              options={roleUpdateOptions}
              onChange={(e) => setNewRole(e.value)}
              placeholder="Select a role"
              className="w-full"
            />
          </div>
          <div className="flex justify-content-end gap-2">
            <Button
              label="Cancel"
              outlined
              onClick={() => {
                setShowRoleModal(false);
                setSelectedUser(null);
                setNewRole("");
              }}
            />
            <Button
              label="Update"
              onClick={handleUpdateRole}
              style={{ backgroundColor: '#4A6C6F', border: 'none' }}
            />
          </div>
        </div>
      </Dialog>

      {/* Delete Confirmation Dialog */}
      <Dialog
        visible={showDeleteModal}
        onHide={() => {
          setShowDeleteModal(false);
          setSelectedUser(null);
        }}
        header="Delete User"
        style={{ width: '450px' }}
        modal
      >
        <div className="flex flex-column gap-4">
          <p className="text-600 line-height-3">
            Are you sure you want to delete this user? This action cannot be undone.
          </p>
          <div className="flex justify-content-end gap-2">
            <Button
              label="Cancel"
              outlined
              onClick={() => {
                setShowDeleteModal(false);
                setSelectedUser(null);
              }}
            />
            <Button
              label="Delete"
              severity="danger"
              onClick={handleDeleteUser}
            />
          </div>
        </div>
      </Dialog>
    </AdminLayout>
  );
}
