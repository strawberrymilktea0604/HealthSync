import { useState, useEffect } from "react";
import AdminLayout from "@/components/admin/AdminLayout";
import Table from "@/components/admin/Table";
import Button from "@/components/admin/Button";
import Badge from "@/components/admin/Badge";
import Input from "@/components/admin/Input";
import { adminService, AdminUserListDto } from "@/services/adminService";
import { useToast } from "@/hooks/use-toast";

export default function UserManagement() {
  const [searchTerm, setSearchTerm] = useState("");
  const [selectedRole, setSelectedRole] = useState("All Roles");
  const [users, setUsers] = useState<AdminUserListDto[]>([]);
  const [loading, setLoading] = useState(true);
  const [selectedUser, setSelectedUser] = useState<number | null>(null);
  const [showRoleModal, setShowRoleModal] = useState(false);
  const [showDeleteModal, setShowDeleteModal] = useState(false);
  const [newRole, setNewRole] = useState("");
  const { toast } = useToast();

  useEffect(() => {
    fetchUsers();
  }, [searchTerm, selectedRole]);

  const fetchUsers = async () => {
    try {
      setLoading(true);
      const response = await adminService.getAllUsers(1, 50, searchTerm, selectedRole);
      setUsers(response.users);
    } catch (error) {
      toast({
        title: "Error",
        description: "Failed to load users",
        variant: "destructive",
      });
    } finally {
      setLoading(false);
    }
  };

  const handleUpdateRole = async () => {
    if (!selectedUser || !newRole) return;

    try {
      await adminService.updateUserRole(selectedUser, newRole);
      toast({
        title: "Success",
        description: "User role updated successfully",
      });
      setShowRoleModal(false);
      setSelectedUser(null);
      setNewRole("");
      fetchUsers();
    } catch (error) {
      toast({
        title: "Error",
        description: "Failed to update user role",
        variant: "destructive",
      });
    }
  };

  const handleDeleteUser = async () => {
    if (!selectedUser) return;

    try {
      await adminService.deleteUser(selectedUser);
      toast({
        title: "Success",
        description: "User deleted successfully",
      });
      setShowDeleteModal(false);
      setSelectedUser(null);
      fetchUsers();
    } catch (error) {
      toast({
        title: "Error",
        description: "Failed to delete user",
        variant: "destructive",
      });
    }
  };

  const columns = [
    {
      header: "USER ID",
      accessor: (row: AdminUserListDto) => `USR${row.userId.toString().padStart(3, "0")}`,
    },
    {
      header: "NAME",
      accessor: (row: AdminUserListDto) => (
        <div className="flex items-center gap-3">
          <div className="w-10 h-10 rounded-full bg-[#4A6F6F] text-white flex items-center justify-center font-semibold">
            {row.fullName
              ? row.fullName
                  .split(" ")
                  .map((n) => n[0])
                  .join("")
              : row.email[0].toUpperCase()}
          </div>
          <span className="font-medium">{row.fullName || "N/A"}</span>
        </div>
      ),
    },
    {
      header: "EMAIL",
      accessor: "email" as keyof AdminUserListDto,
    },
    {
      header: "ROLE",
      accessor: (row: AdminUserListDto) => (
        <Badge variant={row.role === "Admin" ? "info" : "success"}>
          {row.role}
        </Badge>
      ),
    },
    {
      header: "STATUS",
      accessor: (row: AdminUserListDto) => (
        <Badge variant={row.isActive ? "success" : "danger"}>
          {row.isActive ? "Active" : "Inactive"}
        </Badge>
      ),
    },
    {
      header: "JOIN DATE",
      accessor: (row: AdminUserListDto) => new Date(row.createdAt).toLocaleDateString(),
    },
    {
      header: "ACTIONS",
      accessor: (row: AdminUserListDto) => (
        <div className="flex gap-2">
          <Button
            size="sm"
            variant="primary"
            onClick={() => {
              setSelectedUser(row.userId);
              setNewRole(row.role);
              setShowRoleModal(true);
            }}
          >
            Edit Role
          </Button>
          <Button
            size="sm"
            variant="danger"
            onClick={() => {
              setSelectedUser(row.userId);
              setShowDeleteModal(true);
            }}
          >
            Delete
          </Button>
        </div>
      ),
    },
  ];

  return (
    <AdminLayout>
      <div className="space-y-6">
        <div className="flex items-center justify-between">
          <div>
            <h2 className="text-3xl font-bold text-gray-900">
              User Management
            </h2>
            <p className="text-gray-600 mt-1">
              Manage users and assign roles
            </p>
          </div>
          <Button size="lg" variant="primary">
            + Add New User
          </Button>
        </div>

        <div className="bg-white rounded-lg shadow p-6">
          <div className="flex gap-4 mb-6">
            <div className="flex-1">
              <Input
                type="text"
                placeholder="Search users..."
                value={searchTerm}
                onChange={(e) => setSearchTerm(e.target.value)}
              />
            </div>
            <select
              value={selectedRole}
              onChange={(e) => setSelectedRole(e.target.value)}
              className="px-4 py-2 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-[#4A6F6F]"
            >
              <option>All Roles</option>
              <option>Admin</option>
              <option>Customer</option>
            </select>
          </div>

          {loading ? (
            <div className="text-center py-12 text-gray-500">Loading...</div>
          ) : (
            <Table data={users} columns={columns} />
          )}
        </div>
      </div>

      {showRoleModal && (
        <div className="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center z-50">
          <div className="bg-white rounded-lg p-6 max-w-md w-full mx-4">
            <h3 className="text-xl font-bold mb-4">Update User Role</h3>
            <div className="mb-4">
              <label className="block text-sm font-medium mb-2">Select Role</label>
              <select
                value={newRole}
                onChange={(e) => setNewRole(e.target.value)}
                className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-[#4A6F6F]"
              >
                <option value="Customer">Customer</option>
                <option value="Admin">Admin</option>
              </select>
            </div>
            <div className="flex gap-3 justify-end">
              <Button
                variant="secondary"
                onClick={() => {
                  setShowRoleModal(false);
                  setSelectedUser(null);
                  setNewRole("");
                }}
              >
                Cancel
              </Button>
              <Button variant="primary" onClick={handleUpdateRole}>
                Update
              </Button>
            </div>
          </div>
        </div>
      )}

      {showDeleteModal && (
        <div className="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center z-50">
          <div className="bg-white rounded-lg p-6 max-w-md w-full mx-4">
            <h3 className="text-xl font-bold mb-4">Delete User</h3>
            <p className="text-gray-600 mb-6">
              Are you sure you want to delete this user? This action cannot be undone.
            </p>
            <div className="flex gap-3 justify-end">
              <Button
                variant="secondary"
                onClick={() => {
                  setShowDeleteModal(false);
                  setSelectedUser(null);
                }}
              >
                Cancel
              </Button>
              <Button variant="danger" onClick={handleDeleteUser}>
                Delete
              </Button>
            </div>
          </div>
        </div>
      )}
    </AdminLayout>
  );
}
