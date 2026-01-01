import { useState, useEffect, useCallback } from "react";
import AdminLayout from "@/components/admin/AdminLayout";
import { Card } from 'primereact/card';
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

import { Permission } from "@/types/rbac";
import 'primeflex/primeflex.css';

export default function UserManagement() {
  const [searchTerm, setSearchTerm] = useState("");
  const [selectedRole, setSelectedRole] = useState("All Roles");
  const [users, setUsers] = useState<AdminUserListDto[]>([]);
  const [loading, setLoading] = useState(true);
  const [selectedUser, setSelectedUser] = useState<AdminUserListDto | null>(null);
  const [showEditModal, setShowEditModal] = useState(false);
  const [showCreateModal, setShowCreateModal] = useState(false);
  const [showDeleteModal, setShowDeleteModal] = useState(false);
  const [showAvatarModal, setShowAvatarModal] = useState(false);
  const [showPasswordModal, setShowPasswordModal] = useState(false);
  const [showStatusModal, setShowStatusModal] = useState(false);
  const [page, setPage] = useState(1);
  const [totalRecords, setTotalRecords] = useState(0);
  const [avatarFile, setAvatarFile] = useState<File | null>(null);
  const [avatarPreview, setAvatarPreview] = useState<string | null>(null);
  const [newPassword, setNewPassword] = useState('');
  const [confirmNewPassword, setConfirmNewPassword] = useState('');


  const [pageSize, setPageSize] = useState(10);
  const [sortField, setSortField] = useState<string | undefined>(undefined);
  const [sortOrder, setSortOrder] = useState<0 | 1 | -1 | null | undefined>(null);

  const [formData, setFormData] = useState({
    email: '',
    fullName: '',
    role: 'Customer',
    password: '',
    confirmPassword: ''
  });

  const fetchUsers = useCallback(async () => {
    try {
      setLoading(true);
      let orderStr: string | undefined = undefined;
      if (sortOrder === 1) orderStr = "asc";
      else if (sortOrder === -1) orderStr = "desc";

      const response = await adminService.getAllUsers(page, pageSize, searchTerm, selectedRole, sortField, orderStr);
      setUsers(response.users);
      setTotalRecords(response.totalCount);
    } catch (error: any) {
      console.error('Error fetching users:', error);
      toast.error(error.response?.data?.message || "Không thể tải danh sách người dùng");
    } finally {
      setLoading(false);
    }
  }, [page, pageSize, searchTerm, selectedRole, sortField, sortOrder]);

  useEffect(() => {
    fetchUsers();
  }, [fetchUsers]);

  useEffect(() => {
    // Reset to page 1 when search or filter changes
    setPage(1);
  }, [searchTerm, selectedRole, pageSize]);

  const handleCreate = async () => {
    if (!formData.email || !formData.fullName || !formData.password) {
      toast.error("Vui lòng điền đầy đủ thông tin");
      return;
    }

    if (formData.password !== formData.confirmPassword) {
      toast.error("Mật khẩu xác nhận không khớp");
      return;
    }

    try {
      await adminService.createUser({
        email: formData.email,
        fullName: formData.fullName,
        role: formData.role,
        password: formData.password
      });
      toast.success("Tạo người dùng thành công");
      setShowCreateModal(false);
      setFormData({ email: '', fullName: '', role: 'Customer', password: '', confirmPassword: '' });
      fetchUsers();
    } catch (error: any) {
      toast.error(error.response?.data?.message || "Không thể tạo người dùng");
    }
  };

  const handleEdit = async () => {
    if (!selectedUser || !formData.fullName || !formData.role) {
      toast.error("Vui lòng điền đầy đủ thông tin");
      return;
    }

    // Check if trying to downgrade last admin
    if (selectedUser.role === 'Admin' && formData.role === 'Customer') {
      const adminCount = users.filter(u => u.role === 'Admin').length;
      if (adminCount <= 1) {
        toast.error("Không thể chuyển admin cuối cùng thành user. Phải có ít nhất 1 admin trong hệ thống.");
        return;
      }
    }

    try {
      await adminService.updateUser(selectedUser.userId, {
        fullName: formData.fullName,
        role: formData.role
      });
      toast.success("Cập nhật người dùng thành công");
      setShowEditModal(false);
      setSelectedUser(null);
      fetchUsers();
    } catch (error: any) {
      toast.error(error.response?.data?.message || "Không thể cập nhật người dùng");
    }
  };

  const openEditDialog = (user: AdminUserListDto) => {
    setSelectedUser(user);
    setFormData({
      email: user.email,
      fullName: user.fullName,
      role: user.role,
      password: '',
      confirmPassword: ''
    });
    setShowEditModal(true);
  };

  const handleAvatarChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    const file = e.target.files?.[0];
    if (file) {
      setAvatarFile(file);
      const reader = new FileReader();
      reader.onloadend = () => {
        setAvatarPreview(reader.result as string);
      };
      reader.readAsDataURL(file);
    }
  };

  const handleUpdateAvatar = async () => {
    if (!selectedUser) return;

    try {
      await adminService.updateUserAvatar(selectedUser.userId, avatarFile);
      toast.success("Cập nhật avatar thành công");
      setShowAvatarModal(false);
      setSelectedUser(null);
      setAvatarFile(null);
      setAvatarPreview(null);
      fetchUsers();
    } catch (error: any) {
      toast.error(error.response?.data?.message || "Không thể cập nhật avatar");
    }
  };

  const handleUpdatePassword = async () => {
    if (!selectedUser) return;

    if (!newPassword) {
      toast.error("Vui lòng nhập mật khẩu mới");
      return;
    }

    if (newPassword.length < 6) {
      toast.error("Mật khẩu phải có ít nhất 6 ký tự");
      return;
    }

    if (newPassword !== confirmNewPassword) {
      toast.error("Mật khẩu xác nhận không khớp");
      return;
    }

    try {
      await adminService.updateUserPassword(selectedUser.userId, newPassword);
      toast.success("Cập nhật mật khẩu thành công");
      setShowPasswordModal(false);
      setSelectedUser(null);
      setNewPassword('');
      setConfirmNewPassword('');
    } catch (error: any) {
      toast.error(error.response?.data?.message || "Không thể cập nhật mật khẩu");
    }
  };

  const handleDeleteUser = async () => {
    if (!selectedUser) return;

    // Check if trying to delete last admin
    if (selectedUser.role === 'Admin') {
      const adminCount = users.filter(u => u.role === 'Admin').length;
      if (adminCount <= 1) {
        toast.error("Không thể xóa admin cuối cùng. Phải có ít nhất 1 admin trong hệ thống.");
        return;
      }
    }

    try {
      await adminService.deleteUser(selectedUser.userId);
      toast.success("Xóa người dùng thành công");
      setShowDeleteModal(false);
      setSelectedUser(null);
      fetchUsers();
    } catch (error: any) {
      toast.error(error.response?.data?.message || "Không thể xóa người dùng");
    }
  };

  const handleToggleStatus = async () => {
    if (!selectedUser) return;

    // Check if trying to lock own account
    // We need current user info here. For now, we can check basic logic or rely on backend error
    // Ideally we should decode token or get user info from context

    try {
      await adminService.toggleUserStatus(selectedUser.userId, !selectedUser.isActive);
      toast.success(`Đã ${selectedUser.isActive ? 'khóa' : 'mở khóa'} tài khoản thành công`);
      setShowDeleteModal(false); // Reuse or create new modal if needed, but for toggle usually direct action or simple confirm. 
      // Let's create a specific modal for this or just use confirm dialog?
      // Since we don't have a confirm dialog for status, let's add one.
      setShowStatusModal(false);
      setSelectedUser(null);
      fetchUsers();
    } catch (error: any) {
      toast.error(error.response?.data?.message || "Không thể cập nhật trạng thái");
    }
  };

  const userIdBodyTemplate = (rowData: AdminUserListDto) => {
    return `USR${rowData.userId.toString().padStart(3, "0")}`;
  };

  const nameBodyTemplate = (rowData: AdminUserListDto) => {
    const initials = rowData.fullName
      ? rowData.fullName.split(" ").map((n: string) => n[0]).join("")
      : rowData.email[0].toUpperCase();

    return (
      <div className="flex align-items-center gap-3">
        <Avatar
          image={rowData.avatarUrl ? `${rowData.avatarUrl}?t=${Date.now()}` : rowData.avatarUrl}
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
        <Can permission={Permission.UPDATE_USERS}>
          <Button
            icon="pi pi-image"
            className="p-button-rounded p-button-text p-button-success"
            tooltip="Cập nhật avatar"
            tooltipOptions={{ position: 'top' }}
            onClick={() => {
              setSelectedUser(rowData);
              setAvatarFile(null);
              setAvatarPreview(rowData.avatarUrl || null);
              setShowAvatarModal(true);
            }}
          />
        </Can>
        <Can permission={Permission.UPDATE_USERS}>
          <Button
            icon="pi pi-key"
            className="p-button-rounded p-button-text p-button-warning"
            tooltip="Đổi mật khẩu"
            tooltipOptions={{ position: 'top' }}
            onClick={() => {
              setSelectedUser(rowData);
              setNewPassword('');
              setConfirmNewPassword('');
              setShowPasswordModal(true);
            }}
          />
        </Can>
        <Can permission={Permission.UPDATE_USER_ROLES}>
          <Button
            icon="pi pi-pencil"
            className="p-button-rounded p-button-text p-button-info"
            tooltip="Chỉnh sửa"
            tooltipOptions={{ position: 'top' }}
            onClick={() => openEditDialog(rowData)}
          />
        </Can>
        <Can permission={Permission.DELETE_USERS}>
          <Button
            icon="pi pi-trash"
            className="p-button-rounded p-button-text p-button-danger"
            tooltip="Xóa người dùng"
            tooltipOptions={{ position: 'top' }}
            onClick={() => {
              setSelectedUser(rowData);
              setShowDeleteModal(true);
            }}
          />
        </Can>
        <Can permission={Permission.UPDATE_USERS}>
          <Button
            icon={rowData.isActive ? "pi pi-lock" : "pi pi-lock-open"}
            className={`p-button-rounded p-button-text ${rowData.isActive ? 'p-button-danger' : 'p-button-success'}`}
            tooltip={rowData.isActive ? "Khóa tài khoản" : "Mở khóa tài khoản"}
            tooltipOptions={{ position: 'top' }}
            onClick={() => {
              setSelectedUser(rowData);
              setShowStatusModal(true);
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

  const onPage = (event: any) => {
    setPage(event.page + 1); // PrimeReact pages are 0-based
    setPageSize(event.rows);
  };

  const onSort = (event: any) => {
    setSortField(event.sortField);
    setSortOrder(event.sortOrder);
  };

  return (
    <AdminLayout>
      <div className="p-4">
        <Card title="Quản lý Người dùng">
          <div className="flex justify-content-between align-items-center mb-4">
            <div className="flex gap-2">
              <InputText
                placeholder="Tìm kiếm người dùng..."
                value={searchTerm}
                onChange={(e) => setSearchTerm(e.target.value)}
                className="w-12rem"
              />
              <Dropdown
                value={selectedRole}
                options={roleOptions}
                onChange={(e) => setSelectedRole(e.value)}
                placeholder="All Roles"
                className="w-10rem"
              />
            </div>
            <Button
              label="Thêm người dùng"
              icon="pi pi-plus"
              className="p-button-primary"
              onClick={() => {
                setFormData({ email: '', fullName: '', role: 'Customer', password: '', confirmPassword: '' });
                setShowCreateModal(true);
              }}
            />
          </div>

          <DataTable
            value={users}
            loading={loading}
            paginator
            rows={pageSize}
            rowsPerPageOptions={[10, 15, 25, 50]}
            totalRecords={totalRecords}
            onPage={onPage}
            lazy
            dataKey="userId"
            emptyMessage="Không có người dùng nào"
            sortField={sortField}
            sortOrder={sortOrder}
            onSort={onSort}
          >
            <Column
              field="userId"
              header="USER ID"
              body={userIdBodyTemplate}
              sortable
            />
            <Column
              field="fullname"
              header="NAME"
              body={nameBodyTemplate}
              sortable
            />
            <Column
              field="email"
              header="EMAIL"
              sortable
            />
            <Column
              field="role"
              header="ROLE"
              body={roleBodyTemplate}
              sortable
            />
            <Column
              field="isActive"
              header="STATUS"
              body={statusBodyTemplate}
              sortable
            />
            <Column
              field="createdAt"
              header="JOIN DATE"
              body={joinDateBodyTemplate}
              sortable
            />
            <Column
              header="THAO TÁC"
              body={actionsBodyTemplate}
              style={{ width: '160px' }}
            />
          </DataTable>
        </Card>
      </div>

      {/* Status Confirmation Dialog */}
      <Dialog
        header="Xác nhận thay đổi trạng thái"
        visible={showStatusModal}
        onHide={() => {
          setShowStatusModal(false);
          setSelectedUser(null);
        }}
        footer={
          <div>
            <Button
              label="Hủy"
              icon="pi pi-times"
              onClick={() => {
                setShowStatusModal(false);
                setSelectedUser(null);
              }}
              className="p-button-text"
            />
            <Button
              label={selectedUser?.isActive ? "Khóa" : "Mở khóa"}
              icon="pi pi-check"
              severity={selectedUser?.isActive ? "danger" : "success"}
              onClick={handleToggleStatus}
            />
          </div>
        }
      >
        <p>
          Bạn có chắc muốn {selectedUser?.isActive ? "khóa" : "mở khóa"} tài khoản
          <strong> {selectedUser?.fullName || selectedUser?.email}</strong>?
        </p>
      </Dialog>

      {/* Create User Dialog */}
      <Dialog
        header="Thêm người dùng mới"
        visible={showCreateModal}
        onHide={() => {
          setShowCreateModal(false);
          setFormData({ email: '', fullName: '', role: 'Customer', password: '', confirmPassword: '' });
        }}
        footer={
          <div>
            <Button
              label="Hủy"
              icon="pi pi-times"
              onClick={() => {
                setShowCreateModal(false);
                setFormData({ email: '', fullName: '', role: 'Customer', password: '', confirmPassword: '' });
              }}
              className="p-button-text"
            />
            <Button label="Tạo" icon="pi pi-check" onClick={handleCreate} />
          </div>
        }
      >
        <div className="p-fluid">
          <div className="field">
            <label htmlFor="create-email">Email</label>
            <InputText
              id="create-email"
              value={formData.email}
              onChange={(e) => setFormData({ ...formData, email: e.target.value })}
              placeholder="user@example.com"
            />
          </div>
          <div className="field">
            <label htmlFor="create-fullName">Họ và tên</label>
            <InputText
              id="create-fullName"
              value={formData.fullName}
              onChange={(e) => setFormData({ ...formData, fullName: e.target.value })}
              placeholder="Nguyễn Văn A"
            />
          </div>
          <div className="field">
            <label htmlFor="create-role">Vai trò</label>
            <Dropdown
              id="create-role"
              value={formData.role}
              options={roleUpdateOptions}
              onChange={(e) => setFormData({ ...formData, role: e.value })}
              placeholder="Chọn vai trò"
            />
          </div>
          <div className="field">
            <label htmlFor="create-password">Mật khẩu</label>
            <InputText
              id="create-password"
              type="password"
              value={formData.password}
              onChange={(e) => setFormData({ ...formData, password: e.target.value })}
              placeholder="Nhập mật khẩu"
            />
          </div>
          <div className="field">
            <label htmlFor="create-confirmPassword">Xác nhận mật khẩu</label>
            <InputText
              id="create-confirmPassword"
              type="password"
              value={formData.confirmPassword}
              onChange={(e) => setFormData({ ...formData, confirmPassword: e.target.value })}
              placeholder="Nhập lại mật khẩu"
            />
          </div>
        </div>
      </Dialog>

      {/* Edit User Dialog */}
      <Dialog
        header="Chỉnh sửa người dùng"
        visible={showEditModal}
        onHide={() => {
          setShowEditModal(false);
          setSelectedUser(null);
        }}
        footer={
          <div>
            <Button
              label="Hủy"
              icon="pi pi-times"
              onClick={() => {
                setShowEditModal(false);
                setSelectedUser(null);
              }}
              className="p-button-text"
            />
            <Button label="Lưu" icon="pi pi-check" onClick={handleEdit} />
          </div>
        }
      >
        <div className="p-fluid">
          <div className="field">
            <label htmlFor="edit-email">Email</label>
            <InputText
              id="edit-email"
              value={formData.email}
              disabled
            />
          </div>
          <div className="field">
            <label htmlFor="edit-fullName">Họ và tên</label>
            <InputText
              id="edit-fullName"
              value={formData.fullName}
              onChange={(e) => setFormData({ ...formData, fullName: e.target.value })}
            />
          </div>
          <div className="field">
            <label htmlFor="edit-role">Vai trò</label>
            <Dropdown
              id="edit-role"
              value={formData.role}
              options={roleUpdateOptions}
              onChange={(e) => setFormData({ ...formData, role: e.value })}
              placeholder="Chọn vai trò"
            />
          </div>
        </div>
      </Dialog>

      {/* Avatar Update Dialog */}
      <Dialog
        header="Cập nhật Avatar"
        visible={showAvatarModal}
        onHide={() => {
          setShowAvatarModal(false);
          setSelectedUser(null);
          setAvatarFile(null);
          setAvatarPreview(null);
        }}
        footer={
          <div>
            <Button
              label="Hủy"
              icon="pi pi-times"
              onClick={() => {
                setShowAvatarModal(false);
                setSelectedUser(null);
                setAvatarFile(null);
                setAvatarPreview(null);
              }}
              className="p-button-text"
            />
            <Button label="Cập nhật" icon="pi pi-check" onClick={handleUpdateAvatar} />
          </div>
        }
      >
        <div className="p-fluid">
          <div className="field text-center mb-4">
            {avatarPreview && (
              <img
                src={avatarPreview}
                alt="Avatar Preview"
                className="border-circle mx-auto"
                style={{ width: '150px', height: '150px', objectFit: 'cover' }}
              />
            )}
          </div>
          <div className="field">
            <label htmlFor="avatar-file">Chọn ảnh mới</label>
            <input
              id="avatar-file"
              type="file"
              accept="image/*"
              onChange={handleAvatarChange}
              className="p-inputtext"
            />
            <small className="text-500">Nếu không chọn ảnh, hệ thống sẽ tạo avatar ngẫu nhiên</small>
          </div>
        </div>
      </Dialog>

      {/* Password Update Dialog */}
      <Dialog
        header="Đổi mật khẩu"
        visible={showPasswordModal}
        onHide={() => {
          setShowPasswordModal(false);
          setSelectedUser(null);
          setNewPassword('');
          setConfirmNewPassword('');
        }}
        footer={
          <div>
            <Button
              label="Hủy"
              icon="pi pi-times"
              onClick={() => {
                setShowPasswordModal(false);
                setSelectedUser(null);
                setNewPassword('');
                setConfirmNewPassword('');
              }}
              className="p-button-text"
            />
            <Button label="Cập nhật" icon="pi pi-check" onClick={handleUpdatePassword} />
          </div>
        }
      >
        <div className="p-fluid">
          <div className="field mb-3">
            <label htmlFor="user-email">Email người dùng</label>
            <InputText
              id="user-email"
              value={selectedUser?.email || ''}
              disabled
            />
          </div>
          <div className="field mb-3">
            <label htmlFor="new-password">Mật khẩu mới *</label>
            <InputText
              id="new-password"
              type="password"
              value={newPassword}
              onChange={(e) => setNewPassword(e.target.value)}
              placeholder="Nhập mật khẩu mới (tối thiểu 6 ký tự)"
            />
          </div>
          <div className="field">
            <label htmlFor="confirm-new-password">Xác nhận mật khẩu *</label>
            <InputText
              id="confirm-new-password"
              type="password"
              value={confirmNewPassword}
              onChange={(e) => setConfirmNewPassword(e.target.value)}
              placeholder="Nhập lại mật khẩu mới"
            />
          </div>
        </div>
      </Dialog>

      {/* Delete Confirmation Dialog */}
      <Dialog
        header="Xác nhận xóa"
        visible={showDeleteModal}
        onHide={() => {
          setShowDeleteModal(false);
          setSelectedUser(null);
        }}
        footer={
          <div>
            <Button
              label="Hủy"
              icon="pi pi-times"
              onClick={() => {
                setShowDeleteModal(false);
                setSelectedUser(null);
              }}
              className="p-button-text"
            />
            <Button
              label="Xóa"
              icon="pi pi-check"
              severity="danger"
              onClick={handleDeleteUser}
            />
          </div>
        }
      >
        <p>Bạn có chắc muốn xóa người dùng này? Hành động này không thể hoàn tác.</p>
      </Dialog>
    </AdminLayout>
  );
}
