import api from './api';

export interface LoginRequest {
  email: string;
  password: string;
}

export interface RegisterRequest {
  email: string;
  password: string;
  fullName?: string;
  verificationCode?: string;
}

export interface AuthResponse {
  userId: number;
  email: string;
  fullName: string;
  role: string;
  token: string;
  roles: string[];
  permissions: string[];
  expiresAt: string;
  isProfileComplete: boolean;
  requiresPassword: boolean;
}

export interface ProfileResponse {
  userId: string;
  email: string;
  fullName: string;
  role: string;
}

export interface SetPasswordRequest {
  userId: string;
  password: string;
}

export interface UpdateProfileRequest {
  fullName: string;
  dob: string;
  gender: string;
  heightCm: number;
  weightKg: number;
  activityLevel: string;
}

export interface SendVerificationCodeRequest {
  email: string;
}

class AuthService {
  private getApiBaseUrl(): string {
    return import.meta.env.VITE_API_BASE_URL || '/api';
  }

  getGoogleAuthUrl(): string {
    return `${this.getApiBaseUrl()}/auth/google/web`;
  }

  async login(credentials: LoginRequest): Promise<AuthResponse> {
    const response = await api.post('/auth/login', credentials);
    return response.data;
  }

  async register(userData: RegisterRequest): Promise<AuthResponse> {
    const response = await api.post('/auth/register', userData);
    return response.data;
  }

  async getProfile(): Promise<ProfileResponse> {
    const response = await api.get('/user/profile');
    return response.data;
  }

  async setPassword(data: SetPasswordRequest): Promise<void> {
    await api.post('/auth/set-password', data);
  }

  async sendVerificationCode(data: SendVerificationCodeRequest): Promise<void> {
    await api.post('/auth/send-verification-code', data);
  }

  async updateProfile(data: UpdateProfileRequest): Promise<void> {
    await api.put('/user/profile', data);
  }
}

export default new AuthService();