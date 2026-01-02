import api from './api';

// Helper to map PascalCase to camelCase
const mapUserData = (data: Record<string, unknown>): AdminUserListDto => ({
  userId: data.UserId || data.userId,
  email: data.Email || data.email,
  fullName: data.FullName || data.fullName,
  role: data.Role || data.role,
  isActive: data.IsActive ?? data.isActive,
  createdAt: data.CreatedAt || data.createdAt,
  avatarUrl: data.AvatarUrl || data.avatarUrl,
});

const mapUserDetailData = (data: Record<string, unknown>): AdminUserDto => mapUserData(data);

export interface AdminUserListDto {
  userId: number;
  email: string;
  fullName: string;
  role: string;
  isActive: boolean;
  createdAt: string;
  avatarUrl?: string;
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

export interface CreateUserRequest {
  email: string;
  fullName: string;
  role: string;
  password: string;
}

export interface UpdateUserRequest {
  fullName: string;
  role: string;
}

// --- Dashboard Types ---
export interface KpiValueDto {
  value: number;
  growthRate: number;
  trend: 'up' | 'down' | 'neutral';
}

export interface ActiveUsersDto {
  daily: number;
  monthly: number;
  growthRate: number;
  trend: 'up' | 'down' | 'neutral';
}

export interface ContentCountDto {
  exercises: number;
  foodItems: number;
  total: number;
}

export interface AiUsageDto {
  totalRequests: number;
  costEstimate: number;
  limitWarning: boolean;
}

export interface KpiStatsDto {
  totalUsers: KpiValueDto;
  activeUsers: ActiveUsersDto;
  contentCount: ContentCountDto;
  aiUsage: AiUsageDto;
}

export interface ChartDataDto {
  labels: string[];
  data: number[];
  period: string;
}

export interface PieChartDataDto {
  labels: string[];
  data: number[];
  totalGoals: number;
}

export interface HeatmapPointDto {
  day: number;
  hour: number;
  count: number;
}

export interface DashboardChartsDto {
  userGrowth: ChartDataDto;
  goalSuccessRate: PieChartDataDto;
  activityHeatmap: HeatmapPointDto[];
}

export interface TopContentItemDto {
  id: number;
  name: string;
  count: number;
}

export interface MissedSearchDto {
  keyword: string;
  count: number;
}

export interface ContentInsightsDto {
  topExercises: TopContentItemDto[];
  topFoods: TopContentItemDto[];
  missedSearches: MissedSearchDto[];
}

export interface ServiceStatusDto {
  name: string;
  status: string;
  latencyMs: number;
}

export interface ErrorLogDto {
  id: string;
  timestamp: string;
  message: string;
  code: number;
}

export interface SystemHealthDto {
  status: string;
  services: ServiceStatusDto[];
  recentErrors: ErrorLogDto[];
}

export interface AdminDashboardDto {
  timestamp: string;
  kpiStats: KpiStatsDto;
  charts: DashboardChartsDto;
  contentInsights: ContentInsightsDto;
  systemHealth: SystemHealthDto;
}

export const adminService = {
  getAllUsers: async (
    page: number = 1,
    pageSize: number = 50,
    searchTerm?: string,
    role?: string,
    sortBy?: string,
    sortOrder?: string
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

    if (sortBy) {
      params.append('sortBy', sortBy);
    }

    if (sortOrder) {
      params.append('sortOrder', sortOrder);
    }

    const response = await api.get<Record<string, unknown>>(`/admin/users?${params.toString()}`);
    const data = response.data;

    return {
      users: ((data.Users || data.users || []) as any[]).map(mapUserData),
      totalCount: (data.TotalCount || data.totalCount || 0) as number,
      page: (data.Page || data.page || 1) as number,
      pageSize: (data.PageSize || data.pageSize || 50) as number,
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

  updateUserPassword: async (userId: number, newPassword: string): Promise<{ message: string }> => {
    const response = await api.put<Record<string, unknown>>(`/admin/users/${userId}/password`, { newPassword });
    const data = response.data;
    return {
      message: data.Message || data.message || 'Password updated successfully',
    };
  },

  createUser: async (data: CreateUserRequest): Promise<AdminUserDto> => {
    const response = await api.post<Record<string, unknown>>('/admin/users', data);
    return mapUserDetailData(response.data);
  },

  updateUser: async (userId: number, data: UpdateUserRequest): Promise<AdminUserDto> => {
    const response = await api.put<Record<string, unknown>>(`/admin/users/${userId}`, data);
    return mapUserDetailData(response.data);
  },

  deleteUser: async (userId: number): Promise<{ success: boolean; message: string }> => {
    const response = await api.delete<Record<string, unknown>>(`/admin/users/${userId}`);
    const data = response.data;
    return {
      success: data.Success ?? data.success,
      message: data.Message || data.message || 'User deleted successfully',
    };
  },

  updateUserAvatar: async (userId: number, file: File | null): Promise<{ message: string; avatarUrl: string }> => {
    const formData = new FormData();
    if (file) {
      formData.append('file', file);
    }
    const response = await api.put<Record<string, unknown>>(`/admin/users/${userId}/avatar`, formData, {
      headers: {
        'Content-Type': 'multipart/form-data',
      },
    });
    const data = response.data;
    return {
      message: data.Message || data.message || 'Avatar updated successfully',
      avatarUrl: data.AvatarUrl || data.avatarUrl || '',
    };
  },

  toggleUserStatus: async (userId: number, isActive: boolean): Promise<{ message: string }> => {
    const response = await api.put<Record<string, unknown>>(`/admin/users/${userId}/status`, { isActive });
    const data = response.data;
    return {
      message: data.Message || data.message || 'User status updated successfully',
    };
  },

  getDashboard: async (): Promise<AdminDashboardDto> => {
    const response = await api.get<AdminDashboardDto>('/admin/dashboard');
    return response.data;
  },
};
