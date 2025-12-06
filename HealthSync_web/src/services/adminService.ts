import api from './api';

// Helper to map PascalCase to camelCase
const mapUserData = (data: Record<string, unknown>): AdminUserListDto => ({
  userId: data.UserId || data.userId,
  email: data.Email || data.email,
  fullName: data.FullName || data.fullName,
  role: data.Role || data.role,
  isActive: data.IsActive !== undefined ? data.IsActive : data.isActive,
  createdAt: data.CreatedAt || data.createdAt,
});

const mapUserDetailData = (data: Record<string, unknown>): AdminUserDto => ({
  userId: data.UserId || data.userId,
  email: data.Email || data.email,
  fullName: data.FullName || data.fullName,
  role: data.Role || data.role,
  isActive: data.IsActive !== undefined ? data.IsActive : data.isActive,
  createdAt: data.CreatedAt || data.createdAt,
  avatarUrl: data.AvatarUrl || data.avatarUrl,
});

export interface AdminUserListDto {
  userId: number;
  email: string;
  fullName: string;
  role: string;
  isActive: boolean;
  createdAt: string;
}

export interface AdminUserDto {
  userId: number;
  email: string;
  fullName: string;
  role: string;
  isActive: boolean;
  createdAt: string;
  avatarUrl?: string;
}

export interface AdminUsersResponse {
  users: AdminUserListDto[];
  totalCount: number;
  page: number;
  pageSize: number;
}

export interface UpdateUserRoleRequest {
  role: string;
}

export const adminService = {
  getAllUsers: async (
    page: number = 1,
    pageSize: number = 50,
    searchTerm?: string,
    role?: string
  ): Promise<AdminUsersResponse> => {
    const params = new URLSearchParams({
      page: page.toString(),
      pageSize: pageSize.toString(),
    });
    
    if (searchTerm) {
      params.append('searchTerm', searchTerm);
    }
    
    if (role && role !== 'All Roles') {
      params.append('role', role);
    }

    const response = await api.get<Record<string, unknown>>(`/admin/users?${params.toString()}`);
    const data = response.data;
    
    return {
      users: (data.Users || data.users || []).map(mapUserData),
      totalCount: data.TotalCount || data.totalCount || 0,
      page: data.Page || data.page || 1,
      pageSize: data.PageSize || data.pageSize || 50,
    };
  },

  getUserById: async (userId: number): Promise<AdminUserDto> => {
    const response = await api.get<Record<string, unknown>>(`/admin/users/${userId}`);
    return mapUserDetailData(response.data);
  },

  updateUserRole: async (userId: number, role: string): Promise<AdminUserDto> => {
    const response = await api.put<Record<string, unknown>>(`/admin/users/${userId}/role`, { role });
    return mapUserDetailData(response.data);
  },

  deleteUser: async (userId: number): Promise<{ success: boolean; message: string }> => {
    const response = await api.delete<Record<string, unknown>>(`/admin/users/${userId}`);
    const data = response.data;
    return {
      success: data.Success !== undefined ? data.Success : data.success,
      message: data.Message || data.message || 'User deleted successfully',
    };
  },
};
