import { AUTH_LOGIN, AUTH_REGISTER  } from "./apiConfig";

type LoginRequest = {
  email: string;
  password: string;
};

type LoginResponse = {
  token?: string;
  refreshToken?: string;
  expiresIn?: number;
  message?: string;
};

class ApiError extends Error {
  status: number;
  data?: unknown;
  constructor(message: string, status = 0, data?: unknown) {
    super(message);
    this.status = status;
    this.data = data;
  }
}
export type RegisterRequest = {
  email: string;
  password: string;
  fullName: string;
  dateOfBirth: string; 
  gender: string;
  heightCm: number;
  weightKg: number;
};

export type RegisterResponse = {
  userId: string;
  message: string;
};

async function requestJson<T>(url: string, opts: RequestInit = {}): Promise<T> {
  const init: RequestInit = {
    headers: {
      "Content-Type": "application/json", 
      ...(opts.headers || {}),
    },
    ...opts,
  };

  const res = await fetch(url, init);
  const text = await res.text();
  let data: unknown = undefined;
  try {
    data = text ? JSON.parse(text) : undefined;
  } catch {
    data = text;
  }

  if (!res.ok) {
    const errorMessage = (data && typeof data === 'object' && 'message' in data ? (data as { message?: string }).message : undefined) || res.statusText || "Request failed";
    throw new ApiError(errorMessage, res.status, data);
  }

  return data as T;
}

export async function login(body: LoginRequest): Promise<LoginResponse> {
  return requestJson<LoginResponse>(AUTH_LOGIN, { method: "POST", body: JSON.stringify(body) });
}
export async function register(body: RegisterRequest): Promise<RegisterResponse> {
  return requestJson<RegisterResponse>(AUTH_REGISTER, { method: "POST", body: JSON.stringify(body) });
}

export default { login, register };
