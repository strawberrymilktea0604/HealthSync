import api from './api';

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

    const response = await api.get<AdminUsersResponse>(`/admin/users?${params.toString()}`);
    return response.data;
  },

  getUserById: async (userId: number): Promise<AdminUserDto> => {
    const response = await api.get<AdminUserDto>(`/admin/users/${userId}`);
    return response.data;
  },

  updateUserRole: async (userId: number, role: string): Promise<AdminUserDto> => {
    const response = await api.put<AdminUserDto>(`/admin/users/${userId}/role`, { role });
    return response.data;
  },

  deleteUser: async (userId: number): Promise<{ success: boolean; message: string }> => {
    const response = await api.delete(`/admin/users/${userId}`);
    return response.data;
  },
};
